export function getFileSize(fileSize: number): string {
  if (fileSize === undefined || fileSize === null) {
    return '';
  }

  if (fileSize < 1024) {
    return `${fileSize} B`;
  }

  if (fileSize < 1048576) {
    return `${Math.round(fileSize / 1024)} KB`;
  }

  return `${Math.round(fileSize / 1048576)} MB`;
}
