import { RsbuildConfig } from "@rsbuild/core";
import { pluginReact } from "@rsbuild/plugin-react";
import { pluginLess } from "@rsbuild/plugin-less";
import { pluginSvgr } from "@rsbuild/plugin-svgr";
import path from "path";
import { createRequire } from "module";

const requireModule = createRequire(__filename);
const webpackExternals = requireModule(
  "@dnnsoftware/dnn-react-common/WebpackExternals",
);

export const defaultConfig: (
  moduleDir: string,
  entry: string,
  jsFile: string,
  cssFile: string,
  isProduction: boolean,
  distPath: string,
  externalOverrides?: Record<string, string>,
) => RsbuildConfig = (moduleDir, entry, jsFile, cssFile, isProduction, distPath, externalOverrides) => {
  return {
    source: {
      entry: {
        main: path.resolve(moduleDir, entry),
      },
    },
    output: {
      target: "web",
      minify: isProduction,
      filenameHash: false,
      cleanDistPath: false,
      cssModules: {
        auto: true,
        localIdentName: "[local]",
      },
      distPath: {
        root: distPath,
        js: "scripts/bundles/",
        css: "css/",
        html: "",
      },
      filename: {
        js: jsFile,
        css: cssFile,
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
          if (externalOverrides && externalOverrides[request]) {
            return externalOverrides[request];
          }
          // Handle exact matches
          if (webpackExternals[request]) {
            return webpackExternals[request];
          }
          // Handle React submodules (e.g., react/jsx-runtime, react-dom/client)
          if (
            request?.startsWith("react/") ||
            request?.startsWith("react-dom/")
          ) {
            const baseModule = request.split("/")[0];
            if (webpackExternals[baseModule]) {
              // For submodules, return the base module
              return webpackExternals[baseModule];
            }
          }
          return undefined;
        },
        resolve: {
          mainFields: ["module", "main", "browser"],
          alias: {
            exenv: path.resolve(moduleDir, "./src/shims/exenv.js"),
          },
          modules: [
            path.resolve(moduleDir, "./src"),
            path.resolve(moduleDir, "./node_modules"),
            path.resolve(moduleDir, "../../../node_modules"),
          ],
        },
        module: {
          rules: [
            {
              resourceQuery: /raw/,
              type: "asset/source",
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
      pluginSvgr({
          svgrOptions: {
              exportType: "default"
          }
      }),
    ],
  };
};

export const resolveWebsitePath: () => string = () => {
  try {
    const settings = requireModule("../../settings.local.json");
    if (settings?.WebsitePath) {
      return settings.WebsitePath;
    }
  } catch (error) {
    console.error("Error resolving website path", error);
    // ignore missing local settings
  }
  return "";
};
