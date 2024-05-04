import './polyfill-fromentries';
import './polyfill-textencoder';
import type { MonoConfig } from './dotnet';
export interface ManifestEntry {
    path: string;
    compressed: boolean;
    b64: string;
}
export interface Manifest {
    manifest: ManifestEntry[];
    config: MonoConfig;
}
export declare class DotNet {
    private readonly manifest;
    private readonly monoConfig;
    private readonly fileMap;
    private tickIndex;
    private runtimeApi;
    private readonly imports;
    private exports;
    private _tickBarrier?;
    private perfFn?;
    private verboseLogging;
    private get isTickBarrier();
    constructor(manifest: Readonly<Manifest>, env: 'world'|'arena');
    setModuleImports(moduleName: string, imports: Record<string, unknown>): void;
    setVerboseLogging(verboseLogging: boolean): void;
    setPerfFn(perfFn: () => number): void;
    getExports(): Record<string, any> | undefined;
    init(): void;
    loop(loopFn?: () => void): void;
    private profile;
    private profileAccum;
    private decodeManifest;
    private createRuntime;
    private downloadResource;
    private setupRuntime;
    private runPendingAsyncActions;
    private tickBarrier;
}
