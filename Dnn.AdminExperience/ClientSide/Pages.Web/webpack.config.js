const isProduction = process.env.NODE_ENV === "production";
const packageJson = require("./package.json");
const path = require("path");
const webpack = require("webpack");
const webpackExternals = require("@dnnsoftware/dnn-react-common/WebpackExternals");
const settings = require("../../../settings.local.json");

module.exports = {
  entry: "./src/main.jsx",
  output: {
    path:
      isProduction || settings.WebsitePath == ""
        ? path.resolve(
            __dirname,
            "../../Dnn.PersonaBar.Extensions/admin/personaBar/Dnn.Pages/scripts/bundles/"
          )
        : settings.WebsitePath +
          "\\DesktopModules\\Admin\\Dnn.PersonaBar\\Modules\\Dnn.Pages\\scripts\\bundles\\",
    filename: "pages-bundle.js",
    publicPath: isProduction ? "" : "http://localhost:8080/dist/"
  },
  devServer: {
    disableHostCheck: !isProduction
  },
  module: {
    rules: [
      {
        test: /\.(js|jsx)$/,
        exclude: /node_modules/,
        use: { loader: "eslint-loader", options: { fix: true } },
        enforce: "pre"
      },
      {
        test: /\.(js|jsx)$/,
        exclude: /node_modules/,
        use: {
          loader: "babel-loader"
        }
      },
      {
        test: /\.less$/,
        use: [
          {
            loader: "style-loader"
          },
          {
            loader: "css-loader",
            options: { modules: "global" }
          },
          {
            loader: "less-loader"
          }
        ]
      },
      {
        test: /\.css$/,
        use: [
          {
            loader: "style-loader"
          },
          {
            loader: "css-loader",
            options: { modules: "global" }
          }
        ]
      },
      {
        test: /\.(ttf|woff|gif|png)$/,
        use: {
          loader: "url-loader?limit=8192"
        }
      }
    ]
  },
  resolve: {
    extensions: ["*", ".js", ".json", ".jsx"],
    modules: [
      path.resolve("./src"), // Look in src first
      path.resolve("./node_modules"), // Try local node_modules
      path.resolve("../../../node_modules") // Last fallback to workspaces node_modules
    ]
  },
  externals: webpackExternals,
  optimization: {
    minimize: isProduction
  },
  plugins: isProduction
    ? [
        new webpack.DefinePlugin({
          VERSION: JSON.stringify(packageJson.version),
          "process.env": {
            NODE_ENV: JSON.stringify("production")
          }
        })
      ]
    : [
        new webpack.DefinePlugin({
          VERSION: JSON.stringify(packageJson.version)
        })
      ],
  devtool: "source-map"
};
