const webpack = require("webpack");
const packageJson = require("./package.json");
const isProduction = process.env.NODE_ENV === "production";
const isAnalyze = process.env.NODE_ENV === "analyze";
const path = require("path");
const extractTextPlugin = require("extract-text-webpack-plugin");
const optimizeCssAssetsPlugin = require('optimize-css-assets-webpack-plugin');
const BundleAnalyzerPlugin = require('webpack-bundle-analyzer').BundleAnalyzerPlugin;

module.exports = {
    context: path.resolve(__dirname, '.'),
    entry: "./src/main.jsx",
    optimization: {
        minimize: isProduction
    },
    output: {
        path: path.resolve(__dirname, '../admin/personaBar/scripts/bundles/'),
        publicPath: isProduction ? "" : "http://localhost:8100/dist/",
        filename: 'prompt-bundle.js'
    },
    devtool: '#source-map',
    resolve: {
        extensions: ["*", ".js", ".json", ".jsx"],
        modules: [
            path.resolve('./src'),           // Look in src first
            path.resolve('./node_modules')   // Last fallback to node_modules
        ]
    },
    module: {
        rules: [
            {
                test: /\.(js|jsx)$/,
                exclude: /node_modules/,
                enforce: "pre",
                use: [
                    'eslint-loader'
                ]
            },
            {
                test: /\.less$/,
                use: [{
                    loader: 'style-loader'  // creates style nodes from JS strings
                  }, {
                    loader: 'css-loader'    // translates CSS into CommonJS
                  }, {
                    loader: 'less-loader'   // compiles Less to CSS
                  }]
            },
            { 
                test: /\.(js|jsx)$/,
                exclude: /node_modules/,
                use: { 
                    loader: 'babel-loader',
                    options: {
                        presets: ['@babel/preset-env','@babel/preset-react']
                    }
                }
            },
            { 
                test: /\.(ttf|woff)$/,
                use: {
                    loader: 'url-loader?limit=8192'
                }
            }
        ]
    },
    externals: require("dnn-webpack-externals"),
    plugins:
    isProduction
        ?
        [
            new webpack.DefinePlugin({
                VERSION: JSON.stringify(packageJson.version),
                "process.env": {
                    "NODE_ENV": JSON.stringify("production")
                }
            })
        ]
        :
        [
            new webpack.DefinePlugin({
                VERSION: JSON.stringify(packageJson.version)
            })
        ]
};

if(isAnalyze) {
    module.exports.plugins.push(new BundleAnalyzerPlugin());
}