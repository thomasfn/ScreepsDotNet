import * as fs from 'fs';

import typescript from '@rollup/plugin-typescript';
import { nodeResolve } from '@rollup/plugin-node-resolve';
import replace from '@rollup/plugin-replace';
import { prepend } from 'rollup-plugin-insert'
import { babel } from '@rollup/plugin-babel';
import nodePolyfills from 'rollup-plugin-polyfill-node';
import path from 'path';

const dotnetjsPre = fs.readFileSync('src/dotnet.pre.js', { encoding: 'utf8' });

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
        typescript(),
        replace({
            'import(': '_import(',
            'document.': '_document.',

            // TextDecoder seems to have some serious performance issues but dotnet.js bundles it's own fallback logic if it's missing
            // Therefore let's use replacements to null it out in specific areas
            'var UTF8Decoder=typeof TextDecoder!="undefined"?new TextDecoder("utf8"):undefined;': 'var UTF8Decoder=undefined;',
            'this.mono_text_decoder="undefined"!==typeof TextDecoder?new TextDecoder("utf-16le"):null': 'this.mono_text_decoder=null',
        }),
        prepend(dotnetjsPre, { include: 'src/dotnet.js' }),
        nodeResolve(),
        nodePolyfills({
            include: [],
        }),
        replace({
            'setTimeoutFunc = setTimeout': 'setTimeoutFunc = global.setTimeout',
            'y.allocUnsafe': 'y && y.allocUnsafe',
            'WebAssembly.instantiate': '_WebAssemblyInstantiate',
        }),
        babel({
            babelHelpers: 'bundled',
            extensions: ['.js', '.mjs', '.cjs', '.ts'],
            include: ['**/*']
        }),
        replace({
            'console.warn': 'console.log',
        }),
        {
            writeBundle(bundle) {
                if (bundle.file !== 'dist/bootloader.js') { return; }
                const str = fs.readFileSync(bundle.file, { encoding: 'utf8' });
                const arenaStr = `${str}\nexport const DotNet = bootloader.DotNet;\n`;
                fs.writeFileSync(bundle.file.replace(path.extname(bundle.file), '-arena.mjs'), arenaStr);
                const worldStr = `${str}\nmodule.exports = bootloader;\n`;
                fs.writeFileSync(bundle.file.replace(path.extname(bundle.file), '-world.js'), worldStr);
            }
        }
    ],
};
