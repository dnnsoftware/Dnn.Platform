import { defineConfig } from "@rsbuild/core";
import { pluginReact } from "@rsbuild/plugin-react";
import { pluginLess } from "@rsbuild/plugin-less";
import { pluginSvgr } from "@rsbuild/plugin-svgr";
import path from "path";
import { createRequire } from "module";

const requireModule = createRequire(__filename);
const packageJson = requireModule("./package.json");

const isProduction = process.env.npm_lifecycle_event === "build";

// Modules explicitly provided by Persona Bar runtime globals.
const runtimeProvidedExternals = new Set([
  "react",
  "prop-types",
  "redux",
  "react-redux",
  "react-dom",
  "redux-immutable-state-invariant",
  "redux-thunk",
  "react-collapse",
  "react-custom-scrollbars",
  "react-widgets",
  "es6-promise",
]);

export default defineConfig({
  source: {
    entry: {
      main: path.resolve(__dirname, "src/index.js"),
    },
  },
  output: {
    target: "web",
    filenameHash: false,
    cleanDistPath: true,
    injectStyles: true,
    cssModules: {
      auto: true,
      mode: "global",
      localIdentName: "[name]__[local]___[hash:base64:5]",
    },
    distPath: {
      root: path.resolve(__dirname, "dist"),
      js: "",
      css: "",
      html: "",
    },
    filename: {
      js: "dnn-react-common.min.js",
    },
    legalComments: "none",
  },
  performance: {
    chunkSplit: {
      strategy: "all-in-one",
    },
  },
  tools: {
    rspack: {
      output: {
        library: {
          name: "DnnReactCommon",
          type: "umd",
        },
        umdNamedDefine: true,
        globalObject: "this",
      },
      externals: [
        ({ request }: { request?: string }) => {
          if (!request) {
            return undefined;
          }

          if (runtimeProvidedExternals.has(request)) {
            return request;
          }

          // Bundle everything else in dnn-react-common so runtime-only globals
          // do not break components (for example react-modal internals).
          return undefined;
        },
      ],
      resolve: {
        modules: [
          path.resolve(__dirname, "./src"),
          path.resolve(__dirname, "./node_modules"),
          path.resolve(__dirname, "../../../node_modules"),
        ],
      },
      plugins: [
        // Keep VERSION and NODE_ENV globals consumed by existing code.
        new (requireModule("@rspack/core").DefinePlugin)({
          VERSION: JSON.stringify(packageJson.version),
          "process.env.NODE_ENV": JSON.stringify(
            isProduction ? "production" : "development"
          ),
        }),
      ],
    },
    htmlPlugin: false,
  },
  dev: {
    writeToDisk: true,
    hmr: false,
    liveReload: false,
  },
  plugins: [
    pluginReact({
      swcReactOptions: {
        runtime: "classic",
      },
    }),
    pluginLess({
      lessLoaderOptions: {
        lessOptions: {
          javascriptEnabled: true,
        },
      },
    }),
    pluginSvgr({
        svgrOptions: {
            exportType: "default"
        }
    }),
  ],
});
