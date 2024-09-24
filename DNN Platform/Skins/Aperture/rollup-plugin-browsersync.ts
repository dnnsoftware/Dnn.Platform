import bs from "browser-sync";
import { Plugin } from "rollup";
const browserSync = bs.create("rollup");

type RollupPluginBrowserSync = (options?: bs.Options) => Plugin;

const browsersync: RollupPluginBrowserSync = (browsersyncOptions) => {
    return {
        name: "browsersync",
        writeBundle: function(options) {
            if (!browserSync.active) {
                browserSync.init(browsersyncOptions || { server: "." });
            } else {
                browserSync.reload(options.file || "");
            }
        }
    }
};

export default browsersync;
