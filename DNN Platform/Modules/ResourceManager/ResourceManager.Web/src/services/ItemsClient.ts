import { DnnServicesFramework } from "@dnncommunity/dnn-elements";
import { IPermissions } from "@dnncommunity/dnn-elements/dist/types/components/dnn-permissions-grid/permissions-interface";
import { IRoleGroup } from "@dnncommunity/dnn-elements/dist/types/components/dnn-permissions-grid/role-group-interface";
import { IRole } from "@dnncommunity/dnn-elements/dist/types/components/dnn-permissions-grid/role-interface";
import { ISearchedUser } from "@dnncommunity/dnn-elements/dist/types/components/dnn-permissions-grid/searched-user-interface";
import { SortFieldInfo } from "../enums/SortField";
import { SortOrderInfo } from "../enums/SortOrder";

export class ItemsClient{
    private sf: DnnServicesFramework;
    private requestUrl: string;
    private abortController: AbortController;

    /**
     * Initializes the api client.
     * @param moduleId The ID of the current module.
     */
    constructor(moduleId: number) {
        this.sf = new DnnServicesFramework(moduleId);
        this.requestUrl = `${this.sf.getServiceRoot("ResourceManager")}Items/`
    }

    public async getSettings() {
        const url =`${this.requestUrl}GetSettings`;
        const headers = this.sf.getModuleHeaders();
        headers.append("groupId", this.getGroupId());
        const response = await fetch(url, { headers });
        if (response.status !== 200){
            const error = await this.getGenericErrorMessage(response);
            throw new Error(error);
        }
        const data = await response.json() as GetSettingsResponse;
        return data;
    }

    /**
     * Gets a specific folder contents.
     * @param folderId Gets the content of a folder.
     * @param startIndex Which item to start at in the paging mechanism.
     * @param numItems How many items to return.
     * @param sorting How to sort the items returned.
     * @returns
     */
    public async getFolderContent(
        folderId: number,
        startIndex = 0,
        numItems = 20,
        sortingField: SortFieldInfo = new SortFieldInfo("ItemName"),
        sortingOrder: SortOrderInfo = new SortOrderInfo()
    ) {
        const url = `${this.requestUrl}GetFolderContent?folderId=${folderId}&startIndex=${startIndex}&numItems=${numItems}&sorting=${sortingField.sortKey}&sortingOrder=${sortingOrder.sortOrderKey}`;
        const headers = this.sf.getModuleHeaders();
        headers.append("groupId", this.getGroupId());
        this.abortController?.abort();
        this.abortController = new AbortController();
        const response = await fetch(url, {
            headers,
            signal: this.abortController.signal,
        });
        if (response.status !== 200){
            const error = await this.getGenericErrorMessage(response);
            throw new Error(error);
        }
        const data = await response.json() as GetFolderContentResponse;
        return data;
    }

    public async getFolderIconUrl(folderId: number) {
        const url = `${this.requestUrl}GetFolderIconUrl?folderId=${folderId}`;
        const response = await fetch(url, {
            headers: this.sf.getModuleHeaders(),
        });
        if (response.status !== 200){
            const error = await this.getGenericErrorMessage(response);
            throw new Error(error);
        }
        const data = await response.json() as GetFolderIconUrlResponse;
        return data.url;
    }

    public async getAllowedFileExtensions() {
        const url = `${this.requestUrl}GetAllowedFileExtensions`;
        const response = await fetch(url, {
            headers: this.sf.getModuleHeaders(),
        });
        if (response.status !== 200){
            const error = await this.getGenericErrorMessage(response);
            throw new Error(error);
        }
        const data = await response.json() as GetAllowedFileExtensionsResponse;
        return data;
    }

