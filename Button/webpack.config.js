module.exports = {
    entry: "./src/Button.jsx",
    output: {
        path: "./lib/",
        filename: "Button.js",
        libraryTarget: "umd",
        library: "Button"
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
        "react": "react"
    },
    resolve: {
        extensions: ["", ".js", ".json", ".jsx"] 
    }
};
