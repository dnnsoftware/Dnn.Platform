module.exports = {
    entry: "./src/ContentLoadWrapper.jsx",
    output: {
        path: "./lib/",
        filename: "ContentLoadWrapper.js",
        libraryTarget: "umd",
        library: "ContentLoadWrapper"
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
        "dnn-svg-icons": "dnn-svg-icons"
    },
    resolve: {
        extensions: ["", ".js", ".json", ".jsx"] 
    }
};