    /**
     * Syncs the folder content.
     * @param folderId The folder id.
     * @param numItems The number of items.
     * @param sorting The sorting.
     * @param recursive If true sync recursively.
     * @returns
     */
     public async syncFolderContent(
        folderId: number,
        numItems = 20,
        sorting: SortFieldInfo = new SortFieldInfo("ItemName"),
        recursive = false){
        const url = `${this.requestUrl}SyncFolderContent?folderId=${folderId}&numItems=${numItems}&sorting=${sorting.sortKey}&recursive=${recursive}`;
        const headers = this.sf.getModuleHeaders();
        headers.append("groupId", this.getGroupId());
        this.abortController?.abort();
        this.abortController = new AbortController();
        const response = await fetch(url, {
            headers,
            signal: this.abortController.signal,
        });
        if (response.status !== 200){
            const error = await this.getGenericErrorMessage(response);
            throw new Error(error);
        }
        const data = await response.json() as GetFolderContentResponse;
        return data;
    }

    /**
     * Downloads a file.
     * @param fileId The id of the file to download.
     * @param forceDownload A value indicating whether to force the download.
     *                      When true, will download the file as an attachment and ensures the browser won't just render the file if supported.
     *                      When false, the browser may render the file instead of downloading it for some formats like pdf or images.
     * @returns The actual requested file.
     */
     public async download(
        fileId: number,
        forceDownload = true
    ){
        const url = `${this.requestUrl}Download?fileId=${fileId}&forceDownload=${forceDownload}`;
        const headers = this.sf.getModuleHeaders();
        this.abortController?.abort();
        this.abortController = new AbortController();
        const response = await fetch(url, {
            headers,
            signal: this.abortController.signal,
        });
        if (response.status !== 200){
            const error = await this.getGenericErrorMessage(response);
            throw new Error(error);
        }

        let filename = response.headers.get("Content-Disposition").split("filename=")[1];
        filename = this.decodeRFC5987ContentDisposition(filename);

        const blob = await response.blob()        
        const oldDownloadLink = document.querySelector("#downloadLink");
        if (oldDownloadLink !== null) {
            oldDownloadLink.remove();
        }
        const downloadLink = document.createElement('a');
        downloadLink.id = "downloadLink";
        downloadLink.href = window.URL.createObjectURL(blob);
        downloadLink.download = filename;
        document.body.appendChild(downloadLink);
        downloadLink.click();
    }

    public async search(
        folderId: number,
        search: string,
        pageIndex: number,
        pageSize = 20,
        sorting: SortFieldInfo = new SortFieldInfo("ItemName"),
        culture = "")
    {
        const url = `${this.requestUrl}Search?folderId=${folderId}&search=${search}&pageIndex=${pageIndex}&pageSize=${pageSize}&sorting=${sorting.sortKey}&culture=${culture}`;
        const headers = this.sf.getModuleHeaders();
        headers.append("groupId", this.getGroupId());
        this.abortController?.abort();
        this.abortController = new AbortController();
        const response = await fetch(url, {
            headers,
            signal: this.abortController.signal,
        });
        if (response.status !== 200){
            const error = await this.getGenericErrorMessage(response);
            throw new Error(error);
        }
        const data = await response.json() as SearchResponse;
        return data;
    }

    public async getFolderMappings(){
        const url = `${this.requestUrl}GetFolderMappings`;
        const response = await fetch(url, {
            headers: this.sf.getModuleHeaders(),
        });
        if (response.status !== 200){
            const error = await this.getGenericErrorMessage(response);
            throw new Error(error);
        }
        const data = await response.json() as FolderMappingInfo[];
        return data;
    }

    public async canManageFolderTypes(){
        const url = `${this.requestUrl}CanManageFolderTypes`;
        const response = await fetch(url, {
            headers: this.sf.getModuleHeaders(),
        });
        if (response.status !== 200){
            const error = await this.getGenericErrorMessage(response);
            throw new Error(error);
        }
        const data = await response.json() as boolean;
        return data;
    }

    public async getAddFolderTypeUrl(){
        const url = `${this.requestUrl}GetAddFolderTypeUrl`;
        const response = await fetch(url, {
            headers: this.sf.getModuleHeaders(),
        });
        if (response.status !== 200){
            const error = await this.getGenericErrorMessage(response);
            throw new Error(error);
        }
        const data = await response.json() as string;
        return data;
    };

