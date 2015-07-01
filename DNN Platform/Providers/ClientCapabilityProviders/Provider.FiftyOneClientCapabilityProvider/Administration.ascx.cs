#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2015
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

namespace DotNetNuke.Providers.FiftyOneClientCapabilityProvider
{
    using System;
    using System.Web.UI.WebControls;
    using FiftyOne.Foundation.Mobile.Detection;
    using FiftyOne.Foundation.UI.Web;
    using System.Collections.Generic;

    /// <summary>
    /// Administration control is used as the main control off the hosts
    /// page to activate 51Degrees.
    /// </summary>
    partial class Administration : DotNetNuke.Entities.Modules.PortalModuleBase
    {
        /// <summary>
        /// Returns true if the data set being used for detection is a premium version.
        /// </summary>
        protected bool IsPremium
        {
            get
            {
                return FiftyOne.Foundation.Mobile.Detection.Configuration.Manager.Enabled &&
                    WebProvider.ActiveProvider != null &&
                    "Lite".Equals(WebProvider.ActiveProvider.DataSet.Name) == false;
            }
        }

        /// <summary>
        /// Used to create a new DNN URL for a get request from the device explorer.
        /// </summary>
        /// <param name="parameters">List of parameters to include in the URL</param>
        /// <returns></returns>
        private string CreateUrl(List<string> parameters)
        {
            return DotNetNuke.Common.Globals.NavigateURL(
                base.TabId,
                "",
                parameters.ToArray());
        }

        /// <summary>
        /// Executes the page initialization event.
        /// </summary>
        /// <param name="e">The event arguments.</param>
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            Upload.UploadButtonText = LocalizeString("UploadData.Text");
            Upload.UploadComplete += UploadComplete;
            UploadSuccess.Visible = false;
            UploadError.Visible = false;
            SettingsChangedError.Visible = false;
            SettingsChangedSuccess.Visible = false;

            ButtonSettingsRefresh.Command += ButtonSettingsRefresh_Command;
            ButtonSettingsRefresh.Enabled = IsPremium;

            if (!IsPremium)
            {
                // Lite data is being used so we offer an option to upgrade to premium.
                DeviceBrowser.Visible = false;
                Activate.ActivateButtonText = LocalizeString("ActivateButtonText.Text");
                Activate.ActivatedMessageHtml = LocalizeString("ActivatedMessageHtml.Text");
                Activate.ActivateInstructionsHtml = LocalizeString("ActivateInstructionsHtml.Text");
                Activate.ActivationDataInvalidHtml = LocalizeString("ActivationDataInvalidHtml.Text");
                Activate.ActivationFailureCouldNotUpdateConfigHtml = LocalizeString("ActivationFailureCouldNotUpdateConfigHtml.Text");
                Activate.ActivationFailureCouldNotWriteDataFileHtml = LocalizeString("ActivationFailureCouldNotWriteDataFileHtml.Text");
                Activate.ActivationFailureCouldNotWriteLicenceFileHtml = LocalizeString("ActivationFailureCouldNotWriteLicenceFileHtml.Text");
                Activate.ActivationFailureGenericHtml = LocalizeString("ActivationFailureGenericHtml.Text");
                Activate.ActivationFailureHttpHtml = LocalizeString("ActivationFailureHttpHtml.Text");
                Activate.ActivationFailureInvalidHtml = LocalizeString("ActivationFailureInvalidHtml.Text");
                Activate.ActivationStreamFailureHtml = LocalizeString("ActivationStreamFailureHtml.Text");
                Activate.ActivationSuccessHtml = LocalizeString("ActivationSuccessHtml.Text");
                Activate.ValidationFileErrorText = LocalizeString("ValidationFileErrorText.Text");
                Activate.ValidationRequiredErrorText = LocalizeString("ValidationRequiredErrorText.Text");
                Activate.ValidationRegExErrorText = LocalizeString("ValidationRegExErrorText.Text");
                Activate.RefreshButtonText = LocalizeString("RefreshButtonText.Text");
            }
            else
            {
                // Premium data is being used so we'll configure the device browser.
                DeviceBrowser.Visible = true;
                DeviceBrowser.BackButtonDeviceText = LocalizeString("BackButtonDeviceText.Text");
                DeviceBrowser.BackButtonDevicesText = LocalizeString("BackButtonDevicesText.Text");
                DeviceBrowser.DeviceExplorerDeviceHtml = LocalizeString("DeviceExplorerDeviceInstructionsHtml.Text");
                DeviceBrowser.DeviceExplorerModelsHtml = LocalizeString("DeviceExplorerModelsInstructionsHtml.Text");
                DeviceBrowser.DeviceExplorerVendorsHtml = LocalizeString("DeviceExplorerVendorsHtml.Text");
                DeviceBrowser.SearchButtonText = LocalizeString("SearchButton.Text");
                DeviceBrowser.CreateUrl += CreateUrl;
            }

