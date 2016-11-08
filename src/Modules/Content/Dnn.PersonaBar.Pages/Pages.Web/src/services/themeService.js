import Api from "./api";

const themeService = {
    getThemes() {
        const api = new Api("Themes");
        return api.get("GetThemes", { level: 3});
    }
};
export default themeService;