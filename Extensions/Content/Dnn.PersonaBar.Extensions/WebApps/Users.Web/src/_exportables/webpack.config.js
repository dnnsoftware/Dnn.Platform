const webpack = require("webpack");
const path = require("path");

const isProduction = process.env.NODE_ENV === "production";
module.exports = {
    entry: "./index",
    optimization: {
        minimize: isProduction
    },
    node: {
        fs: "empty"
    },
    output: {
        path: path.resolve("../../../../admin/personaBar/Dnn.Users/scripts/exportables/Users"),
        filename: "UsersCommon.js",
        publicPath: isProduction ? "" : "http://localhost:8050/dist/"
    },
    module: {
        rules: [
            { test: /\.(js|jsx)$/, enforce: "pre", exclude: /node_modules/, loader: "eslint-loader", options: { fix: true } },
            { test: /\.(js|jsx)$/, exclude: /node_modules/, loaders: "babel-loader", 
                options: { 
                    presets: ['@babel/preset-env', '@babel/preset-react'], 
                    "plugins": [
                        "@babel/plugin-transform-react-jsx",
                        "@babel/plugin-proposal-object-rest-spread"
                    ] 
                } 
            },
            { test: /\.(less|css)$/, loader: ["style-loader","css-loader","less-loader"] },
            { test: /\.(ttf|woff)$/, loader: "url-loader?limit=8192" },
            { test: /\.(gif|png)$/, loader: "url-loader?mimetype=image/png" },
            { test: /\.woff(2)?(\?v=[0-9].[0-9].[0-9])?$/, loader: "url-loader?mimetype=application/font-woff" },
            { test: /\.(ttf|eot|svg)(\?v=[0-9].[0-9].[0-9])?$/, loader: "file-loader?name=[name].[ext]" },
        ]
    },
    externals: require("@dnnsoftware/dnn-react-common/WebpackExternals"),
    resolve: {
        extensions: [".js", ".json", ".jsx"],
        modules: [
            path.resolve(__dirname, "./src"),
            path.resolve(__dirname, "../"),
            path.resolve(__dirname, "node_modules"),
            path.resolve(__dirname, "../../node_modules"),
            path.resolve(__dirname, "../../../../../../../node_modules")
        ]
    },
    devtool: "source-map"
};
