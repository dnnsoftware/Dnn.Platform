import { SessionStatusInfo } from '../../../enums/SessionStatus';

export enum UploadStatus {
  InProgress,
  Success,
  Error,
  Cancelled,
}

export interface Session {
  sessionGuid: string;
  status: SessionStatusInfo;
  response: InstallJob[];
  lastUsed: Date;
}

export interface InstallJob {
  name: string;
  packages: PackageJob[];
  failures: string[];
  attempted: boolean;
  success: boolean;
  canInstall: boolean;
}

export interface PackageJob {
  name: string;
  dependencies: PackageDependency[];
  version: string;
  canInstall: boolean;
}

export interface PackageDependency {
  isPackageDependency: boolean;
  packageName: string;
  dependencyVersion: string;
  isMet: boolean;
}
