export interface UploadFromLocalResponse {
  alreadyExists: boolean;
  fileIconUrl: string;
  fileId: number;
  fileName?: string;
  message?: string;
  orientation: number;
  path?: string;
  prompt?: string;
}
