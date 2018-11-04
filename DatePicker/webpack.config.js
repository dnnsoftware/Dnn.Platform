const webpack = require("webpack");
const path = require("path");
const packageJson = require("./package.json");
const isProduction = process.env.NODE_ENV === "production";
const nodeExternals = require("webpack-node-externals");
module.exports = {
    entry: "./src/DatePicker.jsx",
    output: {
        path: __dirname + '/lib',
        filename: "DatePicker.js",
        libraryTarget: "umd",
        library: "DatePicker"
    },
    module: {
        rules: [
            { test: /\.(js|jsx)$/, enforce: "pre", exclude: /node_modules/, loader: "eslint-loader", options: { fix: true }},
            { test: /\.(js|jsx)$/, exclude: /node_modules/, loaders: ["babel-loader?presets[]=react"] },
            { test: /\.less$/, loader: "style-loader!css-loader!less-loader" },
            { test: /\.(gif|png)$/, loader: "url-loader?mimetype=image/png" },
            { test: /\.svg$/, exclude: /node_modules/, loader: "raw-loader" }
        ]
    },
    target: "node", // in order to ignore built-in modules like path, fs, etc.
    externals: ["react", nodeExternals()], // in order to ignore all modules in node_modules folder
    resolve: {
        extensions: [".js", ".json", ".jsx"],
        modules: [
            "node_modules",
            path.resolve(__dirname, "src")
        ]
    },
    plugins: isProduction ? [
        new webpack.optimize.UglifyJsPlugin(),
        new webpack.optimize.DedupePlugin(),
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
    ]
};