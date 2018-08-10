import Api from "./api";

const themeService = {
    getThemes() {
        const api = new Api("Pages");
        return api.get("GetThemes");
    },
    getThemeFiles(themeName, level) {
        const api = new Api("Pages");
        return api.get("GetThemeFiles", {themeName, level});
    }
};
export default themeService;