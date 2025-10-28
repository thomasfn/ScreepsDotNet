import * as fs from 'fs';

import typescript from '@rollup/plugin-typescript';
import { nodeResolve } from '@rollup/plugin-node-resolve';
import alias from '@rollup/plugin-alias';
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
        alias({
            entries: [
                { find: './noop-world.js', replacement: './world.js' },
            ],
        }),
        typescript({}),
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
