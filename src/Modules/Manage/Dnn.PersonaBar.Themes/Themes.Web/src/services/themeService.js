
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

        sf.moduleRoot = "PersonaBar/Admin";
        sf.controller = controller;

        return sf;
    }

    getCurrentTheme(callback, errorCallback) {
        const sf = this.getServiceFramework("Themes");
        sf.get("GetCurrentTheme", {}, callback, errorCallback);
    }

    getThemeFiles(themeName, themeType, themeLevel, callback, errorCallback){
        const sf = this.getServiceFramework("Themes");
        sf.get("GetThemeFiles", {themeName: themeName, type: themeType, level: themeLevel}, callback, errorCallback);
    }

    applyTheme(themeFile, scope, callback, errorCallback){
        const sf = this.getServiceFramework("Themes");
        sf.post("ApplyTheme", {themeFile: themeFile, scope: scope}, callback, errorCallback);
    }

    getThemes(level, callback, errorCallback){
        const sf = this.getServiceFramework("Themes");
        sf.get("GetThemes", {level: level}, callback, errorCallback);
    }
}
    
const themeService = new ThemeService();
export default themeService;