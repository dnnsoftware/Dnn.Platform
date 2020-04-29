namespace Dnn.Modules.ResourceManager.Services.Dto
{
    public class ThumbnailDownloadRequest
    {
        public int FileId { get; set; }

        public int Width { get; set; }

        public int Height { get; set; }

        public int? Version { get; set; }
    }
}