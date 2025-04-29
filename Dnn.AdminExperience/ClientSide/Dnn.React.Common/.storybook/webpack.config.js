const path = require("path");

module.exports = {
  module: {
    rules: [
      {
        test: /\.less$/,
        use: [
          {
            loader: "css-loader",
            options: {
              importLoaders: 1,
              sourceMap: true,
              esModule: false,
            },
          },
          "less-loader",
        ],
        include: path.resolve(__dirname, "../"),
      },
      {
        test: /\.(svg)$/,
        exclude: /node_modules/,
        use: ["raw-loader"],
        include: path.resolve(__dirname, "../"),
      },
      { test: /\.(gif|png)$/, use: ["url-loader?mimetype=image/png"] },
    ],
  },
};
