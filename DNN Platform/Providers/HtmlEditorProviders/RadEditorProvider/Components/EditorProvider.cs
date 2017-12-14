#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2017
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
using System.IO;
using System.Reflection;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Xml;

using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Host;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Tabs;
using DotNetNuke.Entities.Users;
using DotNetNuke.Framework;
using DotNetNuke.Instrumentation;
using DotNetNuke.Modules.HTMLEditorProvider;
using DotNetNuke.RadEditorProvider.Components;
using DotNetNuke.Security;
using DotNetNuke.Security.Roles;
using DotNetNuke.Services.FileSystem;
using DotNetNuke.Services.Localization;
using DotNetNuke.UI;
using DotNetNuke.Web.Client.ClientResourceManagement;
using Telerik.Web.UI;
using DotNetNuke.UI.Utilities;

#endregion

namespace DotNetNuke.Providers.RadEditorProvider
{
    public class EditorProvider : HtmlEditorProvider
    {
        private const string moduleFolderPath = "~/DesktopModules/Admin/RadEditorProvider/";
        private const string ProviderType = "htmlEditor";

        #region Properties

        public override ArrayList AdditionalToolbars { get; set; }

        ///<summary>
        ///</summary>
        public override string ControlID
        {
            get
            {
                return _editor.ID;
            }
            set
            {
                _editor.ID = value;
            }
        }

        public override Unit Height
        {
            get
            {
                return _editor.Height;
            }
            set
            {
                if (! value.IsEmpty)
                {
                    _editor.Height = value;
                }
            }
        }

        public override Control HtmlEditorControl
        {
            get
            {
                return _panel;
            }
        }

        public override string RootImageDirectory { get; set; }

        public override string Text
        {
            get
            {
                return _editor.Content;
            }
            set
            {
                _editor.Content = value;
            }
        }

        public override Unit Width
        {
            get
            {
                return _editor.Width;
            }
            set
            {
                if (! value.IsEmpty)
                {
                    _editor.Width = value;
                }
            }
        }

        public string ConfigFile
        {
            get
            {
                //get current user
                UserInfo objUserInfo = UserController.Instance.GetCurrentUserInfo();
                //load default tools file
                string tempConfigFile = ConfigFileName;
                //get absolute path of default tools file
                string path = HttpContext.Current.Server.MapPath(tempConfigFile).ToLower();

                string rolepath = "";
                string tabpath = "";
                string portalpath = "";

                //lookup host specific config file
                if (objUserInfo != null)
                {
                    if (objUserInfo.IsSuperUser)
                    {
                        var hostPart = ".RoleId." + DotNetNuke.Common.Globals.glbRoleSuperUser;
                        rolepath = path.Replace(".xml", hostPart + ".xml");
                        tabpath = path.Replace(".xml", hostPart + ".TabId." + PortalSettings.ActiveTab.TabID + ".xml");
                        portalpath = path.Replace(".xml", hostPart + ".PortalId." + PortalSettings.PortalId + ".xml");

                        if (File.Exists(tabpath))
                        {
                            return tempConfigFile.ToLower().Replace(".xml", hostPart + ".TabId." + PortalSettings.ActiveTab.TabID + ".xml");
                        }
                        if (File.Exists(portalpath))
                        {
                            return tempConfigFile.ToLower().Replace(".xml", hostPart + ".PortalId." + PortalSettings.PortalId + ".xml");
                        }
                        if (File.Exists(rolepath))
                        {
                            return tempConfigFile.ToLower().Replace(".xml", hostPart + ".xml");
                        }
                    }

                    //lookup admin specific config file
                    if (PortalSecurity.IsInRole(PortalSettings.AdministratorRoleName))
                    {
                        var adminPart = ".RoleId." + PortalSettings.AdministratorRoleId;
                        rolepath = path.Replace(".xml", adminPart + ".xml");
                        tabpath = path.Replace(".xml", adminPart + ".TabId." + PortalSettings.ActiveTab.TabID + ".xml");
                        portalpath = path.Replace(".xml", adminPart + ".PortalId." + PortalSettings.PortalId + ".xml");

                        if (File.Exists(tabpath))
                        {
                            return tempConfigFile.ToLower().Replace(".xml", adminPart + ".TabId." + PortalSettings.ActiveTab.TabID + ".xml");
                        }
                        if (File.Exists(portalpath))
                        {
                            return tempConfigFile.ToLower().Replace(".xml", adminPart + ".PortalId." + PortalSettings.PortalId + ".xml");
                        }
                        if (File.Exists(rolepath))
                        {
                            return tempConfigFile.ToLower().Replace(".xml", adminPart + ".xml");
                        }
                    }

                    //lookup user roles specific config file
                    foreach (var role in RoleController.Instance.GetUserRoles(objUserInfo, true))
                    {
                        var rolePart = ".RoleId." + role.RoleID;
                        rolepath = path.Replace(".xml", rolePart + ".xml");
                        tabpath = path.Replace(".xml", rolePart + ".TabId." + PortalSettings.ActiveTab.TabID + ".xml");
                        portalpath = path.Replace(".xml", rolePart + ".PortalId." + PortalSettings.PortalId + ".xml");

                        if (File.Exists(tabpath))
                        {
                            return tempConfigFile.ToLower().Replace(".xml", rolePart + ".TabId." + PortalSettings.ActiveTab.TabID + ".xml");
                        }
                        if (File.Exists(portalpath))
                        {
                            return tempConfigFile.ToLower().Replace(".xml", rolePart + ".PortalId." + PortalSettings.PortalId + ".xml");
                        }
                        if (File.Exists(rolepath))
                        {
                            return tempConfigFile.ToLower().Replace(".xml", rolePart + ".xml");
                        }
                    }
                }

                //lookup tab specific config file
                tabpath = path.Replace(".xml", ".TabId." + PortalSettings.ActiveTab.TabID + ".xml");
                if (File.Exists(tabpath))
                {
                    return tempConfigFile.ToLower().Replace(".xml", ".TabId." + PortalSettings.ActiveTab.TabID + ".xml");
                }

                //lookup portal specific config file
                portalpath = path.Replace(".xml", ".PortalId." + PortalSettings.PortalId + ".xml");
                if (File.Exists(portalpath))
                {
                    return tempConfigFile.ToLower().Replace(".xml", ".PortalId." + PortalSettings.PortalId + ".xml");
                }

                //nothing else found, return default file
                EnsureDefaultFileExists(path);

                return tempConfigFile;
            }
        }

