module.exports = {
    entry: "./src/DropdownWithError.jsx",
    output: {
        path: "./lib/",
        filename: "DropdownWithError.js",
        libraryTarget: "umd",
        library: "DropdownWithError"
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
        "react-tooltip": "react-tooltip",
        "dnn-tooltip": "dnn-tooltip",
        "dnn-dropdown": "dnn-dropdown",
        "dnn-label": "dnn-label"
    },
    resolve: {
        extensions: ["", ".js", ".json", ".jsx"] 
    }
};