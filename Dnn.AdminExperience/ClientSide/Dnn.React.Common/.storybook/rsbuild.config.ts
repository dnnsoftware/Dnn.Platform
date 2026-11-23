import { defineConfig } from "@rsbuild/core";
import { pluginReact } from "@rsbuild/plugin-react";
import { pluginLess } from "@rsbuild/plugin-less";
import { pluginSvgr } from "@rsbuild/plugin-svgr";
import { createRequire } from "node:module";

const requireModule = createRequire(__filename);
const packageJson = requireModule("../package.json");

// Storybook runs standalone: do not inherit Persona Bar externals, forced
// dependency resolution, or the library's UMD output configuration.
export default defineConfig({
    source: {
        define: {
            VERSION: JSON.stringify(packageJson.version),
        },
    },
    output: {
        injectStyles: true,
        cssModules: {
            auto: true,
            mode: "global",
            localIdentName: "[name]__[local]___[hash:base64:5]",
        },
    },
    plugins: [
        pluginReact({
            swcReactOptions: {
                runtime: "classic",
            },
        }),
        pluginLess({
            lessLoaderOptions: {
                lessOptions: {
                    javascriptEnabled: true,
                },
            },
        }),
        pluginSvgr({
            svgrOptions: {
                exportType: "default",
            },
        }),
    ],
});