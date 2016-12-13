#region Copyright
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2017
// by DotNetNuke Corporation
// All Rights Reserved
#endregion
using System;
using System.Linq;
using DotNetNuke.Common;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Tabs;
using DotNetNuke.Modules.MobileManagement.Views;
using DotNetNuke.Services.Exceptions;
using DotNetNuke.Services.Localization;
using DotNetNuke.Services.Mobile;
using DotNetNuke.UI.Skins.Controls;
using DotNetNuke.Web.Mvp;

namespace DotNetNuke.Modules.MobileManagement
{
    public partial class SimpleSettingsView : ModuleView, ISimpleSettingsView
    {
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            lnkCancel.Click += lnkCancel_OnClick;
            lnkSave.Click += lnkSave_OnClick;
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            lblHomePage.Text = TabController.Instance.GetTab(ModuleContext.PortalSettings.HomeTabId, ModuleContext.PortalId, false).TabName;
            
            if (!IsPostBack) BindSettingControls();
        }

        #region "Event Handlers"
        private void lnkSave_OnClick(object sender, EventArgs e)
        {
            // Checks for duplicate names
            var name = txtRedirectName.Text;
            var nameCount = new RedirectionController().GetRedirectionsByPortal(ModuleContext.PortalId).Count(r => r.Name.ToLower() == name.ToLower());
            if (nameCount < 1)
            {
                SaveRedirection();
                Response.Redirect(Globals.NavigateURL(""), true);
            }
            else
            {
                UI.Skins.Skin.AddModuleMessage(this, Localization.GetString("DuplicateNameError.Text", LocalResourceFile), ModuleMessage.ModuleMessageType.RedError);
            }          
        }

        private void lnkCancel_OnClick(object sender, EventArgs e)
        {
            try
            {
                Response.Redirect(ModuleContext.NavigateUrl(ModuleContext.TabId, "", true), true);
            }
            catch (Exception exc)
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

        #endregion


        private void BindSettingControls()
        {
			// Toggle fields if a redirect already exists for the Portal Home Page
			var defaultRedirect = HomePageRedirectExists();

            // Populating Portals dropdown
            var portals = PortalController.Instance.GetPortals().Cast<PortalInfo>().Where(p => p.PortalID != ModuleContext.PortalId).ToList();
            if (portals.Count > 0)
            {
                cboPortal.DataSource = portals;
                cboPortal.DataTextField = "PortalName";
                cboPortal.DataValueField = "PortalID";
                cboPortal.DataBind();
            }
            else
            {
                optRedirectTarget.Items[0].Enabled = false;
                optRedirectTarget.Items[0].Selected = false;
                optRedirectTarget.Items[1].Selected = true;
            }

            cboSourcePage.Visible = defaultRedirect;
            lblHomePage.Visible = !defaultRedirect;
            lblRedirectName.Visible = defaultRedirect;
            txtRedirectName.Visible = defaultRedirect;
        }

        private bool HomePageRedirectExists()
        {
            var redirectionController = new RedirectionController();
            var homeRedirects = redirectionController.GetRedirectionsByPortal(ModuleContext.PortalId).Where(r => r.SourceTabId == ModuleContext.PortalSettings.HomeTabId);
            return (homeRedirects.Any());
        }

        private void SaveRedirection()
        {
            var redirection = new Redirection();
            var redirectionController = new RedirectionController();

            redirection.Name = (HomePageRedirectExists()) ? txtRedirectName.Text : Localization.GetString("DefaultRedirectName.Text", LocalResourceFile);
            redirection.Enabled = true;
            redirection.PortalId = ModuleContext.PortalId;
            redirection.SourceTabId = (cboSourcePage.Visible) ? cboSourcePage.SelectedItemValueAsInt : ModuleContext.PortalSettings.HomeTabId;

            redirection.Type = RedirectionType.MobilePhone;
            redirection.TargetType = (TargetType) Enum.Parse(typeof (TargetType), optRedirectTarget.SelectedValue);

            switch (redirection.TargetType)
            {
                case TargetType.Portal:
                    redirection.TargetValue = cboPortal.SelectedItem.Value;
                    break;
                case TargetType.Tab:
                    redirection.TargetValue = cboTargetPage.SelectedItemValueAsInt;
                    break;
                case TargetType.Url:
                    redirection.TargetValue = txtTargetUrl.Text;
                    break;
            }

            // Save the redirect
            redirectionController.Save(redirection);
        }
    }
}