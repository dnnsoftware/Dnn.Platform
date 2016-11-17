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
                var folder = FolderManager.Instance.GetFolder(template.Folder.Key);

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

            //if (!Page.IsPostBack)
            //{
            //    cboTemplate.ClearSelection();
            //    var defaultItem = cboTemplate.FindItemByText("Default");
            //    if (defaultItem != null)
            //    {
            //        defaultItem.Selected = true;
            //    }
            //}

            //if (cboTemplate.SelectedIndex == -1)
            //{
            //    cboTemplate.SelectedIndex = 0;
            //}
            return templates;
        }

        #endregion
    }
}