namespace DotNetNuke.Modules.DigitalAssets.Services.Models
{
    public class GetFolderContentRequest
    {
        public int FolderId { get; set; }

        public int StartIndex { get; set; }

        public int NumItems { get; set; }

        public string SortExpression { get; set; }
    }
}