        internal static void EnsureDefaultConfigFileExists()
        {
            EnsureDefaultFileExists(HttpContext.Current.Server.MapPath(ConfigFileName));
        }

        internal static void EnsurecDefaultToolsFileExists()
        {
            EnsureDefaultFileExists(HttpContext.Current.Server.MapPath(ToolsFileName));
        }

        private static void EnsureDefaultFileExists(string path)
        {
            if (!File.Exists(path))
            {
                string filePath = Path.GetDirectoryName(path);
                string name = "default." + Path.GetFileName(path);
                string defaultConfigFile = Path.Combine(filePath, name);

                //if defaultConfigFile is missing there is a big problem
                //let the error propogate to the module level
                File.Copy(defaultConfigFile, path, true);
            }
        }

        public string ToolsFile
        {
            get
            {
                //get current user
                UserInfo objUserInfo = UserController.Instance.GetCurrentUserInfo();
                //load default tools file
                string tempToolsFile = ToolsFileName;
                //get absolute path of default tools file
                string path = HttpContext.Current.Server.MapPath(tempToolsFile).ToLower();

                string rolepath = "";
                string tabpath = "";
                string portalpath = "";

                //lookup host specific tools file
                if (objUserInfo != null)
                {
                    if (objUserInfo.IsSuperUser)
                    {
                        var hostPart = ".RoleId." + DotNetNuke.Common.Globals.glbRoleSuperUser;
                        rolepath = path.Replace(".xml", hostPart + ".xml");
                        tabpath = path.Replace(".xml", hostPart + ".TabId." + PortalSettings.ActiveTab.TabID + ".xml");
                        portalpath = path.Replace(".xml", hostPart + ".PortalId." + PortalSettings.PortalId + ".xml");

                        if (File.Exists(tabpath))
                        {
                            return tempToolsFile.ToLower().Replace(".xml", hostPart + ".TabId." + PortalSettings.ActiveTab.TabID + ".xml");
                        }
                        if (File.Exists(portalpath))
                        {
                            return tempToolsFile.ToLower().Replace(".xml", hostPart + ".PortalId." + PortalSettings.PortalId + ".xml");
                        }
                        if (File.Exists(rolepath))
                        {
                            return tempToolsFile.ToLower().Replace(".xml", hostPart + ".xml");
                        }
                    }

                    //lookup admin specific tools file
                    if (PortalSecurity.IsInRole(PortalSettings.AdministratorRoleName))
                    {
                        var adminPart = ".RoleId." + PortalSettings.AdministratorRoleId;
                        rolepath = path.Replace(".xml", adminPart + ".xml");
                        tabpath = path.Replace(".xml", adminPart + ".TabId." + PortalSettings.ActiveTab.TabID + ".xml");
                        portalpath = path.Replace(".xml", adminPart + ".PortalId." + PortalSettings.PortalId + ".xml");

                        if (File.Exists(tabpath))
                        {
                            return tempToolsFile.ToLower().Replace(".xml", adminPart + ".TabId." + PortalSettings.ActiveTab.TabID + ".xml");
                        }
                        if (File.Exists(portalpath))
                        {
                            return tempToolsFile.ToLower().Replace(".xml", adminPart + ".PortalId." + PortalSettings.PortalId + ".xml");
                        }
                        if (File.Exists(rolepath))
                        {
                            return tempToolsFile.ToLower().Replace(".xml", adminPart + ".xml");
                        }
                    }

                    //lookup user roles specific tools file
                    foreach (var role in RoleController.Instance.GetUserRoles(objUserInfo, false))
                    {
                        var rolePart = ".RoleId." + role.RoleID;
                        rolepath = path.Replace(".xml", rolePart + ".xml");
                        tabpath = path.Replace(".xml", rolePart + ".TabId." + PortalSettings.ActiveTab.TabID + ".xml");
                        portalpath = path.Replace(".xml", rolePart + ".PortalId." + PortalSettings.PortalId + ".xml");

                        if (File.Exists(tabpath))
                        {
                            return tempToolsFile.ToLower().Replace(".xml", rolePart + ".TabId." + PortalSettings.ActiveTab.TabID + ".xml");
                        }
                        if (File.Exists(portalpath))
                        {
                            return tempToolsFile.ToLower().Replace(".xml", rolePart + ".PortalId." + PortalSettings.PortalId + ".xml");
                        }
                        if (File.Exists(rolepath))
                        {
                            return tempToolsFile.ToLower().Replace(".xml", rolePart + ".xml");
                        }
                    }
                }

                //lookup tab specific tools file
                tabpath = path.Replace(".xml", ".TabId." + PortalSettings.ActiveTab.TabID + ".xml");
                if (File.Exists(tabpath))
                {
                    return tempToolsFile.ToLower().Replace(".xml", ".TabId." + PortalSettings.ActiveTab.TabID + ".xml");
                }

                //lookup portal specific tools file
                portalpath = path.Replace(".xml", ".PortalId." + PortalSettings.PortalId + ".xml");
                if (File.Exists(portalpath))
                {
                    return tempToolsFile.ToLower().Replace(".xml", ".PortalId." + PortalSettings.PortalId + ".xml");
                }

                //nothing else found, return default file
                EnsureDefaultFileExists(path);

                return tempToolsFile;
            }
        }

