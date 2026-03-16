import { defineConfig } from "@rsbuild/core";
import { pluginReact } from "@rsbuild/plugin-react";
import { pluginLess } from "@rsbuild/plugin-less";
import path from "path";
import { createRequire } from "module";

const requireModule = createRequire(__filename);
const packageJson = requireModule("./package.json");
const { DefinePlugin } = requireModule("@rspack/core");

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
const isProduction = process.env.NODE_ENV === "production";
const useWebsitePath = websitePath;

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
      root: useWebsitePath
        ? path.join(
            websitePath,
            "DesktopModules/Admin/Dnn.PersonaBar/scripts/exports/"
          )
        : "../../Library/Dnn.PersonaBar.UI/admin/personaBar/scripts/exports/",
      js: "",
      css: "",
      html: "",
    },
    filename: {
      js: "export-bundle.js",
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
      resolve: {
        modules: [
          path.resolve(__dirname, "./src"),
          path.resolve(__dirname, "./node_modules"),
          path.resolve(__dirname, "../../../node_modules"),
        ],
      },
      plugins: [
        new DefinePlugin({
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
