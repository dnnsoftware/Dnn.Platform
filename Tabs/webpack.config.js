module.exports = {
    entry: "./src/DnnTabs.jsx",
    output: {
        path: "./lib/",
        filename: "DnnTabs.js",
        libraryTarget: "umd",
        library: "DnnTabs"
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
        "react-tabs": "react-tabs"
    },
    resolve: {
        extensions: ["", ".js", ".json", ".jsx"] 
    }
};