        #endregion

        #region Private Helper Methods

        private string AddSlash(string Folderpath)
        {
            if (Folderpath.StartsWith("/"))
            {
                return Folderpath.Replace("//", "/");
            }
            else
            {
                return "/" + Folderpath;
            }
        }

        private string RemoveEndSlash(string Folderpath)
        {
            if (Folderpath.EndsWith("/"))
            {
                return Folderpath.Substring(0, Folderpath.LastIndexOf("/"));
            }
            else
            {
                return Folderpath;
            }
        }

        private void PopulateFolder(string folderPath, string toolname)
        {
            var ReadPaths = new ArrayList();
            var WritePaths = new ArrayList();

            if (folderPath == "[PORTAL]")
            {
                ReadPaths.Add(RootImageDirectory);
                WritePaths.Add(RootImageDirectory);
            }
			else if (folderPath.ToUpperInvariant() == "[USERFOLDER]")
			{
				var userFolderPath = FolderManager.Instance.GetUserFolder(UserController.Instance.GetCurrentUserInfo()).FolderPath;
				var path = RemoveEndSlash(RootImageDirectory) + AddSlash(userFolderPath);
				WritePaths.Add(path);
				ReadPaths.Add(path);
			}
            else if (folderPath.Length > 0)
            {
                string path = RemoveEndSlash(RootImageDirectory) + AddSlash(folderPath);
                WritePaths.Add(path);
                ReadPaths.Add(path);
            }

            var _readPaths = (string[]) (ReadPaths.ToArray(typeof (string)));
            var _writePaths = (string[]) (WritePaths.ToArray(typeof (string)));

            switch (toolname)
            {
                case "ImageManager":
                    SetFolderPaths(_editor.ImageManager, _readPaths, _writePaths, true, true);
                    break;
                case "FlashManager":
                    SetFolderPaths(_editor.FlashManager, _readPaths, _writePaths, true, true);
                    break;
                case "MediaManager":
                    SetFolderPaths(_editor.MediaManager, _readPaths, _writePaths, true, true);
                    break;
                case "DocumentManager":
                    SetFolderPaths(_editor.DocumentManager, _readPaths, _writePaths, true, true);
                    break;
                case "TemplateManager":
                    SetFolderPaths(_editor.TemplateManager, _readPaths, _writePaths, true, true);
                    break;
                case "SilverlightManager":
                    SetFolderPaths(_editor.SilverlightManager, _readPaths, _writePaths, true, true);
                    break;
            }
        }

        public override void AddToolbar()
        {
            //must override...
        }

        private void SetFolderPaths(FileManagerDialogConfiguration manager, string[] readPaths, string[] writePaths, bool setDeletePath, bool setUploadPath)
        {
            manager.ViewPaths = readPaths;
            if (setUploadPath)
            {
                manager.UploadPaths = writePaths;
            }
            else
            {
                manager.UploadPaths = null;
            }
            if (setDeletePath)
            {
                manager.DeletePaths = writePaths;
            }
            else
            {
                manager.DeletePaths = null;
            }
        }

        private EditorLink AddLink(TabInfo objTab, ref EditorLink parent)
        {
            string linkUrl = string.Empty;
            if (! objTab.DisableLink)
            {
	            switch (_linksType.ToUpperInvariant())
	            {
					case "USETABNAME":
						var nameLinkFormat = "http://{0}/Default.aspx?TabName={1}";
						linkUrl = string.Format(nameLinkFormat, PortalSettings.PortalAlias.HTTPAlias, HttpUtility.UrlEncode(objTab.TabName));
						break;
					case "USETABID":
						var idLinkFormat = "http://{0}/Default.aspx?TabId={1}";
						linkUrl = string.Format(idLinkFormat, PortalSettings.PortalAlias.HTTPAlias, objTab.TabID);
						break;
					default:
						linkUrl = objTab.FullUrl;
						break;
	            }
                if (_linksUseRelativeUrls && (linkUrl.StartsWith("http://") || linkUrl.StartsWith("https://")))
                {
                    int linkIndex = linkUrl.IndexOf("/", 8);
                    if (linkIndex > 0)
                    {
                        linkUrl = linkUrl.Substring(linkIndex);
                    }
                }
            }
            var newLink = new EditorLink(objTab.LocalizedTabName.Replace("\\", "\\\\"), linkUrl);
            parent.ChildLinks.Add(newLink);
            return newLink;
        }

