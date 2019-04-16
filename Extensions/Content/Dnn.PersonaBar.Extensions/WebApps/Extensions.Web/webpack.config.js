const webpack = require("webpack");
const packageJson = require("./package.json");
const path = require("path");
const isProduction = process.env.NODE_ENV === "production";
const webpackExternals = require("@dnnsoftware/dnn-react-common/WebpackExternals");
module.exports = {
    entry: "./src/main.jsx",
    optimization: {
        minimize: isProduction
    },
    output: {
        path: path.resolve(__dirname, '../../admin/personaBar/Dnn.Extensions/scripts/bundles/'),
        publicPath: isProduction ? "" : "http://localhost:8080/dist/",
        filename: "extensions-bundle.js"
    },
    devServer: {
        disableHostCheck: !isProduction
    },
    resolve: {
        extensions: ["*", ".js", ".json", ".jsx"],
        modules: [
            path.resolve('./src'),           // Look in src first
            path.resolve('./node_modules'),  // Try local node_modules
            path.resolve('../node_modules')   // Last fallback to workspaces node_modules
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
                test: /\.css$/, 
                use: [{
                    loader: 'style-loader'
                }, {
                    loader: 'css-loader'
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
            },
            { 
                test: /\.(gif|png)$/, 
                use: {
                    loader: 'url-loader?mimetype=image/png'
                } 
            },
            { 
                test: /\.woff(2)?(\?v=[0-9].[0-9].[0-9])?$/, 
                use: {
                    loader: 'url-loader?mimetype=application/font-woff'
                } 
            },
            { 
                test: /\.(ttf|eot|svg)(\?v=[0-9].[0-9].[0-9])?$/, 
                use: {
                    loader: 'file-loader?name=[name].[ext]' 
                }
            }
        ]
    },
    externals: webpackExternals,

    plugins: isProduction ? [
        new webpack.DefinePlugin({
            VERSION: JSON.stringify(packageJson.version),
            "process.env": {
                "NODE_ENV": JSON.stringify("production")
            }
        })
    ] : [
            new webpack.DefinePlugin({
                VERSION: JSON.stringify(packageJson.version)
            })
        ]
};