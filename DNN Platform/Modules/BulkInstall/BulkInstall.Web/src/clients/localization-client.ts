import { DnnServicesFramework } from '@dnncommunity/dnn-elements';

export class LocalizationClient {
  private sf: DnnServicesFramework;
  private requestUrl: string;

  constructor(moduleId: number) {
    this.sf = new DnnServicesFramework(moduleId);
    this.requestUrl = this.sf.getServiceRoot('BulkInstall') + 'Localization/';
  }

  public async getResources(): Promise<BulkInstallLocalization> {
    const LocalizationStorageKey = 'BulkInstall_Localization';
    const localization = sessionStorage.getItem(LocalizationStorageKey);
    if (localization !== undefined && localization !== null && localization !== '') {
      return JSON.parse(localization) as BulkInstallLocalization;
    }

    const response = await fetch(`${this.requestUrl}GetResources`, {
      headers: this.sf.getModuleHeaders(),
    });
    const vm = (await response.json()) as BulkInstallLocalization;
    sessionStorage.setItem(LocalizationStorageKey, JSON.stringify(vm));
    return vm;
  }
}

export interface BulkInstallLocalization {
  Action: string;
  Add: string;
  ApiError: string;
  ApiKey: string;
  ApiUserNameText: string;
  ApiUserNameHelp: string;
  ApiUserExpiresOnText: string;
  ApiUserExpiresOnHelp: string;
  ApiUsers: string;
  BulkInstall: string;
  BypassIpAllowList: string;
  CannotInstall: string;
  Close: string;
  Create: string;
  Date: string;
  Delete: string;
  EnableIpSafeList: string;
  EncryptionKey: string;
  Events: string;
  EventLogSeverity_Info: string;
  EventLogSeverity_Warning: string;
  EventLogSeverity_Alert: string;
  EventLogSeverity_Critical: string;
  Install: string;
  InstallationComplete: string;
  InstallingPackages: string;
  IPAddress: string;
  IPSafeList: string;
  IPSafeListConfiguration: string;
  IPSafeListEntries: string;
  IPSafeListItemNameText: string;
  IPSafeListItemNameHelp: string;
  IPSafeListItemIpAddressText: string;
  IPSafeListItemIpAddressHelp: string;
  Logs: string;
  Message: string;
  Name: string;
  NewApiUser: string;
  NewIpSafelistEntry: string;
  Reset: string;
  Save: string;
  SessionStatus_NotStared: string;
  SessionStatus_InProgress: string;
  SessionStatus_Complete: string;
  Severity: string;
  Type: string;
  DropZone_DragAndDropFile: string;
  DropZone_Or: string;
  DropZone_UploadFile: string;
  DropZone_UploadSizeTooLarge: string;
  DropZone_FileSizeLimit: string;
  DropZone_InvalidExtension: string;
  DropZone_AllowedFileExtensions: string;
}
