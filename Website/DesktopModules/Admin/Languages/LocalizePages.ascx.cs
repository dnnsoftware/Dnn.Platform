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

using DotNetNuke.Common;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Tabs;
using DotNetNuke.Framework;
using DotNetNuke.Services.Exceptions;
using DotNetNuke.Services.Localization;

using Telerik.Web.UI;
using Telerik.Web.UI.Upload;

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
//

#endregion

namespace DotNetNuke.Modules.Admin.Languages
{
    public partial class LocalizePages : PortalModuleBase
    {
        private string _PortalDefault = "";
        private int timeout = 3600;

        #region "Protected Properties"

        protected string PortalDefault
        {
            get
            {
                return _PortalDefault;
            }
        }

        protected string Locale
        {
            get
            {
                return Request.QueryString["locale"];
            }
        } 

        #endregion

        #region "Private Methods"

        protected bool IsDefaultLanguage(string code)
        {
            return code == PortalDefault;
        }

        protected bool IsLanguageEnabled(string Code)
        {
            Locale enabledLanguage = null;
            return LocaleController.Instance.GetLocales(ModuleContext.PortalId).TryGetValue(Code, out enabledLanguage);
        }

        private void ProcessLanguage(List<TabInfo> pageList, Locale locale, int languageCount, int totalLanguages)
        {
            try
            {
                RadProgressContext progress = RadProgressContext.Current;

                progress.Speed = "N/A";
                progress.PrimaryTotal = totalLanguages;
                progress.PrimaryValue = languageCount;

                int total = pageList.Count;
                for (int i = 0; i <= total - 1; i++)
                {
                    TabInfo currentTab = pageList[i];
                    int stepNo = i + 1;

                    progress.SecondaryTotal = total;
                    progress.SecondaryValue = stepNo;
                    float secondaryPercent = ((float)stepNo / (float)total) * 100;
                    progress.SecondaryPercent = Convert.ToInt32(secondaryPercent);
                    float primaryPercent = ((((float)languageCount + ((float)stepNo / (float)total)) / (float)totalLanguages)) * 100;
                    progress.PrimaryPercent = Convert.ToInt32(primaryPercent);

                    progress.CurrentOperationText = string.Format(Localization.GetString("ProcessingPage", LocalResourceFile), locale.Code, stepNo, total, currentTab.TabName);

                    if (!Response.IsClientConnected)
                    {
                        //clear cache
                        DataCache.ClearPortalCache(PortalId, true);

                        //Cancel button was clicked or the browser was closed, so stop processing
                        break;
                    }

                    progress.TimeEstimated = (total - stepNo) * 100;

                    TabController.Instance.CreateLocalizedCopy(currentTab, locale, false);
                    
                    if ((i % 10) == 0)
                    {
                        KeepConnectionAlive();
                    }
                }
                PortalController.Instance.MapLocalizedSpecialPages(PortalId, locale.Code);

            }
            catch (Exception exc)
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

        /// <summary>
        /// This Write/Flush is needed periodically to avoid issue in Azure.
        /// Azure Load Balancer silently dropping idle connections after 4 minutes.
        /// Sending some data from time to time to the client from server side, 
        /// the Azure Load balancer doesn't kill the TCP connection
        /// </summary>
        private void KeepConnectionAlive()
        {
            Response.Write(' ');
            Response.Flush();
        }
        #endregion

        #region "Event Handlers"

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            cancelButton.Click += cancelButton_Click;
            updateButton.Click += updateButton_Click;

            LocalResourceFile = Localization.GetResourceFile(this, "LocalizePages.ascx");
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            //Set AJAX timeout to 1 hr for large sites
            AJAX.GetScriptManager(Page).AsyncPostBackTimeout = timeout;

            _PortalDefault = PortalSettings.DefaultLanguage;
            defaultLanguageLabel.Language = PortalSettings.DefaultLanguage;
            defaultLanguageLabel.Visible = true;

            if (!IsPostBack)
            {
                //Do not display SelectedFilesCount progress indicator.
                pageCreationProgressArea.ProgressIndicators &= ~ProgressIndicators.SelectedFilesCount;
            }
            pageCreationProgressArea.ProgressIndicators &=  ~ProgressIndicators.TimeEstimated;
            pageCreationProgressArea.ProgressIndicators &=  ~ProgressIndicators.TransferSpeed;

            pageCreationProgressArea.Localization.Total = Localization.GetString("TotalLanguages", LocalResourceFile);
            pageCreationProgressArea.Localization.TotalFiles = Localization.GetString("TotalPages", LocalResourceFile);
            pageCreationProgressArea.Localization.Uploaded = Localization.GetString("TotalProgress", LocalResourceFile);
            pageCreationProgressArea.Localization.UploadedFiles = Localization.GetString("Progress", LocalResourceFile);
            pageCreationProgressArea.Localization.CurrentFileName = Localization.GetString("Processing", LocalResourceFile);


            var pageList = GetTabsToLocalize(PortalId, Locale);
            PagesToLocalize.Text = pageList.Count.ToString(CultureInfo.InvariantCulture);
            if (pageList.Count == 0)
            {
                updateButton.Enabled = false;
            }
        }

        public List<TabInfo> GetTabsToLocalize(int portalId, string code)
        {
            var results = new List<TabInfo>();
            var portalTabs = TabController.Instance.GetTabsByPortal(portalId);
            foreach (var kvp in portalTabs.Where(kvp => kvp.Value.CultureCode == PortalSettings.DefaultLanguage && !kvp.Value.IsDeleted))
            {
                if (kvp.Value.LocalizedTabs.Count == 0)
                {
                    results.Add(kvp.Value);
                }
                else
                {
                    bool tabLocalizedInCulture = kvp.Value.LocalizedTabs.Any(localizedTab => localizedTab.Value.CultureCode == code);
                    if (!tabLocalizedInCulture)
                    {
                        results.Add(kvp.Value);
                    }
                }
            }
            return results;

        }

        protected void cancelButton_Click(object sender, EventArgs e)
        {
            //Redirect to refresh page (and skinobjects)
            Response.Redirect(Globals.NavigateURL(), true);
        }

        protected void updateButton_Click(object sender, EventArgs e)
        {
            // Set RedirectLocation header before make any Write/Flush to keep connection alive
            // This prevents "Cannot redirect after HTTP headers have been sent" error
            Response.RedirectLocation = Globals.NavigateURL();

            var locale = LocaleController.Instance.GetLocale(Locale);
            List<TabInfo> pageList = GetTabsToLocalize(PortalId, Locale);

            int scriptTimeOut = Server.ScriptTimeout;
            Server.ScriptTimeout = timeout;

            //add translator role
            Localization.AddTranslatorRole(PortalId, locale);

            //populate pages
            ProcessLanguage(pageList, locale, 0, 1);

            //Map special pages
            PortalController.Instance.MapLocalizedSpecialPages(PortalSettings.PortalId, locale.Code);

            //clear cache
            DataCache.ClearPortalCache(PortalId, true);

            //Restore Script Timeout
            Server.ScriptTimeout = scriptTimeOut;
            //'Redirect to refresh page (and skinobjects)
            Response.End();
        }

        #endregion
    }
}