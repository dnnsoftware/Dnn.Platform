// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Web.UI.WebControls;

using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;

[DataContract]
public class DnnFileUploadResources
{
    [DataMember(Name = "title")]
    [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
    public string Title;

    [DataMember(Name = "decompressLabel")]
    [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
    public string DecompressLabel;

    [DataMember(Name = "uploadToFolderLabel")]
    [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
    public string UploadToFolderLabel;

    [DataMember(Name = "dragAndDropAreaTitle")]
    [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
    public string DragAndDropAreaTitle;

    [DataMember(Name = "uploadFileMethod")]
    [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
    public string UploadFileMethod;

    [DataMember(Name = "uploadFromWebMethod")]
    [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
    public string UploadFromWebMethod;

    [DataMember(Name = "closeButtonText")]
    [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
    public string CloseButtonText;

    [DataMember(Name = "uploadFromWebButtonText")]
    [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
    public string UploadFromWebButtonText;

    [DataMember(Name = "decompressingFile")]
    [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
    public string DecompressingFile;

    [DataMember(Name = "fileIsTooLarge")]
    [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
    public string FileIsTooLarge;

    [DataMember(Name = "fileUploadCancelled")]
    [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
    public string FileUploadCancelled;

    [DataMember(Name = "fileUploadFailed")]
    [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
    public string FileUploadFailed;

    [DataMember(Name = "fileUploaded")]
    [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
    public string FileUploaded;

    [DataMember(Name = "emptyFileUpload")]
    [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
    public string EmptyFileUpload;

    [DataMember(Name = "fileAlreadyExists")]
    [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
    public string FileAlreadyExists;

    [DataMember(Name = "uploadStopped")]
    [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
    public string UploadStopped;

    [DataMember(Name = "urlTooltip")]
    [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
    public string UrlTooltip;

    [DataMember(Name = "keepButtonText")]
    [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
    public string KeepButtonText;

    [DataMember(Name = "replaceButtonText")]
    [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
    public string ReplaceButtonText;

    [DataMember(Name = "tooManyFiles")]
    [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
    public string TooManyFiles;

    [DataMember(Name = "invalidFileExtensions")]
    [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
    public string InvalidFileExtensions;

    [DataMember(Name = "unzipFilePromptTitle")]
    [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
    public string UnzipFilePromptTitle;

    [DataMember(Name = "unzipFileFailedPromptBody")]
    [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
    public string UnzipFileFailedPromptBody;

    [DataMember(Name = "unzipFileSuccessPromptBody")]
    [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
    public string UnzipFileSuccessPromptBody;

    [DataMember(Name = "errorDialogTitle")]
    [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
    public string ErrorDialogTitle;
}
