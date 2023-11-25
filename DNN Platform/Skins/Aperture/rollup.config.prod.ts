import {defineConfig } from 'rollup';
import typescript from '@rollup/plugin-typescript';
import cleaner from 'rollup-plugin-cleaner';
import copy from 'rollup-plugin-copy';
import scss from 'rollup-plugin-scss';
import sourcemaps from 'rollup-plugin-sourcemaps';
import { terser } from 'rollup-plugin-terser';
import localSettings from '../../../settings.local.json' assert { type: 'json' };
import packageJson from './package.json' assert { type: 'json' };
import dnnPackage from './rollup-plugin-package';

const packageName = packageJson.name.charAt(0).toUpperCase() + packageJson.name.substr(1).toLowerCase();
const containersDist = localSettings.WebsitePath + '/Portals/_default/Containers/' + packageName;
const skinDist = localSettings.WebsitePath + '/Portals/_default/Skins/' + packageName;

export default defineConfig({
    input: 'src/scripts/main.ts',
    output: {
        dir: skinDist,
        format: 'iife',
        sourcemap: true,
        assetFileNames: (assetInfo) => {
            var assetFileName = assetInfo?.name;
            if (!assetFileName) {
                return '';
            }
            if (assetFileName?.endsWith('.css')) {
                return 'css/skin.min.css';
            }
            if (assetFileName?.endsWith('.css.map')) {
                return 'css/skin.min.css.map';
            }
            return assetInfo?.name as string;
        },
        entryFileNames: () => {
            return `js/skin.min.js`;
        },
    },
    plugins: [
        typescript({
            sourceMap: true,
            inlineSources: true,
        }),
        sourcemaps(),
        terser(),
        scss({
            fileName: 'css/skin.min.css',
            sourceMap: true,
            outputStyle: 'compressed',
            failOnError: true,
        }),
        cleaner({
            targets: [
                skinDist,
                containersDist,
            ],
        }),
        copy({
            targets: [
                { src: 'containers/**/*', dest: containersDist },
                { src: [
                    '*.ascx',
                    '*.xml',
                    '*.png',
                    '*.dnn',
                    'LICENSE',
                    '*.txt',
                    './partials/*',
                    './menus/**/*',
                    'src/fonts/*',
                    'src/images/*.{png,jpg,gif}',
                ], dest: skinDist }
            ],
            flatten: false,
            verbose: true,
        }),
        dnnPackage({
            name: packageName,
            version: packageJson.version,
            destinationDirectory: '../../../Website/Install/Skin',
        }),
    ],
    strictDeprecations: true,
});
