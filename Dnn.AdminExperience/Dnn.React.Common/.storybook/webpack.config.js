const path = require("path");

module.exports = {
    module: {
        rules: [
            { test: /\.less$/, loaders: ["style-loader", "css-loader", "less-loader"], include: path.resolve(__dirname, "../") },
            {
                test: /\.(svg)$/, exclude: /node_modules/,
                loader: "raw-loader", 
                include: path.resolve(__dirname, "../")
            },
            { test: /\.(gif|png)$/, loader: "url-loader?mimetype=image/png" },
        ]
    } 
};