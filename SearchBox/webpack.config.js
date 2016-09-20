module.exports = {
    entry: "./src/SearchBox.jsx",
    output: {
        path: "./lib/",
        filename: "SearchBox.js",
        libraryTarget: "umd",
        library: "SearchBox"
    },
    module: {
        loaders: [
            { 
                test: /\.(js|jsx)$/, exclude: /node_modules/,
                loader: "babel-loader",
                query: {
                    presets: ["react", "es2015"],
                    plugins: [ 
                        "transform-object-assign",
                        "transform-object-rest-spread"
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
        "lodash": "lodash"
    },
    resolve: {
        extensions: ["", ".js", ".json", ".jsx"] 
    }
};