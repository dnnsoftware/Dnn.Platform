namespace Dnn.Modules.ResourceManager.Services.Dto
{
    public class CreateNewFolderRequest
    {
        public string FolderName { get; set; }

        public int ParentFolderId { get; set; }

        public int FolderMappingId { get; set; }

        public string MappedName { get; set; }
    }
}