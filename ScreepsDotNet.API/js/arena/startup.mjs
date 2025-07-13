import { getCpuTime } from 'game/utils';

import { Bootloader } from './bootloader';
import { WasmBytes } from './bundle';

const bootloader = new Bootloader('arena', () => getCpuTime() / 1e+6);
bootloader.compile(WasmBytes);
bootloader.start([/*CUSTOM_INIT_EXPORT_NAMES*/]); 
