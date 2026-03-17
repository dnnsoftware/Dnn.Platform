import { defineConfig } from "@rsbuild/core";
import path from "path";
import { defaultConfig, resolveWebsitePath } from "../modules.rsbuild.config";

const websitePath = resolveWebsitePath();
const isProduction = process.env.npm_lifecycle_event === "build";
const useWebsitePath = !isProduction && websitePath !== "";
const distPath = useWebsitePath
  ? path.join(
      websitePath,
      "DesktopModules/Admin/Dnn.PersonaBar/Modules/Dnn.Themes/"
    )
  : "../../Dnn.PersonaBar.Extensions/admin/personaBar/Dnn.Themes/";
const config = defaultConfig(
  __dirname,
  "src/main.jsx",
  "themes-bundle.js",
  "Themes.css",
  isProduction,
  distPath,
);

export default defineConfig(config);
