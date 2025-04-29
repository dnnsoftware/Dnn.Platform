import { DnnServicesFramework } from "@dnncommunity/dnn-elements";
import { IPermissions } from "@dnncommunity/dnn-elements/dist/types/components/dnn-permissions-grid/permissions-interface";
import { IRoleGroup } from "@dnncommunity/dnn-elements/dist/types/components/dnn-permissions-grid/role-group-interface";
import { IRole } from "@dnncommunity/dnn-elements/dist/types/components/dnn-permissions-grid/role-interface";
import { ISearchedUser } from "@dnncommunity/dnn-elements/dist/types/components/dnn-permissions-grid/searched-user-interface";
import { SortFieldInfo } from "../enums/SortField";

export class ItemsClient {
  private sf: DnnServicesFramework;
  private requestUrl: string;
  private abortController: AbortController;

  /**
   * Initializes the api client.
   * @param moduleId The ID of the current module.
   */
  constructor(moduleId: number) {
    this.sf = new DnnServicesFramework(moduleId);
    this.requestUrl = `${this.sf.getServiceRoot("ResourceManager")}Items/`;
  }

  public getSettings() {
    return new Promise<GetSettingsResponse>((resolve, reject) => {
      const url = `${this.requestUrl}GetSettings`;
      const headers = this.sf.getModuleHeaders();
      headers.append("groupId", this.getGroupId());
      fetch(url, {
        headers,
      })
        .then((response) => {
          if (response.status == 200) {
            response.json().then((data) => {
              resolve(data);
            });
          } else {
            response.json().then((error) => reject(error));
          }
        })
        .catch((error) => reject(error));
    });
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
  ) {
    return new Promise<GetFolderContentResponse>((resolve, reject) => {
      const url = `${this.requestUrl}GetFolderContent?folderId=${folderId}&startIndex=${startIndex}&numItems=${numItems}&sorting=${sorting.sortKey}`;
      const headers = this.sf.getModuleHeaders();
      headers.append("groupId", this.getGroupId());
      this.abortController?.abort();
      this.abortController = new AbortController();
      fetch(url, {
        headers,
        signal: this.abortController.signal,
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

  public getFolderIconUrl(folderId: number) {
    return new Promise<string>((resolve, reject) => {
      const url = `${this.requestUrl}GetFolderIconUrl?folderId=${folderId}`;
      fetch(url, {
        headers: this.sf.getModuleHeaders(),
      })
        .then((response) => {
          if (response.status == 200) {
            response.json().then((data) => resolve(data.url));
          } else {
            response.json().then((error) => reject(error));
          }
        })
        .catch((error) => reject(error));
    });
  }

  public getAllowedFileExtensions() {
    return new Promise<GetAllowedFileExtensionsResponse>((resolve, reject) => {
      const url = `${this.requestUrl}GetAllowedFileExtensions`;
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

  /**
   * Syncs the folder content.
   * @param folderId The folder id.
   * @param numItems The number of items.
   * @param sorting The sorting.
   * @param recursive If true sync recursively.
   * @returns
   */
  public syncFolderContent(
    folderId: number,
    numItems = 20,
    sorting: SortFieldInfo = new SortFieldInfo("ItemName"),
    recursive = false,
  ) {
    return new Promise<void>((resolve, reject) => {
      const url = `${this.requestUrl}SyncFolderContent?folderId=${folderId}&numItems=${numItems}&sorting=${sorting.sortKey}&recursive=${recursive}`;
      const headers = this.sf.getModuleHeaders();
      headers.append("groupId", this.getGroupId());
      this.abortController?.abort();
      this.abortController = new AbortController();
      fetch(url, {
        headers,
        signal: this.abortController.signal,
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

  /**
   * Downloads a file.
   * @param fileId The id of the file to download.
   * @param forceDownload A value indicating whether to force the download.
   *                      When true, will download the file as an attachment and ensures the browser won't just render the file if supported.
   *                      When false, the browser may render the file instead of downloading it for some formats like pdf or images.
   * @returns The actual requested file.
   */
  public download(fileId: number, forceDownload = true) {
    return new Promise<void>((resolve, reject) => {
      const url = `${this.requestUrl}Download?fileId=${fileId}&forceDownload=${forceDownload}`;
      const headers = this.sf.getModuleHeaders();
      this.abortController?.abort();
      this.abortController = new AbortController();
      fetch(url, {
        headers,
        signal: this.abortController.signal,
      })
        .then((response) => {
          if (response.status == 200) {
            var filename = response.headers
              .get("Content-Disposition")
              .split("filename=")[1];
            filename = this.decodeRFC5987ContentDisposition(filename);
            response.blob().then((blob) => {
              var oldDownloadLink = document.querySelector("#downloadLink");
              if (oldDownloadLink) {
                oldDownloadLink.remove();
              }
              var downloadLink = document.createElement("a");
              downloadLink.id = "downloadLink";
              downloadLink.href = window.URL.createObjectURL(blob);
              downloadLink.download = filename;
              document.body.appendChild(downloadLink);
              downloadLink.click();
              resolve();
            });
          }
        })
        .catch((error) => reject(error));
    });
  }
  private decodeRFC5987ContentDisposition(filename: string): string {
    filename = filename.replace(/(^")|("$)/g, "");

    if (filename.startsWith("=?utf-8?B?") && filename.endsWith("?=")) {
      const encoded = filename.slice(10, -2);
      return decodeURIComponent(
        atob(encoded)
          .split("")
          .map((c) => "%" + ("00" + c.charCodeAt(0).toString(16)).slice(-2))
          .join(""),
      );
    }
    return filename;
  }

  public search(
    folderId: number,
    search: string,
    pageIndex: number,
    pageSize = 20,
    sorting: SortFieldInfo = new SortFieldInfo("ItemName"),
    culture = "",
  ) {
    return new Promise<SearchResponse>((resolve, reject) => {
      const url = `${this.requestUrl}Search?folderId=${folderId}&search=${search}&pageIndex=${pageIndex}&pageSize=${pageSize}&sorting=${sorting.sortKey}&culture=${culture}`;
      const headers = this.sf.getModuleHeaders();
      headers.append("groupId", this.getGroupId());
      this.abortController?.abort();
      this.abortController = new AbortController();
      fetch(url, {
        headers,
        signal: this.abortController.signal,
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

  public getFolderMappings() {
    return new Promise<FolderMappingInfo[]>((resolve, reject) => {
      const url = `${this.requestUrl}GetFolderMappings`;
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

  public canManageFolderTypes() {
    return new Promise<boolean>((resolve, reject) => {
      const url = `${this.requestUrl}CanManageFolderTypes`;
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

  public getAddFolderTypeUrl() {
    return new Promise<string>((resolve, reject) => {
      const url = `${this.requestUrl}GetAddFolderTypeUrl`;
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

  public createNewFolder(request: CreateNewFolderRequest) {
    return new Promise<CreateNewFolderResponse>((resolve, reject) => {
      const url = `${this.requestUrl}CreateNewFolder`;
      const headers = this.sf.getModuleHeaders();
      headers.append("groupId", this.getGroupId());
      headers.append("Content-Type", "application/json");
      fetch(url, {
        method: "POST",
        body: JSON.stringify(request),
        headers,
      })
        .then((response) => {
          if (response.status == 200) {
            response.json().then((data) => resolve(data));
          } else {
            response
              .json()
              .then((error) => reject(error.ExceptionMessage || error.message));
          }
        })
        .catch((reason) => reject(reason));
    });
  }

  public getFolderItem(folderId: number) {
    return new Promise<Item>((resolve, reject) => {
      const url = `${this.requestUrl}GetFolderItem?folderId=${folderId}`;
      const headers = this.sf.getModuleHeaders();
      headers.append("groupId", this.getGroupId());
      fetch(url, {
        headers,
      })
        .then((response) => {
          if (response.status == 200) {
            response.json().then((data) => resolve(data));
          } else {
            response
              .json()
              .then((error) => reject(error.ExceptionMessage || error.message));
          }
        })
        .catch((reason) => reject(reason));
    });
  }

  public getFolderDetails(folderId: number) {
    return new Promise<FolderDetails>((resolve, reject) => {
      const url = `${this.requestUrl}GetFolderDetails?folderId=${folderId}`;
      fetch(url, {
        headers: this.sf.getModuleHeaders(),
      })
        .then((response) => {
          if (response.status == 200) {
            response.json().then((data) => resolve(data));
          } else {
            response
              .json()
              .then((error) => reject(error.ExceptionMessage || error.message));
          }
        })
        .catch((reason) => reject(reason));
    });
  }

  public getFileDetails(fileId: number) {
    return new Promise<FileDetails>((resolve, reject) => {
      const url = `${this.requestUrl}GetFileDetails?fileId=${fileId}`;
      fetch(url, {
        headers: this.sf.getModuleHeaders(),
      })
        .then((response) => {
          if (response.status == 200) {
            response.json().then((data) => resolve(data));
          } else {
            response
              .json()
              .then((error) => reject(error.ExceptionMessage || error.message));
          }
        })
        .catch((reason) => reject(reason));
    });
  }

  public getRoleGroups() {
    return new Promise<IRoleGroup[]>((resolve, reject) => {
      const url = `${this.requestUrl}GetRoleGroups`;
      fetch(url, {
        headers: this.sf.getModuleHeaders(),
      })
        .then((response) => {
          if (response.status == 200) {
            response.json().then((data) => resolve(data));
          } else {
            response
              .json()
              .then((error) => reject(error.ExceptionMessage || error.message));
          }
        })
        .catch((reason) => reject(reason));
    });
  }

  public getRoles() {
    return new Promise<IRole[]>((resolve, reject) => {
      const url = `${this.requestUrl}GetRoles`;
      fetch(url, {
        headers: this.sf.getModuleHeaders(),
      })
        .then((response) => {
          if (response.status == 200) {
            response.json().then((data) => resolve(data));
          } else {
            response
              .json()
              .then((error) => reject(error.ExceptionMessage || error.message));
          }
        })
        .catch((reason) => reject(reason));
    });
  }

  public searchUsers(query: string) {
    return new Promise<ISearchedUser[]>((resolve, reject) => {
      const url = `${this.requestUrl}GetSuggestionUsers?keyword=${query}&count=50`;
      fetch(url, {
        headers: this.sf.getModuleHeaders(),
      })
        .then((response) => {
          if (response.status == 200) {
            response.json().then((data) => resolve(data));
          } else {
            response
              .json()
              .then((error) => reject(error.ExceptionMessage || error.message));
          }
        })
        .catch((reason) => reject(reason));
    });
  }

  public saveFolderDetails(request: SaveFolderDetailsRequest) {
    return new Promise<void>((resolve, reject) => {
      const url = `${this.requestUrl}SaveFolderDetails`;
      const headers = this.sf.getModuleHeaders();
      headers.append("Content-Type", "application/json");
      fetch(url, {
        method: "POST",
        body: JSON.stringify(request),
        headers,
      })
        .then((response) => {
          if (response.status == 200) {
            resolve();
          } else {
            response
              .json()
              .then((error) => reject(error.ExceptionMessage || error.message));
          }
        })
        .catch((reason) => reject(reason));
    });
  }

  public saveFileDetails(request: SaveFileDetailsRequest) {
    return new Promise<void>((resolve, reject) => {
      const url = `${this.requestUrl}SaveFileDetails`;
      const headers = this.sf.getModuleHeaders();
      headers.append("Content-Type", "application/json");
      fetch(url, {
        method: "POST",
        body: JSON.stringify(request),
        headers,
      })
        .then((response) => {
          if (response.status == 200) {
            resolve();
          } else {
            response
              .json()
              .then((error) => reject(error.ExceptionMessage || error.message));
          }
        })
        .catch((reason) => reject(reason));
    });
  }

  public moveFile(request: MoveFileRequest) {
    return new Promise<void>((resolve, reject) => {
      const url = `${this.requestUrl}MoveFile`;
      const headers = this.sf.getModuleHeaders();
      headers.append("Content-Type", "application/json");
      headers.append("groupId", this.getGroupId());
      fetch(url, {
        method: "POST",
        body: JSON.stringify(request),
        headers,
      })
        .then((response) => {
          if (response.status == 200) {
            resolve();
          } else {
            response
              .json()
              .then((error) => reject(error.ExceptionMessage || error.message));
          }
        })
        .catch((reason) => reject(reason));
    });
  }

  public moveFolder(request: MoveFolderRequest) {
    return new Promise<void>((resolve, reject) => {
      const url = `${this.requestUrl}MoveFolder`;
      const headers = this.sf.getModuleHeaders();
      headers.append("Content-Type", "application/json");
      headers.append("groupId", this.getGroupId());
      fetch(url, {
        method: "POST",
        body: JSON.stringify(request),
        headers,
      })
        .then((response) => {
          if (response.status == 200) {
            resolve();
          } else {
            response
              .json()
              .then((error) => reject(error.ExceptionMessage || error.message));
          }
        })
        .catch((reason) => reject(reason));
    });
  }

  public deleteFolder(request: DeleteFolderRequest) {
    return new Promise<void>((resolve, reject) => {
      const url = `${this.requestUrl}DeleteFolder`;
      const headers = this.sf.getModuleHeaders();
      headers.append("Content-Type", "application/json");
      headers.append("groupId", this.getGroupId());
      fetch(url, {
        method: "POST",
        body: JSON.stringify(request),
        headers,
      })
        .then((response) => {
          if (response.status == 200) {
            resolve();
          } else {
            response
              .json()
              .then((error) => reject(error.ExceptionMessage || error.message));
          }
        })
        .catch((reason) => reject(reason));
    });
  }

  public deleteFile(request: DeleteFileRequest) {
    return new Promise<void>((resolve, reject) => {
      const url = `${this.requestUrl}DeleteFile`;
      const headers = this.sf.getModuleHeaders();
      headers.append("Content-Type", "application/json");
      headers.append("groupId", this.getGroupId());
      fetch(url, {
        method: "POST",
        body: JSON.stringify(request),
        headers,
      })
        .then((response) => {
          if (response.status == 200) {
            resolve();
          } else {
            response
              .json()
              .then((error) => reject(error.ExceptionMessage || error.message));
          }
        })
        .catch((reason) => reject(reason));
    });
  }

  private getGroupId(): string {
    let result = "-1";
    if (window.location.pathname.indexOf("/groupid/") > -1) {
      const parts = window.location.pathname.split("/");
      const groupIdIndex = parts.indexOf("groupid");
      result = parts[groupIdIndex + 1];
      return result;
    }

    const searchParams = new URLSearchParams(window.location.search);
    const groupId = searchParams.get("groupid");
    if (groupId) {
      result = groupId;
    }

    return result;
  }
}

/** Represents the module settings. */
export interface GetSettingsResponse {
  /** The name of the root folder (could be an actual folder name or "Site Assets" or "Global Assets") */
  HomeFolderName: string;

  /** The ID of the home folder configures in the settings */
  HomeFolderId: number;

  /** The current module mode configured in the settings */
  Mode: ModuleMode;

  /** Whether folder is global or for current portal */
  IsHostPortal: boolean;
}

export enum ModuleMode {
  /** Normal mode is when the module is used to manage site or host files. */
  Normal = 0,

  /** User mode is for when a module is used for a specific user to manage his files. */
  User = 1,

  /** Group mode is for when the module is used by a social group. */
  Group = 2,
}

export interface GetFolderContentResponse {
  folder: FolderInfo;
  hasAddFilesPermission: boolean;
  hasAddFoldersPermission: boolean;
  hasDeletePermission: boolean;
  hasManagePermission: boolean;
  items: Item[];
  totalCount: number;
}

export interface GetAllowedFileExtensionsResponse {
  /** A coma delimited list of the allowed file extensions. */
  allowedExtensions: string;
  validationCode: string;
  maxUploadFileSize: number;
}

export interface FolderInfo {
  folderId: number;
  folderMappingId: number;
  folderName: string;
  folderParentId: number;
  folderPath: string;
}

export interface Item {
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
  fileSize?: number;
  /** Defines if a folder has the possibility of being unlinked instead of deleted. */
  unlinkAllowedStatus?: "true" | "false" | "unlinkOnly";
}

export interface SearchResponse {
  items: Item[];
  totalCount: number;
}

export interface FolderMappingInfo {
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

export interface CreateNewFolderRequest {
  /** Gets or sets the new folder name. */
  FolderName: string;

  /** Gets or sets he parent folder id for the new folder. */
  ParentFolderId: number;

  /** Gets or sets the folder mapping id. */
  FolderMappingId: number;

  /** Gets or sets the optional mapped path. */
  MappedName?: string;
}

export interface CreateNewFolderResponse {
  /** The ID of the recently created folder. */
  FolderID: number;

  /** The created folder name. */
  FolderName: string;

  /** The url to the folder icon. */
  IconUrl: string;

  /** The ID of the folder mapping. */
  FolderMappingID: number;
}

export interface FolderDetails {
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

export interface SaveFolderDetailsRequest {
  folderId: number;
  folderName: string;
  permissions: IPermissions;
}

export interface FileDetails {
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

export interface SaveFileDetailsRequest {
  fileId: number;
  fileName: string;
  title: string;
  description: string;
}

export interface MoveFileRequest {
  SourceFileId: number;
  DestinationFolderId: number;
}

export interface MoveFolderRequest {
  SourceFolderId: number;
  DestinationFolderId: number;
}

export interface DeleteFolderRequest {
  FolderId: number;
  UnlinkAllowedStatus: boolean;
}

export interface DeleteFileRequest {
  FileId: number;
}
