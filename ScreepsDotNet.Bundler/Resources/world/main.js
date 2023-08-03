console.log(`#${Game.time}: INIT state refreshed, beginning init sequence`);

var bootloader, bundle, dotNet, dotNetExports, startupComplete;
var accumCpuTime = 0;

function getCpuTime() {
    return accumCpuTime + Game.cpu.getUsed();
}

// ~50ms
function loadJs() {
    const before = Game.cpu.getUsed();
    bootloader = require('./bootloader');
    bundle = require('./bundle');
    const after = Game.cpu.getUsed();
    console.log(`#${Game.time} INIT: loaded js dependencies (${after - before}ms)`);
}

// ~310ms
function initDotNet() {
    const before = Game.cpu.getUsed();
    dotNet = new bootloader.DotNet(bundle, 'world');
    dotNet.setPerfFn(function() { return getCpuTime() * 1000000; });
    dotNet.setVerboseLogging(false);
    dotNet.setModuleImports('object', {
        getConstructorOf: (x) => Object.getPrototypeOf(x).constructor,
        create: Object.create,
        set: (obj, key, val) => obj[key] = val,
        get: (obj, key) => obj[key],
    });
    dotNet.setModuleImports('game', {
        ...Game,
        getGameObj: () => Game,
    });
    try {
        dotNet.init();
    } catch (err) {
        console.log(`#${Game.time} INIT: fatal error initialising runtime ${err}`);
        console.log(`#${Game.time} INIT: shutting down and starting again`);
        //Game.cpu.halt && Game.cpu.halt();
        return false;
    }
    const after = Game.cpu.getUsed();
    console.log(`#${Game.time} INIT: created dotnet instance (${after - before}ms)`);
}

function startup() {
    if (!dotNet) {
        if (Game.cpu.bucket < 1000) {
            console.log(`#${Game.time}: INIT FROM COLD START REFUSED - waiting for bucket to be full (currently ${Game.cpu.bucket}ms of 1000ms)`);
            return false;
        }
        loadJs();
        initDotNet();
        return false;
    }
    if (Game.cpu.tickLimit < 500) {
        console.log(`#${Game.time}: INIT REFUSED - waiting for tickLimit to be full (currently ${Game.cpu.tickLimit}ms of 500ms)`);
        return false;
    }

    if (!dotNet.ready) {
        dotNet.loop();
        return false;
    }
    // if (!dotNet.ready) {
    //     console.log(`#${Game.time} INIT: fatal error initialising runtime (was not ready at expected time, probably script exec timeout)`);
    //     console.log(`#${Game.time} INIT: shutting down and starting again`);
    //     //Game.cpu.halt && Game.cpu.halt();
    //     return false;
    // }
    dotNetExports = dotNet.getExports();

    console.log(`#${Game.time}: INIT SUCCESS (${Game.cpu.tickLimit - Game.cpu.getUsed()}ms remaining cpu this tick)`);

    return true;
}

function loop() {
    if (!startupComplete) {
        startupComplete = startup();
    }
    if (startupComplete && dotNetExports) {
        dotNet.loop(() => { dotNetExports.ScreepsDotNet.Program.Loop(); });
    }
    accumCpuTime += Game.cpu.getUsed();
}

module.exports.loop = loop;
