import * as fs from 'fs';

import typescript from '@rollup/plugin-typescript';
import { nodeResolve } from '@rollup/plugin-node-resolve';
import replace from '@rollup/plugin-replace';
import { prepend } from 'rollup-plugin-insert'
import { babel } from '@rollup/plugin-babel';
import path from 'path';

export default {
	input: 'src/bootloader.ts',
    output: {
        file: 'dist/bootloader.js',
        format: 'iife',
        inlineDynamicImports: true,
        name: 'bootloader',
    },
    external: ['screeps'],
    context: 'global',
    plugins: [
        typescript({}),
        //replace({}),
        nodeResolve({
            browser: true,
        }),
        babel({
            babelHelpers: 'bundled',
            presets: [
                [
                    '@babel/preset-env',
                    {
                        modules: 'auto'
                    },
                ],
            ],
            extensions: ['.js', '.mjs', '.cjs', '.ts'],
            include: ['**/*'],
        }),
        {
            writeBundle(bundle) {
                if (bundle.file !== 'dist/bootloader.js') { return; }
                const str = fs.readFileSync(bundle.file, { encoding: 'utf8' });
                const arenaStr = `${str}\nexport const Bootloader = bootloader.Bootloader;\nexport const decodeWasm = bootloader.decodeWasm;\nexport const decompressWasm = bootloader.decompressWasm;\n`;
                fs.writeFileSync(bundle.file.replace(path.extname(bundle.file), '-arena.mjs'), arenaStr);
                const worldStr = `${str}\nmodule.exports = bootloader;\n`;
                fs.writeFileSync(bundle.file.replace(path.extname(bundle.file), '-world.js'), worldStr);
            }
        }
    ],
};
