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

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Dnn.PersonaBar.Library;
using Dnn.PersonaBar.Library.Model;
using Dnn.PersonaBar.Library.Repository;
using DotNetNuke.Common;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Tabs;
using DotNetNuke.Framework;

namespace Dnn.PersonaBar.UI.Components.Controllers
{
    public class AdminMenuController : ServiceLocator<IAdminMenuController, AdminMenuController>, IAdminMenuController
    {
        private IDictionary<string, IList<string>> _knownPages;

        protected override Func<IAdminMenuController> GetFactory()
        {
            return () => new AdminMenuController();
        }

        public void CreateLinkMenu(TabInfo tab)
        {
            if (!ValidateTab(tab))
            {
                return;
            }

            var portalId = tab.PortalID;
            var tabId = tab.TabID;

            var identifier = $"Link_{portalId}_{tabId}";
            if (PersonaBarRepository.Instance.GetMenuItem(identifier) != null)
            {
                return;
            }

            var menuItem = new MenuItem
            {
                Identifier = identifier,
                Path = $"LinkMenu?portalId={portalId}&tabId={tabId}",
                ModuleName = "LinkMenu",
                Controller = "Dnn.PersonaBar.UI.MenuControllers.LinkMenuController, Dnn.PersonaBar.UI",
                ResourceKey = tab.LocalizedTabName,
                ParentId = PersonaBarRepository.Instance.GetMenuItem("Manage").MenuId,
                Order = tab.IsSuperTab ? 300 : 200, //show host menus after admin menus
                AllowHost = true,
                Enabled = true
            };

            PersonaBarRepository.Instance.SaveMenuItem(menuItem);

            if (!tab.IsSuperTab)
            {
                var portal = PortalController.Instance.GetPortal(portalId);
                PersonaBarRepository.Instance.SaveMenuDefaultPermissions(menuItem, portal.AdministratorRoleName);
            }
        }

        public void DeleteLinkMenu(TabInfo tab)
        {
            var portalId = tab.PortalID;
            var tabId = tab.TabID;

            var identifier = $"Link_{portalId}_{tabId}";

            PersonaBarRepository.Instance.DeleteMenuItem(identifier);
        }

        private bool ValidateTab(TabInfo tab)
        {
            if (tab.IsDeleted || tab.DisableLink || !tab.IsVisible)
            {
                return false;
            }

            var type = tab.IsSuperTab ? "host" : "admin";
            var portalId = tab.PortalID;
            var tabName = tab.TabName;

            var knownPages = GetKnownPages(type);
            if (knownPages.Contains(tabName, StringComparer.InvariantCultureIgnoreCase))
            {
                return false;
            }

            if (!tab.IsSuperTab)
            {
                var adminPage = TabController.GetTabByTabPath(portalId, "//Admin", string.Empty);
                if (adminPage == Null.NullInteger)
                {
                    return false;
                }

                return tab.ParentId == adminPage;
            }

            return true;
        }

        private IList<string> GetKnownPages(string type)
        {
            if (_knownPages == null)
            {
                _knownPages = new Dictionary<string, IList<string>>();
            }

            if (_knownPages.ContainsKey(type))
            {
                return _knownPages[type];
            }

            var personaBarPath = Constants.PersonaBarRelativePath.Replace("~/", string.Empty);
            var dataPath = Path.Combine(Globals.ApplicationMapPath, personaBarPath, "data/adminpages.resources");
            var xmlDocument = new XmlDocument();
            xmlDocument.Load(dataPath);
            var pages = xmlDocument.SelectNodes($"//pages//{type}//name")?.Cast<XmlNode>().Select(n => n.InnerXml.Trim()).ToList();
            _knownPages.Add(type, pages);

            return pages;
        }
    }
}
