const webpack = require("webpack");
const packageJson = require("./package.json");
const isProduction = process.env.NODE_ENV === "production";
const path = require("path");
const extractTextPlugin = require("extract-text-webpack-plugin");
const optimizeCssAssetsPlugin = require('optimize-css-assets-webpack-plugin');
module.exports = {
    context: path.resolve(__dirname, '.'),
    entry: "./src/main.jsx",
    output: {
        path: path.resolve(__dirname, '../admin/personaBar/scripts/bundles/'),
        publicPath: isProduction ? "" : "http://localhost:8100/dist/",
        filename: 'prompt-bundle.js'
    },
    devtool: '#source-map',
    resolve: {
        //extensions: ['*', '.webpack.js', '.web.js', '.ts', '.tsx', '.js', '.jsx', '.css', '.json'],
        extensions: ["", ".js", ".json", ".jsx"],
        root: [
            path.resolve('./src'),          // Look in src first
            path.resolve('./node_modules')  // Last fallback to node_modules
        ]
    },
    module: {
        // rules: [{
        //     test: /\.js$/, exclude: /(node_modules|bower_components)/, loader: 'babel-loader',
        //     options: { presets: ['es2015'] }
        // },
        // {
        //     test: /\.css$/,
        //     loader: extractTextPlugin.extract({ use: 'css-loader' })
        // }],
        loaders: [
            { test: /\.(js|jsx)$/, exclude: /node_modules/, loaders: ["react-hot-loader", "babel-loader"] },
            { test: /\.less$/, loader: "style-loader!css-loader!less-loader" },
            { test: /\.(ttf|woff)$/, loader: "url-loader?limit=8192" }
        ],

        preLoaders: [
            { test: /\.(js|jsx)$/, exclude: /node_modules/, loader: "eslint-loader" }
        ]
    },
    externals: require("dnn-webpack-externals"),
    plugins:
    isProduction
        ?
        [
            new webpack.optimize.UglifyJsPlugin(),
            new webpack.optimize.DedupePlugin(),
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
    // plugins: [
    //     new webpack.ProvidePlugin({
    //         $: 'jquery',
    //         jQuery: 'jquery',
    //         'window.jQuery': 'jquery'
    //     }),

    //     //new webpack.optimize.UglifyJsPlugin({
    //     //    compress: { warnings: false }
    //     //}),
    //     new extractTextPlugin('../../css/Prompt.css'),
    //     new optimizeCssAssetsPlugin({
    //         assetNameRegExp: /\.css$/g,
    //         cssProcessor: require('cssnano'),
    //         cssProcessorOptions: { discardComments: { removeAll: true } },
    //         canPrint: true
    //     })
    // ]
}