module.exports = {
    entry: "./src/CollapsibleRow.jsx",
    output: {
        path: "./lib/",
        filename: "CollapsibleRow.js",
        libraryTarget: "umd",
        library: "CollapsibleRow"
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
        "react-collapse": "react-collapse",
        "react-motion": "react-motion",
        "react-height": "react-height",
        "dnn-button": "dnn-button"
    },
    resolve: {
        extensions: ["", ".js", ".json", ".jsx"] 
    }
};