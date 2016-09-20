module.exports = {
    entry: "./src/SingleLineInputWithError.jsx",
    output: {
        path: "./lib/",
        filename: "SingleLineInputWithError.js",
        libraryTarget: "umd",
        library: "SingleLineInputWithError"
    },
    module: {
        loaders: [
            { 
                test: /\.(js|jsx)$/, exclude: /node_modules/,
                loader: "babel-loader",
                query: {
                    presets: ["react", "es2015"]
                } 
            },
            { test: /\.less$/, loader: "style-loader!css-loader!less-loader" }
        ],
        preLoaders: [
            { test: /\.(js|jsx)$/, exclude: /node_modules/, loader: "eslint-loader"}
        ]
    },
    externals: {
        "react": "react",
        "react-dom": "react-dom",
        "react-tooltip": "react-tooltip",
        "dnn-tooltip": "dnn-tooltip",
        "dnn-single-line-input": "dnn-single-line-input",
        "dnn-label": "dnn-label"
    },
    resolve: {
        extensions: ["", ".js", ".json", ".jsx"] 
    }
};