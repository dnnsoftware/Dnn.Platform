#region Copyright
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2017
// by DotNetNuke Corporation
// All Rights Reserved
#endregion
using System;
using System.Linq;
using System.Web.UI.WebControls;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Modules.MobileManagement.Views;
using DotNetNuke.Modules.MobileManagement.ViewModels;
using DotNetNuke.Services.Mobile;
using DotNetNuke.Web.Mvp;

namespace DotNetNuke.Modules.MobileManagement.Presenters
{
    /// <summary>
    /// 
    /// </summary>
    public class RedirectionManagerPresenter : ModulePresenter<IRedirectionManagerView, RedirectionManagerViewModel>
    {
        private readonly IRedirectionController _redirectionController;

		/// <summary>
		/// presenter constructor with view
		/// </summary>
		/// <param name="view">the view.</param>
		public RedirectionManagerPresenter(IRedirectionManagerView view) : this(view, new RedirectionController())
		{
			
		}
        /// <summary>
		/// presenter constructor with view and the business controller.
        /// </summary>
        /// <param name="view">the view.</param>
        /// <param name="controller">the redirection business controller.</param>
        public RedirectionManagerPresenter(IRedirectionManagerView view, IRedirectionController controller) : base(view)
        {
			_redirectionController = controller;
            View.RedirectionItemAction += RedirectionItemAction;
        }

        protected override void OnLoad()
        {
			base.OnLoad();

            // Generate the Add redirect URL
            var editUrl = ModuleContext.EditUrl("{0}");
            View.Model.AddUrl = ModuleContext.PortalSettings.EnablePopUps ? UrlUtils.PopUpUrl(editUrl, null, ModuleContext.PortalSettings, false, false) : editUrl;

            View.Model.ModeType = Request.QueryString["type"];

            // Load Redirects            
            View.Model.Redirections = _redirectionController.GetRedirectionsByPortal(ModuleContext.PortalId);
        }

        public void RedirectionItemAction(object sender, DataGridCommandEventArgs e)
        {
            var id = Convert.ToInt32(e.CommandArgument);
            switch (e.CommandName)
            {
                case "delete":
                    _redirectionController.Delete(ModuleContext.PortalId, id);                    
                    break;
                case "enable":
                    var redirection = _redirectionController.GetRedirectionById(ModuleContext.PortalId, id);
                    redirection.Enabled = !redirection.Enabled;
                    _redirectionController.Save(redirection);
                    break;
            }
        }

        public string SortRedirections(int moveRedirectionId, int nextRedirectionId)
        {
            var moveRedirection = _redirectionController.GetRedirectionById(ModuleContext.PortalId, moveRedirectionId);
            var nextRedirection = _redirectionController.GetRedirectionById(ModuleContext.PortalId, nextRedirectionId);
            var allItems = _redirectionController.GetRedirectionsByPortal(ModuleContext.PortalId);

            if (nextRedirectionId > 0)
            {
                if (nextRedirection.SortOrder > moveRedirection.SortOrder)
                {
                    var effectItems = allItems.Where(r => r.SortOrder > moveRedirection.SortOrder && r.SortOrder < nextRedirection.SortOrder).ToList();
                    effectItems.ForEach(r =>
                    {
                        r.SortOrder--;
                        _redirectionController.Save(r);
                    });

                    moveRedirection.SortOrder = nextRedirection.SortOrder - 1;
                    _redirectionController.Save(moveRedirection);
                }
                else
                {
                    int nextOrder = nextRedirection.SortOrder;
                    var effectItems = allItems.Where(r => r.SortOrder >= nextRedirection.SortOrder && r.SortOrder < moveRedirection.SortOrder).ToList();
                    effectItems.ForEach(r =>
                    {
                        r.SortOrder++;
                        _redirectionController.Save(r);
                    });

                    moveRedirection.SortOrder = nextOrder;
                    _redirectionController.Save(moveRedirection);
                }
            }
            else
            {
                var effectItems = allItems.Where(r => r.SortOrder > moveRedirection.SortOrder).ToList();
                effectItems.ForEach(r =>
                {
                    r.SortOrder--;
                    _redirectionController.Save(r);
                });

                moveRedirection.SortOrder = allItems.Count;
                _redirectionController.Save(moveRedirection);
            }
            return string.Empty;
        }
    }
}