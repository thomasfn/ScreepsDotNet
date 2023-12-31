import * as fflate from 'fflate';
import './polyfill-fromentries';
import './polyfill-textencoder';
import { toBytes as decodeB64 } from 'fast-base64/js';
import { decode as decodeB32768 } from 'base32768'
import { debug, error, log, setSuppressedLogMode } from './logging';

import type { MonoConfig, RuntimeAPI } from './dotnet';
import createDotnetRuntime from './dotnet'

import { advanceFrame, cancelAdvanceFrame } from './timeouts';
import Promise from 'promise-polyfill';

export interface BaseManifestEntry {
    path: string;
    originalSize: number;
    compressed: boolean;
}

export interface B64EncodedData {
    b64: string;
}

export interface B32768TextEncodedData {
    b32768: string;
}

export interface BinEncodedData {
    offset: number;
    length: number;
}

export type ManifestEntry = BaseManifestEntry & (B64EncodedData | B32768TextEncodedData | BinEncodedData);

export interface Manifest {
    manifest: ManifestEntry[];
    config: MonoConfig;
}

declare function require(moduleName: string): unknown;

export class DotNet {
    private readonly manifest: readonly Readonly<ManifestEntry>[];
    private readonly monoConfig: Readonly<MonoConfig>;
    private readonly fileMap: Record<string, Uint8Array> = {};
    private readonly isArena: boolean;
    private readonly isWorld: boolean;
    private tickIndex: number = 0;
    private runtimeApi: RuntimeAPI | undefined;
    private readonly imports: Record<string, Record<string, unknown>> = {};
    private exports: Record<string, unknown> | undefined;
    private _tickBarrier?: number;
    private perfFn?: () => number;
    private verboseLogging: boolean = false;
    private _ready: boolean = false;
    private startSetupRuntime: boolean = false;
    private pendingError?: Error;
    private readonly customRuntimeSetupFnList: ((runtime: RuntimeAPI) => void)[] = [];

    public get ready() { return this._ready; }

    private get isTickBarrier() { return this._tickBarrier != null && this._tickBarrier > this.tickIndex; }

    public constructor(manifest: Readonly<Manifest>, env: 'world'|'arena') {
        this.manifest = manifest.manifest;
        this.monoConfig = manifest.config;
        this.isArena = env === 'arena';
        this.isWorld = env === 'world';
    }

    public setModuleImports(moduleName: string, imports: Record<string, unknown>): void {
        this.imports[moduleName] = imports;
    }

    public setVerboseLogging(verboseLogging: boolean): void {
        this.verboseLogging = verboseLogging;
    }

    public setPerfFn(perfFn: () => number): void {
        this.perfFn = perfFn;
    }

    public addCustomRuntimeSetupFunction(setupFn: (runtime: RuntimeAPI) => void) {
        this.customRuntimeSetupFnList.push(setupFn);
    }

    public getExports(): Record<string, any> | undefined {
        return this.exports;
    }

    public init(): void {
        if (this.isArena) { setSuppressedLogMode(true); }
        this.decodeManifest();
        this.createRuntime();
    }

    public loop(loopFn?: () => void): void {
        if (this.isArena) { setSuppressedLogMode(false); }
        try {
            ++this.tickIndex;
            let profiler = this.profile();
            this.runPendingAsyncActions();
            if (loopFn) { loopFn(); }
            profiler = this.profile(profiler, 'loop');
        } finally {
            if (this.isArena) { setSuppressedLogMode(true); }
        }
    }

    private profile(marker?: number, blockName?: string): number {
        if (!this.perfFn) { return 0; }
        const cpuTime = this.perfFn();
        if (blockName == null || marker == null) { return cpuTime; }
        const delta = cpuTime - marker;
        log('PROFILE', blockName, `${((delta / 100000) | 0) / 10} ms`);
        return cpuTime;
    }

    private profileAccum(marker: number, blockName?: string): number {
        if (!this.perfFn) { return 0; }
        if (blockName == null) {
            return this.perfFn() - marker;
        }
        log('PROFILE', blockName, `${((marker / 100000) | 0) / 10} ms`);
        return marker;
    }

