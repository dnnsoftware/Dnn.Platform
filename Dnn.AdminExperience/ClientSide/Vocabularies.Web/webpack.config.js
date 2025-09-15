const webpack = require("webpack");
const ESLintPlugin = require("eslint-webpack-plugin");
const path = require("path");
const packageJson = require("./package.json");
const moduleName = "vocabulary";
const settings = require("../../../settings.local.json");

module.exports = (env, argv) => {
    const isProduction = argv.mode === "production";
    return {
        entry: "./src/main.jsx",
        optimization: {
            minimize: isProduction,
        },
        output: {
            path:
        isProduction || settings.WebsitePath === ""
            ? path.resolve(
                "../../Dnn.PersonaBar.Extensions/admin/personaBar/Dnn.Vocabularies/scripts/bundles/"
            )
            : path.join(
                settings.WebsitePath,
                "DesktopModules\\Admin\\Dnn.PersonaBar\\Modules\\Dnn.Vocabularies\\scripts\\bundles\\"
            ),
            publicPath: isProduction ? "" : "http://localhost:8080/dist/",
            filename: moduleName + "-bundle.js",
        },
        devServer: {
            disableHostCheck: !isProduction,
        },
        resolve: {
            extensions: ["*", ".js", ".json", ".jsx"],
            modules: [
                path.resolve("./src"), // Look in src first
                path.resolve("./node_modules"), // Try local node_modules
                path.resolve("../../../node_modules"), // Last fallback to workspaces node_modules
            ],
        },

        module: {
            rules: [
                {
                    test: /\.less$/,
                    use: [
                        {
                            loader: "style-loader", // creates style nodes from JS strings
                        },
                        {
                            loader: "css-loader",
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
                        {
                            loader: "less-loader", // compiles Less to CSS
                        },
                    ],
                },
                {
                    test: /\.(js|jsx)$/,
                    exclude: /node_modules/,
                    use: {
                        loader: "babel-loader",
                        options: {
                            presets: ["@babel/preset-env", "@babel/preset-react"],
                        },
                    },
                },
                {
                    test: /\.(ttf|woff)$/,
                    use: {
                        loader: "url-loader?limit=8192",
                    },
                },
                {
                    test: /\.svg$/,
                    resourceQuery: /raw/,
                    use: "raw-loader",
                },
                {
                    test: /\.json$/,
                    type: "javascript/auto",
                    loader: "json-loader",
                },
                {
                    test: /\.html$/,
                    use: [
                        {
                            loader: "html-loader",
                            options: {
                                esModule: false,
                            },
                        },
                    ],
                }
            ],
        },

        externals: require("@dnnsoftware/dnn-react-common/WebpackExternals"),

        plugins: 
            [
                isProduction
                    ? new webpack.DefinePlugin({
                        VERSION: JSON.stringify(packageJson.version),
                        "process.env": {
                            NODE_ENV: JSON.stringify("production"),
                        },
                    })
                    : new webpack.DefinePlugin({
                        VERSION: JSON.stringify(packageJson.version),
                    }),
                new webpack.SourceMapDevToolPlugin({
                    filename: "vocabulary-bundle.js.map",
                    append: "\n//# sourceMappingURL=/DesktopModules/Admin/Dnn.PersonaBar/Modules/Dnn.Vocabularies/scripts/bundles/vocabulary-bundle.js.map",
                }),
                new ESLintPlugin({fix: true}),
            ],
        devtool: false,
    };
};
