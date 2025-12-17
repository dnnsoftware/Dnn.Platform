// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace Dnn.PersonaBar.Sites.Components
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.IO;
    using System.IO.Compression;
    using System.Linq;
    using System.Threading;
    using System.Web;
    using System.Web.UI.WebControls;
    using System.Xml;

    using Dnn.PersonaBar.Library.Controllers;
    using Dnn.PersonaBar.Library.Dto.Tabs;
    using Dnn.PersonaBar.Sites.Components.Dto;
    using Dnn.PersonaBar.Sites.Services.Dto;
    using DotNetNuke.Abstractions.Portals;
    using DotNetNuke.Abstractions.Portals.Templates;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Internal;
    using DotNetNuke.Common.Lists;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Host;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Portals.Templates;
    using DotNetNuke.Entities.Profile;
    using DotNetNuke.Entities.Tabs;
    using DotNetNuke.Entities.Urls;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Instrumentation;
    using DotNetNuke.Security.Permissions;
    using DotNetNuke.Services.FileSystem;
    using DotNetNuke.Services.Localization;
    using DotNetNuke.Services.Mail;

    using FileInfo = DotNetNuke.Services.FileSystem.FileInfo;

    public class SitesController
    {
        internal static readonly IList<string> ImageExtensions = new List<string>() { ".png", ".jpg", ".jpeg" };
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(SitesController));
        private readonly TabsController tabsController = new TabsController();

        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Breaking change")]
        public string LocalResourcesFile => Path.Combine("~/DesktopModules/admin/Dnn.PersonaBar/Modules/Dnn.Sites/App_LocalResources/Sites.resx");

        private static PortalSettings PortalSettings => PortalController.Instance.GetCurrentPortalSettings();

        private CultureDropDownTypes DisplayType { get; set; }

#if false
        private string IconHome { get; } = Globals.ResolveUrl("~/DesktopModules/Admin/Tabs/images/Icon_Home.png");

        private string IconPortal { get; } = Globals.ResolveUrl("~/DesktopModules/Admin/Tabs/images/Icon_Portal.png");

        private string AdminOnlyIcon { get; } = Globals.ResolveUrl("~/DesktopModules/Admin/Tabs/images/Icon_UserAdmin.png");

        private string RegisteredUsersIcon { get; } = Globals.ResolveUrl("~/DesktopModules/Admin/Tabs/images/Icon_User.png");

        private string IconPageDisabled { get; } = Globals.ResolveUrl("~/DesktopModules/Admin/Tabs/images/Icon_Disabled.png");

        private string IconPageHidden { get; } = Globals.ResolveUrl("~/DesktopModules/Admin/Tabs/images/Icon_Hidden.png");

        private string IconRedirect { get; } = Globals.ResolveUrl("~/DesktopModules/Admin/Tabs/images/Icon_Redirect.png");

        private string SecuredIcon { get; } = Globals.ResolveUrl("~/DesktopModules/Admin/Tabs/images/Icon_UserSecure.png");

        private string AllUsersIcon { get; } = Globals.ResolveUrl("~/DesktopModules/Admin/Tabs/images/Icon_Everyone.png");