    public async createNewFolder(request: CreateNewFolderRequest){
        const url = `${this.requestUrl}CreateNewFolder`;
        const headers = this.sf.getModuleHeaders();
        headers.append("groupId", this.getGroupId());
        headers.append("Content-Type", "application/json");
        const response = await fetch(url, {
            method: "POST",
            body: JSON.stringify(request),
            headers,
        });
        if (response.status !== 200){
            const error = await this.getGenericErrorMessage(response);
            throw new Error(error);
        }
        const data = await response.json() as CreateNewFolderResponse;
        return data;
    }

    public async getFolderItem(folderId: number){
        const url = `${this.requestUrl}GetFolderItem?folderId=${folderId}`;
        const headers = this.sf.getModuleHeaders();
        headers.append("groupId", this.getGroupId());
        const response = await fetch(url, {
            headers,
        });
        if (response.status !== 200){
            const error = await this.getGenericErrorMessage(response);
            throw new Error(error);
        }
        const data = await response.json() as Item;
        return data;
    }

    public async getFolderDetails(folderId: number){
        const url = `${this.requestUrl}GetFolderDetails?folderId=${folderId}`;
        const response = await fetch(url, {
            headers: this.sf.getModuleHeaders(),
        });
        if (response.status !== 200){
            const error = await this.getGenericErrorMessage(response);
            throw new Error(error);
        }
        const data = await response.json() as FolderDetails;
        return data;
    }

    public async getFileDetails(fileId: number){
        const url = `${this.requestUrl}GetFileDetails?fileId=${fileId}`;
        const response = await fetch(url, {
            headers: this.sf.getModuleHeaders(),
        });
        if (response.status === 404)
        {
            throw new Error(response.statusText);
        }
        if (response.status !== 200){
            const error = await this.getGenericErrorMessage(response);
            throw new Error(error);
        }
        const data = await response.json() as FileDetails;
        return data;
    }

    public async getRoleGroups(){
        const url = `${this.requestUrl}GetRoleGroups`;
        const response = await fetch(url, {
            headers: this.sf.getModuleHeaders(),
        });
        if (response.status === 401){
            throw new Error(response.statusText);
        }
        if (response.status !== 200){
            const error = await this.getGenericErrorMessage(response);
            throw new Error(error);
        }
        const data = await response.json() as IRoleGroup[];
        return data;
    }

    public async getRoles(){
        const url = `${this.requestUrl}GetRoles`;
        const response = await fetch(url, {
            headers: this.sf.getModuleHeaders(),
        });
        if (response.status !== 200){
            const error = await this.getGenericErrorMessage(response);
            throw new Error(error);
        }
        const data = await response.json() as IRole[];
        return data;
    }

    public async searchUsers(query: string) {
        const url = `${this.requestUrl}GetSuggestionUsers?keyword=${query}&count=50`;
        const response = await fetch(url, {
            headers: this.sf.getModuleHeaders(),
        });
        if (response.status !== 200){
            const error = await this.getGenericErrorMessage(response);
            throw new Error(error);
        }
        const data = await response.json() as ISearchedUser[];
        return data;
    }

    public async saveFolderDetails(request: SaveFolderDetailsRequest){
        const url = `${this.requestUrl}SaveFolderDetails`;
        const headers = this.sf.getModuleHeaders();
        headers.append("Content-Type", "application/json");
        const response = await fetch(url, {
            method: "POST",
            body: JSON.stringify(request),
            headers,
        });
        if (response.status === 404){
            throw new Error(response.statusText);
        }
        if (response.status !== 200){
            throw new Error(response.statusText);
        }
    }

    public async saveFileDetails(request: SaveFileDetailsRequest){
        const url = `${this.requestUrl}SaveFileDetails`;
        const headers = this.sf.getModuleHeaders();
        headers.append("Content-Type", "application/json");
        const response = await fetch(url, {
            method: "POST",
            body: JSON.stringify(request),
            headers,
        });
        if (response.status !== 200){
            throw new Error(response.statusText);
        }
    }

