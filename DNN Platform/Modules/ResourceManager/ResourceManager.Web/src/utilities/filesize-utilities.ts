export function getFileSize(fileSize: number): string {
  if (fileSize == undefined || fileSize == undefined) {
    return "";
  }

  if (fileSize < 1024) {
    return fileSize.toString() + " B";
  }

  if (fileSize < 1048576) {
    return Math.round(fileSize / 1024).toString() + " KB";
  }

  return Math.round(fileSize / 1048576).toString() + " MB";
}
