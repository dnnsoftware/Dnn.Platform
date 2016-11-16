using System;
using System.IO;
using System.Text;
using System.Web;
using System.Xml;
using Dnn.PersonaBar.Pages.Components.Exceptions;
using Dnn.PersonaBar.Pages.Services.Dto;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Tabs;
using DotNetNuke.Services.FileSystem;

namespace Dnn.PersonaBar.Pages.Components
{
    public class TemplateController
    {
        private readonly ITabController _tabController;

        public TemplateController()
        {
            _tabController = TabController.Instance;
        }

        public string SaveAsTemplate(PageTemplate template)
        {
            string filename = null;
            try { 
                var folder = FolderManager.Instance.GetFolder(template.Folder.Key);

                if (folder == null)
                {
                    return null;
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

    }
}