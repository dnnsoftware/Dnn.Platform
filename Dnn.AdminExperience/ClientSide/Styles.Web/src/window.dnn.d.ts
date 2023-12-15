declare global {
    interface Window {
        dnn: IDnn;
    }
}

interface IDnn {
    initStyles: () => IInitStylesConfig;
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
    nav_Styles: string;
}

interface IServicesFramework {
    getServiceRoot(): string;
    moduleRoot: string;
    controller: string;
}