    public async moveFile(request: MoveFileRequest){
        const url = `${this.requestUrl}MoveFile`;
        const headers = this.sf.getModuleHeaders();
        headers.append("Content-Type", "application/json");
        headers.append("groupId", this.getGroupId());
        const response = await fetch(url, {
            method: "POST",
            body: JSON.stringify(request),
            headers,
        });
        if (response.status !== 200){
            const error = await this.getGenericErrorMessage(response);
            throw new Error(error);
        }
    }

    public async moveFolder(request: MoveFolderRequest){
        const url = `${this.requestUrl}MoveFolder`;
        const headers = this.sf.getModuleHeaders();
        headers.append("Content-Type", "application/json");
        headers.append("groupId", this.getGroupId());
        const response = await fetch(url, {
            method: "POST",
            body: JSON.stringify(request),
            headers,
        });
        if (response.status !== 200){
            const error = await this.getGenericErrorMessage(response);
            throw new Error(error);
        }
    }

    public async deleteFolder(request: DeleteFolderRequest){
        const url = `${this.requestUrl}DeleteFolder`;
        const headers = this.sf.getModuleHeaders();
        headers.append("Content-Type", "application/json");
        headers.append("groupId", this.getGroupId());
        const response = await fetch(url, {
            method: "POST",
            body: JSON.stringify(request),
            headers,
        });
        if (response.status !== 200){
            const error = await this.getGenericErrorMessage(response);
            throw new Error(error);
        }
    }

    public async deleteFile(request: DeleteFileRequest){
        const url = `${this.requestUrl}DeleteFile`;
        const headers = this.sf.getModuleHeaders();
        headers.append("Content-Type", "application/json");
        headers.append("groupId", this.getGroupId());
        const response = await fetch(url, {
            method: "POST",
            body: JSON.stringify(request),
            headers,
        });
        if (response.status !== 200){
            const error = await this.getGenericErrorMessage(response);
            throw new Error(error);
        }
    }

    private decodeRFC5987ContentDisposition(filename: string): string {
        filename = filename.replace(/(^")|("$)/g, '');

        if (filename.startsWith("=?utf-8?B?") && filename.endsWith("?=")) {
            const encoded = filename.slice(10, -2);
            return decodeURIComponent(
              atob(encoded)
                .split('')
                .map(c => '%' + ('00' + c.charCodeAt(0).toString(16)).slice(-2))
                .join('')
            );
          }
          return filename;
    }

    private getGroupId(): string {
        let result = "-1";
        if (window.location.pathname.indexOf("/groupid/") > -1){
            const parts = window.location.pathname.split("/");
            const groupIdIndex = parts.indexOf("groupid");
            result = parts[groupIdIndex + 1];
            return result;
        }

        const searchParams = new URLSearchParams(window.location.search);
        const groupId = searchParams.get("groupid");
        if (groupId != null){
            result = groupId;
        }

        return result;
    }

    private async getGenericErrorMessage(response: Response) {
        const contentType = response.headers.get("Content-Type");
        if (contentType && contentType.includes("application/json")) {
            const errorBody = await response.json() as { Message?: string };
            if (errorBody?.Message != null){
                return errorBody.Message;
            }
            else if(typeof errorBody === "string"){
                return errorBody;
            }
        }
        else {
            return await response.text();
        }
    }
}

/** Represents the module settings. */
export interface GetSettingsResponse{
        /** The name of the root folder (could be an actual folder name or "Site Assets" or "Global Assets") */
        HomeFolderName: string;

        /** The ID of the home folder configures in the settings */
        HomeFolderId: number;

        /** The current module mode configured in the settings */
        Mode: ModuleMode;

        /** Whether folder is global or for current portal */
        IsHostPortal: boolean;
}

enum ModuleMode{
    /** Normal mode is when the module is used to manage site or host files. */
    Normal = 0,

    /** User mode is for when a module is used for a specific user to manage his files. */
    User = 1,

