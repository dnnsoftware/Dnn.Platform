using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Xml;
using Dnn.PersonaBar.Pages.Components.Dto;
using Dnn.PersonaBar.Pages.Components.Exceptions;
using Dnn.PersonaBar.Pages.Services.Dto;
using DotNetNuke.Common;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Tabs;
using DotNetNuke.Entities.Users;
using DotNetNuke.Framework;
using DotNetNuke.Services.FileSystem;
using DotNetNuke.UI.Skins;
using DotNetNuke.UI.Skins.Controls;
using DotNetNuke.Web.UI;

namespace Dnn.PersonaBar.Pages.Components
{
    public class TemplateController : ServiceLocator<ITemplateController, TemplateController>, ITemplateController
    {
        private readonly ITabController _tabController;

        public TemplateController()
        {
            _tabController = TabController.Instance;
        }

        public string SaveAsTemplate(PageTemplate template)
        {
            string filename;
            try {
                var folder = GetTemplateFolder();

                if (folder == null)
                {
                    throw new TemplateException("Folder could not be found in system.");
                }

                filename = folder.FolderPath + template.Name + ".page.template";
                filename = filename.Replace("/", "\\");

                var xmlTemplate = new XmlDocument();
                var nodePortal = xmlTemplate.AppendChild(xmlTemplate.CreateElement("portal"));
                nodePortal.Attributes?.Append(XmlUtils.CreateAttribute(xmlTemplate, "version", "3.0"));

                //Add template description
                var node = xmlTemplate.CreateElement("description");
                node.InnerXml = HttpUtility.HtmlEncode(template.Description);
                nodePortal.AppendChild(node);

                //Serialize tabs
                var nodeTabs = nodePortal.AppendChild(xmlTemplate.CreateElement("tabs"));
                SerializeTab(template, xmlTemplate, nodeTabs);

                //add file to Files table
                using (var fileContent = new MemoryStream(Encoding.UTF8.GetBytes(xmlTemplate.OuterXml)))
                {
                    FileManager.Instance.AddFile(folder, template.Name + ".page.template", fileContent, true, true, "application/octet-stream");
                }
            }
            catch (Exception)
            {
                throw new TemplateException("Error processing template.");
            }

            return filename;
        }
        private static IFolderInfo GetTemplateFolder()
        {
            const string folderPath = "Templates/";
            return FolderManager.Instance.GetFolder(PortalSettings.Current.PortalId, folderPath);
        }

        private void SerializeTab(PageTemplate template, XmlDocument xmlTemplate, XmlNode nodeTabs)
        {
            var portalSettings = PortalController.Instance.GetCurrentPortalSettings();
            var tab = _tabController.GetTab(template.TabId, portalSettings.PortalId, false);
            var xmlTab = new XmlDocument();
            var nodeTab = TabController.SerializeTab(xmlTab, tab, template.IncludeContent);
            nodeTabs.AppendChild(xmlTemplate.ImportNode(nodeTab, true));
        }

        protected override Func<ITemplateController> GetFactory()
        {
            return () => new TemplateController();
        }


        #region Templates
        public IEnumerable<Template> GetTemplates()
        {
            var user = UserController.Instance.GetCurrentUserInfo();
            var folders = FolderManager.Instance.GetFolders(user, "BROWSE, ADD");
            var templateFolder = folders.SingleOrDefault(f => f.DisplayPath == "Templates/");
            if (templateFolder != null)
            {
                //var folderName = templateFolder != null ? templateFolder.FolderName : null;
                //if (folderName == string.Empty)
                //{
                //    templateFolder.FfolderName = PortalSettings.Current.ActiveTab.IsSuperTab ? DynamicSharedConstants.HostRootFolder : DynamicSharedConstants.RootFolder;
                //}
                return LoadTemplates(templateFolder);
            }

            return null;
        }

        public int GetDefaultTemplateId(IEnumerable<Template> templates)
        {
            var firstOrDefault = templates.FirstOrDefault(t => t.Id == "Default");
            if (firstOrDefault != null)
            {
                return firstOrDefault.Value;
            }

            return Null.NullInteger;
        }

        private IEnumerable<Template> LoadTemplates(IFolderInfo templateFolder)
        {
            var portalSettings = PortalController.Instance.GetCurrentPortalSettings();
            var portalId = portalSettings.PortalId;
            var templates = new List<Template>();
            if (templateFolder == null)
            {
                return templates;
            }

            var folder = FolderManager.Instance.GetFolder(templateFolder.FolderID);
            if (folder == null)
            {
                return templates;
            }

            templates.Add(new Template
            {
                Id = Localization.GetString("None_Specified"),
                Value = Null.NullInteger
            });

            var files = Globals.GetFileList(portalId, "page.template", false, folder.FolderPath);
            templates.AddRange(from FileItem file in files
                               select new Template
                               {
                                   Id = file.Text.Replace(".page.template", ""),
                                   Value = int.Parse(file.Value)
                               });
            
            return templates;
        }

        public void CreatePageFromTemplate(int templateId, TabInfo tab, int portalId)
        {
            // create the page from a template
            if (templateId != Null.NullInteger)
            {
                var xmlDoc = new XmlDocument();
                try
                {
                    // open the XML file
                    var fileId = Convert.ToInt32(templateId);
                    var templateFile = FileManager.Instance.GetFile(fileId);
                    xmlDoc.Load(FileManager.Instance.GetFileContent(templateFile));
                }
                catch (Exception ex)
                {
                    DotNetNuke.Services.Exceptions.Exceptions.LogException(ex);
                    throw new PageException(Localization.GetString("BadTemplate"));
                }
                TabController.DeserializePanes(xmlDoc.SelectSingleNode("//portal/tabs/tab/panes"), tab.PortalID, tab.TabID, PortalTemplateModuleAction.Ignore, new Hashtable());
                //save tab permissions
                RibbonBarManager.DeserializeTabPermissions(xmlDoc.SelectNodes("//portal/tabs/tab/tabpermissions/permission"), tab);

                var tabIndex = 0;
                var exceptions = string.Empty;
                foreach (XmlNode tabNode in xmlDoc.SelectSingleNode("//portal/tabs").ChildNodes)
                {
                    //Create second tab onward tabs. Note first tab is already created above.
                    if (tabIndex > 0)
                    {
                        try
                        {
                            TabController.DeserializeTab(tabNode, null, portalId, PortalTemplateModuleAction.Replace);
                        }
                        catch (Exception ex)
                        {
                            DotNetNuke.Services.Exceptions.Exceptions.LogException(ex);
                            exceptions += string.Format("Template Tab # {0}. Error {1}<br/>", tabIndex + 1, ex.Message);
                        }
                    }
                    else
                    {
                        if (string.IsNullOrEmpty(tab.SkinSrc) && !String.IsNullOrEmpty(XmlUtils.GetNodeValue(tabNode, "skinsrc", "")))
                        {
                            tab.SkinSrc = XmlUtils.GetNodeValue(tabNode, "skinsrc", "");
                        }
                        if (string.IsNullOrEmpty(tab.ContainerSrc) && !String.IsNullOrEmpty(XmlUtils.GetNodeValue(tabNode, "containersrc", "")))
                        {
                            tab.ContainerSrc = XmlUtils.GetNodeValue(tabNode, "containersrc", "");
                        }
                        TabController.Instance.UpdateTab(tab);
                    }
                    tabIndex++;
                }

                if (!string.IsNullOrEmpty(exceptions))
                {
                    throw new PageException(exceptions);
                }
            }
        }
        #endregion
    }
}