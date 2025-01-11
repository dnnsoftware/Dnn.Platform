import { DnnServicesFramework } from "@dnncommunity/dnn-elements";

export class LocalizationClient
{
    private sf: DnnServicesFramework;
    private requestUrl: string;

    constructor(moduleId: number) {
        this.sf = new DnnServicesFramework(moduleId);
        this.requestUrl = this.sf.getServiceRoot("BulkInstall") + "Localization/";
    }

    public async getResources(): Promise<BulkInstallLocalization>
    {
        const LocalizationStorageKey = "BulkInstall_Localization";
        const localization = sessionStorage.getItem(LocalizationStorageKey);
        if (localization){
            return JSON.parse(localization) as BulkInstallLocalization;
        }

        const response = await fetch(
            `${this.requestUrl}GetResources`,
            {
                headers: this.sf.getModuleHeaders(),
            });
        var vm = await response.json() as BulkInstallLocalization;
        sessionStorage.setItem(LocalizationStorageKey, JSON.stringify(vm));
        return vm;
    }
}

export interface BulkInstallLocalization {
    Action: string;
    ApiKey: string;
    ApiUsers: string;
    BulkInstall: string;
    BypassIpAllowList: string;
    Close: string;
    Create: string;
    Date: string;
    EncryptionKey: string;
    Events: string;
    Install: string;
    IpAddress: string;
    IPSafeList: string;
    Logs: string;
    Message: string;
    ApiUserNameText: string;
    NewApiUser: string;
    Type: string;
    ApiUserNameHelp: string;
    IPSafeListItemNameText: string;
    IPSafeListItemNameHelp: string;
    IPSafeListItemIpAddressText: string;
    IPSafeListItemIpAddressHelp: string;
    IPSafeListConfiguration: string;
    Add: string;
    EnableIpSafeList: string;
};