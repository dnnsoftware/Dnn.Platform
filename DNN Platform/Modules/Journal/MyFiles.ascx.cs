﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

using System;

using DotNetNuke.Entities.Modules;
using DotNetNuke.Services.FileSystem;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Services.Localization;
using DotNetNuke.Entities.Host;

namespace DotNetNuke.Modules.Journal {
    public partial class MyFiles : PortalModuleBase {
        override protected void OnInit(EventArgs e) {
            this.InitializeComponent();
            base.OnInit(e);
        }

        private void InitializeComponent() {
            this.Load += new System.EventHandler(this.Page_Load);
            this.btnUp.Click += new System.EventHandler(this.btnUp_Upload);
        }
        protected void Page_Load(object sender, EventArgs e) {
      
        }
        protected void btnUp_Upload(object sender, EventArgs e) {
            var folderManager = FolderManager.Instance;
            var userFolder = folderManager.GetUserFolder(this.UserInfo);

            string message = string.Empty;
            IFileInfo fi = null;
            try {
                fi = FileManager.Instance.AddFile(userFolder, this.fileUp.PostedFile.FileName, this.fileUp.PostedFile.InputStream, true);
            } catch (PermissionsNotMetException) {
                message = string.Format(Localization.GetString("InsufficientFolderPermission"), userFolder.FolderPath);

            } catch (NoSpaceAvailableException) {
                message = string.Format(Localization.GetString("DiskSpaceExceeded"), this.fileUp.PostedFile.FileName);
            } catch (InvalidFileExtensionException) {
                message = string.Format(Localization.GetString("RestrictedFileType"), this.fileUp.PostedFile.FileName, Host.AllowedExtensionWhitelist.ToDisplayString());
            } catch {
                message = string.Format(Localization.GetString("SaveFileError"), this.fileUp.PostedFile.FileName);
            }
            if (String.IsNullOrEmpty(message) && fi != null) {
                this.litOut.Text = "<script type=\"text/javascript\">var fileInfo=" + JsonExtensionsWeb.ToJsonString(fi) + ";alert(fileInfo.FileName);</script>";
                
            } else {
                this.litOut.Text = message;
            }
        }
    }
}
