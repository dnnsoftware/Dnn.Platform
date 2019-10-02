import Api from "./api";

const templateService = {
    savePageAsTemplate(template) {
        const api = new Api("Pages");
        return api.post("SavePageAsTemplate", template);
    }
};
export default templateService;