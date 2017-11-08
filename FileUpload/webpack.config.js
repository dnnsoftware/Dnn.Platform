module.exports = {
    entry: "./src/FileUpload.jsx",
    output: {
        path: "./lib/",
        filename: "FileUpload.js",
        libraryTarget: "umd",
        library: "FileUpload"
    },
    module: {
        loaders: [
            {
                test: /\.(svg|png)$/, exclude: /node_modules/,
                loader: "raw-loader"
            },
            { test: /\.less$/, loader: "style-loader!css-loader!less-loader" },
            { 
                test: /\.(js|jsx)$/, exclude: /node_modules/,
                loader: "babel-loader",
                query: {
                    presets: ["react", "es2015"]
                } 
            }
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