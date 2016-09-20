module.exports = {
    entry: "./src/GridSystem.jsx",
    output: {
        path: "./lib/",
        filename: "GridSystem.js",
        libraryTarget: "umd",
        library: "GridSystem"
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
        "dnn-grid-cell": "dnn-grid-cell"
    },
    resolve: {
        extensions: ["", ".js", ".json", ".jsx"] 
    }
};