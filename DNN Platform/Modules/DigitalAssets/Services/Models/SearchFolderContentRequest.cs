namespace DotNetNuke.Modules.DigitalAssets.Services.Models
{
    public class SearchFolderContentRequest
    {
        public int FolderId { get; set; }

        public string Pattern { get; set; }

        public int StartIndex { get; set; }

        public int NumItems { get; set; }

        public string SortExpression { get; set; }
    }
}
