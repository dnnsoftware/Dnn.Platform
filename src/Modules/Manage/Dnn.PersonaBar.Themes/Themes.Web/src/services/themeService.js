
function serializeQueryStringParameters(obj) {
    let s = [];
    for (let p in obj) {
        if (obj.hasOwnProperty(p)) {
            s.push(encodeURIComponent(p) + "=" + encodeURIComponent(obj[p]));
        }
    }
    return s.join("&");
}
class ThemeService {
    getServiceFramework(controller) {
        let sf = window.dnn.initThemes().utility.sf;

        sf.moduleRoot = "PersonaBar";
        sf.controller = controller;

        return sf;
    }

    getCurrentTheme(callback, errorCallback) {
        const sf = this.getServiceFramework("Themes");
        sf.get("GetCurrentTheme", {}, callback, errorCallback);
    }

    getThemes(level, callback, errorCallback){
        const sf = this.getServiceFramework("Themes");
        sf.get("GetThemes", {level: level}, callback, errorCallback);
    }

    getThemeFiles(themeName, themeType, themeLevel, callback, errorCallback){
        const sf = this.getServiceFramework("Themes");
        sf.get("GetThemeFiles", {themeName: themeName, type: themeType, level: themeLevel}, callback, errorCallback);
    }

    applyTheme(themeFile, scope, callback, errorCallback){
        const sf = this.getServiceFramework("Themes");
        sf.post("ApplyTheme", {themeFile: themeFile, scope: scope}, callback, errorCallback);
    }

    getEditableTokens(callback, errorCallback){
        const sf = this.getServiceFramework("Themes");
        sf.get("GetEditableTokens", {}, callback, errorCallback);
    }

    getEditableSettings(token, callback, errorCallback){
        const sf = this.getServiceFramework("Themes");
        sf.get("GetEditableSettings", {token: token}, callback, errorCallback);
    }

    getEditableValues(token, setting, callback, errorCallback){
        const sf = this.getServiceFramework("Themes");
        sf.get("GetEditableValues", {token: token, setting: setting}, callback, errorCallback);
    }

    updateTheme(path, token, setting, value, callback, errorCallback){
        const sf = this.getServiceFramework("Themes");
        sf.post("UpdateTheme", {path: path, token: token, setting: setting, value: value}, callback, errorCallback);
    }

    parseTheme(themeName, parseType, callback, errorCallback){
        const sf = this.getServiceFramework("Themes");
        sf.post("ParseTheme", {themeName: themeName, parseType: parseType}, callback, errorCallback);
    }

    restoreTheme(callback, errorCallback){
        const sf = this.getServiceFramework("Themes");
        sf.post("RestoreTheme", {}, callback, errorCallback);
    }

    applyDefaultTheme(themeName, callback, errorCallback){
        const sf = this.getServiceFramework("Themes");
        sf.post("ApplyDefaultTheme", {themeName: themeName}, callback, errorCallback);
    }

    deleteTheme(theme, callback, errorCallback){
        const sf = this.getServiceFramework("Themes");
        sf.post("DeleteThemePackage", theme, callback, errorCallback);
    }
}
    
const themeService = new ThemeService();
export default themeService;