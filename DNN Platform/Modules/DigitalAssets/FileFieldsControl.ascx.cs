#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2013
// by DotNetNuke Corporation
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated 
// documentation files (the "Software"), to deal in the Software without restriction, including without limitation 
// the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and 
// to permit persons to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or substantial portions 
// of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED 
// TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL 
// THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF 
// CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
// DEALINGS IN THE SOFTWARE.
#endregion

using System;
using System.IO;
using System.Text.RegularExpressions;

using DotNetNuke.Entities.Modules;
using DotNetNuke.Framework;
using DotNetNuke.Modules.DigitalAssets.Components.Controllers;
using DotNetNuke.Modules.DigitalAssets.Components.Controllers.Models;
using DotNetNuke.Modules.DigitalAssets.Components.ExtensionPoint;
using DotNetNuke.Services.FileSystem;

namespace DotNetNuke.Modules.DigitalAssets
{
    public partial class FileFieldsControl : PortalModuleBase, IFieldsControl
    {
        public virtual void PrepareProperties()
        {
            FileNameInput.Text = Item.ItemName;
            FileNameInvalidCharactersValidator.ValidationExpression = "^([^" + Regex.Escape(Controller.GetInvalidChars()) + "]+)$";
            FileNameInvalidCharactersValidator.ErrorMessage = Controller.GetInvalidCharsErrorText();
        }

        private void PrepareFileAttributes()
        {
            FileAttributeArchiveCheckBox.Checked = (File.FileAttributes & FileAttributes.Archive) == FileAttributes.Archive;
            FileAttributeHiddenCheckBox.Checked = (File.FileAttributes & FileAttributes.Hidden) == FileAttributes.Hidden;
            FileAttributeReadonlyCheckBox.Checked = (File.FileAttributes & FileAttributes.ReadOnly) == FileAttributes.ReadOnly;
            FileAttributeSystemCheckBox.Checked = (File.FileAttributes & FileAttributes.System) == FileAttributes.System;
        }

        private FileAttributes GetFileAttributesUpdated(FileAttributes? attributes)
        {
            var result = FileAttributeArchiveCheckBox.Checked ? (attributes | FileAttributes.Archive) : (attributes & ~FileAttributes.Archive);
            result = FileAttributeHiddenCheckBox.Checked ? (result | FileAttributes.Hidden) : (result & ~FileAttributes.Hidden);
            result = FileAttributeReadonlyCheckBox.Checked ? (result | FileAttributes.ReadOnly) : (result & ~FileAttributes.ReadOnly);
            result = FileAttributeSystemCheckBox.Checked ? (result | FileAttributes.System) : (result & ~FileAttributes.System);

            return result.Value;
        }

        public IDigitalAssetsController Controller { get; private set; }

        public ItemViewModel Item { get; private set; }

        protected IFileInfo File { get; private set; }

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            jQuery.RequestDnnPluginsRegistration();
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            if (!Page.IsPostBack)
            {                
                PrepareProperties();
                FileAttributesContainer.Visible = File.SupportsFileAttributes;
                if (File.SupportsFileAttributes)
                {
                    PrepareFileAttributes();
                }                
            }
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
            FileNameInput.Enabled = availability;
            FileAttributeArchiveCheckBox.Enabled = availability;
            FileAttributeHiddenCheckBox.Enabled = availability;
            FileAttributeReadonlyCheckBox.Enabled = availability;
            FileAttributeSystemCheckBox.Enabled = availability;
        }

        public virtual void SetPropertiesVisibility(bool visibility)
        {
            FileNameInput.Visible = visibility;
            FileAttributesContainer.Visible = visibility;
        }

        public void SetFileInfo(IFileInfo fileInfo)
        {
            File = fileInfo;
        }

        public virtual object SaveProperties()
        {
            Controller.RenameFile(Item.ItemID, FileNameInput.Text);
            if (File.SupportsFileAttributes)
            {
                File = FileManager.Instance.GetFile(Item.ItemID, true);
                FileManager.Instance.SetAttributes(File, GetFileAttributesUpdated(File.FileAttributes));
            }

            return File;
        }
    }
}