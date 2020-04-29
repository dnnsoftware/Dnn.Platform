using System.Runtime.Serialization;

namespace Dnn.Modules.ResourceManager.Services.Dto
{
    public class FolderDetailsRequest
    {
        [DataMember(Name = "folderId")]
        public int FolderId { get; set; }

        [DataMember(Name = "folderName")]
        public string FolderName { get; set; }

    }
}