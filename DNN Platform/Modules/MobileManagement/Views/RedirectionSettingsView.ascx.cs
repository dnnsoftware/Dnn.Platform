#region Copyright
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2017
// by DotNetNuke Corporation
// All Rights Reserved
#endregion
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI.WebControls;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Tabs;
using DotNetNuke.Framework;
using DotNetNuke.Modules.MobileManagement.Presenters;
using DotNetNuke.Modules.MobileManagement.Views;
using DotNetNuke.Services.ClientCapability;
using DotNetNuke.Services.Exceptions;
using DotNetNuke.Services.Localization;
using DotNetNuke.Services.Mobile;
using DotNetNuke.UI.Skins.Controls;
using DotNetNuke.Web.Mvp;
using DotNetNuke.Web.UI.WebControls;

using WebFormsMvp;

using Globals = DotNetNuke.Common.Globals;

namespace DotNetNuke.Modules.MobileManagement
{
    /// <summary>
    /// 
    /// </summary>
    [PresenterBinding(typeof(RedirectionSettingsPresenter))]
    public partial class RedirectionSettingsView : ModuleView, IRedirectionSettingsView
    {
        #region Properties

        public int RedirectId
        {
            get
            {
                if (Request.QueryString["Id"] != null)
                {
                    return Convert.ToInt32(Request.QueryString["Id"]);
                }
                else
                {
                    return Null.NullInteger;
                }
            }
        }

        public IList<IMatchRule> Capabilities
        {
            get
            {
                if (ViewState["capabilities"] == null)
                {
                    ViewState["capabilities"] = RedirectId > Null.NullInteger ? new RedirectionController().GetRedirectionById(ModuleContext.PortalId, RedirectId).MatchRules : new List<IMatchRule>();
                }
                return (List<IMatchRule>)ViewState["capabilities"];
            }
            set
            {
                ViewState["capabilities"] = value;
            }            
        }

	    private bool SupportsSmartPhoneDetection
	    {
		    get
		    {
			    return ClientCapabilityProvider.Instance().GetAllClientCapabilityValues().ContainsKey("IsSmartPhone");
		    }
	    }

        private string _capability = string.Empty;
        private string _capabilityValue = string.Empty;

        #endregion

        #region Event Handlers

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            ServicesFramework.Instance.RequestAjaxAntiForgerySupport();


            dgCapabilityList.ItemCommand += OnCapabilitiesItemCommand;
            dgCapabilityList.ItemDataBound += CapabilitiesItemDataBound;
            //cboCapabilityValue.ItemsRequested += LoadCapabilityValues;
            //cboCapabilityValue.SelectedIndexChanged += SetCapabilityValue;

            lnkSave.Click += lnkSave_OnClick;
            lnkCancel.Click += lnkCancel_OnClick;
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            if (!IsPostBack)
            {
                optRedirectType.Items[1].Enabled = optRedirectType.Items[2].Enabled = ClientCapabilityProvider.Instance().SupportsTabletDetection;
				optRedirectType.Items[3].Enabled = SupportsSmartPhoneDetection;
                BindSettingControls();
                BindRedirection(RedirectId);
            }
            BindRedirectionCapabilties();
        }

        protected void AddCapability(object sender, EventArgs e)
        {
            if (_capabilityValue.Trim() != string.Empty)
            {
                // Must rebind Capability values due to a telerik in dependant combos, losing initial dependant value on post back.
                BindCapabilityValues(cboCapabilityName.SelectedItem.Text);
                SetCapabilityAndValue();

                var capability = new MatchRule { Capability = _capability, Expression = _capabilityValue };
                var capabilitylist = Capabilities;
                capabilitylist.Add(capability);
                Capabilities = capabilitylist;
                BindRedirectionCapabilties();
                BindCapabilities();
                cboCapabilityName.SelectedIndex = 0;
                BindCapabilityValues(string.Empty);
            }
        }

        protected void OnCapabilitiesItemCommand(object source, DataGridCommandEventArgs e)
        {
            switch (e.CommandName)
            {
                case "delete":
                    var capability = Capabilities.Where(c => c.Capability == e.CommandArgument.ToString()).First();
                        var capabilitylist = Capabilities;
                        capabilitylist.Remove(capability);
                        Capabilities = capabilitylist;                        
                        BindRedirectionCapabilties();
                        BindCapabilities();
                        cboCapabilityName.SelectedIndex = 0;
                        BindCapabilityValues(string.Empty);
					break;
            }
        }

        protected void CapabilitiesItemDataBound(object sender, DataGridItemEventArgs e)
        {
            if (e.Item.ItemType == ListItemType.Item || e.Item.ItemType == ListItemType.AlternatingItem || e.Item.ItemType == ListItemType.EditItem)
            {
                var rule = e.Item.DataItem as IMatchRule;
                e.Item.Attributes.Add("data", rule.Id.ToString());
            }
        }

