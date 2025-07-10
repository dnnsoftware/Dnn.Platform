import { join, dirname } from "path";

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
        getAbsolutePath("@storybook/addon-docs")
    ],
    framework: {
        name: getAbsolutePath("@storybook/react-webpack5"),
        options: {}
    },
    webpackFinal: async (config) => {
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