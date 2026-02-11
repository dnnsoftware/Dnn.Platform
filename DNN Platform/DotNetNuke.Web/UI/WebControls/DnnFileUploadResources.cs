// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Web.UI.WebControls
{
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime.Serialization;

    /// <summary>A data transfer object with information about resources for the file upload control.</summary>
    [DataContract]
    public class DnnFileUploadResources
    {
        /// <summary>Gets or sets the title.</summary>
        [DataMember(Name = "title")]
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        [SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields", Justification = "Breaking change")]
        public string Title;

        /// <summary>Gets or sets the decompress label.</summary>
        [DataMember(Name = "decompressLabel")]
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        [SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields", Justification = "Breaking change")]
        public string DecompressLabel;

        /// <summary>Gets or sets the upload to folder label.</summary>
        [DataMember(Name = "uploadToFolderLabel")]
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        [SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields", Justification = "Breaking change")]
        public string UploadToFolderLabel;

        /// <summary>Gets or sets the title of the drag and drop area.</summary>
        [DataMember(Name = "dragAndDropAreaTitle")]
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        [SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields", Justification = "Breaking change")]
        public string DragAndDropAreaTitle;

        /// <summary>Gets or sets the upload file method.</summary>
        [DataMember(Name = "uploadFileMethod")]
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        [SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields", Justification = "Breaking change")]
        public string UploadFileMethod;

        /// <summary>Gets or sets the upload from web method.</summary>
        [DataMember(Name = "uploadFromWebMethod")]
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        [SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields", Justification = "Breaking change")]
        public string UploadFromWebMethod;

        /// <summary>Gets or sets the close button text.</summary>
        [DataMember(Name = "closeButtonText")]
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        [SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields", Justification = "Breaking change")]
        public string CloseButtonText;

        /// <summary>Gets or sets the text of the upload from web button.</summary>
        [DataMember(Name = "uploadFromWebButtonText")]
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        [SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields", Justification = "Breaking change")]
        public string UploadFromWebButtonText;

        /// <summary>Gets or sets the decompressing file message.</summary>
        [DataMember(Name = "decompressingFile")]
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        [SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields", Justification = "Breaking change")]
        public string DecompressingFile;

        /// <summary>Gets or sets the file is too large message.</summary>
        [DataMember(Name = "fileIsTooLarge")]
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        [SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields", Justification = "Breaking change")]
        public string FileIsTooLarge;

        /// <summary>Gets or sets the file upload canceled message.</summary>
        [DataMember(Name = "fileUploadCancelled")]
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        [SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields", Justification = "Breaking change")]
        public string FileUploadCancelled;

        /// <summary>Gets or sets the file upload failed message.</summary>
        [DataMember(Name = "fileUploadFailed")]
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        [SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields", Justification = "Breaking change")]
        public string FileUploadFailed;

        /// <summary>Gets or sets the file uploaded message.</summary>
        [DataMember(Name = "fileUploaded")]
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        [SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields", Justification = "Breaking change")]
        public string FileUploaded;

        /// <summary>Gets or sets the empty file upload message.</summary>
        [DataMember(Name = "emptyFileUpload")]
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        [SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields", Justification = "Breaking change")]
        public string EmptyFileUpload;

        /// <summary>Gets or sets the file already exists message.</summary>
        [DataMember(Name = "fileAlreadyExists")]
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        [SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields", Justification = "Breaking change")]
        public string FileAlreadyExists;

        /// <summary>Gets or sets the upload stopped message.</summary>
        [DataMember(Name = "uploadStopped")]
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        [SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields", Justification = "Breaking change")]
        public string UploadStopped;

        /// <summary>Gets or sets the URL tooltip message.</summary>
        [DataMember(Name = "urlTooltip")]
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        [SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields", Justification = "Breaking change")]
        public string UrlTooltip;

        /// <summary>Gets or sets the text of the keep button.</summary>
        [DataMember(Name = "keepButtonText")]
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        [SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields", Justification = "Breaking change")]
        public string KeepButtonText;

        /// <summary>Gets or sets the text of the replace button.</summary>
        [DataMember(Name = "replaceButtonText")]
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        [SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields", Justification = "Breaking change")]
        public string ReplaceButtonText;

        /// <summary>Gets or sets the too many files message.</summary>
        [DataMember(Name = "tooManyFiles")]
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        [SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields", Justification = "Breaking change")]
        public string TooManyFiles;

        /// <summary>Gets or sets the invalid file extensions message.</summary>
        [DataMember(Name = "invalidFileExtensions")]
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        [SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields", Justification = "Breaking change")]
        public string InvalidFileExtensions;

        /// <summary>Gets or sets the title of the unzip file prompt.</summary>
        [DataMember(Name = "unzipFilePromptTitle")]
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        [SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields", Justification = "Breaking change")]
        public string UnzipFilePromptTitle;

        /// <summary>Gets or sets the body of the unzip failed prompt.</summary>
        [DataMember(Name = "unzipFileFailedPromptBody")]
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        [SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields", Justification = "Breaking change")]
        public string UnzipFileFailedPromptBody;

        /// <summary>Gets or sets the body of the unzip success prompt.</summary>
        [DataMember(Name = "unzipFileSuccessPromptBody")]
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        [SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields", Justification = "Breaking change")]
        public string UnzipFileSuccessPromptBody;

        /// <summary>Gets or sets the title of the error dialog.</summary>
        [DataMember(Name = "errorDialogTitle")]
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        [SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields", Justification = "Breaking change")]
        public string ErrorDialogTitle;
    }
}
