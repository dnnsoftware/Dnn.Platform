const webpack = require("webpack");

module.exports = {
    entry: "./src/DatePicker.jsx",
    output: {
        path: "./lib/",
        filename: "DatePicker.js",
        libraryTarget: "umd",
        library: "DatePicker"
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
            { test: /\.(ttf|woff)$/, loader: "url-loader?limit=8192" },
            { test: /\.(eot|svg|jpg|png)$/, loader: "url-loader?limit=8192" }
        ],
        preLoaders: [
            { test: /\.(js|jsx)$/, exclude: /node_modules/, loader: "eslint-loader"}
        ]
    },
    externals: {
        "react": "react",
        "react-dom": "react-dom"
    },
    resolve: {
        extensions: ["", ".js", ".json", ".jsx"] 
    },

    plugins: [new webpack.ContextReplacementPlugin(/moment[\/\\]locale$/, /en/)]
};