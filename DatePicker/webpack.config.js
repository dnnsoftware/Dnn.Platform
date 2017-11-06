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