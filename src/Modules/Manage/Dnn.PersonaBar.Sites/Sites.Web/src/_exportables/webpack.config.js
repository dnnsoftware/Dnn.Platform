const path = require("path");
module.exports = {
    entry: "./index",
    output: {
        path: "../../../admin/personaBar/scripts/exportables/Sites",
        filename: "SitesListView.js",
        publicPath: "http://localhost:8050/dist/"
    },
    module: {
        loaders: [{
            test: /\.(js|jsx)$/,
            exclude: /node_modules/,
            loader: "babel-loader",
            query: {
                presets: ["react", "es2015"]
            }
        }, {
            test: /\.less$/,
            loader: "style-loader!css-loader!less-loader"
        }],
        preLoaders: [{
            test: /\.(js|jsx)$/,
            exclude: /node_modules/,
            loader: "eslint-loader"
        }]
    },
    externals: require("dnn-webpack-externals"),
    resolve: {
        extensions: ["", ".js", ".json", ".jsx"],
        root: [
            path.resolve('./src'),
            path.resolve('../') // Look in src first
        ]
    }
};