    private decodeManifest(): void {
        let profiler = this.profile();
        let profilerB64 = 0, profilerInflate = 0;
        let totalBytes = 0;
        let binaryBaseData: ArrayBuffer | undefined;
        for (const entry of this.manifest) {
            const profilerB64Marker = this.profile();
            let fileDataRaw: Uint8Array;
            if ('b64' in entry) {
                fileDataRaw = decodeB64(entry.b64);
            } else if ('b32768' in entry) {
                fileDataRaw = decodeB32768(entry.b32768);
            } else if ('offset' in entry && 'length' in entry) {
                if (!binaryBaseData) {
                    binaryBaseData = (require('./bundle-bin') as Uint8Array).buffer;
                }
                fileDataRaw = new Uint8Array(binaryBaseData, entry.offset, entry.length);
            } else {
                log(`entry '${(entry as BaseManifestEntry).path}' does not contain b64, b32768 or binary data`);
                continue;
            }
            profilerB64 += this.profileAccum(profilerB64Marker);
            if (entry.compressed) {
                const profilerInflateMarker = this.profile();
                const fileData = new Uint8Array(entry.originalSize);
                fflate.inflateSync(fileDataRaw, { out: fileData });
                profilerInflate += this.profileAccum(profilerInflateMarker);
                this.fileMap[entry.path] = fileData;
                totalBytes += fileData.length;
            } else {
                this.fileMap[entry.path] = fileDataRaw;
                totalBytes += fileDataRaw.length;
            }
        }
        log(`loaded ${this.manifest.length} items from the manifest, totalling ${(totalBytes / 1024)|0} KiB of data`);
        profiler = this.profile(profiler, 'decodeManifest');
        profilerB64 = this.profileAccum(profilerB64, 'decodeManifest (b64)');
        profilerInflate = this.profileAccum(profilerInflate, 'decodeManifest (inflate)');
    }

    private createRuntime(): void {
        debug(`creating dotnet runtime...`);
        createDotnetRuntime((api) => {
            return {
                config: {
                    ...this.monoConfig,
                    diagnosticTracing: this.verboseLogging,
                },
                imports: {},
                downloadResource: (request) => ({
                    name: request.name,
                    url: request.resolvedUrl!,
                    response: Promise.resolve(this.downloadResource(request.resolvedUrl!)),
                }),
                preRun: () => {
                    if (this.isWorld) { this.tickBarrier(); }
                },
            };
        }).then(x => {
            this.runtimeApi = x;
            return this.setupRuntime();
        });
        this.runPendingAsyncActions();
    }

    private downloadResource(url: string): Response {
        if (this.fileMap[url]) {
            //log(`got downloadResource for '${request.resolvedUrl}' - found in file map`);
            return {
                ok: true,
                url,
                status: 200,
                statusText: 'ok',
                arrayBuffer: () => Promise.resolve(this.fileMap[url]),
                json: () => Promise.reject('json not yet supported'),
            } as unknown as Response;
        } else {
            //log(`got downloadResource for '${request.resolvedUrl!}' - NOT found in file map`);
            return {
                ok: false,
                url,
                status: 404,
                statusText: 'not found',
                arrayBuffer: () => undefined,
                json: () => undefined,
            } as unknown as Response;
        }
    }

    private async setupRuntime(): Promise<void> {
        if (!this.runtimeApi) {
            this.pendingError = new Error(`Tried to setupRuntime when runtimeApi was not set`);
            return;
        }
        if (this.startSetupRuntime) {
            this.pendingError = new Error(`Tried to setupRuntime when it was already called`);
            return;
        }
        this.startSetupRuntime = true;
        debug(`setting up dotnet runtime...`);
        for (const setupFn of this.customRuntimeSetupFnList) {
            setupFn(this.runtimeApi);
          }
        for (const moduleName in this.imports) {
            this.runtimeApi.setModuleImports(moduleName, this.imports[moduleName]);
        }
        this.exports = await this.runtimeApi.getAssemblyExports(this.monoConfig.mainAssemblyName!);
        if (this.exports) {
            debug(`exports: ${Object.keys(this.exports)}`);
        } else {
            debug(`failed to retrieve exports`);
        }
        let profiler = this.profile();
        try {
            await this.runtimeApi.runMain(this.monoConfig.mainAssemblyName!, []);
        } catch (err) {
            error(`got error when running Program.Main(): ${(err as Error).stack}`);
        }
        profiler = this.profile(profiler, 'runMain');
        this._ready = true;
    }

    private runPendingAsyncActions(): void {
        if (this.isTickBarrier) {
            debug(`refusing runPendingAsyncActions as tick barrier is in place`);
            return;
        }
        let numTimersProcessed: number;
        do {
            numTimersProcessed = advanceFrame();
            if (numTimersProcessed > 0) {
                //debug(`ran ${numTimersProcessed} async timers to completion`);
            }
        } while (numTimersProcessed > 0 && !this.isTickBarrier && !this.pendingError);
        if (this.pendingError) { throw this.pendingError; }
    }

    private tickBarrier(): void {
        //debug(`TICK BARRIER`);
        this._tickBarrier = this.tickIndex + 1;
        cancelAdvanceFrame();
    }
}
