// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Web.UI.WebControls
{
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Runtime.Serialization;

    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Portals;

    using Microsoft.Extensions.DependencyInjection;

    [DataContract]
    public class DnnFileUploadOptions
    {
        [DataMember(Name = "clientId")]
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        public string ClientId;

        [DataMember(Name = "moduleId")]
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        public string ModuleId = string.Empty;

        [DataMember(Name = "parentClientId")]
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        public string ParentClientId;

        [DataMember(Name = "showOnStartup")]
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        public bool ShowOnStartup;

        [DataMember(Name = "folderPicker")]
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        public DnnDropDownListOptions FolderPicker;

        [DataMember(Name = "maxFileSize")]
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        public int MaxFileSize;

        [DataMember(Name = "maxFiles")]
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        public int MaxFiles;

        [DataMember(Name = "extensions")]
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        public List<string> Extensions;

        [DataMember(Name = "resources")]
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        public DnnFileUploadResources Resources;

        [DataMember(Name = "width")]
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        public int Width;

        [DataMember(Name = "height")]
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        public int Height;

        [DataMember(Name = "folderPath")]
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        public string FolderPath;

        private const int DefaultWidth = 780;
        private const int DefaultHeight = 630;

        private readonly IHostSettings hostSettings;
        private Dictionary<string, string> parameters;

        public DnnFileUploadOptions()
            : this(null)
        {
        }

        public DnnFileUploadOptions(IHostSettings hostSettings)
        {
            this.hostSettings = hostSettings ?? Globals.GetCurrentServiceProvider().GetRequiredService<IHostSettings>();
            this.FolderPicker = new DnnDropDownListOptions();
            this.MaxFileSize = (int)Config.GetMaxUploadSize();
            this.Extensions = new List<string>();
            this.Width = DefaultWidth;
            this.Height = DefaultHeight;
            this.Resources = new DnnFileUploadResources
            {
                Title = Utilities.GetLocalizedString("FileUpload.Title.Text"),
                DecompressLabel = Utilities.GetLocalizedString("FileUpload.DecompressLabel.Text"),
                UploadToFolderLabel = Utilities.GetLocalizedString("FileUpload.UploadToFolderLabel.Text"),
                DragAndDropAreaTitle = Utilities.GetLocalizedString("FileUpload.DragAndDropAreaTitle.Text"),
                UploadFileMethod = Utilities.GetLocalizedString("FileUpload.UploadFileMethod.Text"),
                UploadFromWebMethod = Utilities.GetLocalizedString("FileUpload.UploadFromWebMethod.Text"),
                CloseButtonText = Utilities.GetLocalizedString("FileUpload.CloseButton.Text"),
                UploadFromWebButtonText = Utilities.GetLocalizedString("FileUpload.UploadFromWebButton.Text"),
                DecompressingFile = Utilities.GetLocalizedString("FileUpload.DecompressingFile.Text"),
                FileIsTooLarge = string.Format(Utilities.GetLocalizedString("FileUpload.FileIsTooLarge.Error") + " Mb", (this.MaxFileSize / (1024 * 1024)).ToString(CultureInfo.InvariantCulture)),
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
                UnzipFileSuccessPromptBody = Utilities.GetLocalizedString("FileUpload.UnzipFileSuccessPromptBody.Text"),
            };
        }

        [DataMember(Name = "parameters")]
        public Dictionary<string, string> Parameters
        {
            get
            {
                return this.parameters ?? (this.parameters = new Dictionary<string, string>());
            }
        }

        [DataMember(Name = "validationCode")]
        public string ValidationCode
        {
            get
            {
                var portalSettings = PortalSettings.Current;
                var parameters = new List<object>() { this.Extensions };
                if (portalSettings != null)
                {
                    parameters.Add(portalSettings.UserInfo.UserID);
                    if (!portalSettings.UserInfo.IsSuperUser)
                    {
                        parameters.Add(portalSettings.PortalId);
                    }
                }

                return ValidationUtils.ComputeValidationCode(this.hostSettings, parameters);
            }
        }
    }
}
