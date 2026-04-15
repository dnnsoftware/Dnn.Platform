import { defineConfig } from "@rsbuild/core";
import { pluginReact } from "@rsbuild/plugin-react";
import { pluginLess } from "@rsbuild/plugin-less";
import { pluginSvgr } from "@rsbuild/plugin-svgr";
import path from "path";
import { defaultConfig, resolveWebsitePath } from "../modules.rsbuild.config";

const websitePath = resolveWebsitePath();
const isProduction = process.env.npm_lifecycle_event === "build";
const useWebsitePath = !isProduction && websitePath !== "";
const distPath = useWebsitePath
  ? path.join(
      websitePath,
      "DesktopModules/Admin/Dnn.PersonaBar/Modules/Dnn.Servers/"
    )
  : "../../Dnn.PersonaBar.Extensions/admin/personaBar/Dnn.Servers/";
const config = defaultConfig(
  __dirname,
  "src/main.jsx",
  "servers-bundle.js",
  "Servers.css",
  isProduction,
  distPath,
);

export default defineConfig(config);
