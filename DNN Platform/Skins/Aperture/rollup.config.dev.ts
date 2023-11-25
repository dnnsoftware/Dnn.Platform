import { Plugin, defineConfig } from 'rollup';
import prodConfig from './rollup.config.prod';

export default defineConfig({
    ...prodConfig,
    plugins: [
        ...(prodConfig.plugins as Plugin[])
            .filter((plugin: Plugin) => plugin.name !== 'rollup-plugin-dnn-package'),
        ,
    ],
});