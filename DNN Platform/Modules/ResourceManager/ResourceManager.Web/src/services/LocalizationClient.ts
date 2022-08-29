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
    AddAsset: string;
    AddFolder: string;
    AssetsPanelEmpty_Subtitle: string;
    AssetsPanelEmpty_Title: string;
    Cancel: string;
    Close: string;
    Database: string;
    Delete: string;
    DeleteItems: string;
    Edit: string;
    EditFolderMappings: string;
    ExtractUploads: string;
    FileAlreadyExistsMessage: string;
    FileId: string;
    Files: string;
    FileUploadedMessage: string;
    FileUploadStoppedMessage: string;
    FolderNamePlaceholder: string;
    FolderNameRequiredMessage: string;
    FolderId: string;
    FolderParent: string;
    Folders: string;
    FolderTypeRequiredMessage: string;
    General: string;
    Name: string;
    Overwrite: string;
    Permissions: string;
    Save: string;
    Secure: string;
    Standard: string;
    Type: string;
    "UserHasNoPermissionToDeleteFolder.Error": string;
    "UserHasNoPermissionToDownloadError": string;
    FileUploadPanelMessage: string;
    AssetsPanelNoSearchResults: string;
    SearchInputPlaceholder: string;
    Search: string;
    UrlCopiedMessage: string;
    Created: string;
    Description: string;
    LastModified: string;
    Size: string;
    Title: string;
    URL: string;
    ItemNameRequiredMessage: string;
    "UserHasNoPermissionToReadFileProperties.Error": string;
    GenericErrorMessage: string;
    Items: string;
    ItemSavedMessage: string;
    FolderType: string;
    CreatedOnDate: string;
    ItemName: string;
    LastModifiedOnDate: string;
    FileSizeErrorMessage: string;
    "UserHasNoPermissionToDeleteFile.Error": string;
    "UserHasNoPermissionToManageFileProperties.Error": string;
    "UserHasNoPermissionToManageFolder.Error": string;
    NoFolderSelected: string;
    SearchFolder: string;
    DisabledForAnonymousUsersMessage: string;
    "UserHasNoPermissionToAddFolders.Error": string;
    "GroupIconCantBeDeleted.Error": string;
    InvalidExtensionMessage: string;
    Refresh: string;
    Sync: string;
    SyncThisFolder: string;
    SyncThisFolderAndSubfolders: string;
    ManageFolderTypes: string;
    AddFolderType: string;
    FolderProvider: string;
    FolderTypeDefinitions: string;
    EditFolderType: string;
    RemoveFolderType: string;
    RemoveFolderTypeDialogBody: string;
    RemoveFolderTypeDialogHeader: string;
    MappedPath: string;
    Move: string;
    MoveItems: string;
    NewLocation: string;
    SortField_ItemName: string;
    SortField_LastModifiedOnDate: string;
    SortField_Size: string;
    SortField_ParentFolder: string;
    SortField_CreatedOnDate: string;
    Sort: string;
    Yes: string;
    No: string;
    ConfirmDeleteMessage: string;
    ConfirmMoveMessage: string;
    SelectDestinationFolder: string;
    MovingItems: string;
    DeletingItems: string;
    UnlinkFolders: string;
    ConfirmUnlinkMessage: string;
    UnlinkingFolders: string;
    Unlink: string;
    CopyUrl: string;
    StatusBarMessage: string;
    Download: string;
    Upload: string;
};