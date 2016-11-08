module.exports = {
    entry: "./src/Droppable.jsx",
    output: {
        path: "./lib/",
        filename: "Droppable.js",
        libraryTarget: "umd",
        library: "Droppable"
    },
    module: {
        loaders: [
            { 
                test: /\.(js|jsx)$/, exclude: /node_modules/,
                loader: "babel-loader",
                query: {
                    presets: ["react", "es2015"]
                } 
            }
        ],
        preLoaders: [
            { test: /\.(js|jsx)$/, exclude: /node_modules/, loader: "eslint-loader"}
        ]
    },
    externals: {
        "react": "react",
        "react-dom": "react-dom"
    },
    resolve: {
        extensions: ["", ".js", ".json", ".jsx"] 
    }
};