import { Version } from "./Version";

export interface LocalUpgradeInfo {
  IsValid: boolean;
  IsOutdated: boolean;
  PackageName: string;
  Version: Version;
  MinDnnVersion: Version;
  CanInstall: Version;
}
