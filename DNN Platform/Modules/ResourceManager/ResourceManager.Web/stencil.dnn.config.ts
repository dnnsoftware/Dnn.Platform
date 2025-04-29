import { config as originalConfig } from "./stencil.config";
import dnnConfig from "../../../../settings.local.json";
import { Config } from "@stencil/core";

if (!dnnConfig || !dnnConfig.WebsitePath) {
  console.error("WebsitePath is not defined in settings.local.json");
}
const outPath = `${dnnConfig.WebsitePath}\\DesktopModules\\ResourceManager\\Scripts`;

export const config: Config = {
  ...originalConfig,
  outputTargets: [
    {
      // For DNN yarn watch --scope dnn-resource-manager
      type: "dist",
      esmLoaderPath: "../loader",
      dir: dnnConfig?.WebsitePath ? outPath : "../Scripts",
    },
  ],
};
