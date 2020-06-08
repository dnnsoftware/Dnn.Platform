// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
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

