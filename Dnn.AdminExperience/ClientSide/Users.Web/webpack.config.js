const webpack = require("webpack");
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
                "../../Dnn.PersonaBar.Extensions/admin/personaBar/Dnn.Users/scripts/bundles/"
            )
            : path.join(
                settings.WebsitePath,
                "DesktopModules\\Admin\\Dnn.PersonaBar\\Modules\\Dnn.Users\\scripts\\bundles\\"
            ),
            filename: "users-bundle.js",
            publicPath: isProduction ? "" : "http://localhost:8080/dist/",
        },
        devServer: {
            disableHostCheck: !isProduction,
        },
        module: {
            rules: [
                {
                    test: /\.(js|jsx)$/,
                    enforce: "pre",
                    exclude: [/node_modules/],
                    loader: "eslint-loader",
                    options: { fix: true },
                },
                {
                    test: /\.(js|jsx)$/,
                    exclude: [/node_modules/],
                    use: {
                        loader: "babel-loader",
                        options: {
                            presets: ["@babel/preset-env", "@babel/preset-react"],
                            plugins: [
                                "@babel/plugin-transform-react-jsx",
                                "@babel/plugin-proposal-object-rest-spread",
                            ],
                        },
                    },
                },
                {
                    test: /\.(less|css)$/,
                    use: [
                        { loader: "style-loader" },
                        { loader: "css-loader", options: { modules: "global" } },
                        { loader: "less-loader" },
                    ],
                },
                { test: /\.(ttf|woff)$/, loader: "url-loader?limit=8192" },
                { test: /\.(gif|png)$/, loader: "url-loader?mimetype=image/png" },
                {
                    test: /\.woff(2)?(\?v=[0-9].[0-9].[0-9])?$/,
                    loader: "url-loader?mimetype=application/font-woff",
                },
                {
                    test: /\.(ttf|eot|svg)(\?v=[0-9].[0-9].[0-9])?$/,
                    loader: "file-loader?name=[name].[ext]",
                },
            ],
        },
        resolve: {
            extensions: [".jsx", ".js", ".json"],
            modules: [
                path.resolve("./src"),
                path.resolve("./node_modules"), // Try local node_modules
                path.resolve("./src/_exportables/src"),
                path.resolve("./src/_exportables/node_modules"),
                path.resolve("../../../node_modules"), // Last fallback to workspaces node_modules
            ],
        },
        externals: Object.assign(webpackExternals, {
            "dnn-users-common-action-types": "window.dnn.Users.CommonActionTypes",
            "dnn-users-common-components": "window.dnn.Users.CommonComponents",
            "dnn-users-common-reducers": "window.dnn.Users.CommonReducers",
            "dnn-users-common-actions": "window.dnn.Users.CommonActions",
        }),
        plugins:
        [ isProduction
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
            filename: "users-bundle.js.map",
            append: "\n//# sourceMappingURL=/DesktopModules/Admin/Dnn.PersonaBar/Modules/Dnn.Users/scripts/bundles/users-bundle.js.map"
        })
        ],
        devtool: false,
    };
};
