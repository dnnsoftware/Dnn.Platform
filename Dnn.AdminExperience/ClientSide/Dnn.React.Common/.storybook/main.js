// This file has been automatically migrated to valid ESM format by Storybook.
import { join, dirname } from "path";
import { createRequire } from "module";

const require = createRequire(import.meta.url);

/**
* This function is used to resolve the absolute path of a package.
* It is needed in projects that use Yarn PnP or are set up within a monorepo.
*/
function getAbsolutePath(value) {
    return dirname(require.resolve(join(value, "package.json")));
}

/** @type { import('@storybook/react-webpack5').StorybookConfig } */
const config = {
    stories: [
        "../src/**/*.mdx",
        "../src/**/*.stories.@(js|jsx|mjs|ts|tsx)"
    ],
    addons: [
        getAbsolutePath("@storybook/addon-webpack5-compiler-swc"),
        getAbsolutePath("@storybook/addon-onboarding"),
        getAbsolutePath("@storybook/addon-docs"),
    ],
    framework: {
        name: getAbsolutePath("@storybook/react-webpack5"),
        options: {}
    },
    webpackFinal: async (config) => {
        // Helper to check if a rule matches SVG files
        const matchesSvg = (rule) => rule.test instanceof RegExp && rule.test.test('.svg');

        // Exclude SVG from the default asset/resource rule (top-level and inside oneOf blocks)
        config.module.rules.forEach((rule) => {
            if (matchesSvg(rule)) {
                rule.exclude = /\.svg$/i;
            }
            if (rule.oneOf) {
                rule.oneOf.forEach((subRule) => {
                    if (matchesSvg(subRule)) {
                        subRule.exclude = /\.svg$/i;
                    }
                });
            }
        });

        // Add @svgr/webpack to handle SVG imports as React components
        config.module.rules.push({
            test: /\.svg$/i,
            issuer: /\.[jt]sx?$/,
            use: ["@svgr/webpack"],
        });

        config.module.rules.push({
            test: /\.less$/,
            use: [
                require.resolve("style-loader"),
                {
                    loader: require.resolve("css-loader"),
                    options: {
                        importLoaders: 1,
                        sourceMap: true,
                        modules: {
                            auto: true,
                            mode: "global",
                            localIdentName: "[name]__[local]___[hash:base64:5]",
                        },
                        esModule: false,
                    },
                },
                require.resolve("less-loader"),
            ],
        });
        return config;
    },
};
export default config;