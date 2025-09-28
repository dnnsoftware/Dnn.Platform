// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.UI.Skins.Controls
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Xml;

    using DotNetNuke.Abstractions;
    using DotNetNuke.Abstractions.ClientResources;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Entities.Tabs;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Framework;
    using DotNetNuke.Framework.JavaScriptLibraries;
    using DotNetNuke.Instrumentation;
    using DotNetNuke.Services.ClientDependency;
    using DotNetNuke.Services.Localization;
    using Microsoft.Extensions.DependencyInjection;

    /// <summary>A skin/theme object which allows the display of toast messages.</summary>
    public partial class Toast : SkinObjectBase
    {
        private const string MyFileName = "Toast.ascx";

        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Toast));
        private static readonly string ToastCacheKey = "DNN_Toast_Config";
        private readonly INavigationManager navigationManager;
        private readonly IJavaScriptLibraryHelper javaScript;
        private readonly IClientResourcesController clientResourcesController;

        public Toast()
            : this(null, null, null)
        {
        }

        public Toast(INavigationManager navigationManager, IJavaScriptLibraryHelper javaScript, IClientResourcesController clientResourcesController)
        {
            this.navigationManager = navigationManager ?? Globals.GetCurrentServiceProvider().GetRequiredService<INavigationManager>();
            this.javaScript = javaScript ?? Globals.GetCurrentServiceProvider().GetRequiredService<IJavaScriptLibraryHelper>();
            this.clientResourcesController = clientResourcesController ?? Globals.GetCurrentServiceProvider().GetRequiredService<IClientResourcesController>();
        }

        protected string ServiceModuleName { get; private set; }

        protected string ServiceAction { get; private set; }

        public bool IsOnline()
        {
            var userInfo = UserController.Instance.GetCurrentUserInfo();
            return userInfo.UserID != -1;
        }

        public string GetNotificationLink()
        {
            return this.GetMessageLink() + "?view=notifications&action=notifications";
        }

        public string GetMessageLink()
        {
            return this.navigationManager.NavigateURL(this.GetMessageTab(), string.Empty, string.Format("userId={0}", this.PortalSettings.UserId));
        }

        public string GetMessageLabel()
        {
            return Localization.GetString("SeeAllMessage", Localization.GetResourceFile(this, MyFileName));
        }

        public string GetNotificationLabel()
        {
            return Localization.GetString("SeeAllNotification", Localization.GetResourceFile(this, MyFileName));
        }

        /// <inheritdoc/>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            this.javaScript.RequestRegistration(CommonJs.jQueryUI);
            ServicesFramework.Instance.RequestAjaxAntiForgerySupport();

            this.clientResourcesController.RegisterScript("~/Resources/Shared/components/Toast/jquery.toastmessage.js", FileOrder.Js.jQuery);
            this.clientResourcesController.RegisterStylesheet("~/Resources/Shared/components/Toast/jquery.toastmessage.css", FileOrder.Css.DefaultCss);

            this.InitializeConfig();
        }

        // This method is copied from user skin object
        private int GetMessageTab()
        {
            var cacheKey = string.Format("MessageCenterTab:{0}:{1}", this.PortalSettings.PortalId, this.PortalSettings.CultureCode);
            var messageTabId = DataCache.GetCache<int>(cacheKey);
            if (messageTabId > 0)
            {
                return messageTabId;
            }

            // Find the Message Tab
            messageTabId = this.FindMessageTab();

            // save in cache
            // NOTE - This cache is not being cleared. There is no easy way to clear this, except Tools->Clear Cache
            DataCache.SetCache(cacheKey, messageTabId, TimeSpan.FromMinutes(20));

            return messageTabId;
        }

        // This method is copied from user skin object
        private int FindMessageTab()
        {
            // On brand new install the new Message Center Module is on the child page of User Profile Page
            // On Upgrade to 6.2.0, the Message Center module is on the User Profile Page
            var profileTab = TabController.Instance.GetTab(this.PortalSettings.UserTabId, this.PortalSettings.PortalId, false);
            if (profileTab != null)
            {
                var childTabs = TabController.Instance.GetTabsByPortal(profileTab.PortalID).DescendentsOf(profileTab.TabID);
                foreach (TabInfo tab in childTabs)
                {
                    foreach (KeyValuePair<int, ModuleInfo> kvp in ModuleController.Instance.GetTabModules(tab.TabID))
                    {
                        var module = kvp.Value;
                        if (module.DesktopModule.FriendlyName == "Message Center")
                        {
                            return tab.TabID;
                        }
                    }
                }
            }

            // default to User Profile Page
            return this.PortalSettings.UserTabId;
        }

        private void InitializeConfig()
        {
            this.ServiceModuleName = "InternalServices";
            this.ServiceAction = "NotificationsService/GetToasts";

            try
            {
                var toastConfig = DataCache.GetCache<IDictionary<string, string>>(ToastCacheKey);
                if (toastConfig == null)
                {
                    var configFile = this.Server.MapPath(Path.Combine(this.TemplateSourceDirectory, "Toast.config"));

                    if (File.Exists(configFile))
                    {
                        var xmlDocument = new XmlDocument { XmlResolver = null };
                        xmlDocument.Load(configFile);
                        var moduleNameNode = xmlDocument.DocumentElement?.SelectSingleNode("moduleName");
                        var actionNode = xmlDocument.DocumentElement?.SelectSingleNode("action");
                        var scriptsNode = xmlDocument.DocumentElement?.SelectSingleNode("scripts");

                        if (moduleNameNode != null && !string.IsNullOrEmpty(moduleNameNode.InnerText))
                        {
                            this.ServiceModuleName = moduleNameNode.InnerText;
                        }

                        if (actionNode != null && !string.IsNullOrEmpty(actionNode.InnerText))
                        {
                            this.ServiceAction = actionNode.InnerText;
                        }

                        if (scriptsNode != null && !string.IsNullOrEmpty(scriptsNode.InnerText))
                        {
                            this.addtionalScripts.Text = scriptsNode.InnerText;
                            this.addtionalScripts.Visible = true;
                        }
                    }

                    var config = new Dictionary<string, string>()
                    {
                        { "ServiceModuleName", this.ServiceModuleName },
                        { "ServiceAction", this.ServiceAction },
                        { "AddtionalScripts", this.addtionalScripts.Text },
                    };
                    DataCache.SetCache(ToastCacheKey, config);
                }
                else
                {
                    if (!string.IsNullOrEmpty(toastConfig["ServiceModuleName"]))
                    {
                        this.ServiceModuleName = toastConfig["ServiceModuleName"];
                    }

                    if (!string.IsNullOrEmpty(toastConfig["ServiceAction"]))
                    {
                        this.ServiceAction = toastConfig["ServiceAction"];
                    }

                    if (!string.IsNullOrEmpty(toastConfig["AddtionalScripts"]))
                    {
                        this.addtionalScripts.Text = toastConfig["AddtionalScripts"];
                        this.addtionalScripts.Visible = true;
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
        }
    }
}