#endif

        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Breaking change")]
        public IList<HttpAliasDto> FormatPortalAliases(int portalId)
        {
            var alias = PortalAliasController.Instance.GetPortalAliasesByPortalId(portalId).OrderByDescending(a => a.IsPrimary).FirstOrDefault();
            if (alias == null)
            {
                return null;
            }

            var httpAlias = Globals.AddHTTP(alias.HTTPAlias);
            var originalUrl = HttpContext.Current.Items["UrlRewrite:OriginalUrl"];

            httpAlias = Globals.AddPort(httpAlias, originalUrl?.ToString().ToLowerInvariant() ?? httpAlias);
            return new List<HttpAliasDto> { new HttpAliasDto { Url = alias.HTTPAlias, Link = httpAlias } };
        }

        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Breaking change")]
        public string FormatExpiryDate(DateTime dateTime)
        {
            var strDate = string.Empty;
            if (!Null.IsNull(dateTime))
            {
                strDate = dateTime.ToShortDateString();
            }

            return strDate;
        }

        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Breaking change")]
        public IList<IPortalTemplateInfo> GetPortalTemplates()
        {
            var templates = PortalTemplateController.Instance.GetPortalTemplates();
            templates = templates.OrderBy(x => x, new TemplateDisplayComparer()).ToList();
            return templates;
        }

        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Breaking change")]
        public IPortalTemplateInfo GetPortalTemplate(string fileName, string cultureCode)
        {
            return PortalTemplateController.Instance.GetPortalTemplate(fileName, cultureCode);
        }

        public ListItem CreateListItem(IPortalTemplateInfo template)
        {
            string text, value;
            var fileName = Path.GetFileName(template.TemplateFilePath);
            if (string.IsNullOrEmpty(template.CultureCode))
            {
                text = template.Name;
                value = $"{fileName}|{GetThumbnail(fileName)}";
            }
            else
            {
                if (this.DisplayType == 0)
                {
                    var viewType = Convert.ToString(DotNetNuke.Services.Personalization.Personalization.GetProfile("LanguageDisplayMode", "ViewType" + PortalSettings.Current.PortalId));
                    switch (viewType)
                    {
                        case "NATIVE":
                            this.DisplayType = CultureDropDownTypes.NativeName;
                            break;
                        case "ENGLISH":
                            this.DisplayType = CultureDropDownTypes.EnglishName;
                            break;
                        default:
                            this.DisplayType = CultureDropDownTypes.DisplayName;
                            break;
                    }
                }

                text = $"{template.Name} - {Localization.GetLocaleName(template.CultureCode, this.DisplayType)}";

                value = $"{fileName}|{template.CultureCode}|{GetThumbnail(fileName)}";
            }

            return new ListItem(text, value);
        }

        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Breaking change")]
        public string GetDefaultTemplate()
        {
            var templates = PortalController.Instance.GetAvailablePortalTemplates();
            var currentCulture = Thread.CurrentThread.CurrentUICulture.Name;

            var defaultTemplates =
                templates.Where(x => Path.GetFileNameWithoutExtension(x.TemplateFilePath) == "Default Website").ToList();

            var match = defaultTemplates.FirstOrDefault(x => x.CultureCode == currentCulture);
            if (match == null)
            {
                match = defaultTemplates.FirstOrDefault(x => x.CultureCode.StartsWith(currentCulture.Substring(0, 2)));
            }

            if (match == null)
            {
                match = defaultTemplates.FirstOrDefault(x => string.IsNullOrEmpty(x.CultureCode));
            }

            return match != null ? string.Format("{0}|{1}", Path.GetFileName(match.TemplateFilePath), match.CultureCode) : string.Empty;
        }

        public TabDto GetTabByCulture(int tabId, int portalId, string cultureCode)
        {
            return this.tabsController.GetTabByCulture(tabId, portalId, cultureCode);
        }

        public string ExportPortalTemplate(ExportTemplateRequest request, UserInfo userInfo, out bool success)
        {
            var pages = request.Pages.ToList();
            var isValid = true;
            success = false;

            // Verify all ancestor pages are selected
            foreach (var page in pages)
            {
                if (page.ParentTabId != Null.NullInteger && pages.All(p => p.TabId != page.ParentTabId.ToString(CultureInfo.InvariantCulture)))
                {
                    isValid = false;
                }
            }

            if (!isValid)
            {
                return Localization.GetString("ErrorAncestorPages", this.LocalResourcesFile);
            }

            if (pages.Count == 0)
            {
                return Localization.GetString("ErrorPages", this.LocalResourcesFile);
            }

            var portal = PortalController.Instance.GetPortal(request.PortalId);
            var tabsToExport = this.GetTabsToExport(userInfo, request.PortalId, portal.DefaultLanguage, request.IsMultilanguage, pages, null)
                                    .Select(t => int.Parse(t.TabId))
                                    .ToList();

            var exportResult = DotNetNuke.Entities.Portals.Templates.PortalTemplateController.Instance.ExportPortalTemplate(
                request.PortalId,
                request.FileName,
                request.Description,
                request.IsMultilanguage,
                request.Locales,
                request.LocalizationCulture,
                tabsToExport,
                request.IncludeContent,
                request.IncludeFiles,
                request.IncludeModules,
                request.IncludeProfile,
                request.IncludeRoles);

            success = exportResult.Success;
            return exportResult.Message;
        }

        public int CreatePortal(List<string> errors, string domainName, string serverPath, string siteTemplate, string siteName, string siteAlias, string siteDescription, string siteKeywords, bool isChildSite, string homeDirectory, int siteGroupId, bool useCurrent, string firstname, string lastname, string username, string email, string password, string confirm, string question = "", string answer = "")
        {
            var template = LoadPortalTemplateInfoForSelectedItem(siteTemplate);

            var strChildPath = string.Empty;
            var closePopUpStr = string.Empty;
            var intPortalId = -1;

            // check template validity
            var schemaFilename = HttpContext.Current.Server.MapPath("~/Components/Portals/portal.template.xsd");
            var xmlFilename = template.TemplateFilePath;
            var xval = new PortalTemplateValidator();
            if (!xval.Validate(xmlFilename, schemaFilename))
            {
                errors.AddRange(xval.Errors.OfType<string>());
                return intPortalId;
            }

            // Set Portal Name
            siteAlias = siteAlias.ToLowerInvariant().Replace("http://", string.Empty).Replace("https://", string.Empty);

            // Validate Portal Name
            var strPortalAlias = isChildSite
                ? PortalController.GetPortalFolder(siteAlias)
                : siteAlias;

            var error = false;
            var message = string.Empty;
            if (!PortalAliasController.ValidateAlias(strPortalAlias, isChildSite))
            {
                error = true;
                message = Localization.GetString("InvalidName", this.LocalResourcesFile);
            }

            // check whether have conflict between tab path and portal alias.
            var checkTabPath = string.Format("//{0}", strPortalAlias);
            if (TabController.GetTabByTabPath(PortalSettings.PortalId, checkTabPath, string.Empty) != Null.NullInteger
                || TabController.GetTabByTabPath(Null.NullInteger, checkTabPath, string.Empty) != Null.NullInteger)
            {
                error = true;
                message = Localization.GetString("DuplicateWithTab", this.LocalResourcesFile);
            }

            // Validate Password
            if (password != confirm)
            {
                error = true;
                if (!string.IsNullOrEmpty(message))
                {
                    message += "<br/>";
                }

                message += Localization.GetString("InvalidPassword", this.LocalResourcesFile);
            }

            // Set Portal Alias for Child Portals
            if (string.IsNullOrEmpty(message))
            {
                if (isChildSite)
                {
                    strChildPath = serverPath + strPortalAlias;

                    if (Directory.Exists(strChildPath))
                    {
                        error = true;
                        message = Localization.GetString("ChildExists", this.LocalResourcesFile);
                    }
                    else
                    {
                        strPortalAlias = siteAlias;
                    }
                }
            }

            // Get Home Directory
            var homeDir = homeDirectory != @"Portals/[PortalID]" ? homeDirectory : string.Empty;

            // Validate Home Folder
            if (!string.IsNullOrEmpty(homeDir))
            {
                var fullHomeDir = string.Format("{0}\\{1}\\", Globals.ApplicationMapPath, homeDir).Replace("/", "\\");
                if (Directory.Exists(fullHomeDir))
                {
                    error = true;
                    message = string.Format(Localization.GetString("CreatePortalHomeFolderExists.Error", this.LocalResourcesFile), homeDir);
                }

                if (homeDir.Contains("admin") || homeDir.Contains("DesktopModules") || homeDir.Equals("portals/", StringComparison.OrdinalIgnoreCase))
                {
                    error = true;
                    message = Localization.GetString("InvalidHomeFolder", this.LocalResourcesFile);
                }
            }

            // Validate Portal Alias
            if (!string.IsNullOrEmpty(strPortalAlias))
            {
                PortalAliasInfo portalAlias = null;
                foreach (PortalAliasInfo alias in PortalAliasController.Instance.GetPortalAliases().Values)
                {
                    if (string.Equals(alias.HTTPAlias, strPortalAlias, StringComparison.OrdinalIgnoreCase))
                    {
                        portalAlias = alias;
                        break;
                    }
                }

                if (portalAlias != null)
                {
                    error = true;
                    message = Localization.GetString("DuplicatePortalAlias", this.LocalResourcesFile);
                }
            }

            // Create Portal
            if (!error)
            {
                // Attempt to create the portal
                var adminUser = new UserInfo();
                try
                {
                    if (useCurrent)
                    {
                        adminUser = PortalSettings.Current.UserInfo;
                        intPortalId = PortalController.Instance.CreatePortal(
                            siteName,
                            adminUser.UserID,
                            siteDescription,
                            siteKeywords,
                            template,
                            homeDir,
                            strPortalAlias,
                            serverPath,
                            strChildPath,
                            isChildSite);
                    }
                    else
                    {
                        adminUser = new UserInfo
                        {
                            FirstName = firstname,
                            LastName = lastname,
                            Username = username,
                            DisplayName = firstname + " " + lastname,
                            Email = email,
                            IsSuperUser = false,
                            Membership =
                            {
                                Approved = true,
                                Password = password,
                                PasswordQuestion = question,
                                PasswordAnswer = answer,
                            },
                            Profile =
                            {
                                FirstName = firstname,
                                LastName = lastname,
                            },
                        };

                        intPortalId = PortalController.Instance.CreatePortal(
                            siteName,
                            adminUser,
                            siteDescription,
                            siteKeywords,
                            template,
                            homeDir,
                            strPortalAlias,
                            serverPath,
                            strChildPath,
                            isChildSite);
                    }
                }
                catch (Exception ex)
                {
                    Logger.Error(ex);

                    intPortalId = Null.NullInteger;
                    message = ex.Message;

                    TryDeleteCreatingPortal(serverPath, isChildSite ? strChildPath : string.Empty);
                }

                if (intPortalId != -1)
                {
                    // Add new portal to Site Group
                    if (siteGroupId != Null.NullInteger)
                    {
                        var portal = PortalController.Instance.GetPortal(intPortalId);
                        var portalGroup = PortalGroupController.Instance.GetPortalGroups().SingleOrDefault(g => g.PortalGroupId == siteGroupId);
                        if (portalGroup != null)
                        {
                            PortalGroupController.Instance.AddPortalToGroup(portal, portalGroup, args => { });
                        }
                    }

                    // Create a Portal Settings object for the new Portal
                    var objPortal = PortalController.Instance.GetPortal(intPortalId);
                    var newSettings = new PortalSettings
                    {
                        PortalAlias = new PortalAliasInfo { HTTPAlias = strPortalAlias },
                        PortalId = intPortalId,
                        DefaultLanguage = objPortal.DefaultLanguage,
                    };
                    var webUrl = Globals.AddHTTP(strPortalAlias);
                    try
                    {
                        message = string.IsNullOrEmpty(Host.HostEmail)
                            ? string.Format(Localization.GetString("UnknownEmailAddress.Error", this.LocalResourcesFile), message, webUrl, closePopUpStr)
                            : Mail.SendMail(
                                Host.HostEmail,
                                email,
                                Host.HostEmail,
                                Localization.GetSystemMessage(newSettings, "EMAIL_PORTAL_SIGNUP_SUBJECT", adminUser),
                                Localization.GetSystemMessage(newSettings, "EMAIL_PORTAL_SIGNUP_BODY", adminUser),
                                string.Empty,
                                string.Empty,
                                string.Empty,
                                string.Empty,
                                string.Empty,
                                string.Empty);
                    }
                    catch (Exception exc)
                    {
                        Logger.Error(exc);
                        message = string.Format(Localization.GetString("UnknownSendMail.Error", this.LocalResourcesFile), webUrl, closePopUpStr);
                    }

                    // mark default language as published if content localization is enabled
                    var contentLocalizationEnabled = PortalController.GetPortalSettingAsBoolean("ContentLocalizationEnabled", PortalSettings.PortalId, false);
                    if (contentLocalizationEnabled)
                    {
                        var lc = new LocaleController();
                        lc.PublishLanguage(intPortalId, objPortal.DefaultLanguage, true);
                    }

                    // Redirect to this new site
                    if (message != Null.NullString)
                    {
                        message = string.Format(Localization.GetString("SendMail.Error", this.LocalResourcesFile), message, webUrl, closePopUpStr);
                    }
                }
            }

            if (!string.IsNullOrEmpty(message))
            {
                errors.Add(message);
            }

            return intPortalId;
        }

        private static IPortalTemplateInfo LoadPortalTemplateInfoForSelectedItem(string template)
        {
            var values = template.Split('|');
            return PortalTemplateController.Instance.GetPortalTemplate(Path.Combine(TestableGlobals.Instance.HostMapPath, values[0]), values.Length > 1 ? values[1] : null);
        }

        private static void TryDeleteCreatingPortal(string serverPath, string childPath)
        {
            try
            {
                if (HttpContext.Current != null && HttpContext.Current.Items.Contains("CreatingPortalId"))
                {
                    var creatingPortalId = Convert.ToInt32(HttpContext.Current.Items["CreatingPortalId"]);
                    var portalInfo = PortalController.Instance.GetPortal(creatingPortalId);
                    PortalController.DeletePortal(portalInfo, serverPath);
                }

                if (!string.IsNullOrEmpty(childPath))
                {
                    PortalController.DeletePortalFolder(string.Empty, childPath);
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
        }

        private static string GetThumbnail(string templateName)
        {
            var filePath = Path.Combine(Globals.HostMapPath, templateName);
            var imagePath = string.Empty;
            foreach (var ext in ImageExtensions)
            {
                var path = Path.ChangeExtension(filePath, ext);
                if (File.Exists(path))
                {
                    imagePath = path;
                    break;
                }
            }

            imagePath = "~/" + imagePath.Replace(Globals.ApplicationMapPath, string.Empty)
                .TrimStart('\\')
                .Replace("\\", "/");

            return Globals.ResolveUrl(imagePath);
        }

        private List<TabDto> GetTabsToExport(UserInfo userInfo, int portalId, string cultureCode, bool isMultiLanguage, IEnumerable<TabDto> userSelection, List<TabDto> tabsCollection)
        {
            if (tabsCollection == null)
            {
                var tab = this.tabsController.GetPortalTabs(userInfo, portalId, cultureCode, isMultiLanguage);
                tabsCollection = tab.ChildTabs.ToList();
                tab.ChildTabs = null;
                tab.HasChildren = false;
                tabsCollection.Add(tab);
            }

            var selectedTabs = userSelection as List<TabDto> ?? userSelection.ToList();
            foreach (var tab in tabsCollection)
            {
                if (selectedTabs.Exists(x => x.TabId == tab.TabId))
                {
                    var existingTab = selectedTabs.First(x => x.TabId == tab.TabId);
                    tab.CheckedState = existingTab.CheckedState;
                    if (string.IsNullOrEmpty(Convert.ToString(existingTab.Name)))
                    {
                        selectedTabs.Remove(existingTab);
                        selectedTabs.Add(tab);
                    }
                }
                else
                {
                    selectedTabs.Add(tab);
                }

                if (tab.HasChildren)
                {
                    var checkedState = NodeCheckedState.UnChecked;
                    if (tab.CheckedState == NodeCheckedState.Checked)
                    {
                        checkedState = NodeCheckedState.Checked;
                    }

                    var descendants = this.tabsController.GetTabsDescendants(portalId, Convert.ToInt32(tab.TabId), cultureCode, isMultiLanguage).ToList();
                    descendants.ForEach(x => { x.CheckedState = checkedState; });

                    selectedTabs.AddRange(this.GetTabsToExport(userInfo, portalId, cultureCode, isMultiLanguage, selectedTabs, descendants).Where(x => !selectedTabs.Exists(y => y.TabId == x.TabId)));
                }
            }

            return selectedTabs;
        }

        private class TemplateDisplayComparer : IComparer<IPortalTemplateInfo>
        {
            public int Compare(IPortalTemplateInfo x, IPortalTemplateInfo y)
            {
                var cultureCompare = string.Compare(x.CultureCode, y.CultureCode, StringComparison.Ordinal);
                if (cultureCompare == 0)
                {
                    return string.Compare(x.Name, y.Name, StringComparison.Ordinal);
                }

                // put blank cultures last
                if (string.IsNullOrEmpty(x.CultureCode) || string.IsNullOrEmpty(y.CultureCode))
                {
                    cultureCompare *= -1;
                }

                return cultureCompare;
            }
        }
    }
}
