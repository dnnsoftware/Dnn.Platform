const path = require("path");
const settings = require("../../../settings.local.json");
const isProduction = process.env.NODE_ENV === "production";

module.exports = {
  entry: "./index",
  optimization: {
    minimize: true
  },
  node: {
    fs: "empty"
  },
  output: {
    path:
      isProduction || settings.WebsitePath == ""
        ? path.resolve(
            "../../Dnn.PersonaBar.Extensions/admin/personaBar/Dnn.Sites/scripts/exportables/Sites"
          )
        : settings.WebsitePath +
          "\\DesktopModules\\Admin\\Dnn.PersonaBar\\Modules\\Dnn.Sites\\scripts\\exportables\\Sites\\",
    filename: "SitesListView.js",
    publicPath: isProduction ? "" : "http://localhost:8050/dist/"
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
      { test: /\.(less|css)$/, loader: "style-loader!css-loader!less-loader" }
    ]
  },
  externals: require("@dnnsoftware/dnn-react-common/WebpackExternals"),
  resolve: {
    extensions: [".js", ".json", ".jsx"],
    modules: [
      path.resolve(__dirname, "./src"),
      path.resolve(__dirname, "../"),
      path.resolve(__dirname, "node_modules"),
      path.resolve(__dirname, "../../../node_modules")
    ]
  }
};
