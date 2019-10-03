const webpack = require("webpack");
const I18nPlugin = require("i18n-webpack-plugin");
const packageJson = require("./package.json");
const path = require("path");
const isProduction = process.env.NODE_ENV === "production";
module.exports = {
    entry: "./src/main.jsx",
    optimization: {
        minimize: isProduction
    },
    output: {
        path: path.resolve("../../admin/personaBar/Dnn.Roles/scripts/bundles/"),
        filename: "roles-bundle.js",
        publicPath: isProduction ? "" : "http://localhost:8080/dist/"
    },
    devServer: {
        disableHostCheck: !isProduction
    },
    module: {
        rules: [
            { test: /\.(js|jsx)$/, enforce: "pre", exclude: /node_modules/, loader: "eslint-loader", options: { fix: true } },
            { test: /\.(js|jsx)$/ , exclude: /node_modules/, loader: "babel-loader" },
            { test: /\.(less|css)$/, use: [
                { loader: "style-loader" },
                { loader: "css-loader", options: { modules: "global" } },
                { loader: "less-loader" }
            ] },
            { test: /\.(ttf|woff)$/, loader: "url-loader?limit=8192" },
            { test: /\.(gif|png)$/, loader: "url-loader?mimetype=image/png" },
            { test: /\.woff(2)?(\?v=[0-9].[0-9].[0-9])?$/, loader: "url-loader?mimetype=application/font-woff" },
            { test: /\.(ttf|eot|svg)(\?v=[0-9].[0-9].[0-9])?$/, loader: "file-loader?name=[name].[ext]" },
        ]
    },
    resolve: {
        extensions: [".js", ".json", ".jsx"],
        modules: [
            path.resolve("./src"),          // Look in src first
            path.resolve("./node_modules"),  // Try local node_modules
            path.resolve("../../../../../node_modules")   // Last fallback to workspaces node_modules
        ]        
    },

    externals: require("@dnnsoftware/dnn-react-common/WebpackExternals"),

    plugins: isProduction ? [
        new webpack.DefinePlugin({
            VERSION: JSON.stringify(packageJson.version),
            "process.env": {
                "NODE_ENV": JSON.stringify("production")
            }
        })
    ] : [
        new webpack.DefinePlugin({
            VERSION: JSON.stringify(packageJson.version)
        })
    ],
    devtool: "source-map"
};