        //protected void LoadCapabilityValues(object source, RadComboBoxItemsRequestedEventArgs e)
        //{
        //    BindCapabilityValues(e.Text);
        //    SetCapabilityAndValue();
        //}

        //protected void SetCapabilityValue(object source, RadComboBoxSelectedIndexChangedEventArgs e)
        //{
        //    _capabilityValue = e.Text;
        //}

        #endregion

        #region Private Methods

        private void BindRedirection(int redirectId)
        {
            // Populating existing redirection settings
            if (redirectId != Null.NullInteger)
            {
                var redirectController = new RedirectionController();
                var redirect = redirectController.GetRedirectionById(ModuleContext.PortalId, redirectId);

                txtRedirectName.Text = redirect.Name;
                chkEnable.Checked = redirect.Enabled;
                chkChildPages.Checked = redirect.IncludeChildTabs;
                var tabs = TabController.Instance.GetTabsByPortal(ModuleContext.PortalId).AsList().Where(IsVisible);
                var tabInfos = tabs as IList<TabInfo> ?? tabs.ToList();
                if (redirect.SourceTabId != -1)
                {
                    optRedirectSource.SelectedValue = "Tab";
                    cboSourcePage.SelectedPage = tabInfos.SingleOrDefault(t => t.TabID == redirect.SourceTabId);
                }
                else
                {
                    optRedirectSource.SelectedValue = "Portal";
                }

				if (IsSmartPhoneRedirect(redirect))
				{
					optRedirectType.SelectedValue = "SmartPhone";
				}
				else
				{
					optRedirectType.SelectedValue = redirect.Type.ToString();
				}

                optRedirectTarget.SelectedValue = redirect.TargetType.ToString();

                //Other, populate Capabilities
                if (redirect.Type == RedirectionType.Other)
                {
                    BindRedirectionCapabilties();
                }

                switch (redirect.TargetType)
                {
                    case TargetType.Portal:
                        if (cboPortal.Items.Count < 1) optRedirectTarget.SelectedValue = "Tab";
                        else
                            cboPortal.Select(redirect.TargetValue.ToString(), false);
                        break;
                    case TargetType.Tab:
                        int redirectTargetInt;
                        if(int.TryParse(redirect.TargetValue.ToString(), out redirectTargetInt))
                            cboTargetPage.SelectedPage = tabInfos.SingleOrDefault(t => t.TabID == redirectTargetInt);
                        break;
                    case TargetType.Url:
                        txtTargetUrl.Text = redirect.TargetValue.ToString();
                        break;
                }
            }
        }

		private bool IsSmartPhoneRedirect(IRedirection redirect)
		{
			return SupportsSmartPhoneDetection && redirect.Type == RedirectionType.Other && redirect.MatchRules.Count == 1 && redirect.MatchRules[0].Capability == "IsSmartPhone";
		}

        private void BindRedirectionCapabilties()
        {
            dgCapabilityList.DataSource = Capabilities;
            dgCapabilityList.DataBind();
            dgCapabilityList.Visible = (Capabilities.Count > 0);
        }

        private void BindSettingControls()
        {
            BindPortals();
            BindCapabilities();
        }

        private void BindPortals()
        {
            // Populating Portals dropdown
            var portals = PortalController.Instance.GetPortals().Cast<PortalInfo>().Where(p => p.PortalID != ModuleContext.PortalId).ToList();
            if (portals.Count > 0)
            {
				cboPortal.Items.Clear();
	            foreach (var portalInfo in portals)
	            {
		            cboPortal.Items.Add(new ListItem(portalInfo.PortalName, portalInfo.PortalID.ToString()));
	            }
            }
            else
            {
                optRedirectTarget.Items[0].Enabled = false;
                if (RedirectId == Null.NullInteger)
                {
                    optRedirectTarget.Items[0].Selected = false;
                    optRedirectTarget.Items[1].Selected = true;
                }
            }

        }
        
        private void BindCapabilities()
        {
            //Bind only capabilities that have not yet been added
			var capabilities = new List<string>(ClientCapabilityProvider.Instance().GetAllClientCapabilityValues().Keys.Where(capability => Capabilities.Where( c => c.Capability == capability).Count() < 1));
            capabilities.Insert(0, LocalizeString("selectCapabilityName"));
            cboCapabilityName.DataSource = capabilities;
            cboCapabilityName.DataBind();
        }

