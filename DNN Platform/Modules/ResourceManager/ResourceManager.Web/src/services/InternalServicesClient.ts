import { DnnServicesFramework } from "@dnncommunity/dnn-elements";

export class InternalServicesClient{
    private sf: DnnServicesFramework;
    private requestUrl: string;

    /**
     * Initializes the api client.
     * @param moduleId The ID of the current module.
     */
    constructor(moduleId: number) {
        this.sf = new DnnServicesFramework(moduleId);
        this.requestUrl = `${this.sf.getServiceRoot("InternalServices")}ItemListService/`
    }

    public async getFolders(parentFolderId: number){
        const url = `${this.requestUrl}GetFolders?parentFolderId=${parentFolderId}`;
        const response = await fetch(url, {
            headers: this.sf.getModuleHeaders(),
        });
        if (response.status != 200){
            const error = await this.getGenericErrorMessage(response);
            throw new Error(error);
        }
        const data = await response.json() as GetFoldersResponse;
        return data;
    }

    public async getFolderDescendants(
        parentId?: string,
        sortOrder = 0,
        searchText = "",
        permission = "",
        portalId = -1)
    {
        let url = `${this.requestUrl}GetFolderDescendants?parentId=${parentId}&sortOrder=${sortOrder}&searchText=${searchText}&permission=${permission}`;
        if (portalId > -1){
            url += "&portalId=${portalId}";
        }

        const response = await fetch(url, {
            headers: this.sf.getModuleHeaders(),
        });
        if (response.status != 200){
            const error = await this.getGenericErrorMessage(response);
            throw new Error(error);
        }
        const data = await response.json() as GetFolderDescendantsResponse;
        return data;
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

export interface GetFoldersResponse{
    IgnoreRoot: boolean;
    Success: boolean;
    Tree: FolderTreeItem;
}

export interface FolderTreeItem{
    data: FolderTreeData;
    children?: FolderTreeItem[];
}

export interface FolderTreeData{
    hasChildren: boolean;
    key: string;
    selectable: boolean;
    value?: string;
}

export interface GetFolderDescendantsResponse{
    Items: FolderTreeData[];
    Success: boolean;
}