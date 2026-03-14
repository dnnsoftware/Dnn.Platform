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
    const settings = requireModule("../../../../../settings.local.json");
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
const useWebsitePath = !isProduction && websitePath;

export default defineConfig({
  source: {
    entry: {
      main: path.resolve(__dirname, "index.jsx"),
    },
  },
  output: {
    target: "web",
    filenameHash: false,
    cleanDistPath: false,
    injectStyles: true,
    cssModules: {
      auto: true,
      localIdentName: "[local]",
    },
    distPath: {
      root: useWebsitePath
        ? path.join(
            websitePath,
            "DesktopModules/Admin/Dnn.PersonaBar/Modules/Dnn.Users/scripts/exportables/Users/"
          )
        : "../../../../Dnn.PersonaBar.Extensions/admin/personaBar/Dnn.Users/scripts/exportables/Users/",
      js: "",
      css: "",
      html: "",
    },
    filename: {
      js: "UsersCommon.js",
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
          path.resolve(__dirname, "../"),
          path.resolve(__dirname, "./node_modules"),
          path.resolve(__dirname, "../../node_modules"),
          path.resolve(__dirname, "../../../../../node_modules"),
        ],
        fallback: {
          fs: false,
        },
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
