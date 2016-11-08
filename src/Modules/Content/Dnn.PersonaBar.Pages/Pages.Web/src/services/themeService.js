import Api from "./api";

const themeService = {
    getThemes() {
        const api = new Api("Themes");
        return api.get("GetThemes");
    }
};
export default themeService;