        private void AddChildLinks(int TabId, ref EditorLink links)
        {
            List<TabInfo> tabs = TabController.GetPortalTabs(PortalSettings.PortalId, Null.NullInteger, false, "", true, false, true, true, false);
            foreach (TabInfo objTab in tabs)
            {
                if (objTab.ParentId == TabId)
                {
                    //these are viewable children (current user's rights)
                    if (objTab.HasChildren)
                    {
                        //has more children
                        EditorLink tempVar = AddLink(objTab, ref links);
                        AddChildLinks(objTab.TabID, ref tempVar);
                    }
                    else
                    {
                        AddLink(objTab, ref links);
                    }
                }
            }
        }

        private void AddPortalLinks()
        {
            var portalLinks = new EditorLink(Localization.GetString("PortalLinks", Localization.GlobalResourceFile), string.Empty);
            _editor.Links.Add(portalLinks);

            //Add links to custom link menu
            List<TabInfo> tabs = TabController.GetPortalTabs(PortalSettings.PortalId, Null.NullInteger, false, "", true, false, true, true, false);
            foreach (TabInfo objTab in tabs)
            {
                //check permissions and visibility of current tab
                if (objTab.Level == 0)
                {
                    if (objTab.HasChildren)
                    {
                        //is a root tab, and has children
                        EditorLink tempVar = AddLink(objTab, ref portalLinks);
                        AddChildLinks(objTab.TabID, ref tempVar);
                    }
                    else
                    {
                        AddLink(objTab, ref portalLinks);
                    }
                }
            }
        }

        #endregion

        #region Public Methods

