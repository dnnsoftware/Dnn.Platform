import { Version } from "./Version";

export interface UpgradeFile {
  version: Version;
  fileName: string;
  isObsolete: boolean;
}
