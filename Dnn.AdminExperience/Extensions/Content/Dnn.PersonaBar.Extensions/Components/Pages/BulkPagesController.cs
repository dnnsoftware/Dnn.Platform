using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Xml;
using Dnn.PersonaBar.Pages.Components.Exceptions;
using Dnn.PersonaBar.Pages.Services.Dto;
using DotNetNuke.Common;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Tabs;
using DotNetNuke.Framework;
using DotNetNuke.Security.Permissions;
using DotNetNuke.Services.Localization;
using DotNetNuke.Web.UI;

namespace Dnn.PersonaBar.Pages.Components
{
    public class BulkPagesController : ServiceLocator<IBulkPagesController, BulkPagesController>, IBulkPagesController
    {
        private static readonly Regex TabNameRegex = new Regex(">*(.*)", RegexOptions.Compiled);
        private const string DefaultPageTemplate = "Default.page.template";

        public BulkPageResponse AddBulkPages(BulkPage page, bool validateOnly)
        {
            var portalSettings = PortalController.Instance.GetCurrentPortalSettings();
            var portalId = portalSettings.PortalId;
            var response = new BulkPageResponse();
            var parentId = page.ParentId;
            var rootTab = TabController.Instance.GetTab(parentId, portalId, true);

            var strValue = page.BulkPages;
            strValue = strValue.Replace("\r", "\n").Replace("\n\n", "\n").Trim();

            string invalidType;
            if (!TabController.IsValidTabName(strValue, out invalidType))
            {
                throw new BulkPagesException("bulkPages", string.Format(Localization.GetString(invalidType), strValue));
            }

            if (page.StartDate.HasValue && page.EndDate.HasValue && page.StartDate > page.EndDate)
            {
                throw new BulkPagesException("endDate", Localization.GetString("StartDateAfterEndDate"));
            }

            var pages = strValue.Split('\n');
            var tabs = new List<TabInfo>();

            foreach (var strLine in pages)
            {
                var tab = new TabInfo
                    {
                        TabName = TabNameRegex.Replace(strLine, "${1}"),
                        Level = strLine.LastIndexOf(">", StringComparison.Ordinal) + 1,
                        KeyWords = page.Keywords,
                        StartDate = page.StartDate ?? Null.NullDate,
                        EndDate = page.EndDate ?? Null.NullDate,
                        IsVisible = page.IncludeInMenu
                };
                tab.Terms.AddRange(TermHelper.ToTabTerms(page.Tags, portalId));
                tabs.Add(tab);
            }

            var currentIndex = -1;
            var bulkPageItems = new List<BulkPageResponseItem>();
            foreach (var oTab in tabs)
            {
                currentIndex += 1;

                try
                {
                    string errorMessage = null;
                    if (oTab.Level == 0)
                    {
                        oTab.TabID = CreateTabFromParent(portalSettings, rootTab, oTab, parentId, validateOnly, out errorMessage);
                    }
                    else if (validateOnly)
                    {
                        errorMessage = string.Empty;
                    }
                    else
                    {
                        var parentTabId = GetParentTabId(tabs, currentIndex, oTab.Level - 1);
                        if (parentTabId != Null.NullInteger)
                        {
                            oTab.TabID = CreateTabFromParent(portalSettings, rootTab, oTab, parentTabId, validateOnly, out errorMessage);
                        }
                    }
                    bulkPageItems.Add(ToBulkPageResponseItem(oTab, errorMessage));
                }
                catch (Exception ex)
                {
                    throw new DotNetNukeException("Unable to process page.", ex, DotNetNukeErrorCode.DeserializePanesFailed);
                }
            }
            response.Pages = bulkPageItems;

            return response;
        }

        private static BulkPageResponseItem ToBulkPageResponseItem(TabInfo tab, string error)
        {
            return new BulkPageResponseItem
            {
                TabId = tab.TabID,
                ErrorMessage = error,
                Status = (error == null && tab.TabID > 0) ? 0 : 1,
                PageName = tab.TabName
            };
        }

