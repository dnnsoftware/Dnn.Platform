﻿const webpack = require("webpack");
const path = require("path");
const packageJson = require("./package.json");
const isProduction = process.env.NODE_ENV === "production";
const webpackExternals = require("@dnnsoftware/dnn-react-common/WebpackExternals");
const languages = {
  en: null
  // TODO: create locallizaton files per language
  // "de": require("./localizations/de.json"),
  // "es": require("./localizations/es.json"),
  // "fr": require("./localizations/fr.json"),
  // "it": require("./localizations/it.json"),
  // "nl": require("./localizations/nl.json")
};
const settings = require("../../../settings.local.json");
const moduleName = "site-groups";

module.exports = {
  entry: "./src/main.jsx",
  optimization: {
    minimize: isProduction
  },
  output: {
    path:
      isProduction || settings.WebsitePath == ""
        ? path.resolve("../../Dnn.PersonaBar.Extensions/admin/personaBar/Dnn.SiteGroups/scripts/bundles/")
        : settings.WebsitePath +
          "\\DesktopModules\\Admin\\Dnn.PersonaBar\\Modules\\Dnn.SiteGroups\\scripts\\bundles\\",
    filename: moduleName + "-bundle.js",
    publicPath: isProduction ? "" : "http://localhost:8080/dist/"
  },
  devServer: {
    disableHostCheck: !isProduction
  },
  module: {
    rules: [
      {
        test: /\.(js|jsx)$/,
        enforce: "pre",
        exclude: /node_modules/,
        loader: "eslint-loader",
        options: { fix: true }
      },
      {
        test: /\.(js|jsx)$/,
        exclude: /node_modules/,
        loaders: "babel-loader",
        options: {
          presets: ["@babel/preset-env", "@babel/preset-react"],
          plugins: [
            "@babel/plugin-transform-react-jsx",
            "@babel/plugin-proposal-object-rest-spread"
          ]
        }
      },
      {
        test: /\.(less|css)$/,
        use: [
          { loader: "style-loader" },
          { loader: "css-loader", options: { modules: "global" } },
          { loader: "less-loader" }
        ]
      },
      { test: /\.(ttf|woff)$/, loader: "url-loader?limit=8192" }
    ]
  },
  resolve: {
    extensions: [".js", ".json", ".jsx"],
    modules: [
      path.resolve("./src"), // Look in src first
      path.resolve("./exportables"), // Look in exportables after
      path.resolve("./node_modules"), // Try local node_modules
      path.resolve("../../../node_modules") // Last fallback to workspaces node_modules
    ]
  },
  externals: webpackExternals,
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
