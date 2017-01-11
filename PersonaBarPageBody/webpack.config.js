module.exports = {
    entry: "./src/PersonaBarPageBody.jsx",
    output: {
        path: "./lib/",
        filename: "PersonaBarPageBody.js",
        libraryTarget: "umd",
        library: "PersonaBarPageBody"
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
        "dnn-grid-cell": "dnn-grid-cell",
        "dnn-back-to-link": "dnn-back-to-link"
    },
    resolve: {
        extensions: ["", ".js", ".json", ".jsx"] 
    }
};