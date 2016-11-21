module.exports = {
    entry: "./src/Checkbox.jsx",
    output: {
        path: "./lib/",
        filename: "Checkbox.js",
        libraryTarget: "umd",
        library: "Checkbox"
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
        "dnn-label": "dnn-label"
    },
    resolve: {
        extensions: ["", ".js", ".json", ".jsx"] 
    }
};