        private void BindCapabilityValues(string capability)
        {
            if (capability != string.Empty)
            {
                var capabillityValues = new List<string>(ClientCapabilityProvider.Instance().GetAllClientCapabilityValues().Where(c => c.Key == capability).First().Value);
                capabillityValues.Sort();
                cboCapabilityValue.DataSource = capabillityValues;
            }
            else
            {
                cboCapabilityValue.DataSource = string.Empty;
                cboCapabilityValue.Text = string.Empty;
            }
            cboCapabilityValue.DataBind();                 
        }

        private void SetCapabilityAndValue()
        {
            if (_capability == string.Empty) _capability = cboCapabilityName.SelectedValue;
            if (_capabilityValue == string.Empty) _capabilityValue = cboCapabilityValue.SelectedValue;
        }

        private void SaveRedirection()
        {
            IRedirection redirection = new Redirection();
            var redirectionController = new RedirectionController();

            if (RedirectId > Null.NullInteger)
            {
                redirection = redirectionController.GetRedirectionById(ModuleContext.PortalId, RedirectId);
            }

            redirection.Name = txtRedirectName.Text;
            redirection.Enabled = chkEnable.Checked;
            redirection.PortalId = ModuleContext.PortalId;
            if (optRedirectSource.SelectedValue == "Tab")
            {
                redirection.SourceTabId = cboSourcePage.SelectedItemValueAsInt;
                redirection.IncludeChildTabs = chkChildPages.Checked;
            }
            else
            {
                redirection.SourceTabId = -1;
                redirection.IncludeChildTabs = false;
            }

            redirection.Type = (RedirectionType)Enum.Parse(typeof(RedirectionType), optRedirectType.SelectedValue);
			if (redirection.Type == RedirectionType.SmartPhone && optRedirectType.SelectedValue != "")//save smart phone value to other type with capability match.
			{
				if (RedirectId > Null.NullInteger)
                {
                    // Delete capabilities that no longer exist in the grid
                    foreach (var rule in redirection.MatchRules)
                    {
                        redirectionController.DeleteRule(ModuleContext.PortalId, redirection.Id, rule.Id);
                    }
                }

				redirection.Type = RedirectionType.Other;
				redirection.MatchRules.Add(new MatchRule(){Capability = "IsSmartPhone", Expression = "True"});
			}
			else if (redirection.Type == RedirectionType.Other)//Other, save new capabilities
            {
                if (RedirectId > Null.NullInteger)
                {
                    // Delete capabilities that no longer exist in the grid
                    foreach (var rule in redirection.MatchRules.Where(rule => Capabilities.All(c => c.Id != rule.Id)))
                    {
                        redirectionController.DeleteRule(ModuleContext.PortalId, redirection.Id, rule.Id);
                    }
                }

                redirection.MatchRules = Capabilities;
            }
            else if (RedirectId > Null.NullInteger && redirection.MatchRules.Count > 0)
            {
                foreach(var rule in redirection.MatchRules)
                {
                    redirectionController.DeleteRule(ModuleContext.PortalId, redirection.Id, rule.Id);
                }
            }

            redirection.TargetType = (TargetType)Enum.Parse(typeof(TargetType), optRedirectTarget.SelectedValue);
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

		private bool IsVisible(TabInfo tab)
		{
            if (tab == null || tab.TabID == PortalSettings.Current.AdminTabId || tab.TabID == PortalSettings.Current.UserTabId)
            {
                return false;
            }
            
            if(tab.ParentId != Null.NullInteger)
            {
                do
                {
                    if (tab.ParentId == PortalSettings.Current.AdminTabId || tab.ParentId == PortalSettings.Current.UserTabId)
                    {
                        return false;
                    }

                    tab = TabController.Instance.GetTab(tab.ParentId, tab.PortalID, false);
                } while (tab != null && tab.ParentId != Null.NullInteger);
            }

			return true;
		}

        #endregion

        #region Event Handlers

        private void lnkSave_OnClick(object sender, EventArgs e)
        {
                     
            var redirectionController = new RedirectionController();
            var name = txtRedirectName.Text;
            int nameCount;
            // Checks for duplicate names   
            if (RedirectId > Null.NullInteger)
            {
                nameCount = redirectionController.GetRedirectionsByPortal(ModuleContext.PortalId).Where(r =>( r.Id != RedirectId && r.Name.ToLower() == name.ToLower())).Count();
            }
            else
            {
                nameCount = redirectionController.GetRedirectionsByPortal(ModuleContext.PortalId).Where(r => r.Name.ToLower() == name.ToLower()).Count();
            }

            if (nameCount < 1)
            {
                SaveRedirection();
                Response.Redirect(Globals.NavigateURL("", "type=RedirectionSettings"), true);
            }
            else
            {
                DotNetNuke.UI.Skins.Skin.AddModuleMessage(this, Localization.GetString("DuplicateNameError.Text", LocalResourceFile), ModuleMessage.ModuleMessageType.RedError);                
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
    }
}