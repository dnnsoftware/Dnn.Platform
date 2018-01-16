const path = require("path");

module.exports = {
    entry: "./src/Flag.tsx",
    output: {
        path: path.resolve(__dirname, "lib"),
        filename: "Flag.js",
        libraryTarget: "umd",
        library: "Flag"
    },
    module: {
            rules: [
                { test: /\.(js|ts|tsx)$/, exclude: /node_modules/, loaders: [ "awesome-typescript-loader"] },
                { test: /\.png$/, loader: "url-loader?limit=8192" },
                { test: /\.(ts|tsx)$/, enforce: "pre", exclude: /node_modules/, loader: "tslint-loader" }
            ]
    },
    devtool: "inline-source-map",
    resolve: {
            extensions: [".tsx"]
        },
    externals: {
        "react": "react"
    }
};