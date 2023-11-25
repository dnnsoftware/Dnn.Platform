import { Plugin, defineConfig } from 'rollup';
import prodConfig from './rollup.config.prod';
import browserSync from './rollup-plugin-browsersync';
import { glob } from 'glob';
import localSettings from '../../../settings.local.json' assert { type: 'json' };
import packageJson from './package.json' assert { type: 'json' };

const packageName = packageJson.name.charAt(0).toUpperCase() + packageJson.name.substr(1).toLowerCase();
const containersDist = localSettings.WebsitePath + '/Portals/_default/Containers/' + packageName;
const skinDist = localSettings.WebsitePath + '/Portals/_default/Skins/' + packageName;

export default defineConfig({
    ...prodConfig,
    plugins: [
        ...(prodConfig.plugins as Plugin[])
            .filter((plugin: Plugin) => plugin.name !== 'rollup-plugin-dnn-package'),
        ,
        {
            name: "watch-other-files",
            async buildStart() {
                const files = await glob(["**/*"]);
                for(let file of files){
                    this.addWatchFile(file);
                }
            }
        },
        browserSync({
            proxy: localSettings.WebsiteUrl,
            files: [
                skinDist + '/**/*',
                containersDist + '/**/*',
            ],
        })
    ],
});