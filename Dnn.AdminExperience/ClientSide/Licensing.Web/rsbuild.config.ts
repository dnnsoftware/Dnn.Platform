import { defineConfig } from "@rsbuild/core";
import { pluginReact } from "@rsbuild/plugin-react";
import { pluginLess } from "@rsbuild/plugin-less";
import path from "path";
import { createRequire } from "module";

const requireModule = createRequire(__filename);
const webpackExternals = requireModule(
  "@dnnsoftware/dnn-react-common/WebpackExternals"
);

const resolveWebsitePath = () => {
  try {
    const settings = requireModule("../../../settings.local.json");
    if (settings?.WebsitePath) {
      return settings.WebsitePath;
    }
  } catch {
    // ignore missing local settings
  }
  return "";
};

const websitePath = resolveWebsitePath();
const isProduction = process.env.npm_lifecycle_event === "build";
const useWebsitePath = !isProduction && websitePath !== "";
const distPath = useWebsitePath
  ? path.join(
      websitePath,
      "DesktopModules/Admin/Dnn.PersonaBar/Modules/Dnn.Licensing/"
    )
  : "../../Dnn.PersonaBar.Extensions/admin/personaBar/Dnn.Licensing/";
console.log("distPath", distPath);

export default defineConfig({
  source: {
    entry: {
      main: path.resolve(__dirname, "src/main.jsx"),
    },
  },
  output: {
    target: "web",
    filenameHash: false,
    cleanDistPath: false,
    injectStyles: true,
    cssModules: {
      auto: true,
      mode: "global",
      localIdentName: "[name]__[local]___[hash:base64:5]",
    },
    distPath: {
      root: distPath,
      js: "scripts/bundles/",
      css: "",
      html: "",
    },
    filename: {
      js: "licensing-bundle.js",
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
      externals: (data) => {
        const { request } = data;
        if (webpackExternals[request]) {
          return webpackExternals[request];
        }
        if (request?.startsWith("react/") || request?.startsWith("react-dom/")) {
          const baseModule = request.split("/")[0];
          if (webpackExternals[baseModule]) {
            return webpackExternals[baseModule];
          }
        }
        return undefined;
      },
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
            type: "javascript/auto",
            use: ["@svgr/webpack"],
          },
        ],
      },
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
