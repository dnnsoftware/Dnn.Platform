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
        path: path.resolve(__dirname, "../../admin/personaBar/Dnn.SiteSettings/scripts/bundles/"),
        publicPath: isProduction ? "" : "http://localhost:8085/dist/",
        filename: "site-settings-bundle.js"
    },
    devServer: {
        disableHostCheck: !isProduction
    },
    resolve: {
        extensions: ["*", ".js", ".json", ".jsx"],
        modules: [
            path.resolve("./src"),           // Look in src first
            path.resolve("./node_modules"),  // Try local node_modules
            path.resolve("../../../../../node_modules")   // Last fallback to workspaces node_modules
        ]
    },
    module: {
        rules: [
            { 
                test: /\.(js|jsx)$/, 
                exclude: /node_modules/, 
                enforce: "pre",
                loader: "eslint-loader",
                options: {
                    fix: true
                }                
            },
            { 
                test: /\.js$/, 
                enforce: "pre",
                use: [
                    "source-map-loader"
                ] 
            },
            { 
                test: /\.less$/, 
                use: [{
                    loader: "style-loader"  // creates style nodes from JS strings
                }, {
                    loader: "css-loader",    // translates CSS into CommonJS
                    options: { modules: "global" }
                }, {
                    loader: "less-loader"   // compiles Less to CSS
                }]
            },
            { 
                test: /\.(js|jsx)$/, 
                exclude: /node_modules/,
                use: {
                    loader:"babel-loader",
                    options: { 
                        presets: ["@babel/preset-env", "@babel/preset-react"], 
                        "plugins": [
                            "@babel/plugin-transform-react-jsx",
                            "@babel/plugin-proposal-object-rest-spread"
                        ] 
                    }
                }
            },
            {
                test: /\.(ttf|woff)$/, 
                use: {
                    loader: "url-loader?limit=8192"
                }
            },
            { 
                test: /\.(svg)$/, 
                exclude: /node_modules/, 
                use: [
                    "raw-loader"
                ] 
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
    ],
    devtool: "source-map"
};