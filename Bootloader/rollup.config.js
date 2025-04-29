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
                const arenaStr = [
                    `import * as utils from 'game/utils';`,
                    `import * as prototypes from 'game/prototypes';`,
                    `import * as constants from 'game/constants';`,
                    `import * as pathFinder from 'game/path-finder';`,
                    `import * as visual from 'game/visual';`,
                    `import { arenaInfo } from 'game';`,
                    `${str}`,
                    `export const Bootloader = bootloader.Bootloader;`,
                    `export const decodeWasm = bootloader.decodeWasm;`,
                    `export const decompressWasm = bootloader.decompressWasm;`,
                    ``,
                ].join('\n');
                fs.writeFileSync(bundle.file.replace(path.extname(bundle.file), '-arena.mjs'), arenaStr);
                const worldStr = [
                    `${str}`,
                    `module.exports = bootloader;`,
                    ``,
                ].join('\n');
                fs.writeFileSync(bundle.file.replace(path.extname(bundle.file), '-world.js'), worldStr);
            }
        }
    ],
};
