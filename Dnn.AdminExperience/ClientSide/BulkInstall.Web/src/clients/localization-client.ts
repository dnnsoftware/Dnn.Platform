import { PersonaBarServicesFramework } from './persona-bar-services-framework';

export class LocalizationClient {
  private sf: PersonaBarServicesFramework;

  constructor() {
    this.sf = new PersonaBarServicesFramework();
  }

  public getResources(): BulkInstallLocalization {
    const resx = (key: string) => this.sf.getResx(key);
    return {
      Action: resx('Action'),
      Add: resx('Add'),
      All: resx('All'),
      ApiError: resx('ApiError'),
      ApiAuthDisabled: resx('ApiAuthDisabled'),
      ApiKey: resx('ApiKey'),
      ApiUserNameText: resx('ApiUserNameText'),
      ApiUserNameHelp: resx('ApiUserNameHelp'),
      ApiUserExpiresOnText: resx('ApiUserExpiresOnText'),
      ApiUserExpiresOnHelp: resx('ApiUserExpiresOnHelp'),
      ApiUsers: resx('ApiUsers'),
      BulkInstall: resx('BulkInstall'),
      BypassIpAllowList: resx('BypassIpAllowList'),
      Cancel: resx('Cancel'),
      CannotInstall: resx('CannotInstall'),
      Close: resx('Close'),
      Copy: resx('Copy'),
      Create: resx('Create'),
      Date: resx('Date'),
      Delete: resx('Delete'),
      EnableIpSafeList: resx('EnableIpSafeList'),
      EncryptionKey: resx('EncryptionKey'),
      Events: resx('Events'),
      EventLogSeverity_Info: resx('EventLogSeverity_Info'),
      EventLogSeverity_Warning: resx('EventLogSeverity_Warning'),
      EventLogSeverity_Alert: resx('EventLogSeverity_Alert'),
      EventLogSeverity_Critical: resx('EventLogSeverity_Critical'),
      FileUploadedMessage: resx('FileUploadedMessage'),
      Install: resx('Install'),
      InstallationComplete: resx('InstallationComplete'),
      InstallingPackages: resx('InstallingPackages'),
      IPAddress: resx('IPAddress'),
      IPSafeList: resx('IPSafeList'),
      IPSafeListConfiguration: resx('IPSafeListConfiguration'),
      IPSafeListEntries: resx('IPSafeListEntries'),
      IPSafeListItemNameText: resx('IPSafeListItemNameText'),
      IPSafeListItemNameHelp: resx('IPSafeListItemNameHelp'),
      IPSafeListItemIpAddressText: resx('IPSafeListItemIpAddressText'),
      IPSafeListItemIpAddressHelp: resx('IPSafeListItemIpAddressHelp'),
      Logs: resx('Logs'),
      Message: resx('Message'),
      Name: resx('Name'),
      NewApiUser: resx('NewApiUser'),
      NewApiUserCredentialsTitle: resx('NewApiUserCredentialsTitle'),
      NewApiUserCredentialsWarning: resx('NewApiUserCredentialsWarning'),
      NewIpSafelistEntry: resx('NewIpSafelistEntry'),
      Packages: resx('Packages'),
      PackageDependencies: resx('PackageDependencies'),
      PlatformVersion: resx('PlatformVersion'),
      Reset: resx('Reset'),
      Save: resx('Save'),
      SessionStatus_NotStarted: resx('SessionStatus_NotStarted'),
      SessionStatus_InProgress: resx('SessionStatus_InProgress'),
      SessionStatus_Complete: resx('SessionStatus_Complete'),
      Severity: resx('Severity'),
      Type: resx('Type'),
      UploadInstallPackages: resx('UploadInstallPackages'),
      DropZone_DragAndDropFile: resx('DropZone_DragAndDropFile'),
      DropZone_Or: resx('DropZone_Or'),
      DropZone_UploadFile: resx('DropZone_UploadFile'),
      DropZone_UploadSizeTooLarge: resx('DropZone_UploadSizeTooLarge'),
      DropZone_FileSizeLimit: resx('DropZone_FileSizeLimit'),
      DropZone_InvalidExtension: resx('DropZone_InvalidExtension'),
      DropZone_AllowedFileExtensions: resx('DropZone_AllowedFileExtensions'),
      nav_BulkInstall: resx('nav_BulkInstall'),
    };
  }
}

export interface BulkInstallLocalization {
  [key: string]: string;

  Action: string;
  Add: string;
  All: string;
  ApiError: string;
  ApiAuthDisabled: string;
  ApiKey: string;
  ApiUserNameText: string;
  ApiUserNameHelp: string;
  ApiUserExpiresOnText: string;
  ApiUserExpiresOnHelp: string;
  ApiUsers: string;
  BulkInstall: string;
  BypassIpAllowList: string;
  Cancel: string;
  CannotInstall: string;
  Close: string;
  Copy: string;
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
  FileUploadedMessage: string;
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
  NewApiUserCredentialsTitle: string;
  NewApiUserCredentialsWarning: string;
  NewIpSafelistEntry: string;
  Packages: string;
  PackageDependencies: string;
  PlatformVersion: string;
  Reset: string;
  Save: string;
  SessionStatus_NotStarted: string;
  SessionStatus_InProgress: string;
  SessionStatus_Complete: string;
  Severity: string;
  Type: string;
  UploadInstallPackages: string;
  DropZone_DragAndDropFile: string;
  DropZone_Or: string;
  DropZone_UploadFile: string;
  DropZone_UploadSizeTooLarge: string;
  DropZone_FileSizeLimit: string;
  DropZone_InvalidExtension: string;
  DropZone_AllowedFileExtensions: string;
  nav_BulkInstall: string;
}