    /** Group mode is for when the module is used by a social group. */
    Group = 2,
}

export interface GetFolderContentResponse{
    folder: FolderInfo;
    hasAddFilesPermission: boolean;
    hasAddFoldersPermission: boolean;
    hasDeletePermission: boolean;
    hasManagePermission: boolean;
    items: Item[];
    totalCount: number;
}

export interface GetFolderIconUrlResponse{
    url: string;
}

export interface GetAllowedFileExtensionsResponse {
    /** A coma delimited list of the allowed file extensions. */
    allowedExtensions: string;
    validationCode: string;
    maxUploadFileSize: number;
}

export interface FolderInfo{
    folderId: number;
    folderMappingId: number;
    folderName: string;
    folderParentId: number;
    folderPath: string;
}

export interface Item{
    /** The file icon (not the image thumbnail) */
    iconUrl: string;
    /** If true, the item is a folder and the itemId represents a folderId, if false then the item is a file and the id is the fileId. */
    isFolder: boolean;
    /** The folder or file id. */
    itemId: number;
    /** The folder or file name. */
    itemName: string;
    /** The relative url to the file (no present on folders) */
    path?: string;
    /** If true, a thumbnail is available for this item. */
    thumbnailAvailable?: boolean | undefined;
    /** The relative url to the item thumbnail. */
    thumbnailUrl?: string;
    /** And ISO 8601 string representing the created date of the item. */
    createdOn: string;
    /** And ISO 8601 string representing the last modified date of the item. */
    modifiedOn: string;
    /** The size of the file (only available for file Items) */
    fileSize?: number,
    /** Defines if a folder has the possibility of being unlinked instead of deleted. */
    unlinkAllowedStatus?: "true" | "false" | "unlinkOnly",
}

export interface SearchResponse{
    items: Item[],
    totalCount: number,
}

export interface FolderMappingInfo{
    /** The ID of the folder mapping. */
    FolderMappingID: number;
    /** The provider type name such as "AzureFolderProvider" */
    FolderProviderType: string;
    /** True if this is the default provider type. */
    IsDefault: boolean;
    /** A friendly name for this mapping type. */
    MappingName: string;
    /** A url that allows editing this folder mapping. */
    editUrl: string;
}

export interface CreateNewFolderRequest{
    /** Gets or sets the new folder name. */
    FolderName: string;

    /** Gets or sets he parent folder id for the new folder. */
    ParentFolderId: number;

    /** Gets or sets the folder mapping id. */
    FolderMappingId: number;

    /** Gets or sets the optional mapped path. */
    MappedName?: string;
}

export interface CreateNewFolderResponse{
    /** The ID of the recently created folder. */
    FolderID: number;

    /** The created folder name. */
    FolderName: string,

    /** The url to the folder icon. */
    IconUrl: string;

    /** The ID of the folder mapping. */
    FolderMappingID: number,
}

export interface FolderDetails{
    folderId: number;
    folderName: string;
    createdOnDate: string;
    createdBy: string;
    lastModifiedOnDate: string;
    lastModifiedBy: string;
    type: string;
    isVersioned: boolean;
    permissions: IPermissions;
}

export interface SaveFolderDetailsRequest{
    folderId: number;
    folderName: string;
    permissions: IPermissions;
}

export interface FileDetails{
    fileId: number;
    fileName: string;
    title: string;
    description: string;
    size: string;
    createdOnDate: string;
    createdBy: string;
    lastModifiedOnDate: string;
    lastModifiedBy: string;
    url: string;
    iconUrl: string;
}

export interface SaveFileDetailsRequest{
    fileId: number;
    fileName: string;
    title: string;
    description: string;
}

export interface MoveFileRequest{
    SourceFileId: number;
    DestinationFolderId: number;
}

export interface MoveFolderRequest{
    SourceFolderId: number;
    DestinationFolderId: number;
}

export interface DeleteFolderRequest{
    FolderId: number;
    UnlinkAllowedStatus: boolean;
}

export interface DeleteFileRequest{
    FileId: number;
}

