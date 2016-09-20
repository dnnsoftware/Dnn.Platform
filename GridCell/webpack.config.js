module.exports = {
    entry: "./src/GridCell.jsx",
    output: {
        path: "./lib/",
        filename: "GridCell.js",
        libraryTarget: "umd",
        library: "GridCell"
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