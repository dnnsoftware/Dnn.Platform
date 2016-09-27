module.exports = {
    entry: "./src/Pager.jsx",
    output: {
        path: "./lib/",
        filename: "Pager.js",
        libraryTarget: "umd",
        library: "Pager"
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
            { test: /\.(js|jsx)$/, exclude: /node_modules/, loader: "eslint-loader" }
        ]
    },
    externals: {
        "react": "react",
        "react-dom": "react-dom",
        "react-collapse": "react-collapse",
        "react-motion": "react-motion",
        "react-height": "react-height",
        "react-custom-scrollbars": "react-custom-scrollbars",
        "dnn-svg-icons": "dnn-svg-icons",
        "dnn-dropdown": "dnn-dropdown",
        "lodash": "lodash"
    },
    resolve: {
        extensions: ["", ".js", ".json", ".jsx"]
    }
};