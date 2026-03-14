import { defineConfig } from "@rsbuild/core";
import { pluginReact } from "@rsbuild/plugin-react";
import { pluginLess } from "@rsbuild/plugin-less";
import path from "path";
import { createRequire } from "module";

const requireModule = createRequire(__filename);
const packageJson = requireModule("./package.json");

const isProduction = process.env.NODE_ENV === "production";

const externalizeNodeModules = ({ request }: { request?: string }) => {
  if (!request) {
    return undefined;
  }

  if (request.startsWith(".") || path.isAbsolute(request)) {
    return undefined;
  }

  if (request.startsWith("@babel/runtime")) {
    return undefined;
  }

  return request;
};

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
        ({ request }) => {
          if (request === "react" || request === "prop-types") {
            return request;
          }
          return externalizeNodeModules({ request });
        },
      ],
      resolve: {
        modules: [
          path.resolve(__dirname, "./src"),
          path.resolve(__dirname, "./node_modules"),
          path.resolve(__dirname, "../../../node_modules"),
        ],
      },
      module: {
        rules: [
          {
            test: /\.svg$/i,
            issuer: /\.[jt]sx?$/,
            use: ["@svgr/webpack"],
          },
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
  ],
});
