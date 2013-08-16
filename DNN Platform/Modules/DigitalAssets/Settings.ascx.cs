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
using System.Collections.Generic;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Services.Exceptions;
using DotNetNuke.Services.FileSystem;

namespace DotNetNuke.Modules.DigitalAssets
{
    public partial class Settings : ModuleSettingsBase
    {
        private const string DefaultFolderTypeIdSetting = "DefaultFolderTypeId";

        #region Base Method Implementations

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// LoadSettings loads the settings from the Database and displays them
        /// </summary>
        /// -----------------------------------------------------------------------------
        public override void LoadSettings()
        {
            if (Page.IsPostBack)
            {
                return;
            }

            try
            {
                DefaultFolderTypeComboBox.DataSource = new List<FolderMappingInfo>
                    {
                        FolderMappingController.Instance.GetFolderMapping(PortalId, "Standard"),
                        FolderMappingController.Instance.GetFolderMapping(PortalId, "Secure"),
                        FolderMappingController.Instance.GetFolderMapping(PortalId, "Database")
                    };

                DefaultFolderTypeComboBox.DataBind();

                if (Settings.Contains(DefaultFolderTypeIdSetting))
                {
                    DefaultFolderTypeComboBox.SelectedValue = Settings[DefaultFolderTypeIdSetting].ToString();
                }
            }
            catch (Exception exc)
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// UpdateSettings saves the modified settings to the Database
        /// </summary>
        /// -----------------------------------------------------------------------------
        public override void UpdateSettings()
        {
            try
            {
                var modules = new ModuleController();

                modules.UpdateModuleSetting(ModuleId, DefaultFolderTypeIdSetting, DefaultFolderTypeComboBox.SelectedValue);
            }
            catch (Exception exc)
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

        #endregion
    }
}