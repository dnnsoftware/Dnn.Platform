module.exports = {
    entry: "./src/TreeControlInteractor.jsx",
    output: {
        path: "./lib/",
        filename: "TreeControlInteractor.js",
        libraryTarget: "umd",
        library: "TreeControlInteractor"
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
            { test: /\.less$/, loader: "style-loader!css-loader!less-loader" },
            { test: /\.svg$/, loader: 'svg-url-loader'}
        ],
        preLoaders: [
            { test: /\.(js|jsx)$/, exclude: /node_modules/, loaders: ["babel-loader", "eslint-loader"] }
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