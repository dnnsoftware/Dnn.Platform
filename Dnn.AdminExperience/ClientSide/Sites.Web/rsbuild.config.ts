import { defineConfig } from "@rsbuild/core";
import path from "path";
import { defaultConfig, resolveWebsitePath } from "../modules.rsbuild.config";

const websitePath = resolveWebsitePath();
const isProduction = process.env.npm_lifecycle_event === "build";
const useWebsitePath = !isProduction && websitePath !== "";
const distPath = useWebsitePath
  ? path.join(
      websitePath,
      "DesktopModules/Admin/Dnn.PersonaBar/Modules/Dnn.Sites/"
    )
  : "../../Dnn.PersonaBar.Extensions/admin/personaBar/Dnn.Sites/";
const externalOverrides = {
  "dnn-sites-common-action-types": "window.dnn.Sites.CommonActionTypes",
  "dnn-sites-common-components": "window.dnn.Sites.CommonComponents",
  "dnn-sites-common-reducers": "window.dnn.Sites.CommonReducers",
  "dnn-sites-common-actions": "window.dnn.Sites.CommonActions",
};
const config = defaultConfig(
  __dirname,
  "src/main.jsx",
  "sites-bundle.js",
  "Sites.css",
  isProduction,
  distPath,
  externalOverrides,
);

export default defineConfig(config);
