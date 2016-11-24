module.exports = {
    entry: "./src/PersonaBarPageHeader.jsx",
    output: {
        path: "./lib/",
        filename: "PersonaBarPageHeader.js",
        libraryTarget: "umd",
        library: "PersonaBarPageHeader"
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
        "dnn-text-overflow-wrapper": "dnn-text-overflow-wrapper"
    },
    resolve: {
        extensions: ["", ".js", ".json", ".jsx"] 
    }
};