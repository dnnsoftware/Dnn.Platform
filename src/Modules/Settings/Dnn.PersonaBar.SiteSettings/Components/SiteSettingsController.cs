#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2016
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



#endregion

using System.IO;
using System.Web;
using System.Xml;
using DotNetNuke.Services.Localization;

namespace Dnn.PersonaBar.SiteSettings.Components
{
    public class SiteSettingsController
    {
        public void SaveLocalizedKeys(int portalId, string propertyName, string propertyCategory, string cultureCode, string propertyNameString,
            string propertyHelpString, string propertyRequiredString, string propertyValidationString, string categoryNameString)
        {
            var portalResources = new XmlDocument();
            var defaultResources = new XmlDocument();
            XmlNode parent;

            defaultResources.Load(GetResourceFile("", Localization.SystemLocale, portalId));
            string filename = GetResourceFile("Portal", cultureCode, portalId);

            if (File.Exists(filename))
            {
                portalResources.Load(filename);
            }
            else
            {
                portalResources.Load(GetResourceFile("", Localization.SystemLocale, portalId));
            }
            UpdateResourceFileNode(portalResources, "ProfileProperties_" + propertyName + ".Text", propertyNameString);
            UpdateResourceFileNode(portalResources, "ProfileProperties_" + propertyName + ".Help", propertyHelpString);
            UpdateResourceFileNode(portalResources, "ProfileProperties_" + propertyName + ".Required", propertyRequiredString);
            UpdateResourceFileNode(portalResources, "ProfileProperties_" + propertyName + ".Validation", propertyValidationString);
            UpdateResourceFileNode(portalResources, "ProfileProperties_" + propertyCategory + ".Header", categoryNameString);

            //remove unmodified keys
            foreach (XmlNode node in portalResources.SelectNodes("//root/data"))
            {
                XmlNode defaultNode = defaultResources.SelectSingleNode("//root/data[@name='" + node.Attributes["name"].Value + "']");
                if (defaultNode != null && defaultNode.InnerXml == node.InnerXml)
                {
                    parent = node.ParentNode;
                    parent.RemoveChild(node);
                }
            }

            //remove duplicate keys
            foreach (XmlNode node in portalResources.SelectNodes("//root/data"))
            {
                if (portalResources.SelectNodes("//root/data[@name='" + node.Attributes["name"].Value + "']").Count > 1)
                {
                    parent = node.ParentNode;
                    parent.RemoveChild(node);
                }
            }
            if (portalResources.SelectNodes("//root/data").Count > 0)
            {
                //there's something to save
                portalResources.Save(filename);
            }
            else
            {
                //nothing to be saved, if file exists delete
                if (File.Exists(filename))
                {
                    File.Delete(filename);
                }
            }
        }

        private void UpdateResourceFileNode(XmlDocument xmlDoc, string key, string text)
        {
            XmlNode node;
            XmlNode nodeData;
            XmlAttribute attr;
            node = xmlDoc.SelectSingleNode("//root/data[@name='" + key + "']/value");
            if (node == null)
            {
                //missing entry
                nodeData = xmlDoc.CreateElement("data");
                attr = xmlDoc.CreateAttribute("name");
                attr.Value = key;
                nodeData.Attributes.Append(attr);
                xmlDoc.SelectSingleNode("//root").AppendChild(nodeData);
                node = nodeData.AppendChild(xmlDoc.CreateElement("value"));
            }
            node.InnerXml = HttpUtility.HtmlEncode(text);
        }

        private string GetResourceFile(string type, string language, int portalId)
        {
            string resourcefilename = "~/DesktopModules/Admin/Security/App_LocalResources/Profile.ascx";
            if (language != Localization.SystemLocale)
            {
                resourcefilename = resourcefilename + "." + language;
            }
            if (type == "Portal")
            {
                resourcefilename = resourcefilename + "." + "Portal-" + portalId;
            }
            else if (type == "Host")
            {
                resourcefilename = resourcefilename + "." + "Host";
            }
            return HttpContext.Current.Server.MapPath(resourcefilename + ".resx");
        }
    }
}