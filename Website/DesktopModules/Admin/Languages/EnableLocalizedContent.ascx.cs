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
using System.Linq;
using DotNetNuke.Common;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Tabs;
using DotNetNuke.Framework;
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
    public partial class EnableLocalizedContent : PortalModuleBase
    {
        private string _PortalDefault = "";
        private int timeout = 3600;

        #region Protected Properties

        protected string PortalDefault
        {
            get
            {
                return _PortalDefault;
            }
        }

        #endregion

        #region Private Methods
        
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

        protected bool IsDefaultLanguage(string code)
        {
            return code == PortalDefault;

        }

        protected bool IsLanguageEnabled(string Code)
        {
            Locale enabledLanguage;
            return LocaleController.Instance.GetLocales(ModuleContext.PortalId).TryGetValue(Code, out enabledLanguage);
        }

        private void PublishLanguage(string cultureCode, bool publish)
        {
            Dictionary<string, Locale> enabledLanguages = LocaleController.Instance.GetLocales(PortalId);
            Locale enabledlanguage;
            if (enabledLanguages.TryGetValue(cultureCode, out enabledlanguage))
            {
                enabledlanguage.IsPublished = publish;
                LocaleController.Instance.UpdatePortalLocale(enabledlanguage);
            }
        }

        private void ProcessLanguage(IEnumerable<TabInfo> pageList, Locale locale, int languageCount, int totalLanguages)
        {
            RadProgressContext progress = RadProgressContext.Current;

            progress.Speed = "N/A";
            progress.PrimaryTotal = totalLanguages;
            progress.PrimaryValue = languageCount;
            

            int total = pageList.Count();
            if (total == 0)
            {
                progress.SecondaryTotal = 0;
                progress.SecondaryValue = 0;
                progress.SecondaryPercent = 100;
            }

            for (int i = 0; i < total ; i++)
            {
                TabInfo currentTab = pageList.ElementAt(i);
                int stepNo = i + 1;

                progress.SecondaryTotal = total;
                progress.SecondaryValue = stepNo;
                float secondaryPercent = ((float) stepNo/(float) total) * 100;
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

                progress.TimeEstimated = (total - stepNo)*100;

                if (locale.Code == PortalDefault)
                {
                    TabController.Instance.LocalizeTab(currentTab, locale, true);
                }
                else
                {
                    TabController.Instance.CreateLocalizedCopy(currentTab, locale, false);
                }
                
                if ((i % 10) == 0)
                {
                    KeepConnectionAlive();
                }
            }
        }

        private IEnumerable<TabInfo> GetPages(int portalId)
        {
            return (from kvp in TabController.Instance.GetTabsByPortal(portalId)
                                      where !kvp.Value.TabPath.StartsWith("//Admin")
                                            && !kvp.Value.IsDeleted 
                                            && !kvp.Value.IsSystem
                                      select kvp.Value);
        } 
        #endregion

        #region Event Handlers

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            cancelButton.Click += cancelButton_Click;
            updateButton.Click += updateButton_Click;

            LocalResourceFile = Localization.GetResourceFile(this, "EnableLocalizedContent.ascx");
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
            
            int languageCount = LocaleController.Instance.GetLocales(PortalSettings.PortalId).Count;

            var pageList = GetPages(PortalId);

            int scriptTimeOut = Server.ScriptTimeout;
            Server.ScriptTimeout = timeout;

            int languageCounter = 0;
            if (chkAllPagesTranslatable.Checked)
            {
                ProcessLanguage(pageList, LocaleController.Instance.GetLocale(PortalDefault), languageCounter, languageCount);
            }
            PublishLanguage(PortalDefault, true);

            PortalController.UpdatePortalSetting(PortalId, "ContentLocalizationEnabled", "True");

            // populate other languages
            var defaultLanguage = PortalController.Instance.GetCurrentPortalSettings().DefaultLanguage;
            foreach (Locale locale in LocaleController.Instance.GetLocales(PortalSettings.PortalId).Values)
            {
                if (!IsDefaultLanguage(locale.Code))
                {
                    languageCounter += 1;
                    pageList = GetPages(PortalId).Where(p => p.CultureCode == defaultLanguage);
                        
                    //add translator role
                    Localization.AddTranslatorRole(PortalId, locale);

                    //populate pages
                    ProcessLanguage(pageList, locale, languageCounter, languageCount);

                    //Map special pages
                    PortalController.Instance.MapLocalizedSpecialPages(PortalSettings.PortalId, locale.Code);
                }
            }
            //Restore Script Timeout
            Server.ScriptTimeout = scriptTimeOut;
            //clear portal cache
            DataCache.ClearPortalCache(PortalId, true);
            //'Redirect to refresh page (and skinobjects)
            Response.End();
        }

        #endregion
    }
}