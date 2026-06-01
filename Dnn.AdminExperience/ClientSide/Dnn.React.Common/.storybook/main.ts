import type { StorybookConfig } from 'storybook-react-rsbuild';
import { mergeRsbuildConfig } from '@rsbuild/core';
import {pluginLess} from "@rsbuild/plugin-less";
import {pluginSvgr} from "@rsbuild/plugin-svgr";
import {pluginReact} from "@rsbuild/plugin-react";

const config : StorybookConfig = {
    stories: [
        "../src/**/*.mdx",
        "../src/**/*.stories.@(js|jsx|mjs|ts|tsx)"
    ],
    addons: [
        "@storybook/addon-onboarding",
        "@storybook/addon-docs",
    ],
    framework: {
        name: 'storybook-react-rsbuild',
        options: {
            builder: {
                lazyCompilation: false,
            },
        },
    },
    rsbuildFinal: (config) => {
        return mergeRsbuildConfig(config, {
            tools: {
                rspack: {
                    output: {
                        globalObject: "self",
                    },
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
                        exportType: "default"
                    }
                }),
            ],
        });
    },
};
export default config;