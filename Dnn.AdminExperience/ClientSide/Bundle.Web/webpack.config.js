/* eslint-disable no-undef */
const webpack = require("webpack");
const ESLintPlugin = require("eslint-webpack-plugin");
const path = require("path");
const packageJson = require("./package.json");
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
                "../../Library/Dnn.PersonaBar.UI/admin/personaBar/scripts/exports/"
            )
            : path.join(
                settings.WebsitePath,
                "DesktopModules\\Admin\\Dnn.PersonaBar\\scripts\\exports\\"
            ),
            filename: "export-bundle.js",
            publicPath: isProduction ? "" : "http://localhost:8070/dist/",
        },
        devServer: {
            disableHostCheck: !isProduction,
        },
        module: {
            rules: [
                {
                    test: /\.(js|jsx)$/,
                    exclude: /node_modules/,
                    use: ["babel-loader"],
                },
                {
                    test: /\.(less|css)$/,
                    use:
                    [
                        "style-loader",
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
                        "less-loader"
                    ],
                },
                {
                    test: /\.woff(2)?(\?v=[0-9].[0-9].[0-9])?$/,
                    use: ["url-loader?mimetype=application/font-woff"],
                },
                {
                    test: /\.(ttf|eot|svg)(\?v=[0-9].[0-9].[0-9])?$/,
                    use: ["file-loader?name=[name].[ext]"],
                },
                {
                    test: /\.(gif|png)$/,
                    use: ["url-loader?mimetype=image/png"],
                },
            ],
        },
        resolve: {
            extensions: [".js", ".json", ".jsx"],
            modules: [
                path.resolve("./node_modules"), // Try local node_modules
                path.resolve("../../../node_modules"),
                path.resolve(__dirname, "src"),
            ],
        },
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
                    filename: "export-bundle.js.map",
                    append: "\n//# sourceMappingURL=export-bundle.js.map"
                }),
                new ESLintPlugin({fix: true}),
            ],
        devtool: false,
    };
};
