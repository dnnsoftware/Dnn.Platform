const path = require("path");

module.exports = {
    module: {
        rules: [
            {
                test: /\.less$/,
                use: ["style-loader", "css-loader", "less-loader"],
                include: path.resolve(__dirname, "../")
            },
            {
                test: /\.(svg)$/, exclude: /node_modules/,
                use: ["raw-loader"], 
                include: path.resolve(__dirname, "../")
            },
            { test: /\.(gif|png)$/, use: ["url-loader?mimetype=image/png"] },
        ]
    } 
};