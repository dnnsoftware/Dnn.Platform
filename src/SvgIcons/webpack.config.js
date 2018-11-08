module.exports = {
    entry: "./src/SvgIcons.jsx",
    output: {
        path: __dirname + "/lib",
        filename: "SvgIcons.js",
        libraryTarget: "umd",
        library: "SvgIcons"
    },
    module: {
        rules: [
            { test: /\.(js|jsx)$/, enforce: "pre", exclude: /node_modules/, loader: "eslint-loader"},
            {
                test: /\.(svg)$/, exclude: /node_modules/,
                loader: "raw-loader"
            },
            { test: /\.(js|jsx)$/, exclude: /node_modules/, loaders: ["babel-loader?presets[]=react"] },
        ]
    },
    externals: {
        "react": "react"
    },
    resolve: {
        extensions: [".js", ".json", ".jsx"] 
    }
};