            if (IsPostBack)
                return;
        }

        /// <summary>
        /// Clears the current active provider and forces the device data to be
        /// reloaded from source.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void ButtonSettingsRefresh_Command(object sender, CommandEventArgs e)
        {
            if (WebProvider.ActiveProvider != null)
            {
                WebProvider.Download();
            }
        }

        /// <summary>
        /// Updates the 51Degrees configuration file if the configuration settings have been changed.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            if (IsPostBack)
            {
                try
                {
                    FiftyOne.Foundation.Mobile.Detection.Configuration.Manager.Enabled = CheckBoxEnabled.Checked;
                    FiftyOne.Foundation.Mobile.Detection.Configuration.Manager.AutoUpdate = CheckBoxAutoUpdate.Checked;
                    FiftyOne.Foundation.Mobile.Detection.Configuration.Manager.ShareUsage = CheckBoxShareUsage.Checked;
                    FiftyOne.Foundation.Mobile.Detection.Configuration.Manager.ImageOptimiserEnabled = CheckBoxImageOptimiser.Checked;
                    FiftyOne.Foundation.Mobile.Detection.Configuration.Manager.MemoryMode = CheckBoxFileMode.Checked == false;
                    SettingsChangedSuccess.Visible = true;
                }
                catch
                {
                    SettingsChangedError.Visible = true;
                }
            }
            CheckBoxEnabled.Checked = FiftyOne.Foundation.Mobile.Detection.Configuration.Manager.Enabled;
            CheckBoxAutoUpdate.Checked = FiftyOne.Foundation.Mobile.Detection.Configuration.Manager.AutoUpdate;
            CheckBoxShareUsage.Checked = FiftyOne.Foundation.Mobile.Detection.Configuration.Manager.ShareUsage;
            CheckBoxImageOptimiser.Checked = FiftyOne.Foundation.Mobile.Detection.Configuration.Manager.ImageOptimiserEnabled;
            CheckBoxFileMode.Checked = FiftyOne.Foundation.Mobile.Detection.Configuration.Manager.MemoryMode == false;
        }

        private void UploadComplete(object sender, ActivityResult e)
        {
            UploadError.Visible = !e.Success;
            UploadSuccess.Visible = e.Success;

            // Enable all the device detection options if not already set if it worked.
            if (e.Success)
            {
                FiftyOne.Foundation.Mobile.Detection.Configuration.Manager.Enabled = true;
                FiftyOne.Foundation.Mobile.Detection.Configuration.Manager.AutoUpdate = true;
                FiftyOne.Foundation.Mobile.Detection.Configuration.Manager.ShareUsage = true;
                FiftyOne.Foundation.Mobile.Detection.Configuration.Manager.ImageOptimiserEnabled = true;
                FiftyOne.Foundation.Mobile.Detection.Configuration.Manager.MemoryMode = false;
            }
        }

        protected string GetLicenseFormatString(string key)
        {
            var content = LocalizeString(key);
            var dataSetName = FiftyOne.Foundation.Mobile.Detection.Configuration.Manager.Enabled && 
                WebProvider.ActiveProvider != null ? WebProvider.ActiveProvider.DataSet.Name : LocalizeString("LicenseType_Lite.Text");
            return string.Format(content, dataSetName);
        }
    }
}