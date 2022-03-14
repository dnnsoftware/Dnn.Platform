import { DnnServicesFramework } from "@dnncommunity/dnn-elements";

export class LocalizationClient{
    private sf: DnnServicesFramework;
    private requestUrl: string;

    /**
     * Initializes the api client.
     * @param moduleId The ID of the current module.
     */
    constructor(moduleId: number){
        this.sf = new DnnServicesFramework(moduleId);
        this.requestUrl = `${this.sf.getServiceRoot("ResourceManager")}Localization/`
    }

    /** Gets the localized strings */
    public getResources(){
        return new Promise<LocalizedStrings>((resolve, reject) => {
            const LocalizationStorageKey = "ResourceManagerLocalization";
            const localization = sessionStorage.getItem(LocalizationStorageKey);
            if (localization){
                resolve(JSON.parse(localization));
                return;
            }

            const url = `${this.requestUrl}GetResources`;
            fetch(url, {
                headers: this.sf.getModuleHeaders(),
            })
            .then(response => {
                if (response.status == 200){
                    response.json().then(data => {
                        sessionStorage.setItem(LocalizationStorageKey, JSON.stringify(data));
                        resolve(data);
                    });
                }
                else{
                    response.json().then(error => reject(error));
                }
            })
            .catch(error => reject(error));
        })
    }
}

export interface LocalizedStrings {    
    AddAsset: String;
    AddFolder: String;
    AssetsPanelEmpty_Subtitle: String;
    AssetsPanelEmpty_Title: String;
    Cancel: String;
    Close: String;
    Database: String;
    DeleteFileDialogHeader: String;
    DeleteFileDialogMessage: String;
    DeleteFolderDialogHeader: String;
    DeleteFolderDialogMessage: String;
    FileAlreadyExistsMessage: String;
    FileUploadedMessage: String;
    FileUploadStoppedMessage: String;
    FolderNamePlaceholder: String;
    FolderNameRequiredMessage: String;
    FolderParent: String;
    FolderTypeRequiredMessage: String;
    Name: String;
    Save: String;
    Secure: String;
    Standard: String;
    Type: String;
    "UserHasNoPermissionToDeleteFolder.Error": String;
    "UserHasNoPermissionToDownloadError": String;
    FileUploadPanelMessage: String;
    AssetsPanelNoSearchResults: String;
    SearchInputPlaceholder: String;
    Search: String;
    UrlCopiedMessage: String;
    Created: String;
    Description: String;
    LastModified: String;
    Size: String;
    Title: String;
    URL: String;
    ItemNameRequiredMessage: String;
    "UserHasNoPermissionToReadFileProperties.Error": String;
    GenericErrorMessage: String;
    Items: String;
    ItemSavedMessage: String;
    FolderType: String;
    CreatedOnDate: String;
    ItemName: String;
    LastModifiedOnDate: String;
    FileSizeErrorMessage: String;
    "UserHasNoPermissionToDeleteFile.Error": String;
    "UserHasNoPermissionToManageFileProperties.Error": String;
    "UserHasNoPermissionToManageFolder.Error": String;
    NoFolderSelected: String;
    SearchFolder: String;
    DisabledForAnonymousUsersMessage: String;
    "UserHasNoPermissionToAddFolders.Error": String;
    "GroupIconCantBeDeleted.Error": String;
    InvalidExtensionMessage: String;
    Refresh: String;
    SyncThisFolder: String;
    SyncThisFolderAndSubfolders: String;
    ManageFolderTypes: String;
    AddFolderType: String;
    FolderProvider: String;
    FolderTypeDefinitions: String;
    EditFolderType: String;
    RemoveFolderType: String;
    RemoveFolderTypeDialogBody: String;
    RemoveFolderTypeDialogHeader: String;
    MappedPath: String;
    MoveItem: String;
    NewLocation: String;
    SortField_ItemName: String;
    SortField_LastModifiedOnDate: String;
    SortField_Size: String;
    SortField_ParentFolder: String;
    SortField_CreatedOnDate: String;
    Sort: String;
  };