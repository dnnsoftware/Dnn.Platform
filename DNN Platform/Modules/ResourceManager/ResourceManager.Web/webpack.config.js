var webpack = require("webpack");
var path = require("path");

var APP_DIR = path.resolve(__dirname, "app/");
var moduleName = "resourceManager";
const packageJson = require("./package.json");
const isProduction = process.env.NODE_ENV === "production";
const settings = require("../../../../settings.local.json");

var config = {
  entry: path.resolve(APP_DIR, "main.jsx"),
  output: {
    path:
      isProduction || settings.WebsitePath == ""
        ? path.resolve(
            __dirname,
            "../Scripts/"
          )
        : settings.WebsitePath +
          "\\DesktopModules\\ResourceManager\\Scripts\\",
    publicPath: isProduction ? "" : "http://localhost:8080/dist/",
    filename: moduleName + "-bundle.js",
  },
  module: {
    loaders: [
      {
        test: /\.(js|jsx)$/,
        exclude: /node_modules/,
        loaders: ["react-hot-loader", "babel-loader"],
      },
      { test: /\.less$/, loader: "style-loader!css-loader!less-loader" },
      { test: /\.(ttf|woff)$/, loader: "url-loader?limit=8192" },
      { test: /\.(eot|jpg|png|gif|svg)$/, loader: "url-loader?limit=8192" },
    ],
    preLoaders: [
      { test: /\.(js|jsx)$/, exclude: /node_modules/, loader: "eslint-loader" },
    ],
  },
  resolve: {
    extensions: ["", ".js", ".json", ".jsx"],
  },
  plugins: isProduction
    ? [
        new webpack.optimize.UglifyJsPlugin(),
        new webpack.optimize.DedupePlugin(),
        new webpack.DefinePlugin({
          VERSION: JSON.stringify(packageJson.version),
          "process.env": {
            NODE_ENV: JSON.stringify("production"),
          },
        }),
      ]
    : [
        new webpack.DefinePlugin({
          VERSION: JSON.stringify(packageJson.version),
        }),
      ],
};

module.exports = config;
