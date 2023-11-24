import {defineConfig } from 'rollup';
import copy from 'rollup-plugin-copy2';
import zip from 'rollup-plugin-zip';
import localSettings from '../../../settings.local.json' assert { type: 'json' };
import packageJson from './package.json' assert { type: 'json' };

const packageName = packageJson.name.charAt(0).toUpperCase() + packageJson.name.substr(1).toLowerCase();
const skinDist = localSettings.WebsitePath + '/Portals/_default/Skins/' + packageName;
const skinInstallDist = '../../../Website/Install/Skin/';

export default defineConfig({
    input: skinDist,
    output: {
        dir: skinInstallDist,
    },
    plugins: [
        copy({
            assets: ['**/*']
        }),
        zip({
            file: packageName + '\_' + packageJson.version + '\_install.zip'
        }),
    ],
    strictDeprecations: true,
});
