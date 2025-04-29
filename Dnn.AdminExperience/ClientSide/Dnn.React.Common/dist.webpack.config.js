const webpack = require("webpack");
const ESLintPlugin = require("eslint-webpack-plugin");
const path = require("path");
const packageJson = require("./package.json");
const nodeExternals = require("webpack-node-externals");

module.exports = {
  mode: "production",
  optimization: {
    minimize: true,
  },
  entry: "./src/index.js",
  output: {
    path: path.resolve(__dirname, "dist"),
    filename: "dnn-react-common.min.js",
    library: "DnnReactCommon",
    libraryTarget: "umd",
  },
  module: {
    rules: [
      {
        test: /\.(js|jsx|ts|tsx)$/,
        exclude: /node_modules/,
        use: {
          loader: "babel-loader",
          options: {
            presets: [
              "@babel/preset-env",
              "@babel/preset-react",
              "@babel/preset-typescript",
            ],
          },
        },
      },
      {
        test: /\.less$/,
        use: [
          "style-loader",
          {
            loader: "css-loader",
            options: {
              importLoaders: 1,
              sourceMap: true,
              modules: {
                auto: true,
                mode: "global",
                localIdentName: "[name]__[local]___[hash:base64:5]",
              },
              esModule: false,
            },
          },
          "less-loader",
        ],
      },
      { test: /\.(ttf|woff)$/, use: ["url-loader?limit=8192"] },
      { test: /\.css$/, use: ["style-loader!css-loader"] },
      { test: /\.(gif|png)$/, use: ["url-loader?mimetype=image/png"] },
      { test: /\.(svg)$/, issuer: /\.[jt]sx?$/, use: ["@svgr/webpack"] },
      {
        test: /\.woff(2)?(\?v=[0-9].[0-9].[0-9])?$/,
        use: ["url-loader?mimetype=application/font-woff"],
      },
      {
        test: /\.(ttf|eot)(\?v=[0-9].[0-9].[0-9])?$/,
        use: ["file-loader?name=[name].[ext]"],
      },
    ],
  },
  externals: ["react", "prop-types", nodeExternals()], // in order to ignore all modules in node_modules folder
  resolve: {
    extensions: [".js", ".json", ".jsx", ".ts", ".tsx"],
    modules: [
      path.resolve(__dirname, "./src"), // Look in src first
      path.resolve("./node_modules"), // Try local node_modules
      path.resolve("../../../node_modules"), // Last fallback to workspaces node_modules
    ],
  },
  plugins: [
    new webpack.DefinePlugin({
      VERSION: JSON.stringify(packageJson.version),
      "process.env": {
        NODE_ENV: JSON.stringify("production"),
      },
    }),
    new ESLintPlugin({ fix: true }),
  ],
};
