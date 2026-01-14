declare global {
    interface IDnnWrapper {
        initStyles: () => IInitStylesConfig;
    }
}

export interface IInitStylesConfig {
    moduleName: string;
    params: IParams;
    utility: IUtility;
}

interface IParams {
    folderName?: string;
    identifier?: string;
    moduleName?: string;
    path?: string;
    query?: string;
    settings?: ISettings;
}

interface ISettings {
    isAdmin?: boolean;
    isHost?: boolean;
    permissions?: IPermissionsDictionary;
}

interface IPermissionsDictionary{
    [key: string]: string;
}

interface IUtility {
    resx: IResx;
    sf: IServicesFramework;
    notify: (message: string, options?: INotifyOptions) => void;
    notifyError: (message: string, options?: INotifyOptions) => void;
}

interface IResx {
    Styles: IStylesResx;
}

interface IStylesResx {
    ActionColors: string;
    AllowAdminEdits: string;
    AllowAdminEditsHelp: string;
    BackgroundColor: string;
    BackgroundColorHelp: string;
    BaseFontSize: string;
    BaseFontSizeHelp: string;
    BrandColors: string;
    Colors: string;
    ColorVariationOpacity: string;
    ColorVariationOpacityHelp: string;
    Contrast: string;
    Controls: string;
    ControlsPadding: string;
    ControlsPaddingHelp: string;
    ControlsRadius: string;
    ControlsRadiusHelp: string;
    DangerColor: string;
    DangerColorHelp: string;
    ForegroundColor: string;
    ForegroundColorHelp: string;
    GeneralColors: string;
    GetStylesError: string;
    InformationColor: string;
    InformationColorHelp: string;
    ModuleDescription: string;
    nav_Styles: string;
    NeutralColor: string;
    NeutralColorHelp: string;
    No: string;
    Permissions: string;
    PrimaryColor: string;
    PrimaryColorHelp: string;
    Reset: string;
    RestoreDefault: string;
    RestoreDefaultMessage: string;
    Save: string;
    SaveError: string;
    SaveSuccess: string;
    SecondaryColor: string;
    SecondaryColorHelp: string;
    SuccessColor: string;
    SuccessColorHelp: string;
    SurfaceColor: string;
    SurfaceColorHelp: string;
    TertiaryColor: string;
    TertiaryColorHelp: string;
    Typography: string;
    WarningColor: string;
    WarningColorHelp: string;
    Yes: string;
}

interface IServicesFramework {
    antiForgeryToken: string;
    getServiceRoot(): string;
    moduleRoot: string;
    controller: string;
}

interface INotifyOptions {
    timeout?: number;
    clickToClose?: boolean;
    closeButtonText?: string;
    type?: "notify" | "error";
}