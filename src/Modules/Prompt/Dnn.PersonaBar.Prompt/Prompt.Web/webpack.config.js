var path = require('path'),
    webpack = require('webpack');

module.exports = {
    context: path.resolve(__dirname, '.'),
    entry: "./src/app.js",
    output: {
        path: path.resolve(__dirname, '../admin/personaBar/scripts/'),
        publicPath: '/scripts/',
        filename: 'prompt-bundle.js'
    },
    devtool: '#source-map',
    resolve: {
        extensions: ['*', '.webpack.js', '.web.js', '.ts', '.tsx', '.js', '.jsx']
    },
    module: {
        loaders: [{
            test: /\.js$/,
            exclude: /(node_modules|bower_components)/,
            loader: 'babel-loader',
            query: {
                presets: ['es2015']
            }
        }]
    },
    externals: {
        'jquery': 'jQuery'
    },
    plugins: [
        new webpack.ProvidePlugin({
            $: 'jquery',
            jQuery: 'jquery',
            'window.jQuery': 'jquery'
        }),
        // new webpack.optimize.UglifyJsPlugin({
        //     compress: { warnings: false }
        // })
    ]
}