import { Plugin, defineConfig } from 'rollup';
import prodConfig from './rollup.config.prod';
import chokidarWatch from './rollup-plugin-chokidar';
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
            chokidarWatch({
                paths: await glob(["**/*.*"]),
                onChange: () => {
                    console.log('Rebuilding due to file change...');
                }
            }),
            browserSync({
                proxy: localSettings.WebsiteUrl,
                rewriteRules: [
                    {
                        match: /w\[".*"].*/g,
                        fn: (req, _res, match: string) => {
                            return match.replace(/(http:\/\/|https:\/\/)[a-zA-Z0-9.-]+\//g, `//${req.headers.host}/`);
                        }
                    },
                ],
                files: [
                    skinDist + '/**/*',
                    containersDist + '/**/*',
                ],
            }),
    
    ],
});