// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Web.UI.WebControls
{
    using System.Runtime.Serialization;

    [DataContract]
    public class DnnFileUploadResources
    {
        [DataMember(Name = "title")]
        public string Title;

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
}
