// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.EditBar.UI.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Web;
    using System.Web.Hosting;

    using Dnn.EditBar.Library.Items;

    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Extensions;
    using DotNetNuke.Entities.Controllers;
    using DotNetNuke.Entities.Host;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Framework;
    using DotNetNuke.Framework.Reflections;
    using DotNetNuke.Instrumentation;
    using DotNetNuke.Services.Log.EventLog;

    using Microsoft.Extensions.DependencyInjection;

    /// <summary>An <see cref="IEditBarController"/> implementation.</summary>
    /// <param name="hostSettings">The host settings.</param>
    /// <param name="menuItems">The menu items.</param>
    public class EditBarController(IHostSettings hostSettings, IEnumerable<BaseMenuItem> menuItems)
        : ServiceLocator<IEditBarController, EditBarController>, IEditBarController
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(EditBarController));

        private readonly IHostSettings hostSettings = hostSettings ??
                                                      HttpContextSource.Current?.GetScope().ServiceProvider.GetRequiredService<IHostSettings>() ??
                                                      new HostSettings(
                                                          new HostController(
#pragma warning disable CS0618 // Type or member is obsolete
                                                              new EventLogController(),
#pragma warning restore CS0618 // Type or member is obsolete
                                                              new Lazy<IPortalController>(() => PortalController.Instance)));

        private readonly IEnumerable<BaseMenuItem> menuItems = menuItems ?? GetMenuItemInstances();

        /// <summary>Initializes a new instance of the <see cref="EditBarController"/> class.</summary>
        [Obsolete("Deprecated in DotNetNuke 10.2.4. Please use overload with IEnumerable<BaseMenuItem>. Scheduled removal in v12.0.0.")]
        public EditBarController()
            : this(null, null)
        {
        }

        /// <inheritdoc />
        public IDictionary<string, object> GetConfigurations(int portalId)
        {
            var settings = new Dictionary<string, object>();
            var portalSettings = PortalSettings.Current;
            var user = portalSettings.UserInfo;

            settings.Add("applicationPath", Globals.ApplicationPath);
            settings.Add("buildNumber", this.hostSettings.CrmVersion.ToString(CultureInfo.InvariantCulture));
            settings.Add("userId", user.UserID);
            settings.Add("debugMode", HttpContextSource.Current is { IsDebuggingEnabled: true, });
            settings.Add("portalId", portalSettings.PortalId);
            settings.Add("culture", portalSettings.CultureCode);
            settings.Add("loginUrl", Globals.LoginURL(HttpContext.Current?.Request.RawUrl, false));
            settings.Add("items", this.GetMenuItems());

            var customEditBarThemePath = HostingEnvironment.MapPath("~/Portals/_default/EditBarTheme.css");
            var customEditBarThemeExists = File.Exists(customEditBarThemePath);
            settings.Add("editBarTheme", customEditBarThemeExists);

            return settings;
        }

        /// <inheritdoc />
        public IList<BaseMenuItem> GetMenuItems()
        {
            return this.menuItems
                    .Where(m => m.Visible())
                    .OrderBy(m => m.Parent)
                    .ThenBy(m => m.Order)
                    .ToList();
        }

        /// <inheritdoc />
        protected override Func<IEditBarController> GetFactory()
        {
#pragma warning disable CS0618 // Type or member is obsolete
            return () => new EditBarController();
#pragma warning restore CS0618 // Type or member is obsolete
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
                        CultureInfo.InvariantCulture,
                        "Unable to create {0} while getting all edit bar menu items. {1}",
                        type.FullName,
                        e.Message);
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
                t => t is { IsClass: true, IsAbstract: false, } && typeof(BaseMenuItem).IsAssignableFrom(t));
        }
    }
}
