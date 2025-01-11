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
    Add: string;
    ApiKey: string;
    ApiUserNameText: string;
    ApiUserNameHelp: string;
    ApiUsers: string;
    BulkInstall: string;
    BypassIpAllowList: string;
    Close: string;
    Create: string;
    Date: string;
    Delete: string;
    EnableIpSafeList: string;
    EncryptionKey: string;
    Events: string;
    Install: string;
    IPAddress: string;
    IPSafeList: string;
    IPSafeListConfiguration: string;
    IPSafeListEntries: any;
    IPSafeListItemNameText: string;
    IPSafeListItemNameHelp: string;
    IPSafeListItemIpAddressText: string;
    IPSafeListItemIpAddressHelp: string;
    Logs: string;
    Message: string;
    Name: string;
    NewApiUser: string;
    NewIpSafelistEntry: any;
    Save: any;
    Type: string;
};