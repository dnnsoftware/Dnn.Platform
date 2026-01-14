export interface ChunkToUpload {
  chunk: Blob;
  start: number;
  totalSize: number;
  fileId: string;
}