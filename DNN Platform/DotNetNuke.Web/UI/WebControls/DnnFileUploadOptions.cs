using System.Collections.Generic;
using System.Globalization;
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

        [DataMember(Name = "fileIsTooLarge")]
        public string FileIsTooLarge;

        [DataMember(Name = "fileUploadCancelled")]
        public string FileUploadCancelled;

        [DataMember(Name = "fileUploadFailed")]
        public string FileUploadFailed;

        [DataMember(Name = "fileUploaded")]
        public string FileUploaded;

        [DataMember(Name = "emptyFileUpload")]
        public string EmptyFileUpload;

        [DataMember(Name = "fileAlreadyExists")]
        public string FileAlreadyExists;

        [DataMember(Name = "uploadStopped")]
        public string UploadStopped;

        [DataMember(Name = "urlTooltip")]
        public string UrlTooltip;

        [DataMember(Name = "keepButtonText")]
        public string KeepButtonText;

        [DataMember(Name = "replaceButtonText")]
        public string ReplaceButtonText;

        [DataMember(Name = "tooManyFiles")]
        public string TooManyFiles;

        [DataMember(Name = "invalidFileExtensions")]
        public string InvalidFileExtensions;

        [DataMember(Name = "unzipFilePromptTitle")]
        public string UnzipFilePromptTitle;

        [DataMember(Name = "unzipFileFailedPromptBody")]
        public string UnzipFileFailedPromptBody;

        [DataMember(Name = "unzipFileSuccessPromptBody")]
        public string UnzipFileSuccessPromptBody;

        [DataMember(Name = "errorDialogTitle")]
        public string ErrorDialogTitle;
    }

    [DataContract]
    public class DnnFileUploadOptions
    {
        private const int DefaultWidth = 780;
        private const int DefaultHeight = 630;
        [DataMember(Name = "clientId")]
        public string ClientId;

        [DataMember(Name = "moduleId")]
        public string ModuleId = string.Empty;

        [DataMember(Name = "parentClientId")]
        public string ParentClientId;

        [DataMember(Name = "showOnStartup")]
        public bool ShowOnStartup;

        [DataMember(Name = "folderPicker")]
        public DnnDropDownListOptions FolderPicker;

        [DataMember(Name = "maxFileSize")]
        public int MaxFileSize;

        [DataMember(Name = "maxFiles")]
        public int MaxFiles = 0;

        [DataMember(Name = "extensions")]
        public List<string> Extensions;

        [DataMember(Name = "resources")]
        public DnnFileUploadResources Resources;

        [DataMember(Name = "width")]
        public int Width;

        [DataMember(Name = "height")]
        public int Height;

        private Dictionary<string, string> _parameters;

        [DataMember(Name = "parameters")]
        public Dictionary<string, string> Parameters
        {
            get
            {
                return _parameters ?? (_parameters = new Dictionary<string, string>());
            }
        }

        [DataMember(Name = "folderPath")]
        public string FolderPath;

        public DnnFileUploadOptions()
        {
            FolderPicker = new DnnDropDownListOptions();
            MaxFileSize = (int)Config.GetMaxUploadSize();
            Extensions = new List<string>();
            Width = DefaultWidth;
            Height = DefaultHeight;
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
                DecompressingFile = Utilities.GetLocalizedString("FileUpload.DecompressingFile.Text"),
                FileIsTooLarge = string.Format(Utilities.GetLocalizedString("FileUpload.FileIsTooLarge.Error") + " Mb", (MaxFileSize / (1024 * 1024)).ToString(CultureInfo.InvariantCulture)),
                FileUploadCancelled = Utilities.GetLocalizedString("FileUpload.FileUploadCancelled.Error"),
                FileUploadFailed = Utilities.GetLocalizedString("FileUpload.FileUploadFailed.Error"),
                TooManyFiles = Utilities.GetLocalizedString("FileUpload.TooManyFiles.Error"),
                InvalidFileExtensions = Utilities.GetLocalizedString("FileUpload.InvalidFileExtensions.Error"),
                FileUploaded = Utilities.GetLocalizedString("FileUpload.FileUploaded.Text"),
                EmptyFileUpload = Utilities.GetLocalizedString("FileUpload.EmptyFileUpload.Error"),
                FileAlreadyExists = Utilities.GetLocalizedString("FileUpload.FileAlreadyExists.Error"),
                ErrorDialogTitle = Utilities.GetLocalizedString("FileUpload.ErrorDialogTitle.Text"),
                UploadStopped = Utilities.GetLocalizedString("FileUpload.UploadStopped.Text"),
                UrlTooltip = Utilities.GetLocalizedString("FileUpload.UrlTooltip.Text"),
                KeepButtonText = Utilities.GetLocalizedString("FileUpload.KeepButton.Text"),
                ReplaceButtonText = Utilities.GetLocalizedString("FileUpload.ReplaceButton.Text"),
                UnzipFilePromptTitle = Utilities.GetLocalizedString("FileUpload.UnzipFilePromptTitle.Text"),
                UnzipFileFailedPromptBody = Utilities.GetLocalizedString("FileUpload.UnzipFileFailedPromptBody.Text"),
                UnzipFileSuccessPromptBody = Utilities.GetLocalizedString("FileUpload.UnzipFileSuccessPromptBody.Text")
            };
        }
    }
}
