module.exports = {
    entry: "./src/BackToLink.jsx",
    output: {
        path: "./lib/",
        filename: "BackToLink.js",
        libraryTarget: "umd",
        library: "BackToLink"
    },
    module: {
        loaders: [
            { 
                test: /\.(js|jsx)$/, exclude: /node_modules/,
                loader: "babel-loader",
                query: {
                    presets: ["react", "es2015"],
                    plugins: [ 
                        "transform-object-assign"
                    ]
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
        "dnn-svg-icons": "dnn-svg-icons"
    },
    resolve: {
        extensions: ["", ".js", ".json", ".jsx"] 
    }
};
