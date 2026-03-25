import { defineConfig } from "@rsbuild/core";
import path from "path";
import { defaultConfig, resolveWebsitePath } from "../modules.rsbuild.config";

const websitePath = resolveWebsitePath();
const isProduction = process.env.npm_lifecycle_event === "build";
const useWebsitePath = !isProduction && websitePath !== "";
const distPath = useWebsitePath
  ? path.join(
      websitePath,
      "DesktopModules/Admin/Dnn.PersonaBar/Modules/Dnn.Users/"
    )
  : "../../Dnn.PersonaBar.Extensions/admin/personaBar/Dnn.Users/";
const externalOverrides = {
  "dnn-users-common-action-types": "window.dnn.Users.CommonActionTypes",
  "dnn-users-common-components": "window.dnn.Users.CommonComponents",
  "dnn-users-common-reducers": "window.dnn.Users.CommonReducers",
  "dnn-users-common-actions": "window.dnn.Users.CommonActions",
};
const config = defaultConfig(
  __dirname,
  "src/main.jsx",
  "users-bundle.js",
  "Users.css",
  isProduction,
  distPath,
  externalOverrides,
);

export default defineConfig(config);
