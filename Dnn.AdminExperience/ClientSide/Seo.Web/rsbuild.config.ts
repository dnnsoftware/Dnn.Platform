import { defineConfig } from "@rsbuild/core";
import path from "path";
import { defaultConfig, resolveWebsitePath } from "../modules.rsbuild.config";

const websitePath = resolveWebsitePath();
const isProduction = process.env.npm_lifecycle_event === "build";
const useWebsitePath = !isProduction && websitePath !== "";
const distPath = useWebsitePath
  ? path.join(
      websitePath,
      "DesktopModules/Admin/Dnn.PersonaBar/Modules/Dnn.Seo/"
    )
  : "../../Dnn.PersonaBar.Extensions/admin/personaBar/Dnn.Seo/";
const config = defaultConfig(
  __dirname,
  "src/main.jsx",
  "seo-bundle.js",
  "Seo.css",
  isProduction,
  distPath,
);

export default defineConfig(config);