        public override void Initialize()
        {
            _editor.ToolsFile = ToolsFile;
            _editor.EnableViewState = false;   

            _editor.ExternalDialogsPath = moduleFolderPath + "Dialogs/";
            _editor.OnClientCommandExecuting = "OnClientCommandExecuting";


            if (! string.IsNullOrEmpty(ConfigFile))
            {
                XmlDocument xmlDoc = GetValidConfigFile();
                var colorConverter = new WebColorConverter();
                var items = new ArrayList();

                if (xmlDoc != null)
                {
                    foreach (XmlNode node in xmlDoc.DocumentElement.ChildNodes)
                    {
                        if (node.Attributes == null || node.Attributes["name"] == null || node.InnerText.Length == 0)
                        {
                            continue;
                        }

                        string propertyName = node.Attributes["name"].Value;
                        string propValue = node.InnerText;
                        //use reflection to set all string and bool properties
                        SetEditorProperty(propertyName, propValue);
                        //the following collections are handled by the tools file now:
                        //CssFiles, Colors, Symbols, Links, FontNames, FontSizes, Paragraphs, RealFontSizes
                        //CssClasses, Snippets, Languages
                        switch (propertyName)
                        {
                            case "AutoResizeHeight":
                                {
                                    _editor.AutoResizeHeight = bool.Parse(node.InnerText);
                                    break;
                                }
                            case "BorderWidth":
                                {
                                    _editor.BorderWidth = Unit.Parse(node.InnerText);
                                    break;
                                }
                            case "EnableResize":
                                {
                                    _editor.EnableResize = bool.Parse(node.InnerText);
                                    break;
                                }
                            case "NewLineBr":
                                {
                                    //use NewLineMode as NewLineBR has been obsoleted
                                    if (bool.Parse(node.InnerText)==true)
                                    {
                                        _editor.NewLineMode = EditorNewLineModes.Br;
                                        }
                                    else
                                    {
                                        _editor.NewLineMode = EditorNewLineModes.P; 
                                    }
                                    break;
                                }
                            case "Height":
                                {
                                    _editor.Height = Unit.Parse(node.InnerText);
                                    break;
                                }
                            case "Width":
                                {
                                    _editor.Width = Unit.Parse(node.InnerText);
                                    break;
                                }
                            case "ScriptToLoad":
                                {
                                    string path = Context.Request.MapPath(PortalSettings.ActiveTab.SkinPath) + node.InnerText;
                                    if (File.Exists(path))
                                    {
                                        _scripttoload = PortalSettings.ActiveTab.SkinPath + node.InnerText;
                                    }
                                    break;
                                }
                            case "ContentFilters":
                                {
                                    _editor.ContentFilters = (EditorFilters) (Enum.Parse(typeof (EditorFilters), node.InnerText));
                                    break;
                                }
                            case "ToolbarMode":
                                {
                                    _editor.ToolbarMode = (EditorToolbarMode) (Enum.Parse(typeof (EditorToolbarMode), node.InnerText, true));
                                    break;
                                }
                            case "EditModes":
                                {
                                    _editor.EditModes = (EditModes) (Enum.Parse(typeof (EditModes), node.InnerText, true));
                                    break;
                                }
                            case "StripFormattingOptions":
                                {
                                    _editor.StripFormattingOptions = (EditorStripFormattingOptions) (Enum.Parse(typeof (EditorStripFormattingOptions), node.InnerText, true));
                                    break;
                                }
                            case "MaxImageSize":
                                {
                                    _editor.ImageManager.MaxUploadFileSize = int.Parse(node.InnerText);
                                    break;
                                }
                            case "MaxFlashSize":
                                {
                                    _editor.FlashManager.MaxUploadFileSize = int.Parse(node.InnerText);
                                    break;
                                }
                            case "MaxMediaSize":
                                {
                                    _editor.MediaManager.MaxUploadFileSize = int.Parse(node.InnerText);
                                    break;
                                }
                            case "MaxDocumentSize":
                                {
                                    _editor.DocumentManager.MaxUploadFileSize = int.Parse(node.InnerText);
                                    break;
                                }
                            case "MaxTemplateSize":
                                {
                                    _editor.TemplateManager.MaxUploadFileSize = int.Parse(node.InnerText);
                                    break;
                                }
                            case "MaxSilverlightSize":
                                {
                                    _editor.SilverlightManager.MaxUploadFileSize = int.Parse(node.InnerText);
                                    break;
                                }
                            case "FileBrowserContentProviderTypeName":
                                {
                                    _editor.ImageManager.ContentProviderTypeName = node.InnerText;
                                    _editor.FlashManager.ContentProviderTypeName = node.InnerText;
                                    _editor.MediaManager.ContentProviderTypeName = node.InnerText;
                                    _editor.DocumentManager.ContentProviderTypeName = node.InnerText;
                                    _editor.TemplateManager.ContentProviderTypeName = node.InnerText;
                                    _editor.SilverlightManager.ContentProviderTypeName = node.InnerText;
                                    break;
                                }
                            case "SpellAllowAddCustom":
                                {
                                    // RadSpell properties
                                    _editor.SpellCheckSettings.AllowAddCustom = bool.Parse(node.InnerText);
                                    break;
                                }
                            case "SpellCustomDictionarySourceTypeName":
                                {
                                    _editor.SpellCheckSettings.CustomDictionarySourceTypeName = node.InnerText;
                                    break;
                                }
                            case "SpellCustomDictionarySuffix":
                                {
                                    _editor.SpellCheckSettings.CustomDictionarySuffix = node.InnerText;
                                    break;
                                }
                            case "SpellDictionaryPath":
                                {
                                    _editor.SpellCheckSettings.DictionaryPath = node.InnerText;
                                    break;
                                }
                            case "SpellDictionaryLanguage":
                                {
                                    _editor.SpellCheckSettings.DictionaryLanguage = node.InnerText;
                                    break;
                                }
                            case "SpellEditDistance":
                                {
                                    _editor.SpellCheckSettings.EditDistance = int.Parse(node.InnerText);
                                    break;
                                }
                            case "SpellFragmentIgnoreOptions":
                                {
                                    //SpellCheckSettings.FragmentIgnoreOptions = (FragmentIgnoreOptions)Enum.Parse(typeof(FragmentIgnoreOptions), node.InnerText, true);
                                    break;
                                }
                            case "SpellCheckProvider":
                                {
                                    _editor.SpellCheckSettings.SpellCheckProvider = (SpellCheckProvider) (Enum.Parse(typeof (SpellCheckProvider), node.InnerText, true));
                                    break;
                                }
                            case "SpellWordIgnoreOptions":
                                {
                                    _editor.SpellCheckSettings.WordIgnoreOptions = (WordIgnoreOptions) (Enum.Parse(typeof (WordIgnoreOptions), node.InnerText, true));
                                    break;
                                }
                            case "ImagesPath":
                                {
                                    PopulateFolder(node.InnerText, "ImageManager");
                                    break;
                                }
                            case "FlashPath":
                                {
                                    PopulateFolder(node.InnerText, "FlashManager");
                                    break;
                                }
                            case "MediaPath":
                                {
                                    PopulateFolder(node.InnerText, "MediaManager");
                                    break;
                                }
                            case "DocumentsPath":
                                {
                                    PopulateFolder(node.InnerText, "DocumentManager");
                                    break;
                                }
                            case "TemplatePath":
                                {
                                    PopulateFolder(node.InnerText, "TemplateManager");
                                    break;
                                }
                            case "SilverlightPath":
                                {
                                    PopulateFolder(node.InnerText, "SilverlightManager");
                                    break;
                                }
                            case "ContentAreaMode":
                                {
                                    _editor.ContentAreaMode = (EditorContentAreaMode)Enum.Parse(typeof(EditorContentAreaMode), node.InnerText);
                                    break;
                                }
                            case "LinksType":
                                {
                                    try
                                    {
                                        _linksType = node.InnerText;
                                    }
                                    catch
                                    {
                                    }
                                    break;
                                }
                            case "LinksUseRelativeUrls":
                                {
                                    try
                                    {
                                        _linksUseRelativeUrls = bool.Parse(node.InnerText);
                                    }
                                    catch
                                    {
                                    }
                                    break;
                                }
                            case "ShowPortalLinks":
                                {
                                    try
                                    {
                                        _ShowPortalLinks = bool.Parse(node.InnerText);
                                    }
                                    catch
                                    {
                                    }
                                    break;
                                }
                            case "CssFile":
                                {
                                    string path = Context.Request.MapPath(PortalSettings.ActiveTab.SkinPath) + node.InnerText;
                                    if (File.Exists(path))
                                    {
                                        _editor.CssFiles.Clear();
                                        _editor.CssFiles.Add(PortalSettings.ActiveTab.SkinPath + node.InnerText);
                                    }
                                    else
                                    {
                                        path = Context.Request.MapPath(PortalSettings.HomeDirectory) + node.InnerText;
                                        if (File.Exists(path))
                                        {
                                            _editor.CssFiles.Clear();
                                            _editor.CssFiles.Add(PortalSettings.HomeDirectory + node.InnerText);
                                        }
                                    }

                                    break;
                                }
                            default:
                                {
                                    // end of RadSpell properties
                                    if (propertyName.EndsWith("Filters"))
                                    {
                                        items.Clear();

                                        if (node.HasChildNodes)
                                        {
                                            if (node.ChildNodes.Count == 1)
                                            {
                                                if (node.ChildNodes[0].NodeType == XmlNodeType.Text)
                                                {
                                                    items.Add(node.InnerText);
                                                }
                                                else if (node.ChildNodes[0].NodeType == XmlNodeType.Element)
                                                {
                                                    items.Add(node.ChildNodes[0].InnerText);
                                                }
                                            }
                                            else
                                            {
                                                foreach (XmlNode itemnode in node.ChildNodes)
                                                {
                                                    items.Add(itemnode.InnerText);
                                                }
                                            }
                                        }

                                        var itemsArray = (string[]) (items.ToArray(typeof (string)));
                                        switch (propertyName)
                                        {
                                            case "ImagesFilters":
                                                _editor.ImageManager.SearchPatterns = ApplySearchPatternFilter(itemsArray);
                                                break;

                                            case "FlashFilters":
                                                _editor.FlashManager.SearchPatterns = ApplySearchPatternFilter(itemsArray);
                                                break;

                                            case "MediaFilters":
                                                _editor.MediaManager.SearchPatterns = ApplySearchPatternFilter(itemsArray);
                                                break;

                                            case "DocumentsFilters":
                                                _editor.DocumentManager.SearchPatterns = ApplySearchPatternFilter(itemsArray);
                                                break;

                                            case "TemplateFilters":
                                                _editor.TemplateManager.SearchPatterns = ApplySearchPatternFilter(itemsArray);
                                                break;

                                            case "SilverlightFilters":
                                                _editor.SilverlightManager.SearchPatterns = ApplySearchPatternFilter(itemsArray);
                                                break;
                                        }
                                    }

                                    break;
                                }
                        }
                    }
                }
                else
                {
                    //could not load config
                }
            }
            else
            {
                //could not load config (config file property empty?)
            }
        }

