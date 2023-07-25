const pendingLogMessages: unknown[][] = [];
let suppressedLogMode = false;

function dispatchLog(...data: unknown[]): void {
    console['log']('DOTNET ', ...data);
}

function log(...data: unknown[]): void {
    if (suppressedLogMode) {
        pendingLogMessages.push(data);
        return;
    }
    dispatchLog(...data);
}

function warn(...data: unknown[]): void {
    log('WARN: ', ...data);
}

function debug(...data: unknown[]): void {
    log('DEBUG: ', ...data);
}

function error(...data: unknown[]): void {
    log('ERROR: ', ...data);
}

function trace(...data: unknown[]): void {
    log('TRACE: ', ...data);
}

function assert(condition: unknown, ...data: unknown[]): void {
    if (condition) { return; }
    log('ASSERTION FAIL: ', ...data);
}

function setSuppressedLogMode(mode: boolean): void {
    suppressedLogMode = mode;
    if (!suppressedLogMode) {
        for (const data of pendingLogMessages) {
            dispatchLog(...data);
        }
        pendingLogMessages.length = 0;
    }
}

export {
    log,
    warn,
    debug,
    error,
    trace,
    assert,
    setSuppressedLogMode,
};
