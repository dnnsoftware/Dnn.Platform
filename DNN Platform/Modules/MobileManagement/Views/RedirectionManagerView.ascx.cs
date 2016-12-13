#region Copyright
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2017
// by DotNetNuke Corporation
// All Rights Reserved
#endregion
using System;
using System.Collections.Generic;
using System.Web.UI.WebControls;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Tabs;
using DotNetNuke.Framework.Providers;
using DotNetNuke.Modules.MobileManagement.Presenters;
using DotNetNuke.Modules.MobileManagement.Views;
using DotNetNuke.Modules.MobileManagement.ViewModels;
using DotNetNuke.Services.ClientCapability;
using DotNetNuke.Services.Localization;
using DotNetNuke.Services.Mobile;
using DotNetNuke.UI.Skins.Controls;
using DotNetNuke.UI.Utilities;
using DotNetNuke.Web.Mvp;
using System.Linq;

namespace DotNetNuke.Modules.MobileManagement
{
    /// <summary>
    /// 
    /// </summary>
	public partial class RedirectionManagerView : ModuleView<RedirectionManagerViewModel>, IRedirectionManagerView, IClientAPICallbackEventHandler
    {
        public event EventHandler<DataGridCommandEventArgs> RedirectionItemAction;

        #region "Protected Methods"

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            RedirectionsGrid.ItemCommand += RedirectionsListItemCommand;
            RedirectionsGrid.ItemDataBound += RedirectionsListItemDataBound;
            lnkAdd.NavigateUrl = Model.AddUrl;

            if (!IsPostBack)
            {
                var redirectController = new RedirectionController();

                redirectController.PurgeInvalidRedirections(ModuleContext.PortalId);
                Model.Redirections = redirectController.GetRedirectionsByPortal(ModuleContext.PortalId);

                BindRedirectionList(Model.Redirections);
                Localization.LocalizeDataGrid(ref RedirectionsGrid, LocalResourceFile);
                if(Model.ModeType != null)
                {
                    optSimpleAdvanced.SelectedValue = Model.ModeType;
                }
			}

            if (IsTrialFiftyOneProvider())
            {
                var trialMessage = LocalizeString("TrialFiftyOneProvider");
                UI.Skins.Skin.AddModuleMessage(this, trialMessage, ModuleMessage.ModuleMessageType.BlueInfo);
            }

            dvRedirectionsGrid.Visible = (Model.Redirections.Count > 0);
            ClientAPI.RegisterClientVariable(Page, "ActionCallback", ClientAPI.GetCallbackEventReference(this, "[ACTIONTOKEN]", "success", "this", "error"), true);
        }

        protected void RedirectionsListItemDataBound(object sender, DataGridItemEventArgs e)
        {
            if (e.Item.ItemType == ListItemType.Item || e.Item.ItemType == ListItemType.AlternatingItem || e.Item.ItemType == ListItemType.EditItem)
            {
                var redirection = e.Item.DataItem as IRedirection;                
                e.Item.Attributes.Add("data", redirection.Id.ToString());
             
				if (IsSmartPhoneRedirect(redirection))
				{
					redirection.Type = RedirectionType.SmartPhone;
				}
            }
        }

		protected void RedirectionsListItemCommand(object source, DataGridCommandEventArgs e)
		{
            // Perform Item Action
		    RedirectionItemAction(source, e);
            Response.Redirect(DotNetNuke.Common.Globals.NavigateURL(), true);
		}

		protected string GetEditUrl(string id)
		{
			var editUrl = ModuleContext.EditUrl("Id", id, "RedirectionSettings");
			if (ModuleContext.PortalSettings.EnablePopUps)
			{
				editUrl = UrlUtils.PopUpUrl(editUrl, this, ModuleContext.PortalSettings, false, false);
			}
			return editUrl;
		}

        protected string RenderTooltip(string key , string id)
        {
            var redirectionController = new RedirectionController();
            var redirect = redirectionController.GetRedirectionById(ModuleContext.PortalId, int.Parse(id));            
            var tooltip = string.Empty;

            switch (key)
            {
                case "source":
                    var source = (redirect.SourceTabId == -1) ? ModuleContext.PortalAlias.HTTPAlias : TabController.Instance.GetTabsByPortal(ModuleContext.PortalId).First(r => r.Key == redirect.SourceTabId).Value.TabName;
                    tooltip = string.Format(LocalizeString("RedirectTooltipSource.Text"), source);
                    break;
                case "destination":
                    tooltip = string.Format(LocalizeString("RedirectTooltipDestination.Text"), redirectionController.GetRedirectUrlFromRule(redirect, ModuleContext.PortalId, redirect.SourceTabId));
                    break;
            }

            return tooltip;
        }

        #endregion

        #region "Private Methods"

        private void BindRedirectionList(IList<IRedirection> redirections)
		{
		    RedirectionsGrid.DataSource = redirections;
			RedirectionsGrid.DataBind();
		}


        private bool IsTrialFiftyOneProvider()
        {
            var provider = ProviderConfiguration.GetProviderConfiguration("clientcapability");
            return provider.DefaultProvider == "FiftyOneClientCapabilityProvider" 
                && !ClientCapabilityProvider.Instance().SupportsTabletDetection;
        }

		private bool IsSmartPhoneRedirect(IRedirection redirect)
		{
			return redirect.Type == RedirectionType.Other && redirect.MatchRules.Count == 1 && redirect.MatchRules[0].Capability == "IsSmartPhone";
		}

        #endregion

		public string RaiseClientAPICallbackEvent(string eventArgument)
		{
			IDictionary<string, string> arguments = new Dictionary<string, string>();
			foreach (var arg in eventArgument.Split('&'))
			{
				arguments.Add(arg.Split('=')[0], arg.Split('=')[1]);
			}
			switch (arguments["action"])
			{
				case "sort":
					var moveId = Convert.ToInt32(arguments["moveId"]);
					var nextId = Convert.ToInt32(arguments["nextId"]);
			        new RedirectionManagerPresenter(this).SortRedirections(moveId, nextId);
					break;
			}
            
			return string.Empty;
		}

    }
}