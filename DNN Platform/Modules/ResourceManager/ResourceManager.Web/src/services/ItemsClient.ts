import { DnnServicesFramework } from "@dnncommunity/dnn-elements";

export class ItemsClient{
    private sf: DnnServicesFramework;
    private requestUrl: string;

    /**
     * Initializes the api client.
     * @param moduleId The ID of the current module.
     */
    constructor(moduleId: number) {
        this.sf = new DnnServicesFramework(moduleId);
        this.requestUrl = `${this.sf.getServiceRoot("ResourceManager")}Items/`
    }

    /**
     * 
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
        sorting: "ItemName" | "LastModifiedOnDate" | "Size" | "ParentFolder" | "CreatedOnDate" = "ItemName",
        groupId = -1){
        return new Promise<GetFolderContentResponse>((resolve, reject) => {
            const url = `${this.requestUrl}GetFolderContent?folderId=${folderId}&startIndex=${startIndex}&numItems=${numItems}&sorting=${sorting}`;
            const headers = this.sf.getModuleHeaders();
            headers.append("groupId", groupId.toString());
            fetch(url, {
                headers
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
}