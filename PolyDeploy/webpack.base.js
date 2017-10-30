'use strict';

/*
    Modules
    Require the modules we need.
*/
const webpack = require('webpack');
const ExtractTextPlugin = require('extract-text-webpack-plugin');

/*
    Plugin Stack
    The plugins being used will differ for development and production. Any
    plugins specified here are common to both.
*/
let pluginStack = [
    
    /*
        Tell the ExtractTextPlugin what name to use for the combined .css file.
    */
    new ExtractTextPlugin("[name].css")

];

if (process.env.NODE_ENV !== 'development') {

    /*
        Initialise loader options.
    */
    pluginStack.push(
        new webpack.LoaderOptionsPlugin({
            minimize: true,
            debug: false
        })
    );

    /*
        Create global constants.
    */
    pluginStack.push(
        new webpack.DefinePlugin({
            'process.env': {
                'NODE_ENV': JSON.stringify('production')
            }
        })
    );

    /*
        Uglification.
    */
    pluginStack.push(
        new webpack.optimize.UglifyJsPlugin({
            beautify: false,
            mangle: {
                screw_ie8: true,
                keep_fnames: true
            },
            compress: {
                screw_ie8: true,
                warnings: false
            },
            comments: false
        })
    );

    /*
        Aggressive chunk merging.
    */
    pluginStack.push(
        new webpack.optimize.AggressiveMergingPlugin()
    );
}

let baseConfig = {

    /*
        Point at the global entry-point for your application. Here we use two:

        1. The JS entry, where we load all the JS we need.
        2. The CSS entry, where we load all the CSS we need.

        Part 1 is the core of what Webpack does - it pulls in all the assets we
        need to run a powerful client-side webapp.

        Part 2 kicks off the Sass compilation by pointing at the index file
        which in turn imports all our CSS.
    */
    entry: {
        'app.bundle': [
            'babel-polyfill',
            './app/js/index.js'
        ],
        'app.styles': './app/css/main.less'
    },

    /*
        Define where we would like the output to go.
     */
    output: {
        filename: '[name].js'
    },

    /*
        Now we specify what we actually want to do to all the files that get
        loaded in.
    */
    module: {
        rules: [

            /*
                Get all *.js files and run them through Babel. This allows us to
                write ES6 and have it transpile into old-style JS. Further
                config for this can be found in the root .babelrc file - which
                specifies options for Babel.
            */
            {
                test: /\.js$/,
                use: 'babel-loader',
                exclude: /node_modules/
            },

            /*
                Get all *.less files and first run them through the Less loader
                which compiles the CSS, then run it through the CSS loader
                (which minifies, and combines all the CSS).

                Here we're using a function to export the CSS into a seperate
                file - by default Webpack will inline the CSS for use in JS.
            */
            {
                test: /\.less$/,
                use: ExtractTextPlugin.extract({
                    use: [
                        'css-loader',
                        'less-loader'
                    ],
                    fallback: 'style-loader'
                })
            },

            /*
                Get all *.scss files and first run them through the Sass loader
                which compiles the CSS, then run it through the CSS loader
                (which minifies, and combines all the CSS).

                Here we're using a function to export the CSS into a seperate
                file - by default Webpack will inline the CSS for use in JS.
            */
            {
                test: /\.scss$/,
                use: ExtractTextPlugin.extract({
                    use: [
                        'css-loader',
                        'sass-loader',
                        'resolve-url-loader'
                    ],
                    fallback: 'style-loader'
                })
            },
            {
                test: /\.css$/,
                use: 'css-loader'
            },
            {
                test: /\.(ttf|woff|svg|eot)/,
                use: 'file-loader?outputPath=assets/'
            },

            /*
                To enable live-reload we also look at all *.htm files, and just
                output them in their original state using the raw-loader.
            */
            {
                test: /\.(htm|html)$/,
                use: 'raw-loader'
            },
            {
                test: /\.(jpg|png)$/,
                use: 'file-loader?outputPath=assets/'
            }
        ]
    },

    /*
        Enable source-map generation. This is a file which allows the browser to
        translate the minified source-code back into the original source.
    */
    devtool: 'source-map',

    /*
        Pass in our plugin stack.
    */
    plugins: pluginStack
};

/*
    Export a function that will use the base config but allow properties to be
    overridden by the config passed in.
*/
module.exports = function (config) {

    // Passed in config?
    if (config) {

        // Loop properties and override base config.
        for (prop in config) {

            baseConfig[prop] = config[prop];
        }
    }

    return baseConfig;
};
