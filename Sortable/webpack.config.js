module.exports = {
    entry: "./src/Sortable.jsx",
    output: {
        path: "./lib/",
        filename: "Sortable.js",
        libraryTarget: "umd",
        library: "Sortable"
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
        "react-modal": "react-modal",
        "react-custom-scrollbars": "react-custom-scrollbars",
        "dnn-svg-icons": "dnn-svg-icons"
    },
    resolve: {
        extensions: ["", ".js", ".json", ".jsx"] 
    }
};