        private string[] ApplySearchPatternFilter(string[] patterns)
        {
            FileExtensionWhitelist hostWhiteList = Host.AllowedExtensionWhitelist;

            if (patterns.Length == 1 && patterns[0] == "*.*")
            {
                //todisplaystring converts to a "*.xxx, *.yyy" format which is then split for return
                return hostWhiteList.ToDisplayString().Split(',');
            }
            else
            {
                var returnPatterns = new List<string>();

                foreach (string pattern in patterns)
                {
                    if (hostWhiteList.IsAllowedExtension(pattern.Substring(1)))
                    {
                        returnPatterns.Add(pattern);
                    }
                }

                return returnPatterns.ToArray();
            }
        }

        protected void Panel_Init(object sender, EventArgs e)
        {
            //fix for allowing childportal (tabid must be in querystring!)
            string PortalPath = "";
            try
            {
                PortalPath = _editor.TemplateManager.ViewPaths[0];
            }
            catch
            {
            }
            PortalPath = PortalPath.Replace(PortalSettings.HomeDirectory, "").Replace("//", "/");
			var parentModule = ControlUtilities.FindParentControl<PortalModuleBase>(HtmlEditorControl);
			int moduleId = Convert.ToInt32(((parentModule == null) ? Null.NullInteger : parentModule.ModuleId));
            string strSaveTemplateDialogPath = _panel.Page.ResolveUrl(moduleFolderPath + "Dialogs/SaveTemplate.aspx?Path=" + PortalPath + "&TabId=" + PortalSettings.ActiveTab.TabID + "&ModuleId=" + moduleId);

			AJAX.AddScriptManager(_panel.Page);
        	ClientResourceManager.EnableAsyncPostBackHandler();
            ClientResourceManager.RegisterScript(_panel.Page, moduleFolderPath + "js/ClientScripts.js");
            ClientResourceManager.RegisterScript(_panel.Page, moduleFolderPath + "js/RegisterDialogs.js");

			_editor.ContentAreaCssFile = "~/DesktopModules/Admin/RadEditorProvider/Css/EditorContentAreaOverride.css?cdv=" + Host.CrmVersion;

            if (_editor.ToolbarMode == EditorToolbarMode.Default && string.Equals(_editor.Skin, "Default", StringComparison.OrdinalIgnoreCase))
            {
				var editorOverrideCSSPath = _panel.Page.ResolveUrl("~/DesktopModules/Admin/RadEditorProvider/Css/EditorOverride.css?cdv=" + Host.CrmVersion);
                var setEditorOverrideCSSPath = "<script type=\"text/javascript\">var __editorOverrideCSSPath = \"" + editorOverrideCSSPath + "\";</script>";
                _panel.Page.ClientScript.RegisterClientScriptBlock(GetType(), "EditorOverrideCSSPath", setEditorOverrideCSSPath);

				ClientResourceManager.RegisterScript(_panel.Page, moduleFolderPath + "js/overrideCSS.js");
                //_editor.Skin = "Black";
	            _editor.PreventDefaultStylesheet = true;
            }
            else
            {
                var setEditorOverrideCSSPath = "<script type=\"text/javascript\">var __editorOverrideCSSPath = null;</script>";
                _panel.Page.ClientScript.RegisterClientScriptBlock(GetType(), "EditorOverrideCSSPath", setEditorOverrideCSSPath);
            }

            if (!string.IsNullOrEmpty(_scripttoload))
            {
                ScriptManager.RegisterClientScriptInclude(_panel, _panel.GetType(), "ScriptToLoad", _panel.Page.ResolveUrl(_scripttoload));
            }

            //add save template dialog var
        	var saveTemplateDialogJs = 
				"<script type=\"text/javascript\">var __textEditorSaveTemplateDialog = \"" + strSaveTemplateDialogPath + "\";</script>";
			_panel.Page.ClientScript.RegisterClientScriptBlock(GetType(), "SaveTemplateDialog", saveTemplateDialogJs);

            //add css classes for save template tool
            /*
            _panel.Controls.Add(
                new LiteralControl("<style type=\"text/css\">.reTool_text .SaveTemplate, .reTool .SaveTemplate { background-image: url('" + _panel.Page.ResolveUrl(moduleFolderPath + "images/save.png") +
                                   "') !important; }</style>"));
            _panel.Controls.Add(
                new LiteralControl("<style type=\"text/css\">.reTool .TemplateOptions { background-image: url('" + _panel.Page.ResolveUrl(moduleFolderPath + "images/templates.png") +
                                   "') !important; }</style>"));
            _panel.Controls.Add(
                new LiteralControl("<style type=\"text/css\">.reTool_text .TemplateManager, .reTool .TemplateManager { background-image: url('" +
                                   _panel.Page.ResolveUrl(moduleFolderPath + "images/templates.png") + "') !important; background-position: left top !important; }</style>"));
            _panel.Controls.Add(
                new LiteralControl("<style type=\"text/css\">.reTool .InsertOptions { background-image: url('" + _panel.Page.ResolveUrl(moduleFolderPath + "images/Attachment.png") +
                                   "') !important; }</style>"));
             */

            _editor.OnClientSubmit = "OnDnnEditorClientSubmit";

            //add editor control to panel
            _panel.Controls.Add(_editor);
			_panel.Controls.Add(new RenderTemplateUrl());
        }

