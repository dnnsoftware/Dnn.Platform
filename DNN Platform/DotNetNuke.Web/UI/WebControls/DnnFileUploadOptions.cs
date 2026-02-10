// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Web.UI.WebControls
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Runtime.Serialization;

    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Abstractions.Security;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Portals;

    using Microsoft.Extensions.DependencyInjection;

    /// <summary>A data transfer object with information about file upload options.</summary>
    [DataContract]
    public class DnnFileUploadOptions
    {
        /// <summary>Gets or sets the client ID.</summary>
        [DataMember(Name = "clientId")]
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        [SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields", Justification = "Breaking change")]
        public string ClientId;

        /// <summary>Gets or sets the module ID.</summary>
        [DataMember(Name = "moduleId")]
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        [SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields", Justification = "Breaking change")]
        public string ModuleId = string.Empty;

        /// <summary>Gets or sets the client ID of the parent.</summary>
        [DataMember(Name = "parentClientId")]
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        [SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields", Justification = "Breaking change")]
        public string ParentClientId;

        /// <summary>Gets or sets a value indicating whether to show on startup.</summary>
        [DataMember(Name = "showOnStartup")]
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        [SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields", Justification = "Breaking change")]
        public bool ShowOnStartup;

        /// <summary>Gets or sets the folder picker options.</summary>
        [DataMember(Name = "folderPicker")]
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        [SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields", Justification = "Breaking change")]
        public DnnDropDownListOptions FolderPicker;

        /// <summary>Gets or sets the max file size in bytes.</summary>
        [DataMember(Name = "maxFileSize")]
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        [SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields", Justification = "Breaking change")]
        public int MaxFileSize;

        /// <summary>Gets or sets the maximum number of files.</summary>
        [DataMember(Name = "maxFiles")]
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        [SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields", Justification = "Breaking change")]
        public int MaxFiles;

        /// <summary>Gets or sets the allowed extensions.</summary>
        [DataMember(Name = "extensions")]
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        [SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields", Justification = "Breaking change")]
        public List<string> Extensions;

        /// <summary>Gets or sets the resources.</summary>
        [DataMember(Name = "resources")]
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        [SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields", Justification = "Breaking change")]
        public DnnFileUploadResources Resources;

        /// <summary>Gets or sets the width in pixels.</summary>
        [DataMember(Name = "width")]
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        [SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields", Justification = "Breaking change")]
        public int Width;

        /// <summary>Gets or sets the height in pixels.</summary>
        [DataMember(Name = "height")]
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        [SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields", Justification = "Breaking change")]
        public int Height;

        /// <summary>Gets or sets the folder path.</summary>
        [DataMember(Name = "folderPath")]
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        [SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields", Justification = "Breaking change")]
        public string FolderPath;

        private const int DefaultWidth = 780;
        private const int DefaultHeight = 630;

        private readonly IHostSettings hostSettings;
        private readonly IApplicationStatusInfo appStatus;
        private readonly ICryptographyProvider cryptographyProvider;
        private Dictionary<string, string> parameters;

        /// <summary>Initializes a new instance of the <see cref="DnnFileUploadOptions"/> class.</summary>
        [Obsolete("Deprecated in DotNetNuke 10.2.2. Please use overload with IHostSettings. Scheduled removal in v12.0.0.")]
        public DnnFileUploadOptions()
            : this(null, null, null)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="DnnFileUploadOptions"/> class.</summary>
        /// <param name="hostSettings">The host settings.</param>
        [Obsolete("Deprecated in DotNetNuke 10.2.2. Please use overload with IApplicationStatusInfo. Scheduled removal in v12.0.0.")]
        public DnnFileUploadOptions(IHostSettings hostSettings)
            : this(hostSettings, null, null)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="DnnFileUploadOptions"/> class.</summary>
        /// <param name="hostSettings">The host settings.</param>
        /// <param name="appStatus">The application status.</param>
        /// <param name="cryptographyProvider">The cryptography provider.</param>
        public DnnFileUploadOptions(IHostSettings hostSettings, IApplicationStatusInfo appStatus, ICryptographyProvider cryptographyProvider)
        {
            this.hostSettings = hostSettings ?? Globals.GetCurrentServiceProvider().GetRequiredService<IHostSettings>();
            this.appStatus = appStatus ?? Globals.GetCurrentServiceProvider().GetRequiredService<IApplicationStatusInfo>();
            this.cryptographyProvider = cryptographyProvider ?? Globals.GetCurrentServiceProvider().GetRequiredService<ICryptographyProvider>();
            this.FolderPicker = new DnnDropDownListOptions();
            this.MaxFileSize = (int)Config.GetMaxUploadSize(this.appStatus);
            this.Extensions = [];
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
                FileIsTooLarge = string.Format(CultureInfo.CurrentCulture, Utilities.GetLocalizedString("FileUpload.FileIsTooLarge.Error") + " Mb", (this.MaxFileSize / (1024 * 1024)).ToString(CultureInfo.InvariantCulture)),
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

        /// <summary>Gets the parameters.</summary>
        [DataMember(Name = "parameters")]
        public Dictionary<string, string> Parameters => this.parameters ??= new Dictionary<string, string>();

        /// <summary>Gets the validation code.</summary>
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

                return ValidationUtils.ComputeValidationCode(this.cryptographyProvider, this.hostSettings, parameters);
            }
        }
    }
}
