module.exports = {
    entry: "./src/EditableField.jsx",
    output: {
        path: "./lib/",
        filename: "EditableField.js",
        libraryTarget: "umd",
        library: "EditableField"
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
        "react-dom": "react-dom",
        "dnn-svg-icons": "dnn-svg-icons",
        "dnn-single-line-input": "dnn-single-line-input",
        "dnn-multi-line-input": "dnn-multi-line-input"
    },
    resolve: {
        extensions: ["", ".js", ".json", ".jsx"] 
    }
};