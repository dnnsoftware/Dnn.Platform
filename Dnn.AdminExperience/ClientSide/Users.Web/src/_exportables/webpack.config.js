const path = require("path");
const ESLintPlugin = require("eslint-webpack-plugin");
const settings = require("../../../../../settings.local.json");

module.exports = (env, argv) => {
  const isProduction = argv.mode === "production";
  return {
    entry: "./index",
    optimization: {
      minimize: isProduction,
    },
    output: {
      path:
        isProduction || settings.WebsitePath == ""
          ? path.resolve(
              "../../../../Dnn.PersonaBar.Extensions/admin/personaBar/Dnn.Users/scripts/exportables/Users",
            )
          : path.join(
              settings.WebsitePath,
              "DesktopModules\\Admin\\Dnn.PersonaBar\\Modules\\Dnn.Users\\scripts\\exportables\\Users\\",
            ),
      filename: "UsersCommon.js",
      publicPath: isProduction ? "" : "http://localhost:8050/dist/",
    },
    module: {
      rules: [
        {
          test: /\.(js|jsx)$/,
          exclude: /node_modules/,
          use: [
            {
              loader: "babel-loader",
              options: {
                presets: ["@babel/preset-env", "@babel/preset-react"],
                plugins: [
                  "@babel/plugin-transform-react-jsx",
                  "@babel/plugin-proposal-object-rest-spread",
                ],
              },
            },
          ],
        },
        {
          test: /\.(less|css)$/,
          use: [
            { loader: "style-loader" },
            {
              loader: "css-loader",
              options: {
                importLoaders: 1,
                sourceMap: true,
                esModule: false,
              },
            },
            { loader: "less-loader" },
          ],
        },
        { test: /\.(ttf|woff)$/, use: ["url-loader?limit=8192"] },
        { test: /\.(gif|png)$/, use: ["url-loader?mimetype=image/png"] },
        {
          test: /\.woff(2)?(\?v=[0-9].[0-9].[0-9])?$/,
          use: ["url-loader?mimetype=application/font-woff"],
        },
        {
          test: /\.(ttf|eot|svg)(\?v=[0-9].[0-9].[0-9])?$/,
          use: ["file-loader?name=[name].[ext]"],
        },
        {
          test: /\.(d.ts)$/,
          use: ["null-loader"],
        },
      ],
    },
    externals: require("@dnnsoftware/dnn-react-common/WebpackExternals"),
    resolve: {
      extensions: [".js", ".json", ".jsx"],
      modules: [
        path.resolve(__dirname, "./src"),
        path.resolve(__dirname, "../"),
        path.resolve(__dirname, "node_modules"),
        path.resolve(__dirname, "../../node_modules"),
        path.resolve(__dirname, "../../../../../node_modules"),
      ],
      fallback: {
        fs: false,
      },
    },
    plugins: [new ESLintPlugin({ fix: true })],
    devtool: "source-map",
  };
};
