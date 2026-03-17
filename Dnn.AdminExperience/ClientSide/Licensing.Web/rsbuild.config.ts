import { defineConfig } from "@rsbuild/core";
import { pluginReact } from "@rsbuild/plugin-react";
import { pluginLess } from "@rsbuild/plugin-less";
import path from "path";
import { createRequire } from "module";
import { defaultConfig, resolveWebsitePath } from "../modules.rsbuild.config";

const websitePath = resolveWebsitePath();
const isProduction = process.env.npm_lifecycle_event === "build";
const useWebsitePath = !isProduction && websitePath !== "";
const distPath = useWebsitePath
  ? path.join(
      websitePath,
      "DesktopModules/Admin/Dnn.PersonaBar/Modules/Dnn.Licensing/"
    )
  : "../../Dnn.PersonaBar.Extensions/admin/personaBar/Dnn.Licensing/";
const config = defaultConfig(
  __dirname,
  "src/main.jsx",
  "licensing-bundle.js",
  "licensing.css",
  isProduction,
  distPath,
);

export default defineConfig(config);
