import { log, error } from './logging';

export type CallbackFn = (...args: unknown[]) => unknown;

interface TimeoutEntry {
    handle: number;
    callback: CallbackFn;
    args: unknown[];
    nextTime: number;
    interval?: number;
}

const virtualTimerQueue: Readonly<TimeoutEntry>[] = [];

let virtualTime = 0;
let nextHandleId = 1;
let isTicking = false;
let shouldCancelAdvanceFrame = false;
const pendingClearTimeouts: number[] = [];
const pendingClearIntervals: number[] = [];
const pendingInserts: Readonly<TimeoutEntry>[] = [];

const DEBUG = false;

function doInsert(entry: Readonly<TimeoutEntry>): void {
    let i: number;
    // TODO: Binary search for more efficient insertion, given that virtualTimerQueue is sorted by nextTime
    for (i = 0; i < virtualTimerQueue.length; ++i) {
        if (entry.nextTime < virtualTimerQueue[i].nextTime) {
            break;
        }
    }
    virtualTimerQueue.splice(i, 0, entry);
}

function insertNewTimeout(callback: CallbackFn, args: unknown[], nextTime: number, interval?: number): number {
    const handle = nextHandleId++;
    const entry: Readonly<TimeoutEntry> = {
        handle,
        callback,
        args,
        nextTime,
        interval,
    };
    if (isTicking) {
        pendingInserts.push(entry);
    } else {
        doInsert(entry);
    }
    return handle;
}

function setTimeout(callback: CallbackFn, delay?: number, ...args: unknown[]): number {
    if (delay == null) { delay = 0; }
    const handle = insertNewTimeout(callback, args, virtualTime + delay, undefined);
    if (DEBUG) { log(`intercepted setTimeout with delay of ${delay}, handle = ${handle}`); }
    return handle;
}

function clearTimeout(handle: number) {
    if (isTicking) {
        pendingClearTimeouts.push(handle);
        return;
    }
    for (let i = 0; i < virtualTimerQueue.length; ++i) {
        if (virtualTimerQueue[i].interval != null) { return; }
        if (virtualTimerQueue[i].handle !== handle) { return; }
        virtualTimerQueue.splice(i, 1);
        break;
    }
}

function setInterval(callback: CallbackFn, interval: number, ...args: unknown[]): number {
    const handle = insertNewTimeout(callback, args, virtualTime + interval, interval);
    if (DEBUG) { log(`intercepted setInterval with interval of ${interval}, handle = ${handle}`); }
    return -1;
}

function clearInterval(handle: number) {
    if (isTicking) {
        pendingClearIntervals.push(handle);
        return;
    }
    for (let i = 0; i < virtualTimerQueue.length; ++i) {
        if (virtualTimerQueue[i].interval == null) { return; }
        if (virtualTimerQueue[i].handle !== handle) { return; }
        virtualTimerQueue.splice(i, 1);
        break;
    }
}

function setImmediate(callback: CallbackFn, ...args: unknown[]): number {
    const handle = setTimeout(callback, 0, ...args);
    if (DEBUG) { log(`intercepted setImmediate, handle = ${handle}`); }
    return handle;
}

function advanceFrame(): number {
    //log(`advanceFrame`);
    shouldCancelAdvanceFrame = false;
    if (isTicking) {
        // This can happen if script execution is terminated half way through processing timers last frame
        // In this case, let's process any pending ops now and clean up
        log(`detected incomplete async frame, possibly from script execution termination`);
        isTicking = false;
        processPendingOps();
    }
    if (virtualTimerQueue.length === 0) {
        return 0;
    }
    virtualTime = Math.max(virtualTime, virtualTimerQueue[0].nextTime);
    if (DEBUG) { log(`advancing to ${virtualTime}`); }
    isTicking = true;
    let numProcessed = 0;
    while (!shouldCancelAdvanceFrame && virtualTimerQueue.length > 0 && virtualTimerQueue[0].nextTime <= virtualTime) {
        const entry = virtualTimerQueue[0];
        if (entry.interval == null && pendingClearTimeouts.includes(entry.handle)) { continue; }
        if (entry.interval != null && pendingClearIntervals.includes(entry.handle)) { continue; }
        processTimer(entry);
        virtualTimerQueue.shift();
        ++numProcessed;
    }
    if (shouldCancelAdvanceFrame) {
        //log(`advanceFrame cancelled before completion`);
    }
    isTicking = false;
    processPendingOps();
    return numProcessed;
}

function cancelAdvanceFrame(): void {
    if (!isTicking) { return; }
    shouldCancelAdvanceFrame = true;
}

function processPendingOps(): void {
    for (const handle of pendingClearTimeouts) {
        clearTimeout(handle);
    }
    pendingClearTimeouts.length = 0;
    for (const handle of pendingClearIntervals) {
        clearInterval(handle);
    }
    pendingClearIntervals.length = 0;
    for (const entry of pendingInserts) {
        doInsert(entry);
    }
    pendingInserts.length = 0;
}

function processTimer(entry: Readonly<TimeoutEntry>): void {
    if (DEBUG) { log(`firing timer with handle ${entry.handle}`); }
    try {
        entry.callback(...entry.args);
    } catch (err) {
        error(err);
    }
    if (entry.interval != null) {
        insertNewTimeout(entry.callback, entry.args, entry.nextTime + entry.interval, entry.interval);
    }
}

declare const global : any;
global.setTimeout = setTimeout;
global.clearTimeout = clearTimeout;
global.setInterval = setInterval;
global.clearInterval = clearInterval;
global.setImmediate = setImmediate;

export {
    setTimeout,
    clearTimeout,
    setInterval,
    clearInterval,
    setImmediate,
    advanceFrame,
    cancelAdvanceFrame,
};
