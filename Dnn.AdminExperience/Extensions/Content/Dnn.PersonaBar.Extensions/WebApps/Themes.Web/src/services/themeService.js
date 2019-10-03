import utils from "utils";

class ThemeService {
    getServiceFramework(controller) {
        let sf = window.dnn.initThemes().utility.sf;

        sf.moduleRoot = "PersonaBar";
        sf.controller = controller;
        return sf;
    }

    getCurrentTheme(callback, errorCallback) {
        const sf = this.getServiceFramework("Themes");
        sf.get("GetCurrentTheme?language=" + utils.utilities.getCulture(), {}, callback, errorCallback);
    }

    getThemes(level, callback, errorCallback) {
        const sf = this.getServiceFramework("Themes");
        sf.get("GetThemes", { level: level }, callback, errorCallback);
    }

    getThemeFiles(themeName, themeType, themeLevel, callback, errorCallback) {
        const sf = this.getServiceFramework("Themes");
        sf.get("GetThemeFiles", { themeName: themeName, type: themeType, level: themeLevel }, callback, errorCallback);
    }

    applyTheme(themeFile, scope, callback, errorCallback) {
        const sf = this.getServiceFramework("Themes");
        sf.post("ApplyTheme?language=" + utils.utilities.getCulture(), { themeFile: themeFile, scope: scope }, callback, errorCallback);
    }

    getEditableTokens(callback, errorCallback) {
        const sf = this.getServiceFramework("Themes");
        sf.get("GetEditableTokens", {}, callback, errorCallback);
    }

    getEditableSettings(token, callback, errorCallback) {
        const sf = this.getServiceFramework("Themes");
        sf.get("GetEditableSettings", { token: token }, callback, errorCallback);
    }

    getEditableValues(token, setting, callback, errorCallback) {
        const sf = this.getServiceFramework("Themes");
        sf.get("GetEditableValues", { token: token, setting: setting }, callback, errorCallback);
    }

    updateTheme(path, token, setting, value, callback, errorCallback) {
        const sf = this.getServiceFramework("Themes");
        sf.post("UpdateTheme", { path: path, token: token, setting: setting, value: value }, callback, errorCallback);
    }

    parseTheme(themeName, parseType, callback, errorCallback) {
        const sf = this.getServiceFramework("Themes");
        sf.post("ParseTheme", { themeName: themeName, parseType: parseType }, callback, errorCallback);
    }

    restoreTheme(callback, errorCallback) {
        const sf = this.getServiceFramework("Themes");
        sf.post("RestoreTheme?language=" + utils.utilities.getCulture(), {}, callback, errorCallback);
    }

    applyDefaultTheme(themeName, level, callback, errorCallback) {
        const sf = this.getServiceFramework("Themes");
        sf.post("ApplyDefaultTheme?language=" + utils.utilities.getCulture(), { themeName: themeName, level: level }, callback, errorCallback);
    }

    deleteTheme(theme, callback, errorCallback) {
        const sf = this.getServiceFramework("Themes");
        sf.post("DeleteThemePackage", theme, callback, errorCallback);
    }
}

const themeService = new ThemeService();
export default themeService;