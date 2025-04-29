const webpack = require("webpack");
const ESLintPlugin = require("eslint-webpack-plugin");
const packageJson = require("./package.json");
const path = require("path");
const webpackExternals = require("@dnnsoftware/dnn-react-common/WebpackExternals");
const settings = require("../../../settings.local.json");

module.exports = (env, argv) => {
  const isProduction = argv.mode === "production";
  return {
    entry: "./src/main.jsx",
    optimization: {
      minimize: isProduction,
    },
    output: {
      path:
        isProduction || settings.WebsitePath === ""
          ? path.resolve(
              "../../Dnn.PersonaBar.Extensions/admin/personaBar/Dnn.Users/scripts/bundles/",
            )
          : path.join(
              settings.WebsitePath,
              "DesktopModules\\Admin\\Dnn.PersonaBar\\Modules\\Dnn.Users\\scripts\\bundles\\",
            ),
      filename: "users-bundle.js",
      publicPath: isProduction ? "" : "http://localhost:8080/dist/",
    },
    devServer: {
      disableHostCheck: !isProduction,
    },
    module: {
      rules: [
        {
          test: /\.(js|jsx)$/,
          exclude: [/node_modules/],
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
                modules: {
                  auto: true,
                  mode: "global",
                  localIdentName: "[name]__[local]___[hash:base64:5]",
                },
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
    resolve: {
      extensions: [".jsx", ".js", ".json"],
      modules: [
        path.resolve("./src"),
        path.resolve("./node_modules"), // Try local node_modules
        path.resolve("./src/_exportables/src"),
        path.resolve("./src/_exportables/node_modules"),
        path.resolve("../../../node_modules"), // Last fallback to workspaces node_modules
      ],
    },
    externals: Object.assign(webpackExternals, {
      "dnn-users-common-action-types": "window.dnn.Users.CommonActionTypes",
      "dnn-users-common-components": "window.dnn.Users.CommonComponents",
      "dnn-users-common-reducers": "window.dnn.Users.CommonReducers",
      "dnn-users-common-actions": "window.dnn.Users.CommonActions",
    }),
    plugins: [
      isProduction
        ? new webpack.DefinePlugin({
            VERSION: JSON.stringify(packageJson.version),
            "process.env": {
              NODE_ENV: JSON.stringify("production"),
            },
          })
        : new webpack.DefinePlugin({
            VERSION: JSON.stringify(packageJson.version),
          }),
      new webpack.SourceMapDevToolPlugin({
        filename: "users-bundle.js.map",
        append:
          "\n//# sourceMappingURL=/DesktopModules/Admin/Dnn.PersonaBar/Modules/Dnn.Users/scripts/bundles/users-bundle.js.map",
      }),
      new ESLintPlugin({ fix: true }),
    ],
    devtool: false,
  };
};
