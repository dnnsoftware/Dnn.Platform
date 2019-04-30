const webpack = require("webpack");
const path = require("path");
const packageJson = require("./package.json");
const nodeExternals = require("webpack-node-externals");

module.exports = {
    mode: "production",
    optimization: {
        minimize: true,
    },
    entry: "./src/index.js",
    output: {
        path: path.resolve(__dirname, "dist"),
        filename: "dnn-react-common.min.js",
        library: "DnnReactCommon",
        libraryTarget: "umd",
    },
    module: {
        rules: [
            { test: /\.(js|jsx)$/, enforce: "pre", exclude: /node_modules/, loader: "eslint-loader", options: { fix: true } },
            { test: /\.(js|jsx)$/, exclude: /node_modules/, loaders: ["babel-loader"] },
            { test: /\.less$/, loader: "style-loader!css-loader!less-loader" },
            { test: /\.(ttf|woff)$/, loader: "url-loader?limit=8192" },
            { test: /\.css$/, loader: "style-loader!css-loader" },
            { test: /\.(gif|png)$/, loader: "url-loader?mimetype=image/png" },
            { test: /\.(svg)$/, loader: "raw-loader" },
            { test: /\.woff(2)?(\?v=[0-9].[0-9].[0-9])?$/, loader: "url-loader?mimetype=application/font-woff" },
            { test: /\.(ttf|eot)(\?v=[0-9].[0-9].[0-9])?$/, loader: "file-loader?name=[name].[ext]" }
        ]
    },
    target: "node", // in order to ignore built-in modules like path, fs, etc.
    externals: ["react", "prop-types", nodeExternals()], // in order to ignore all modules in node_modules folder
    resolve: {
        extensions: [".js", ".json", ".jsx"],
        modules: [
            path.resolve('./src'),           // Look in src first
            path.resolve('./node_modules'),  // Try local node_modules
            path.resolve('../node_modules')   // Last fallback to workspaces node_modules
        ]
    },
    plugins: [
        new webpack.DefinePlugin({
            VERSION: JSON.stringify(packageJson.version),
            "process.env": {
                "NODE_ENV": JSON.stringify("production")
            }
        })
    ] 
};