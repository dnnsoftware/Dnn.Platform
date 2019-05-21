const webpack = require("webpack");
const packageJson = require("./package.json");
const path = require("path");
const isProduction = process.env.NODE_ENV === "production";
const webpackExternals = require("@dnnsoftware/dnn-react-common/WebpackExternals");
const languages = {
    "en": null
    // TODO: create locallizaton files per language 
    // "de": require("./localizations/de.json"),
    // "es": require("./localizations/es.json"),
    // "fr": require("./localizations/fr.json"),
    // "it": require("./localizations/it.json"),
    // "nl": require("./localizations/nl.json")
};
const moduleName = "themes";
module.exports = {
    entry: "./src/main.jsx",
    optimization: {
        minimize: isProduction
    },
    output: {
        path: path.resolve(__dirname, "../../admin/personaBar/Dnn.Themes/scripts/bundles/"),
        publicPath: isProduction ? "" : "http://localhost:8080/dist/",
        filename: moduleName + "-bundle.js"
    },
    devServer: {
        disableHostCheck: !isProduction
    },
    resolve: {
        extensions: ["*", ".js", ".json", ".jsx"],
        modules: [
            path.resolve("./src"),           // Look in src first
            path.resolve("./exportables"),   // Look in exportables after
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
                use: [
                    "eslint-loader" 
                ]
            },
            { 
                test: /\.less$/, 
                use: [{
                    loader: "style-loader"  // creates style nodes from JS strings
                }, {
                    loader: "css-loader"    // translates CSS into CommonJS
                }, {
                    loader: "less-loader"   // compiles Less to CSS
                }]
            },
            { 
                test: /\.(js|jsx)$/, 
                exclude: /node_modules/, 
                use: {
                    loader: "babel-loader",
                    options: {
                        presets: ["@babel/preset-env","@babel/preset-react"]
                    }
                }
            },
            { 
                test: /\.(ttf|woff)$/, 
                use: {
                    loader: "url-loader?limit=8192"
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
    ],
    devtool: "source-map"
};