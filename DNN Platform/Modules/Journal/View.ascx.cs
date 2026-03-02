// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Modules.Journal
{
    using System;
    using System.Diagnostics.CodeAnalysis;

    using DotNetNuke.Abstractions;
    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Abstractions.ClientResources;
    using DotNetNuke.Abstractions.Logging;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Framework;
    using DotNetNuke.Framework.JavaScriptLibraries;
    using DotNetNuke.Modules.Journal.Components;
    using DotNetNuke.Security.Roles;
    using DotNetNuke.Services.ClientDependency;
    using DotNetNuke.Services.Exceptions;

    using Microsoft.Extensions.DependencyInjection;

    /// <summary>The ViewJournal class displays the content.</summary>
    public partial class View : JournalModuleBase
    {
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        [SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields", Justification = "Breaking change")]
        public int PageSize = 20;
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        [SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields", Justification = "Breaking change")]
        public bool AllowPhotos = true;
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        [SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields", Justification = "Breaking change")]
        public bool AllowFiles = true;
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        [SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields", Justification = "Breaking change")]
        public int MaxMessageLength = 250;
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        [SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields", Justification = "Breaking change")]
        public bool CanRender = true;
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        [SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields", Justification = "Breaking change")]
        public bool ShowEditor = true;
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        [SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields", Justification = "Breaking change")]
        public bool CanComment = true;
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        [SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields", Justification = "Breaking change")]
        public bool IsGroup;
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        [SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields", Justification = "Breaking change")]
        public string BaseUrl;
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        [SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields", Justification = "Breaking change")]
        public string ProfilePage;
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        [SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields", Justification = "Breaking change")]
        public int Gid = -1;
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        [SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields", Justification = "Breaking change")]
        public int Pid = -1;
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        [SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields", Justification = "Breaking change")]
        public long MaxUploadSize;
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        [SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields", Justification = "Breaking change")]
        public bool IsPublicGroup;

        private readonly INavigationManager navigationManager;
        private readonly IClientResourceController clientResourceController;
        private readonly IApplicationStatusInfo appStatus;
        private readonly IEventLogger eventLogger;
        private readonly IServicesFramework servicesFramework;
        private readonly IHostSettings hostSettings;

        /// <summary>Initializes a new instance of the <see cref="View"/> class.</summary>
        [Obsolete("Deprecated in DotNetNuke 10.2.2. Please use overload with INavigationManager. Scheduled removal in v12.0.0.")]
        public View()
            : this(null, null, null, null, null, null)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="View"/> class.</summary>
        /// <param name="navigationManager">The navigation manager.</param>
        /// <param name="clientResourceController">The client resource controller.</param>
        /// <param name="appStatus">The application status.</param>
        /// <param name="eventLogger">The event logger.</param>
        /// <param name="servicesFramework">The web API service framework.</param>
        [Obsolete("Deprecated in DotNetNuke 10.2.4. Please use overload with IHostSettings. Scheduled removal in v12.0.0.")]
        public View(INavigationManager navigationManager, IClientResourceController clientResourceController, IApplicationStatusInfo appStatus, IEventLogger eventLogger, IServicesFramework servicesFramework)
            : this(navigationManager, clientResourceController, appStatus, eventLogger, servicesFramework, null)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="View"/> class.</summary>
        /// <param name="navigationManager">The navigation manager.</param>
        /// <param name="clientResourceController">The client resource controller.</param>
        /// <param name="appStatus">The application status.</param>
        /// <param name="eventLogger">The event logger.</param>
        /// <param name="servicesFramework">The web API service framework.</param>
        /// <param name="hostSettings">The host settings.</param>
        public View(INavigationManager navigationManager, IClientResourceController clientResourceController, IApplicationStatusInfo appStatus, IEventLogger eventLogger, IServicesFramework servicesFramework, IHostSettings hostSettings)
        {
            this.navigationManager = navigationManager ?? this.DependencyProvider.GetRequiredService<INavigationManager>();
            this.clientResourceController = clientResourceController ?? this.DependencyProvider.GetRequiredService<IClientResourceController>();
            this.appStatus = appStatus ?? this.DependencyProvider.GetRequiredService<IApplicationStatusInfo>();
            this.eventLogger = eventLogger ?? this.DependencyProvider.GetRequiredService<IEventLogger>();
            this.servicesFramework = servicesFramework ?? this.DependencyProvider.GetRequiredService<IServicesFramework>();
            this.hostSettings = hostSettings ?? this.DependencyProvider.GetRequiredService<IHostSettings>();
            this.MaxUploadSize = Config.GetMaxUploadSize(this.appStatus);
        }

        protected override void OnInit(EventArgs e)
        {
            JavaScript.RequestRegistration(this.appStatus, this.eventLogger, this.PortalSettings, CommonJs.DnnPlugins);
            JavaScript.RequestRegistration(this.appStatus, this.eventLogger, this.PortalSettings, CommonJs.jQueryFileUpload);
            this.servicesFramework.RequestAjaxAntiForgerySupport();
            JavaScript.RequestRegistration(this.appStatus, this.eventLogger, this.PortalSettings, CommonJs.Knockout);

            this.clientResourceController.RegisterScript("~/DesktopModules/Journal/Scripts/journal.js");
            this.clientResourceController.RegisterScript("~/DesktopModules/Journal/Scripts/journalcomments.js");
            this.clientResourceController.RegisterScript("~/DesktopModules/Journal/Scripts/mentionsInput.js");

            var isAdmin = this.UserInfo.IsInRole(RoleController.Instance.GetRoleById(this.PortalId, this.PortalSettings.AdministratorRoleId).RoleName);
            if (!this.Request.IsAuthenticated || (!this.UserInfo.IsSuperUser && !isAdmin && this.UserInfo.IsInRole("Unverified Users")))
            {
                this.ShowEditor = false;
            }
            else
            {
                this.ShowEditor = this.EditorEnabled;
            }

            if (this.Settings.ContainsKey(Constants.DefaultPageSize))
            {
                this.PageSize = Convert.ToInt16(this.Settings[Constants.DefaultPageSize]);
            }

            if (this.Settings.ContainsKey(Constants.MaxCharacters))
            {
                this.MaxMessageLength = Convert.ToInt16(this.Settings[Constants.MaxCharacters]);
            }

            if (this.Settings.ContainsKey(Constants.AllowPhotos))
            {
                this.AllowPhotos = Convert.ToBoolean(this.Settings[Constants.AllowPhotos]);
            }

            if (this.Settings.ContainsKey(Constants.AllowFiles))
            {
                this.AllowFiles = Convert.ToBoolean(this.Settings[Constants.AllowFiles]);
            }

            this.ctlJournalList.Enabled = true;
            this.ctlJournalList.ProfileId = -1;
            this.ctlJournalList.PageSize = this.PageSize;
            this.ctlJournalList.ModuleId = this.ModuleId;

            ModuleInfo moduleInfo = this.ModuleContext.Configuration;

            foreach (var module in ModuleController.Instance.GetTabModules(this.TabId).Values)
            {
                if (module.ModuleDefinition.FriendlyName == "Social Groups")
                {
                    if (this.GroupId == -1 && this.FilterMode == JournalMode.Auto)
                    {
                        this.ShowEditor = false;
                        this.ctlJournalList.Enabled = false;
                    }

                    if (this.GroupId > 0)
                    {
                        RoleInfo roleInfo = RoleController.Instance.GetRoleById(moduleInfo.OwnerPortalID, this.GroupId);
                        if (roleInfo != null)
                        {
                            if (this.UserInfo.IsInRole(roleInfo.RoleName))
                            {
                                this.ShowEditor = true;
                                this.CanComment = true;
                                this.IsGroup = true;
                            }
                            else
                            {
                                this.ShowEditor = false;
                                this.CanComment = false;
                            }

                            if (!roleInfo.IsPublic && !this.ShowEditor)
                            {
                                this.ctlJournalList.Enabled = false;
                            }

                            if (roleInfo.IsPublic && !this.ShowEditor)
                            {
                                this.ctlJournalList.Enabled = true;
                            }

                            if (roleInfo.IsPublic && this.ShowEditor)
                            {
                                this.ctlJournalList.Enabled = true;
                            }

                            if (roleInfo.IsPublic)
                            {
                                this.IsPublicGroup = true;
                            }
                        }
                        else
                        {
                            this.ShowEditor = false;
                            this.ctlJournalList.Enabled = false;
                        }
                    }
                }
            }

            if (!string.IsNullOrEmpty(this.Request.QueryString["userId"]))
            {
                this.ctlJournalList.ProfileId = Convert.ToInt32(this.Request.QueryString["userId"]);
                if (!this.UserInfo.IsSuperUser && !isAdmin && this.ctlJournalList.ProfileId != this.UserId)
                {
                    this.ShowEditor = this.ShowEditor && Utilities.AreFriends(UserController.GetUserById(this.hostSettings, this.PortalId, this.ctlJournalList.ProfileId), this.UserInfo);
                }
            }
            else if (this.GroupId > 0)
            {
                this.ctlJournalList.SocialGroupId = Convert.ToInt32(this.Request.QueryString["groupId"]);
            }

            this.InitializeComponent();
            base.OnInit(e);
        }

        private void InitializeComponent()
        {
            this.Load += this.Page_Load;
        }

        /// <summary>Page_Load runs when the control is loaded.</summary>
        private void Page_Load(object sender, EventArgs e)
        {
            try
            {
                this.BaseUrl = Globals.ApplicationPath;
                this.BaseUrl = this.BaseUrl.EndsWith("/", StringComparison.Ordinal) ? this.BaseUrl : this.BaseUrl + "/";
                this.BaseUrl += "DesktopModules/Journal/";

                this.ProfilePage = this.navigationManager.NavigateURL(this.PortalSettings.UserTabId, string.Empty, new[] { "userId=xxx" });

                if (!string.IsNullOrEmpty(this.Request.QueryString["userId"]))
                {
                    this.Pid = Convert.ToInt32(this.Request.QueryString["userId"]);
                    this.ctlJournalList.ProfileId = this.Pid;
                }
                else if (this.GroupId > 0)
                {
                    this.Gid = this.GroupId;
                    this.ctlJournalList.SocialGroupId = this.GroupId;
                }

                this.ctlJournalList.PageSize = this.PageSize;
            }
            catch (Exception exc)
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }
    }
}
