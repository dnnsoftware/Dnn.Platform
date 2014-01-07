#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2014
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

using DotNetNuke.Framework;

namespace DotNetNuke.Providers.FiftyOneClientCapabilityProvider
{
    using System;
    using System.Linq;
    using System.Web.UI.HtmlControls;
    using System.Web.UI.WebControls;
    using Components;
    using FiftyOne.Foundation.Mobile.Detection;
    using FiftyOne.Foundation.UI;
    using FiftyOne.Foundation.UI.Web;
/// <summary>
    /// Administration control is used as the main control off the hosts
    /// page to activate 51Degrees.mobi.
    /// </summary>
    partial class Administration : ModuleBase
    {
        /// <summary>
        ///  Records if premium data is in use when the control is first loaded.
        /// </summary>
        protected bool IsPremium = DataProvider.IsPremium || DataProvider.IsCms;

        /// <summary>
        /// Executes the page initialization event.
        /// </summary>
        /// <param name="e">The event arguments.</param>
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            SearchButton.Click += SearchButtonClick;
            HardwareList.ItemDataBound += ListItemDataBound;
            PremiumUpload.UploadComplete += UploadComplete;
            
            NoResultsMessage.Visible = false;
            PremiumUploadSuccess.Visible = false;
            PremiumUploadError.Visible = false;

            jQuery.RequestDnnPluginsRegistration();

            HardwareList.DataSource = DataProvider.HardwareProperties;
            SoftwareList.DataSource = DataProvider.SoftwareProperties;
            BrowserList.DataSource = DataProvider.BrowserProperties;
            ContentList.DataSource = DataProvider.ContentProperties;

            HardwareList.DataBind();
            SoftwareList.DataBind();
            BrowserList.DataBind();
            ContentList.DataBind();

            var refreshButtonText = LocalizeString("StatsRefreshButton.Text");
            var statsHtml = LocalizeString("StatsHtml.Text");
            PremiumStats.RefreshButtonText = refreshButtonText;
            PremiumStats.Html = statsHtml;
            PremiumUpload.UploadButtonText = LocalizeString("UploadData.Text");
            LiteStats.RefreshButtonText = refreshButtonText;
            LiteStats.Html = statsHtml;

            if (!IsPremium)
            {
                // lite
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
                Activate.UploadButtonText = LocalizeString("UploadButtonText.Text");
                Activate.UploadInstructionsHtml = LocalizeString("UploadInstructionsHtml.Text");
            }
            else
            {
                // premium
                DeviceExplorer.BackButtonDeviceText = LocalizeString("BackButtonDeviceText.Text");
                DeviceExplorer.BackButtonDevicesText = LocalizeString("BackButtonDevicesText.Text");
                DeviceExplorer.DeviceExplorerDeviceHtml = LocalizeString("DeviceExplorerDeviceInstructionsHtml.Text");
                DeviceExplorer.DeviceExplorerModelsHtml = LocalizeString("DeviceExplorerModelsInstructionsHtml.Text");
                DeviceExplorer.DeviceExplorerVendorsHtml = LocalizeString("DeviceExplorerVendorsHtml.Text");
            }

            if (IsPostBack)
                return;

            var vendor = Request.QueryString["Vendor"];
            var model = Request.QueryString["Model"];
            var deviceId = Request.QueryString["DeviceID"];
            var searchQuery = Request.QueryString["Query"];

            var hasVendor = !string.IsNullOrEmpty(vendor);
            var hasModel = !string.IsNullOrEmpty(model);
            var hasDeviceId = !string.IsNullOrEmpty(deviceId);
            var hasSearchQuery = !string.IsNullOrEmpty(searchQuery);

            if (hasVendor)
                DeviceExplorer.Vendor = vendor;

            if (hasModel)
                DeviceExplorer.Model = model;
            
            if (hasDeviceId)
                DeviceExplorer.DeviceID = deviceId;
            
            if (hasSearchQuery)
                SearchTextBox.Text = Server.UrlDecode(searchQuery);

            NoResultsMessage.Visible = hasSearchQuery && !hasDeviceId && !hasModel && !hasVendor;
        }

        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);
            PremiumUpload.Visible = DataProvider.IsPremium;
            purchaseBox.Visible = !DataProvider.IsPremium;
        }

        private void UploadComplete(object sender, ActivityResult e)
        {
            PremiumUploadError.Visible = !e.Success;
            PremiumUploadSuccess.Visible = e.Success;
        }

        private static void ListItemDataBound(object sender, RepeaterItemEventArgs e)
        {
            if (e.Item.DataItem == null || !(e.Item.DataItem is Property))
                return;

            var property = (Property)e.Item.DataItem;
            var premiumLabel = e.Item.FindControl("Premium") as HtmlGenericControl;
            if (premiumLabel != null)
            {
                premiumLabel.Visible = property.IsPremium;
            }

            var values = e.Item.FindControl("Values") as HtmlGenericControl;
            if (values == null)
                return;

            values.Visible = property.ShowValues;

            if (property.ShowValues)
            {
                values.InnerText = string.Join(", ", property.Values.Select(item => item.Name).ToArray());
            }
        }

        private void SearchButtonClick(object sender, EventArgs e)
        {

            string additionalParams = string.Empty;
            if (DataProvider.IsPremium)
            {
                var deviceList = DataProvider.FindDevices(this.SearchTextBox.Text);
                if (deviceList != null && deviceList.Count > 0)
                {
                    additionalParams = "DeviceID=" + deviceList.First().DeviceID;
                }
            }
            else
            {
                var device = DataProvider.Provider.GetDeviceInfo(this.SearchTextBox.Text);
                if (device != null)
                {
                    additionalParams = "DeviceID=" + device.DeviceId;
                }
            }
            
            var url = EditUrl(TabId, string.Empty, false, "Query=" + Server.UrlEncode(SearchTextBox.Text), additionalParams);
            Response.Redirect(url);
        }

		protected string GetLicenseFormatString(string key)
		{
			var content = LocalizeString(key);
			var licenseType = DataProvider.IsPremium ? LocalizeString("LicenseType_Premium.Text") 
				: (DataProvider.IsCms ? LocalizeString("LicenseType_CMS.Text") : LocalizeString("LicenseType_Lite.Text"));
			return string.Format(content, licenseType);
		}
    }
}

