module.exports = {
    entry: "./src/Label.jsx",
    output: {
        path: "./lib/",
        filename: "Label.js",
        libraryTarget: "umd",
        library: "Label"
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
        "react-tooltip": "react-tooltip",
        "dnn-tooltip": "dnn-tooltip"
    },
    resolve: {
        extensions: ["", ".js", ".json", ".jsx"] 
    }
};