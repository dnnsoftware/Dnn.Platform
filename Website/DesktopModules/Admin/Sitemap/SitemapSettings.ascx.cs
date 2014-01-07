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
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Web;

using DotNetNuke.Common;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Portals.Internal;
using DotNetNuke.Framework;
using DotNetNuke.Services.Exceptions;
using DotNetNuke.Services.Localization;
using DotNetNuke.Services.Sitemap;
using DotNetNuke.UI.Skins.Controls;
using DotNetNuke.Web.UI.WebControls;

using Telerik.Web.UI;

#endregion

namespace DotNetNuke.Modules.Admin.Sitemap
{

    public partial class SitemapSettings : PortalModuleBase
    {

        #region Private Methods

        private void LoadConfiguration()
        {     
			//core settings
			chkLevelPriority.Checked = bool.Parse(PortalController.GetPortalSetting("SitemapLevelMode", PortalId, "False"));
            var minPriority = float.Parse(PortalController.GetPortalSetting("SitemapMinPriority", PortalId, "0.1"), NumberFormatInfo.InvariantInfo);
            txtMinPagePriority.Text = minPriority.ToString();

            chkIncludeHidden.Checked = bool.Parse(PortalController.GetPortalSetting("SitemapIncludeHidden", PortalId, "False"));

            //General settings
            var excludePriority = float.Parse(PortalController.GetPortalSetting("SitemapExcludePriority", PortalId, "0.1"), NumberFormatInfo.InvariantInfo);
            txtExcludePriority.Text = excludePriority.ToString();

            cmbDaysToCache.SelectedIndex = Int32.Parse(PortalController.GetPortalSetting("SitemapCacheDays", PortalId, "1"));
        }

        private void SavePrioritySettings()
        {
            PortalController.UpdatePortalSetting(PortalId, "SitemapLevelMode", chkLevelPriority.Checked.ToString());

            if (float.Parse(txtMinPagePriority.Text) < 0)
            {
                txtMinPagePriority.Text = "0";
            }

            var minPriority = float.Parse(txtMinPagePriority.Text);

            PortalController.UpdatePortalSetting(PortalId, "SitemapMinPriority", minPriority.ToString(NumberFormatInfo.InvariantInfo));
        }

        private void ResetCache()
        {
            var cacheFolder = new DirectoryInfo(PortalSettings.HomeDirectoryMapPath + "sitemap\\");

            if (cacheFolder.Exists)
            {
                foreach (FileInfo file in cacheFolder.GetFiles("sitemap*.xml"))
                {
                    file.Delete();
                }

                UI.Skins.Skin.AddModuleMessage(this, Localization.GetString("ResetOK", LocalResourceFile), ModuleMessage.ModuleMessageType.GreenSuccess);
            }
        }

        private bool IsChildPortal(PortalSettings ps, HttpContext context)
        {
            var isChild = false;
            var arr = TestablePortalAliasController.Instance.GetPortalAliasesByPortalId(ps.PortalId).ToList();
            var serverPath = Globals.GetAbsoluteServerPath(context.Request);

            if (arr.Count > 0)
            {
                var portalAlias = (PortalAliasInfo)arr[0];
                var portalName = Globals.GetPortalDomainName(ps.PortalAlias.HTTPAlias, Request, true);
                if (portalAlias.HTTPAlias.IndexOf("/") > -1)
                {
                    portalName = PortalController.GetPortalFolder(portalAlias.HTTPAlias);
                }
                if (!string.IsNullOrEmpty(portalName) && Directory.Exists(serverPath + portalName))
                {
                    isChild = true;
                }
            }
            return isChild;
        }

        private void BindProviders()
        {
            var builder = new SitemapBuilder(PortalSettings);

            grdProviders.DataSource = builder.Providers;
        }

        private void GrdProvidersNeedDataSorce()
        {
            BindProviders();
        }

