module.exports = {
    entry: "./src/Modal.jsx",
    output: {
        path: "./lib/",
        filename: "Modal.js",
        libraryTarget: "umd",
        library: "Modal"
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
        "react-modal": "react-modal",
        "react-custom-scrollbars": "react-custom-scrollbars",
        "dnn-svg-icons": "dnn-svg-icons"
    },
    resolve: {
        extensions: ["", ".js", ".json", ".jsx"] 
    }
};