module.exports = {
    entry: "./src/MultiLineInputWithError.jsx",
    output: {
        path: "./lib/",
        filename: "MultiLineInputWithError.js",
        libraryTarget: "umd",
        library: "MultiLineInputWithError"
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
        "dnn-label": "dnn-label",
        "dnn-multi-line-input": "dnn-multi-line-input",
        "dnn-tooltip": "dnn-tooltip"
    },
    resolve: {
        extensions: ["", ".js", ".json", ".jsx"] 
    }
};