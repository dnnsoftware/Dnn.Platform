import type { StorybookConfig } from 'storybook-react-rsbuild';
import { mergeRsbuildConfig } from '@rsbuild/core';
import { fileURLToPath } from "node:url";

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
                rsbuildConfigPath: fileURLToPath(new URL("./rsbuild.config.ts", import.meta.url)),
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
        });
    },
};
export default config;