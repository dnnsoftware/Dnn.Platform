using System.Runtime.Serialization;
using DotNetNuke.Common.Utilities;

namespace DotNetNuke.Web.UI.WebControls
{

    [DataContract]
    public class DnnFileUploadResources
    {
        [DataMember(Name = "title")]
        public string Title;

        [DataMember(Name = "dialogHeader")]
        public string DialogHeader;

        [DataMember(Name = "decompressLabel")]
        public string DecompressLabel;

        [DataMember(Name = "uploadToFolderLabel")]
        public string UploadToFolderLabel;

        [DataMember(Name = "dragAndDropAreaTitle")]
        public string DragAndDropAreaTitle;

        [DataMember(Name = "uploadFileMethod")]
        public string UploadFileMethod;

        [DataMember(Name = "uploadFromWebMethod")]
        public string UploadFromWebMethod;

        [DataMember(Name = "closeButtonText")]
        public string CloseButtonText;

        [DataMember(Name = "uploadFromWebButtonText")]
        public string UploadFromWebButtonText;

        [DataMember(Name = "decompressingFile")]
        public string DecompressingFile;
    }

    [DataContract]
    public class DnnFileUploadOptions
    {

        [DataMember(Name = "folderPicker")]
        public DnnDropDownListOptions FolderPicker;

        [DataMember(Name = "maxFileSize")]
        public int MaxFileSize;

        [DataMember(Name = "resources")]
        public DnnFileUploadResources Resources;

        public DnnFileUploadOptions()
        {
            FolderPicker = new DnnDropDownListOptions();
            Resources = new DnnFileUploadResources
            {
                Title = Utilities.GetLocalizedString("FileUpload.Title.Text"),
                DialogHeader = Utilities.GetLocalizedString("FileUpload.DialogHeader.Text"),
                DecompressLabel = Utilities.GetLocalizedString("FileUpload.DecompressLabel.Text"),
                UploadToFolderLabel = Utilities.GetLocalizedString("FileUpload.UploadToFolderLabel.Text"),
                DragAndDropAreaTitle = Utilities.GetLocalizedString("FileUpload.DragAndDropAreaTitle.Text"),
                UploadFileMethod = Utilities.GetLocalizedString("FileUpload.UploadFileMethod.Text"),
                UploadFromWebMethod = Utilities.GetLocalizedString("FileUpload.UploadFromWebMethod.Text"),
                CloseButtonText = Utilities.GetLocalizedString("FileUpload.CloseButton.Text"),
                UploadFromWebButtonText = Utilities.GetLocalizedString("FileUpload.UploadFromWebButton.Text"),
                DecompressingFile = Utilities.GetLocalizedString("FileUpload.DecompressingFile.Text")
            };
            MaxFileSize = (int)Config.GetMaxUploadSize();
        }
    }
}
