const path = require("path");
module.exports = {
    entry: "./index",
    optimization: {
        minimize: true
    },
    output: {
        path: path.resolve("../../../admin/personaBar/scripts/exportables/Sites"),
        filename: "SitesListView.js",
        publicPath: "http://localhost:8050/dist/"
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
            { test: /\.(less|css)$/, loader: "style-loader!css-loader!less-loader" }
        ]
    },
    externals: require("@dnnsoftware/dnn-react-common/WebpackExternals"),
    resolve: {
        extensions: [".js", ".json", ".jsx"],
        modules: [
            path.resolve('./src'),
            path.resolve('./src'),
            path.resolve('./node_modules'),  // Last fallback to node_modules
            path.resolve('../') // Look in src first
        ]
    }
};
