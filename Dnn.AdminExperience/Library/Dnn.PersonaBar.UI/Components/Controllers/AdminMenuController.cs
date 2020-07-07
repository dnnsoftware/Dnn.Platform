// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.UI.Components.Controllers
{
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

    public class AdminMenuController : ServiceLocator<IAdminMenuController, AdminMenuController>, IAdminMenuController
    {
        private IDictionary<string, IList<string>> _knownPages;

        public void CreateLinkMenu(TabInfo tab)
        {
            if (!this.ValidateTab(tab))
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
                Order = tab.IsSuperTab ? 300 : 200, // show host menus after admin menus
                AllowHost = true,
                Enabled = true,
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

        protected override Func<IAdminMenuController> GetFactory()
        {
            return () => new AdminMenuController();
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

            var knownPages = this.GetKnownPages(type);
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
            if (this._knownPages == null)
            {
                this._knownPages = new Dictionary<string, IList<string>>();
            }

            if (this._knownPages.ContainsKey(type))
            {
                return this._knownPages[type];
            }

            var personaBarPath = Constants.PersonaBarRelativePath.Replace("~/", string.Empty);
            var dataPath = Path.Combine(Globals.ApplicationMapPath, personaBarPath, "data/adminpages.resources");
            var xmlDocument = new XmlDocument { XmlResolver = null };
            xmlDocument.Load(dataPath);
            var pages = xmlDocument.SelectNodes($"//pages//{type}//name")?.Cast<XmlNode>().Select(n => n.InnerXml.Trim()).ToList();
            this._knownPages.Add(type, pages);

            return pages;
        }
    }
}
