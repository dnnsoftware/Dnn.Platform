import { defineConfig } from "@rsbuild/core";
import path from "path";
import { defaultConfig, resolveWebsitePath } from "../modules.rsbuild.config";

const websitePath = resolveWebsitePath();
const isProduction = process.env.npm_lifecycle_event === "build";
const useWebsitePath = !isProduction && websitePath !== "";
const distPath = useWebsitePath
  ? path.join(
      websitePath,
      "DesktopModules/Admin/Dnn.PersonaBar/Modules/Dnn.Vocabularies/"
    )
  : "../../Dnn.PersonaBar.Extensions/admin/personaBar/Dnn.Vocabularies/";
const config = defaultConfig(
  __dirname,
  "src/main.jsx",
  "vocabulary-bundle.js",
  "vocabularies.css",
  isProduction,
  distPath,
);

export default defineConfig(config);
