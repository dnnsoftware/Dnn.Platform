// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
#region Using Statements

using System;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Services.Exceptions;

#endregion

namespace _OWNER_._MODULE_
{

    public partial class Settings : ModuleSettingsBase
    {

        #region Base Method Implementations

        public override void LoadSettings()
        {
            try
            {
                if (!Page.IsPostBack)
                {
                    txtField.Text = (string)TabModuleSettings["field"];
                }
            }
            catch (Exception exc) // Module failed to load
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

        public override void UpdateSettings()
        {
            try
            {
                ModuleController.Instance.UpdateTabModuleSetting(TabModuleId, "field", txtField.Text);
            }
            catch (Exception exc) // Module failed to load
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

        #endregion

    }

}

