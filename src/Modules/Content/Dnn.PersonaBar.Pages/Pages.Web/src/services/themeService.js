import Api from "./api";

const themeService = {
    getThemes() {
        const api = new Api("Pages");
        return api.get("GetThemes");
    }
};
export default themeService;