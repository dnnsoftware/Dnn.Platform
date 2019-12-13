namespace DotNetNuke.Modules.DigitalAssets.Services.Models
{
    public class UnzipFileRequest
    {
        public int FileId { get; set; }

        public bool Overwrite { get; set; }        
    }
}
