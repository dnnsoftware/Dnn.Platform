const webpack = require("webpack");
const ESLintPlugin = require("eslint-webpack-plugin");
const packageJson = require("./package.json");
const path = require("path");
const webpackExternals = require("@dnnsoftware/dnn-react-common/WebpackExternals");
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
                "../../Dnn.PersonaBar.Extensions/admin/personaBar/Dnn.Servers/scripts/bundles/"
            )
            : path.join(
                settings.WebsitePath,
                "DesktopModules\\Admin\\Dnn.PersonaBar\\Modules\\Dnn.Servers\\scripts\\bundles\\"
            ),
            publicPath: isProduction ? "" : "http://localhost:8080/dist/",
            filename: "servers-bundle.js",
        },
        devServer: {
            disableHostCheck: !isProduction,
        },
        resolve: {
            extensions: ["*", ".js", ".json", ".jsx", ".ts", ".tsx"],
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
                    test: /\.(js|jsx|ts|tsx)$/,
                    exclude: /node_modules/,
                    use: {
                        loader: "babel-loader",
                        options: {
                            presets: ["@babel/preset-env", "@babel/preset-react", "@babel/preset-typescript"],
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
                    test: /\.(d.ts)$/,
                    use: ["null-loader"],
                },
                {
                    test: /\.svg$/i,
                    issuer: /\.[jt]sx?$/,
                    use: ["@svgr/webpack"],
                },
            ],
        },
        externals: webpackExternals,
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
                    filename: "servers-bundle.js.map",
                    append: "\n//# sourceMappingURL=/DesktopModules/Admin/Dnn.PersonaBar/Modules/Dnn.Servers/scripts/bundles/servers-bundle.js.map"
                }),
                new ESLintPlugin({fix: true}),
            ],
        devtool: false,
    };
};
