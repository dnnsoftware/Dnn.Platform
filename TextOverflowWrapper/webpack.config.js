module.exports = {
    entry: "./src/TextOverflowWrapper.jsx",
    output: {
        path: "./lib/",
        filename: "TextOverflowWrapper.js",
        libraryTarget: "umd",
        library: "TextOverflowWrapper"
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