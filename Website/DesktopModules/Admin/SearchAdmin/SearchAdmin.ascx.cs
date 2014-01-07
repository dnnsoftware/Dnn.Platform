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
#region Usings

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web.UI.WebControls;

using DotNetNuke.Entities.Portals;
using DotNetNuke.Framework;
using DotNetNuke.Services.Localization;
using DotNetNuke.Services.Search.Entities;
using DotNetNuke.Services.Search.Internals;
using DotNetNuke.UI.Modules;
using DotNetNuke.Web.Client.ClientResourceManagement;
using DotNetNuke.Web.UI.WebControls;

#endregion

namespace DotNetNuke.Modules.Admin.Search
{

    public partial class SearchAdmin : ModuleUserControlBase
    {
        protected IEnumerable<SynonymsGroup> CurrentPortalSynonymsGroups { get; set; }
        protected int SynonymsSelectedPortalId { get; set; }
        protected string SynonymsSeletedCulture { get; set; }

        protected IEnumerable<string> CurrentCultureCodeList { get; set; } 
        protected SearchStopWords CurrentSearchStopWords { get; set; }
        protected int StopWordsSeletedPortalId { get; set; }
        protected string StopWordsSeletedCulture { get; set; }

        protected const string MyFileName = "SearchAdmin.ascx";

		#region "Event Handlers"

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            ServicesFramework.Instance.RequestAjaxAntiForgerySupport();
            ClientResourceManager.RegisterScript(Page, "~/desktopmodules/admin/searchAdmin/dnn.SearchAdmin.js");
            rptStopWordsCultureList.ItemDataBound += RptLanguagesItemDataBound;
            rptStopWordsCultureList.ItemCommand += RptStopWordsLanguagesItemOnCommand;

            rptSynonymsCultureList.ItemDataBound += RptLanguagesItemDataBound;
            rptSynonymsCultureList.ItemCommand += RptSynonymsLanguagesItemOnCommand;
            
            //set init value
            SynonymsSelectedPortalId = PortalSettings.Current.PortalId;
            StopWordsSeletedPortalId = PortalSettings.Current.PortalId;
        }

        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);

            CurrentCultureCodeList = LoadCultureCodeList(StopWordsSeletedPortalId);

            var currentCultureCodeList = CurrentCultureCodeList as IList<string> ?? CurrentCultureCodeList.ToList();
            if (string.IsNullOrEmpty(StopWordsSeletedCulture)) 
                StopWordsSeletedCulture = currentCultureCodeList.FirstOrDefault();

            if (string.IsNullOrEmpty(StopWordsSeletedCulture)) StopWordsSeletedCulture = "en-US";

            if (string.IsNullOrEmpty(SynonymsSeletedCulture)) 
                SynonymsSeletedCulture= currentCultureCodeList.FirstOrDefault();

            if (string.IsNullOrEmpty(SynonymsSeletedCulture)) SynonymsSeletedCulture = "en-US";

	        if (CurrentCultureCodeList.Count() > 1)
	        {
		        rptStopWordsCultureList.DataSource = CurrentCultureCodeList;
		        rptStopWordsCultureList.DataBind();

		        rptSynonymsCultureList.DataSource = CurrentCultureCodeList;
		        rptSynonymsCultureList.DataBind();
	        }
	        else
	        {
				plCultureList.Visible = rptStopWordsCultureList.Visible = plSynonymsCultureList.Visible = rptSynonymsCultureList.Visible = false;
	        }

	        CurrentSearchStopWords = LoadSearchStopWords(StopWordsSeletedPortalId, StopWordsSeletedCulture);
            CurrentPortalSynonymsGroups = LoadSynonymsGroup(SynonymsSelectedPortalId, SynonymsSeletedCulture);
            
            hdnSynonymsSelectedPortalID.Value = SynonymsSelectedPortalId.ToString(CultureInfo.InvariantCulture);
            hdnStopWordsSelectedPortalID.Value = StopWordsSeletedPortalId.ToString(CultureInfo.InvariantCulture);
            hdnSynonymsSelectedCultureCode.Value = SynonymsSeletedCulture.ToString(CultureInfo.InvariantCulture);
            hdnStopWordsSelectedCultureCode.Value = StopWordsSeletedCulture.ToString(CultureInfo.InvariantCulture);
        }
		
		#endregion

        protected void RptStopWordsLanguagesItemOnCommand(object sender, RepeaterCommandEventArgs e)
        {
            StopWordsSeletedPortalId = PortalSettings.Current.PortalId;
            StopWordsSeletedCulture = e.CommandArgument.ToString();
        }

        protected void RptSynonymsLanguagesItemOnCommand(object sender, RepeaterCommandEventArgs e)
        {
            SynonymsSelectedPortalId = PortalSettings.Current.PortalId;
            SynonymsSeletedCulture = e.CommandArgument.ToString();
        }

        protected IEnumerable<string> LoadCultureCodeList(int portalId)
        {
            var locals = LocaleController.Instance.GetLocales(portalId).Values;
            return locals.Select(local => local.Code).ToList();
        } 

        protected IEnumerable<SynonymsGroup> LoadSynonymsGroup(int portalId, string cultureCode)
        {
            return SearchHelper.Instance.GetSynonymsGroups(portalId, cultureCode);
        }

        protected SearchStopWords LoadSearchStopWords(int portalId, string cultureCode)
        {
            return SearchHelper.Instance.GetSearchStopWords(portalId, cultureCode);
        } 

        protected void RptLanguagesItemDataBound(object sender, RepeaterItemEventArgs e)
        {
            var imgBtn = e.Item.FindControl("imgBtnStopWordsCulture") as ImageButton;
            if (imgBtn == null) return;
            if (e.Item.DataItem != null)
            {
                var locale = e.Item.DataItem as String;
                imgBtn.ImageUrl = "~/images/flags/" + locale + ".gif";
                imgBtn.CommandArgument = locale;

				if ((sender == rptStopWordsCultureList && locale == StopWordsSeletedCulture)
						|| (sender == rptSynonymsCultureList && locale == SynonymsSeletedCulture))
				{
					imgBtn.CssClass = "stopwordsCultureSelected";
				}
            }
        }

        protected void ReIndex(object sender, EventArgs e)
        {
            SearchHelper.Instance.SetSearchReindexRequestTime(PortalSettings.Current.PortalId);
        }
    }
}