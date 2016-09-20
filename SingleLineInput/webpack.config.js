module.exports = {
    entry: "./src/SingleLineInput.jsx",
    output: {
        path: "./lib/",
        filename: "SingleLineInput.js",
        libraryTarget: "umd",
        library: "SingleLineInput"
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
        "react": "react"
    },
    resolve: {
        extensions: ["", ".js", ".json", ".jsx"] 
    }
};