﻿const webpack = require("webpack");
const path = require("path");
const packageJson = require("./package.json");
const isProduction = process.env.NODE_ENV === "production";
const moduleName = "vocabulary";
const settings = require("../../../settings.local.json");

module.exports = {
  entry: "./src/main.jsx",
  optimization: {
    minimize: isProduction
  },
  output: {
    path:
      isProduction || settings.WebsitePath == ""
        ? path.resolve(
            __dirname,
            "../../Dnn.PersonaBar.Extensions/admin/personaBar/Dnn.Vocabularies/scripts/bundles/"
          )
        : settings.WebsitePath +
          "\\DesktopModules\\Admin\\Dnn.PersonaBar\\Modules\\Dnn.Vocabularies\\scripts\\bundles\\",
    publicPath: isProduction ? "" : "http://localhost:8080/dist/",
    filename: moduleName + "-bundle.js"
  },
  devServer: {
    disableHostCheck: !isProduction
  },
  resolve: {
    extensions: ["*", ".js", ".json", ".jsx"],
    modules: [
      path.resolve("./src"), // Look in src first
      path.resolve("./node_modules"), // Try local node_modules
      path.resolve("../../../node_modules") // Last fallback to workspaces node_modules
    ]
  },

  module: {
    rules: [
      {
        test: /\.(js|jsx)$/,
        exclude: /node_modules/,
        enforce: "pre",
        use: ["eslint-loader"]
      },
      {
        test: /\.less$/,
        use: [
          {
            loader: "style-loader" // creates style nodes from JS strings
          },
          {
            loader: "css-loader", // translates CSS into CommonJS
            options: { modules: "global" }
          },
          {
            loader: "less-loader" // compiles Less to CSS
          }
        ]
      },
      {
        test: /\.(js|jsx)$/,
        exclude: /node_modules/,
        use: {
          loader: "babel-loader",
          options: {
            presets: ["@babel/preset-env", "@babel/preset-react"]
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

  externals: require("@dnnsoftware/dnn-react-common/WebpackExternals"),

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
