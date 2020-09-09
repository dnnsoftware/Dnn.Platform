import api from "../globals/api";

const GET_CONTENT_ENDPOINT = "Items/GetFolderContent";
const SYNC_CONTENT_ENDPOINT = "Items/SyncFolderContent";
const DOWNLOAD_FILE_ENDPOINT = "Items/Download";
const FILE_DETAILS_ENDPOINT = "Items/GetFileDetails";
const FOLDER_DETAILS_ENDPOINT = "Items/GetFolderDetails";
const LOAD_FOLDER_MAPPINGS_ENDPOINT = "Items/GetFolderMappings";
const ADD_FOLDER_ENDPOINT = "Items/CreateNewFolder";
const DELETE_FOLDER_ENDPOINT = "Items/DeleteFolder";
const DELETE_FILE_ENDPOINT = "Items/DeleteFile";
const API_PATH = "InternalServices/API/";
const FILE_UPLOAD = API_PATH + "FileUpload/UploadFromLocal";
const SEARCH_FILES_ENDPOINT = "Items/Search";
const SAVE_FILE_DETAILS_ENDPOINT = "Items/SaveFileDetails";
const SAVE_FOLDER_DETAILS_ENDPOINT = "Items/SaveFolderDetails";
const REMOVE_FOLDER_TYPE_ENDPOINT = "Items/RemoveFolderType";
const ADD_FOLDER_TYPE_URL_ENDPOINT = "Items/GetAddFolderTypeUrl";

function getUrl(endpoint, ignoreCurrentModuleAPI=false) {
    return api.getServiceRoot(ignoreCurrentModuleAPI) + endpoint;
}

function getContent(folderId, startIndex, numItems, sorting) {
    return api.get(getUrl(GET_CONTENT_ENDPOINT), {folderId, startIndex, numItems, sorting})
        .then(response => {
            return response;
        });
}

function syncContent(folderId, numItems, sorting, recursive) {
    return api.get(getUrl(SYNC_CONTENT_ENDPOINT), {folderId, numItems, sorting, recursive})
        .then(response => {
            return response;
        });
}

function getDownloadUrl(fileId) {
    let {moduleId, tabId} = api.getHeadersObject();
    return getUrl(DOWNLOAD_FILE_ENDPOINT) + "?forceDownload=true&fileId=" + fileId + "&moduleId=" + moduleId + "&tabId=" + tabId;
}

function loadFolderMappings() {
    return (api.get(getUrl(LOAD_FOLDER_MAPPINGS_ENDPOINT)))
        .then(response => {
            return response;
        });
}

function addFolder(data) {
    return api.post(getUrl(ADD_FOLDER_ENDPOINT), data, { "Content-Type":"application/json" });
}

function deleteFolder(folderId) {
    return api.post(getUrl(DELETE_FOLDER_ENDPOINT), {folderId}, { "Content-Type":"application/json" });
}

function deleteFile(fileId) {
    return api.post(getUrl(DELETE_FILE_ENDPOINT), {fileId}, { "Content-Type":"application/json" });
}

function getFileDetails(fileId) {
    return api.get(getUrl(FILE_DETAILS_ENDPOINT), {fileId});
}

function getFolderDetails(folderId) {
    return api.get(getUrl(FOLDER_DETAILS_ENDPOINT), {folderId});
}

function uploadFile(file, folderPath, overwrite, trackProgress) {
    const formData = new FormData();
    formData.append("postfile", file);
    if (folderPath && typeof folderPath === "string") {
        formData.append("folder", folderPath);
    }
    if (overwrite && typeof overwrite === "boolean") {
        formData.append("overwrite", overwrite);
    }
    let {extensionWhitelist, validationCode} = api.getWhitelistObject();
    formData.append("filter", extensionWhitelist);
    formData.append("validationCode", validationCode);
    const url = getUrl(FILE_UPLOAD, true);
    
    return api.postFile(url, formData, trackProgress);
}

function searchFiles(folderId, search, pageIndex, pageSize, sorting, culture) {
    return api.get(getUrl(SEARCH_FILES_ENDPOINT), {folderId, search, pageIndex, pageSize, sorting, culture})
        .then(response => {
            return response;
        });
}

function getItemFullUrl(relativeUrl) {
    return location.protocol + "//" + location.hostname + (location.port ? ":" + location.port : "") + relativeUrl;
}

function getFolderUrl(folderId) {
    return location.protocol + "//" + location.host + location.pathname + "?folderId=" + folderId;
}

function getIconUrl(item) {
    if (item.isFolder) {
        return item.iconUrl;
    } else {
        return !item.thumbnailAvailable ? item.iconUrl : item.thumbnailUrl;
    }
}

function saveFileDetails(item) {
    return api.post(getUrl(SAVE_FILE_DETAILS_ENDPOINT), item, { "Content-Type":"application/json" });
}

function saveFolderDetails(item) {
    return api.post(getUrl(SAVE_FOLDER_DETAILS_ENDPOINT), item, { "Content-Type":"application/json" });
}

function removeFolderType(folderMappingId) {
    return api.postPrimitive(getUrl(REMOVE_FOLDER_TYPE_ENDPOINT), folderMappingId.toString());
}

function getAddFolderTypeUrl() {
    return api.get(getUrl(ADD_FOLDER_TYPE_URL_ENDPOINT))
}

export default {
    getContent,
    syncContent,
    getDownloadUrl,
    loadFolderMappings,
    addFolder,
    deleteFolder,
    deleteFile,
    getFileDetails,
    getFolderDetails,
    uploadFile,
    searchFiles,
    getItemFullUrl,
    getFolderUrl,
    getIconUrl,
    saveFileDetails,
    saveFolderDetails,
    removeFolderType,
    getAddFolderTypeUrl,
};