import { DnnServicesFramework } from "@dnncommunity/dnn-elements";

export class InternalServicesClient {
  private sf: DnnServicesFramework;
  private requestUrl: string;

  /**
   * Initializes the api client.
   * @param moduleId The ID of the current module.
   */
  constructor(moduleId: number) {
    this.sf = new DnnServicesFramework(moduleId);
    this.requestUrl = `${this.sf.getServiceRoot("InternalServices")}ItemListService/`;
  }

  public getFolders(parentFolderId: number) {
    return new Promise<GetFoldersResponse>((resolve, reject) => {
      const url = `${this.requestUrl}GetFolders?parentFolderId=${parentFolderId}`;
      fetch(url, {
        headers: this.sf.getModuleHeaders(),
      })
        .then((response) => {
          if (response.status == 200) {
            response.json().then((data) => resolve(data));
          } else {
            response.json().then((error) => reject(error));
          }
        })
        .catch((error) => reject(error));
    });
  }

  public getFolderDescendants(
    parentId?: string,
    sortOrder = 0,
    searchText = "",
    permission = "",
    portalId = -1,
  ) {
    return new Promise<GetFolderDescendantsResponse>((resolve, reject) => {
      let url = `${this.requestUrl}GetFolderDescendants?parentId=${parentId}&sortOrder=${sortOrder}&searchText=${searchText}&permission=${permission}`;
      if (portalId > -1) {
        url += "&portalId=${portalId}";
      }

      fetch(url, {
        headers: this.sf.getModuleHeaders(),
      })
        .then((response) => {
          if (response.status == 200) {
            response.json().then((data) => resolve(data));
          } else {
            response.json().then((error) => reject(error.message));
          }
        })
        .catch((error) => reject(error));
    });
  }
}

export interface GetFoldersResponse {
  IgnoreRoot: boolean;
  Success: boolean;
  Tree: FolderTreeItem;
}

export interface FolderTreeItem {
  data: FolderTreeData;
  children?: FolderTreeItem[];
}

export interface FolderTreeData {
  hasChildren: boolean;
  key: string;
  selectable: boolean;
  value?: string;
}

export interface GetFolderDescendantsResponse {
  Items: FolderTreeData[];
  Success: boolean;
}
