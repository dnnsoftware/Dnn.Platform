import api from "../globals/api";

const API_PATH = "InternalServices/API/";
const GET_FOLDERS_ENDPOINT = API_PATH + "ItemListService/GetFolders";
const SEARCH_FOLDERS_ENDPOINT = API_PATH + "ItemListService/SearchFolders";
const GET_FOLDER_DESCENDANT_ENDPOINT = API_PATH + "ItemListService/GetFolderDescendants";

function getUrl(endpoint) {
    return api.getServiceRoot(true) + endpoint;
}

function getFolders() {
    return api.get(getUrl(GET_FOLDERS_ENDPOINT));
}

function searchFolders(searchText) {
    return api.get(getUrl(SEARCH_FOLDERS_ENDPOINT), {searchText});
}

function getFolderDescendant(parentId) {
    return api.get(getUrl(GET_FOLDER_DESCENDANT_ENDPOINT), {parentId});
}

export default {
    getFolders,
    searchFolders,
    getFolderDescendant
};