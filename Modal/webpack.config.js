module.exports = {
    entry: "./src/SocialPanelHeader.jsx",
    output: {
        path: "./lib/",
        filename: "SocialPanelHeader.js",
        libraryTarget: "umd",
        library: "SocialPanelHeader"
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
        "react-modal": "react-modal"
    },
    resolve: {
        extensions: ["", ".js", ".json", ".jsx"] 
    }
};