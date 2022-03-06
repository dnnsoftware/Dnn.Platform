import { DnnServicesFramework } from "@dnncommunity/dnn-elements";
import { SortFieldInfo } from "../enums/SortField";

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

    /**
     * Gets a specific folder contents.
     * @param folderId Gets the content of a folder.
     * @param startIndex Which item to start at in the paging mechanism.
     * @param numItems How many items to return.
     * @param sorting How to sort the items returned.
     * @returns 
     */
    public getFolderContent(
        folderId: number,
        startIndex = 0,
        numItems = 20,
        sorting: SortFieldInfo = new SortFieldInfo("ItemName"),
        groupId = -1){
        return new Promise<GetFolderContentResponse>((resolve, reject) => {
            const url = `${this.requestUrl}GetFolderContent?folderId=${folderId}&startIndex=${startIndex}&numItems=${numItems}&sorting=${sorting.sortKey}`;
            const headers = this.sf.getModuleHeaders();
            headers.append("groupId", groupId.toString());
            this.abortController?.abort();
            this.abortController = new AbortController();
            fetch(url, {
                headers,
                signal: this.abortController.signal,
            })
            .then(response => {
                if (response.status == 200){
                    response.json().then(data => resolve(data));
                }
                else{
                    response.json().then(error => reject(error.message));
                }
            })
            .catch(error => reject(error));
        });
    }

    public getFolderIconUrl(folderId: number) {
      return new Promise<string>((resolve, reject) => {
        const url = `${this.requestUrl}GetFolderIconUrl?folderId=${folderId}`;
        fetch(url, {
            headers: this.sf.getModuleHeaders(),
        })
        .then(response => {
            if (response.status == 200){
                response.json().then(data => resolve(data.url));
            }
            else{
                response.json().then(error => reject(error));
            }
        })
        .catch(error => reject(error));
      });
    }

    public search(
        folderId: number,
        search: string,
        pageIndex: number,
        pageSize = 20,
        sorting: SortFieldInfo = new SortFieldInfo("ItemName"),
        culture = "",
        groupId = -1)
    {
        return new Promise<SearchResponse>((resolve, reject) => {
            const url = `${this.requestUrl}Search?folderId=${folderId}&search=${search}&pageIndex=${pageIndex}&pageSize=${pageSize}&sorting=${sorting.sortKey}&culture=${culture}`;
            const headers = this.sf.getModuleHeaders();
            headers.append("groupId", groupId.toString());
            this.abortController?.abort();
            this.abortController = new AbortController();
            fetch(url, {
                headers,
                signal: this.abortController.signal,
            })
            .then(response => {
                if (response.status == 200){
                    response.json().then(data => resolve(data));
                }
                else{
                    response.json().then(error => reject(error.message));
                }
            })
            .catch(error => reject(error));
        });
    }

    public getFolderMappings(){
        return new Promise<FolderMappingInfo[]>((resolve, reject) => {
            const url = `${this.requestUrl}GetFolderMappings`;
            fetch(url, {
                headers: this.sf.getModuleHeaders(),
            })
            .then(response => {
                if (response.status == 200){
                    response.json().then(data => resolve(data));
                }
                else{
                    response.json().then(error => reject(error.message));
                }
            })
            .catch(error => reject(error));
        });
    }

    public createNewFolder(request: CreateNewFolderRequest, groupId = 1){
        return new Promise<CreateNewFolderResponse>((resolve, reject) => {
            const url = `${this.requestUrl}CreateNewFolder`;
            const headers = this.sf.getModuleHeaders();
            headers.append("groupId", groupId.toString());
            headers.append("Content-Type", "application/json");
            fetch(url, {
                method: "POST",
                body: JSON.stringify(request),
                headers,
            })
            .then(response => {
                if (response.status == 200){
                    response.json().then(data => resolve(data));
                }
                else{
                    response.json().then(error => reject(error.message));
                }
            })
            .catch(reason => reject(reason));

        });
    }
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