// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Modules.DigitalAssets
{
    using System;
    using System.IO;
    using System.Text.RegularExpressions;

    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Framework;
    using DotNetNuke.Framework.JavaScriptLibraries;
    using DotNetNuke.Modules.DigitalAssets.Components.Controllers;
    using DotNetNuke.Modules.DigitalAssets.Components.Controllers.Models;
    using DotNetNuke.Modules.DigitalAssets.Components.ExtensionPoint;
    using DotNetNuke.Services.FileSystem;

    public partial class FileFieldsControl : PortalModuleBase, IFieldsControl
    {
        public IDigitalAssetsController Controller { get; private set; }

        public ItemViewModel Item { get; private set; }

        protected IFileInfo File { get; private set; }

        public virtual void PrepareProperties()
        {
            this.FileNameInput.Text = this.Item.ItemName;
            this.FileNameInvalidCharactersValidator.ValidationExpression = "^([^" + Regex.Escape(this.Controller.GetInvalidChars()) + "]+)$";
            this.FileNameInvalidCharactersValidator.ErrorMessage = this.Controller.GetInvalidCharsErrorText();
        }

        public void SetController(IDigitalAssetsController damController)
        {
            this.Controller = damController;
        }

        public void SetModuleConfiguration(ModuleInfo moduleConfiguration)
        {
            this.ModuleConfiguration = moduleConfiguration;
        }

        public void SetItemViewModel(ItemViewModel itemViewModel)
        {
            this.Item = itemViewModel;
        }

        public virtual void SetPropertiesAvailability(bool availability)
        {
            this.FileNameInput.Enabled = availability;
            this.FileAttributeArchiveCheckBox.Enabled = availability;
            this.FileAttributeHiddenCheckBox.Enabled = availability;
            this.FileAttributeReadonlyCheckBox.Enabled = availability;
            this.FileAttributeSystemCheckBox.Enabled = availability;
        }

        public virtual void SetPropertiesVisibility(bool visibility)
        {
            this.FileNameInput.Visible = visibility;
            this.FileAttributesContainer.Visible = visibility;
        }

        public void SetFileInfo(IFileInfo fileInfo)
        {
            this.File = fileInfo;
        }

        public virtual object SaveProperties()
        {
            this.Controller.RenameFile(this.Item.ItemID, this.FileNameInput.Text);
            if (this.File.SupportsFileAttributes)
            {
                this.File = FileManager.Instance.GetFile(this.Item.ItemID, true);
                FileManager.Instance.SetAttributes(this.File, this.GetFileAttributesUpdated(this.File.FileAttributes));
            }

            return this.File;
        }

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            JavaScript.RequestRegistration(CommonJs.DnnPlugins);
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            if (!this.Page.IsPostBack)
            {
                this.PrepareProperties();
                this.FileAttributesContainer.Visible = this.File.SupportsFileAttributes;
                if (this.File.SupportsFileAttributes)
                {
                    this.PrepareFileAttributes();
                }
            }
        }

        private void PrepareFileAttributes()
        {
            this.FileAttributeArchiveCheckBox.Checked = (this.File.FileAttributes & FileAttributes.Archive) == FileAttributes.Archive;
            this.FileAttributeHiddenCheckBox.Checked = (this.File.FileAttributes & FileAttributes.Hidden) == FileAttributes.Hidden;
            this.FileAttributeReadonlyCheckBox.Checked = (this.File.FileAttributes & FileAttributes.ReadOnly) == FileAttributes.ReadOnly;
            this.FileAttributeSystemCheckBox.Checked = (this.File.FileAttributes & FileAttributes.System) == FileAttributes.System;
        }

        private FileAttributes GetFileAttributesUpdated(FileAttributes? attributes)
        {
            var result = this.FileAttributeArchiveCheckBox.Checked ? (attributes | FileAttributes.Archive) : (attributes & ~FileAttributes.Archive);
            result = this.FileAttributeHiddenCheckBox.Checked ? (result | FileAttributes.Hidden) : (result & ~FileAttributes.Hidden);
            result = this.FileAttributeReadonlyCheckBox.Checked ? (result | FileAttributes.ReadOnly) : (result & ~FileAttributes.ReadOnly);
            result = this.FileAttributeSystemCheckBox.Checked ? (result | FileAttributes.System) : (result & ~FileAttributes.System);

            return result.Value;
        }
    }
}
