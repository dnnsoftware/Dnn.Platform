// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.EditBar.UI.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Web;

    using Dnn.EditBar.Library;
    using Dnn.EditBar.Library.Items;
    using DotNetNuke.Application;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Host;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Framework;
    using DotNetNuke.Framework.Reflections;
    using DotNetNuke.Instrumentation;
    using DotNetNuke.Services.FileSystem;
    using DotNetNuke.Web.UI;
    using Newtonsoft.Json.Linq;

    public class EditBarController : ServiceLocator<IEditBarController, EditBarController>, IEditBarController
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(EditBarController));

        private static object _threadLocker = new object();

        public IDictionary<string, object> GetConfigurations(int portalId)
        {
            var settings = new Dictionary<string, object>();
            var portalSettings = PortalSettings.Current;
            var user = portalSettings.UserInfo;

            settings.Add("applicationPath", Globals.ApplicationPath);
            settings.Add("buildNumber", Host.CrmVersion.ToString(CultureInfo.InvariantCulture));
            settings.Add("userId", user.UserID);
            settings.Add("debugMode", HttpContext.Current != null && HttpContext.Current.IsDebuggingEnabled);
            settings.Add("portalId", portalSettings.PortalId);
            settings.Add("culture", portalSettings.CultureCode);
            settings.Add("loginUrl", Globals.LoginURL(HttpContext.Current?.Request.RawUrl, false));
            settings.Add("items", this.GetMenuItems());

            return settings;
        }

        public IList<BaseMenuItem> GetMenuItems()
        {
            var menuItems = DataCache.GetCache<IList<BaseMenuItem>>(Constants.MenuItemsCacheKey);
            if (menuItems == null)
            {
                lock (_threadLocker)
                {
                    menuItems = DataCache.GetCache<IList<BaseMenuItem>>(Constants.MenuItemsCacheKey);
                    if (menuItems == null)
                    {
                        menuItems = GetMenuItemInstances().ToList();

                        DataCache.SetCache(Constants.MenuItemsCacheKey, menuItems);
                    }
                }
            }

            return menuItems
                    .Where(m => m.Visible())
                    .OrderBy(m => m.Parent)
                    .ThenBy(m => m.Order)
                    .ToList();
        }

        protected override Func<IEditBarController> GetFactory()
        {
            return () => new EditBarController();
        }

        private static IEnumerable<BaseMenuItem> GetMenuItemInstances()
        {
            var types = GetAllMenuItemTypes();

            foreach (var type in types)
            {
                BaseMenuItem menuItem;
                try
                {
                    menuItem = Activator.CreateInstance(type) as BaseMenuItem;
                }
                catch (Exception e)
                {
                    Logger.ErrorFormat(
                        "Unable to create {0} while getting all edit bar menu items. {1}",
                        type.FullName, e.Message);
                    menuItem = null;
                }

                if (menuItem != null)
                {
                    yield return menuItem;
                }
            }
        }

        private static IEnumerable<Type> GetAllMenuItemTypes()
        {
            var typeLocator = new TypeLocator();
            return typeLocator.GetAllMatchingTypes(
                t => t != null &&
                     t.IsClass &&
                     !t.IsAbstract &&
                     typeof(BaseMenuItem).IsAssignableFrom(t));
        }
    }
}