        private int CreateTabFromParent(PortalSettings portalSettings, TabInfo objRoot, TabInfo oTab, int parentId, bool validateOnly, out string errorMessage)
        {
            var tab = new TabInfo
            {
                PortalID = portalSettings.PortalId,
                TabName = oTab.TabName,
                ParentId = parentId,
                Title = "",
                Description = "",
                KeyWords = oTab.KeyWords,
                IsVisible = oTab.IsVisible,
                DisableLink = false,
                IconFile = "",
                IconFileLarge = "",
                IsDeleted = false,
                Url = "",
                SkinSrc = "",
                ContainerSrc = "",
                CultureCode = Null.NullString,
                StartDate = oTab.StartDate,
                EndDate = oTab.EndDate
            };

            tab.Terms.AddRange(oTab.Terms);

            if (objRoot != null)
            {
                // TODO: To be retrieved once the parent tab  is selected?
                //tab.IsVisible = objRoot.IsVisible;
                tab.DisableLink = objRoot.DisableLink;
                tab.SkinSrc = objRoot.SkinSrc;
                tab.ContainerSrc = objRoot.ContainerSrc;
            }

            if (portalSettings.ContentLocalizationEnabled)
            {
                tab.CultureCode = LocaleController.Instance.GetDefaultLocale(tab.PortalID).Code;
            }

            var parentTab = TabController.Instance.GetTab(parentId, -1, false);

            if (parentTab != null)
            {
                tab.PortalID = parentTab.PortalID;
                tab.ParentId = parentTab.TabID;
            }
            else
            {
                //return Null.NullInteger;
                tab.PortalID = portalSettings.PortalId;
                tab.ParentId = Null.NullInteger;
            }

            tab.TabPath = Globals.GenerateTabPath(tab.ParentId, tab.TabName);

            //Check for invalid
            string invalidType;
            if (!TabController.IsValidTabName(tab.TabName, out invalidType))
            {
                errorMessage = string.Format(Localization.GetString(invalidType), tab.TabName);
                return Null.NullInteger;
            }

            //Validate Tab Path
            if (!IsValidTabPath(tab, tab.TabPath, out errorMessage))
            {
                return Null.NullInteger;
            }

            //Inherit permissions from parent
            tab.TabPermissions.Clear();
            if (tab.PortalID != Null.NullInteger && objRoot != null)
            {
                tab.TabPermissions.AddRange(objRoot.TabPermissions);
            }
            else if (tab.PortalID != Null.NullInteger)
            {
                //Give admin full permission
                ArrayList permissions = PermissionController.GetPermissionsByTab();

                foreach (PermissionInfo permission in permissions)
                {
                    var newTabPermission = new TabPermissionInfo
                    {
                        PermissionID = permission.PermissionID,
                        PermissionKey = permission.PermissionKey,
                        PermissionName = permission.PermissionName,
                        AllowAccess = true,
                        RoleID = portalSettings.AdministratorRoleId
                    };
                    tab.TabPermissions.Add(newTabPermission);
                }
            }

            //Inherit other information from Parent
            if (objRoot != null)
            {
                // TODO: To be retrieved once the parent tab  is selected?
                //tab.Terms.Clear();
                //tab.StartDate = objRoot.StartDate;
                //tab.EndDate = objRoot.EndDate;
                tab.RefreshInterval = objRoot.RefreshInterval;
                tab.SiteMapPriority = objRoot.SiteMapPriority;
                tab.PageHeadText = objRoot.PageHeadText;
                tab.IsSecure = objRoot.IsSecure;
                tab.PermanentRedirect = objRoot.PermanentRedirect;
            }

            if (validateOnly)
                return -1;

            tab.TabID = TabController.Instance.AddTab(tab);
            ApplyDefaultTabTemplate(tab);

            //create localized tabs if content localization is enabled
            if (portalSettings.ContentLocalizationEnabled)
            {
                TabController.Instance.CreateLocalizedCopies(tab);
            }

            return tab.TabID;
        }

        private static int GetParentTabId(List<TabInfo> lstTabs, int currentIndex, int parentLevel)
        {
            var oParent = lstTabs[0];

            for (var i = 0; i < lstTabs.Count; i++)
            {
                if (i == currentIndex)
                {
                    return oParent.TabID;
                }
                if (lstTabs[i].Level == parentLevel)
                {
                    oParent = lstTabs[i];
                }
            }

            return Null.NullInteger;
        }

        private void ApplyDefaultTabTemplate(TabInfo tab)
        {
            var portalSettings = PortalController.Instance.GetCurrentPortalSettings();
            var templateFile = Path.Combine(portalSettings.HomeDirectoryMapPath, "Templates\\" + DefaultPageTemplate);

            if (!File.Exists(templateFile))
            {
                return;
            }

            var xmlDoc = new XmlDocument { XmlResolver = null };
            try
            {
                xmlDoc.Load(templateFile);
                TabController.DeserializePanes(xmlDoc.SelectSingleNode("//portal/tabs/tab/panes"), tab.PortalID, tab.TabID, PortalTemplateModuleAction.Ignore, new Hashtable());
            }
            catch (Exception ex)
            {
                throw new DotNetNukeException("Unable to process page template.", ex, DotNetNukeErrorCode.DeserializePanesFailed);
            }
        }

        private bool IsValidTabPath(TabInfo tab, string newTabPath, out string errorMessage)
        {
            var valid = true;
            errorMessage = null;

            //get default culture if the tab's culture is null
            var cultureCode = tab.CultureCode;
            if (string.IsNullOrEmpty(cultureCode))
            {
                var portalSettings = PortalController.Instance.GetCurrentPortalSettings();
                cultureCode = portalSettings.DefaultLanguage;
            }

            //Validate Tab Path
            var tabId = TabController.GetTabByTabPath(tab.PortalID, newTabPath, cultureCode);
            if (tabId != Null.NullInteger && tabId != tab.TabID)
            {
                var existingTab = TabController.Instance.GetTab(tabId, tab.PortalID, false);
                if (existingTab != null && existingTab.IsDeleted)
                    errorMessage = Localization.GetString("TabRecycled");
                else
                    errorMessage = Localization.GetString("TabExists");

                valid = false;
            }

            //check whether have conflict between tab path and portal alias.
            if (TabController.IsDuplicateWithPortalAlias(tab.PortalID, newTabPath))
            {
                errorMessage = string.Format(Localization.GetString("PathDuplicateWithAlias"), tab.TabName, newTabPath);
                valid = false;
            }

            return valid;
        }

        protected override Func<IBulkPagesController> GetFactory()
        {
            return () => new BulkPagesController();
        }
    }
}