        protected void Panel_Load(object sender, EventArgs e)
        {
            //register the override CSS file to take care of the DNN default skin problems

            //string cssOverrideUrl = _panel.Page.ResolveUrl(moduleFolderPath + "/Css/EditorOverride.css");
            //ScriptManager pageScriptManager = ScriptManager.GetCurrent(_panel.Page);
            //if ((pageScriptManager != null) && (pageScriptManager.IsInAsyncPostBack))
            //{
            //    _panel.Controls.Add(
            //        new LiteralControl(string.Format("<link title='RadEditor Stylesheet' type='text/css' rel='stylesheet' href='{0}'></link>", _panel.Page.Server.HtmlEncode(cssOverrideUrl))));
            //}
            //else if (_panel.Page.Header != null)
            //{
            //    var link = new HtmlLink();
            //    link.Href = cssOverrideUrl;
            //    link.Attributes.Add("type", "text/css");
            //    link.Attributes.Add("rel", "stylesheet");
            //    link.Attributes.Add("title", "RadEditor Stylesheet");
            //    _panel.Page.Header.Controls.Add(link);
            //}
        }

        protected void Panel_PreRender(object sender, EventArgs e)
        {
            try
            {
                var parentModule = ControlUtilities.FindParentControl<PortalModuleBase>(HtmlEditorControl);
                int moduleid = Convert.ToInt32(((parentModule == null) ? -1 : parentModule.ModuleId));
                int portalId = Convert.ToInt32(((parentModule == null) ? -1 : parentModule.PortalId));
                int tabId = Convert.ToInt32(((parentModule == null) ? -1 : parentModule.TabId));
                ClientAPI.RegisterClientVariable(HtmlEditorControl.Page, "editorModuleId", moduleid.ToString(), true);
                ClientAPI.RegisterClientVariable(HtmlEditorControl.Page, "editorTabId", tabId.ToString(), true);
                ClientAPI.RegisterClientVariable(HtmlEditorControl.Page, "editorPortalId", portalId.ToString(), true);
                ClientAPI.RegisterClientVariable(HtmlEditorControl.Page, "editorHomeDirectory", PortalSettings.HomeDirectory, true);
                ClientAPI.RegisterClientVariable(HtmlEditorControl.Page, "editorPortalGuid", PortalSettings.GUID.ToString(), true);
                ClientAPI.RegisterClientVariable(HtmlEditorControl.Page, "editorEnableUrlLanguage", PortalSettings.EnableUrlLanguage.ToString(), true);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        protected void RadEditor_Load(object sender, EventArgs e)
        {
            Page editorPage = _panel.Page;

            //if not use relative links, we need a parameter in query string to let html module not parse
            //absolute urls to relative;
            if (!_linksUseRelativeUrls && editorPage.Request.QueryString["nuru"] == null)
            {
                var redirectUrl = string.Format("{0}{1}nuru=1", editorPage.Request.RawUrl, editorPage.Request.RawUrl.Contains("?") ? "&" : "?");
                editorPage.Response.Redirect(redirectUrl);
            }

            //set language
            if (! _languageSet) //language might have been set by config file
            {
                string localizationLang = "en-US"; //use teleriks internal fallback language

                //first check portal settings
                if (IsLocaleAvailable(PortalSettings.DefaultLanguage))
                {
                    //use only if resource file exists
                    localizationLang = PortalSettings.DefaultLanguage;
                }

                //then check if language cookie is present
                if (editorPage.Request.Cookies["language"] != null)
                {
                    string cookieValue = editorPage.Request.Cookies.Get("language").Value;
                    if (IsLocaleAvailable(cookieValue))
                    {
                        //only use locale if resource file is present
                        localizationLang = cookieValue;
                    }
                }

                //set new value
                if (! string.IsNullOrEmpty(localizationLang))
                {
                    _editor.Language = localizationLang;
                }
            }

            _editor.LocalizationPath = moduleFolderPath + "/App_LocalResources/";

            if (_ShowPortalLinks)
            {
                AddPortalLinks();
            }

            //set editor /spell properties to work with child portals
            _editor.SpellCheckSettings.DictionaryPath = moduleFolderPath + "RadSpell/";
            //again: fix for allowing childportals (tabid must be in querystring!)
            _editor.DialogHandlerUrl = _panel.Page.ResolveUrl(moduleFolderPath + "DialogHandler.aspx?tabid=" + PortalSettings.ActiveTab.TabID);
            //_editor.SpellCheckSettings.AjaxUrl = moduleFolderPath.Replace("~", "") & "SpellCheckHandler.ashx?tabid=" & PortalSettings.ActiveTab.TabID.ToString()
            _editor.SpellCheckSettings.AjaxUrl = _panel.Page.ResolveUrl(moduleFolderPath + "SpellCheckHandler.ashx?tabid=" + PortalSettings.ActiveTab.TabID);
	        _editor.DialogOpener.AdditionalQueryString = "&linkstype=" + _linksType + "&nuru=" + HttpContext.Current.Request.QueryString["nuru"];
        }

        private bool IsLocaleAvailable(string Locale)
        {
            string path;

            if (Locale.ToLower() == "en-us")
            {
                path = HttpContext.Current.Server.MapPath(_localeFile);
            }
            else
            {
                path = HttpContext.Current.Server.MapPath(_localeFile.ToLower().Replace(".resx", "." + Locale + ".resx"));
            }

            if (File.Exists(path))
            {
                //resource file exists
                return true;
            }

            //does not exist
            return false;
        }

        #endregion

        #region Config File related code

        private string GetXmlFilePath(string path)
        {
            //In case the file is declared as "http://someservername/somefile.xml"
            if (path.StartsWith("http://") || path.StartsWith("https://"))
            {
                return path;
            }
            string convertedPath = Context.Request.MapPath(path);
            if (File.Exists(convertedPath))
            {
                return convertedPath;
            }
            else
            {
                return path;
            }
        }

        protected XmlDocument GetValidConfigFile()
        {
            var xmlConfigFile = new XmlDocument();
            try
            {
                xmlConfigFile.Load(GetXmlFilePath(ConfigFile));
            }
            catch (Exception ex)
            {
                throw new Exception("Invalid Configuration File:" + ConfigFile, ex);
            }
            return xmlConfigFile;
        }

        private void SetEditorProperty(string propertyName, string propValue)
        {
            PropertyInfo pi = _editor.GetType().GetProperty(propertyName);
            if (pi != null)
            {
                if (pi.PropertyType.Equals(typeof (string)))
                {
                    pi.SetValue(_editor, propValue, null);
                }
                else if (pi.PropertyType.Equals(typeof (bool)))
                {
                    pi.SetValue(_editor, bool.Parse(propValue), null);
                }
                else if (pi.PropertyType.Equals(typeof (Unit)))
                {
                    pi.SetValue(_editor, Unit.Parse(propValue), null);
                }
                else if (pi.PropertyType.Equals(typeof (int)))
                {
                    pi.SetValue(_editor, int.Parse(propValue), null);
                }
            }
            if (propertyName == "Language")
            {
                _languageSet = true;
            }
        }

        #endregion

		private readonly DnnEditor _editor = new DnnEditor();
        private readonly Panel _panel = new Panel();
        private bool _ShowPortalLinks = true;

        //must override properties
        private const string ConfigFileName = moduleFolderPath + "/ConfigFile/ConfigFile.xml";
            
        //other provider specific properties

        private bool _languageSet;
        private bool _linksUseRelativeUrls = true;
        private string _linksType = "Normal";
        private string _localeFile = moduleFolderPath + "/App_LocalResources/RadEditor.Main.resx";
        private string _scripttoload = "";
        private const string ToolsFileName = moduleFolderPath + "/ToolsFile/ToolsFile.xml";

        public EditorProvider()
        {
            RootImageDirectory = PortalSettings.HomeDirectory;

            _panel.Init += Panel_Init;
            _panel.Load += Panel_Load;
            _panel.PreRender += Panel_PreRender;
            _editor.Load += RadEditor_Load;
        }
    }
}