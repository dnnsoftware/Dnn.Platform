module.exports = {
    entry: "./src/TextOverflowWrapperNew.jsx",
    output: {
        path: "./lib/",
        filename: "TextOverflowWrapperNew.js",
        libraryTarget: "umd",
        library: "TextOverflowWrapperNew"
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
        "react-tooltip": "react-tooltip"
    },
    resolve: {
        extensions: ["", ".js", ".json", ".jsx"]
    }
};