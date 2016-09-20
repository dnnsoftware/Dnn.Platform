module.exports = {
    entry: "./src/Tooltip.jsx",
    output: {
        path: "./lib/",
        filename: "Tooltip.js",
        libraryTarget: "umd",
        library: "Tooltip"
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
        "lodash": "lodash",
        "react-dom": "react-dom",
        "react-tooltip": "react-tooltip"
    },
    resolve: {
        extensions: ["", ".js", ".json", ".jsx"] 
    }
};