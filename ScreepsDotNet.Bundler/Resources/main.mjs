import * as utils from 'game/utils';
import { DotNet } from './bootloader';
import * as manifest from './bundle.mjs';

const dotNet = new DotNet(manifest);
dotNet.setPerfFn(utils.getCpuTime);
dotNet.setModuleImports('game/utils', utils);
dotNet.init();

const exports = dotNet.getExports();

export function loop() {
    dotNet.loop();
    exports.ScreepsDotNet.Program.Loop();
}

