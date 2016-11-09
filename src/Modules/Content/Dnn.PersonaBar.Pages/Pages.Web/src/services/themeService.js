import Api from "./api";

const themeService = {
    getThemes() {
        const api = new Api("Pages");
        return api.get("GetThemes");
    },
    getThemeFiles(themeName) {
        const api = new Api("Pages");
        return api.get("GetThemeFiles", {themeName});
    }
};
export default themeService;