        private void SetSearchEngineSubmissionURL()
        {
            try
            {
                if ((cboSearchEngine.SelectedItem != null))
                {
                    var strURL = "";
                    switch (cboSearchEngine.SelectedItem.Text.ToLower().Trim())
                    {
                        case "google":
                            strURL += "http://www.google.com/addurl?q=" + Globals.HTTPPOSTEncode(Globals.AddHTTP(Globals.GetDomainName(Request)));
                            strURL += "&dq=";
                            if (!string.IsNullOrEmpty(PortalSettings.PortalName))
                            {
                                strURL += Globals.HTTPPOSTEncode(PortalSettings.PortalName);
                            }
                            if (!string.IsNullOrEmpty(PortalSettings.Description))
                            {
                                strURL += Globals.HTTPPOSTEncode(PortalSettings.Description);
                            }
                            if (!string.IsNullOrEmpty(PortalSettings.KeyWords))
                            {
                                strURL += Globals.HTTPPOSTEncode(PortalSettings.KeyWords);
                            }
                            strURL += "&submit=Add+URL";
                            break;
                        case "yahoo!":
                            strURL = "http://siteexplorer.search.yahoo.com/submit";
                            break;
                        case "bing":
                            strURL = "http://www.bing.com/webmaster";
                            break;
                    }

                    cmdSubmitSitemap.NavigateUrl = strURL;

                    cmdSubmitSitemap.Target = "_blank";
                }
            }
            catch (Exception exc)
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

        #endregion

        #region Event Handlers

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            jQuery.RequestRegistration();
            AJAX.RegisterScriptManager();

            cboSearchEngine.SelectedIndexChanged += OnSearchEngineIndexChanged;
            grdProviders.UpdateCommand += ProvidersUpdateCommand;
            grdProviders.ItemCommand += ProvidersItemCommand;
            lnkResetCache.Click += OnResetCacheClick;
            lnkSaveAll.Click += OnSaveAllClick;
            cmdVerification.Click += OnVerifyClick;
            try
            {
                if (Page.IsPostBack == false)
                {
                    LoadConfiguration();

                    string portalAlias = !String.IsNullOrEmpty(PortalSettings.DefaultPortalAlias)
                                        ? PortalSettings.DefaultPortalAlias
                                        : PortalSettings.PortalAlias.HTTPAlias;
                    lnkSiteMapUrl.Text = Globals.AddHTTP(portalAlias) + @"/SiteMap.aspx";

                    lnkSiteMapUrl.NavigateUrl = lnkSiteMapUrl.Text;

                    BindProviders();
                    SetSearchEngineSubmissionURL();
                }

                GrdProvidersNeedDataSorce();

                grdProviders.NeedDataSource += OnGridNeedDataSource;
            }
            catch (Exception exc)
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

        protected void OnSaveAllClick(object sender, EventArgs e)
        {
            SavePrioritySettings();

            PortalController.UpdatePortalSetting(PortalId, "SitemapIncludeHidden", chkIncludeHidden.Checked.ToString());

            float excludePriority = float.Parse(txtExcludePriority.Text);
            PortalController.UpdatePortalSetting(PortalId, "SitemapExcludePriority", excludePriority.ToString(NumberFormatInfo.InvariantInfo));

            if ((cmbDaysToCache.SelectedIndex == 0))
            {
                ResetCache();
            }

            PortalController.UpdatePortalSetting(PortalId, "SitemapCacheDays", cmbDaysToCache.SelectedIndex.ToString());

            UI.Skins.Skin.AddModuleMessage(this, Localization.GetString("MessageUpdated", LocalResourceFile), ModuleMessage.ModuleMessageType.GreenSuccess);


            LoadConfiguration();
        }

        protected void OnResetCacheClick(object sender, EventArgs e)
        {
            ResetCache();
        }

        protected void OnGridNeedDataSource(object sender, GridNeedDataSourceEventArgs e)
        {
            GrdProvidersNeedDataSorce();
        }

        protected void ProvidersItemCommand(object source, GridCommandEventArgs e)
        {
            if (e.CommandName == RadGrid.UpdateCommandName)
            {
                if (!Page.IsValid)
                {
                    e.Canceled = true;
                }
            }
        }

        protected void ProvidersUpdateCommand(object source, GridCommandEventArgs e)
        {
            //grdProviders.Rebind()

            var editedItem = (GridEditableItem)e.Item;
            var editMan = editedItem.EditManager;
            //var editedSiteMap = (SitemapProvider) e.Item.DataItem;
            var nameCol = ((DnnGrid)source).Columns.FindByUniqueName("Name");
            var nameEditor = editMan.GetColumnEditor((IGridEditableColumn)nameCol);
            var key = ((GridTextColumnEditor)nameEditor).Text;

            var providers = (List<SitemapProvider>)grdProviders.DataSource;
            SitemapProvider editedProvider = null;

            foreach (var p in providers)
            {
                if ((string.Equals(key, p.Name, StringComparison.InvariantCultureIgnoreCase)))
                {
                    editedProvider = p;
                }
            }

            var providerEnabled = false;
            var providerPriorityString = string.Empty;
            var providerOverride = false;

            foreach (GridColumn column in e.Item.OwnerTableView.Columns)
            {
                if (column is IGridEditableColumn)
                {
                    var editableCol = (IGridEditableColumn)column;


                    if ((editableCol.IsEditable))
                    {
                        var editor = editMan.GetColumnEditor(editableCol);

                        //var editorType = (editor).ToString();
                        object editorValue = null;

                        if ((editor is GridTextColumnEditor))
                        {
                            //editorText = ((GridTextColumnEditor) editor).Text;
                            editorValue = ((GridTextColumnEditor)editor).Text;
                        }

                        if ((editor is GridBoolColumnEditor))
                        {
                            //editorText = ((GridBoolColumnEditor) editor).Value.ToString();
                            editorValue = ((GridBoolColumnEditor)editor).Value;
                        }

                        if ((column.UniqueName == "Enabled"))
                        {
                            providerEnabled = Convert.ToBoolean(editorValue);
                        }
                        else if ((column.UniqueName == "Priority"))
                        {
                            providerPriorityString = editorValue.ToString();
                        }
                        else if ((column.UniqueName == "OverridePriority"))
                        {
                            providerOverride = Convert.ToBoolean(editorValue);
                        }
                    }
                }
            }

            float providerPriority;

            if ((float.TryParse(providerPriorityString, out providerPriority)))
            {
                editedProvider.Enabled = providerEnabled;
                editedProvider.OverridePriority = providerOverride;
                editedProvider.Priority = providerPriority;
            }
            else
            {
                UI.Skins.Skin.AddModuleMessage(this, Localization.GetString("valPriority.Text", LocalResourceFile), ModuleMessage.ModuleMessageType.RedError);
            }
        }

        protected void OnSearchEngineIndexChanged(object o, EventArgs e)
        {
            SetSearchEngineSubmissionURL();
        }

        protected void OnVerifyClick(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(txtVerification.Text) && txtVerification.Text.EndsWith(".html"))
            {
                if (!File.Exists(Globals.ApplicationMapPath + "\\" + txtVerification.Text))
                {
					//write SiteMap verification file
                    var objStream = File.CreateText(Globals.ApplicationMapPath + "\\" + txtVerification.Text);
                    objStream.WriteLine("Google SiteMap Verification File");
                    objStream.WriteLine(" - " + lnkSiteMapUrl.Text);
                    objStream.WriteLine(" - " + UserInfo.DisplayName);
                    objStream.Close();
                }
            }
        }

        #endregion

    }
}