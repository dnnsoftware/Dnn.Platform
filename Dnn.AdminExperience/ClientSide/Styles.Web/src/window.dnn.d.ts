declare global {
    interface IDnnWrapper {
        initStyles: () => IInitStylesConfig;
    }
}

export interface IInitStylesConfig {
    moduleName?: string;
    params?: IParams;
    utility?: IUtility;
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
}

interface IResx {
    Styles: IStylesResx;
}

interface IStylesResx {
    ActionColors: string;
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
    InformationColor: string;
    InformationColorHelp: string;
    ModuleDescription: string;
    nav_Styles: string;
    NeutralColor: string;
    NeutralColorHelp: string;
    PrimaryColor: string;
    PrimaryColorHelp: string;
    RestoreDefault: string;
    RestoreDefaultMessage: string;
    Save: string;
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
}

interface IServicesFramework {
    getServiceRoot(): string;
    moduleRoot: string;
    controller: string;
}