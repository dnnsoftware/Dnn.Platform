// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Modules.Journal
{
    /*
' Copyright (c) 2011  DotNetNuke Corporation
'  All rights reserved.
'
' THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED
' TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL
' THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF
' CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
' DEALINGS IN THE SOFTWARE.
'
*/

    using System;

    using DotNetNuke.Abstractions;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Entities.Users.Social;
    using DotNetNuke.Framework;
    using DotNetNuke.Framework.JavaScriptLibraries;
    using DotNetNuke.Modules.Journal.Components;
    using DotNetNuke.Security.Roles;
    using DotNetNuke.Services.Exceptions;
    using DotNetNuke.Web.Client.ClientResourceManagement;
    using Microsoft.Extensions.DependencyInjection;

    /// -----------------------------------------------------------------------------
    /// <summary>
    /// The ViewJournal class displays the content.
    /// </summary>
    /// -----------------------------------------------------------------------------
    public partial class View : JournalModuleBase
    {
        public int PageSize = 20;
        public bool AllowPhotos = true;
        public bool AllowFiles = true;
        public int MaxMessageLength = 250;
        public bool CanRender = true;
        public bool ShowEditor = true;
        public bool CanComment = true;
        public bool IsGroup = false;
        public string BaseUrl;
        public string ProfilePage;
        public int Gid = -1;
        public int Pid = -1;
        public long MaxUploadSize = Config.GetMaxUploadSize();
        public bool IsPublicGroup = false;
        private readonly INavigationManager _navigationManager;

        public View()
        {
            this._navigationManager = this.DependencyProvider.GetRequiredService<INavigationManager>();
        }

        protected override void OnInit(EventArgs e)
        {
            JavaScript.RequestRegistration(CommonJs.DnnPlugins);
            JavaScript.RequestRegistration(CommonJs.jQueryFileUpload);
            ServicesFramework.Instance.RequestAjaxAntiForgerySupport();
            JavaScript.RequestRegistration(CommonJs.Knockout);

            ClientResourceManager.RegisterScript(this.Page, "~/DesktopModules/Journal/Scripts/journal.js");
            ClientResourceManager.RegisterScript(this.Page, "~/DesktopModules/Journal/Scripts/journalcomments.js");
            ClientResourceManager.RegisterScript(this.Page, "~/DesktopModules/Journal/Scripts/mentionsInput.js");
            ClientResourceManager.RegisterScript(this.Page, "~/Resources/Shared/Scripts/json2.js");

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
                    this.ShowEditor = this.ShowEditor && Utilities.AreFriends(UserController.GetUserById(this.PortalId, this.ctlJournalList.ProfileId), this.UserInfo);
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

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Page_Load runs when the control is loaded.
        /// </summary>
        /// -----------------------------------------------------------------------------
        private void Page_Load(object sender, EventArgs e)
        {
            try
            {
                this.BaseUrl = Globals.ApplicationPath;
                this.BaseUrl = this.BaseUrl.EndsWith("/") ? this.BaseUrl : this.BaseUrl + "/";
                this.BaseUrl += "DesktopModules/Journal/";

                this.ProfilePage = this._navigationManager.NavigateURL(this.PortalSettings.UserTabId, string.Empty, new[] { "userId=xxx" });

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
            catch (Exception exc) // Module failed to load
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }
    }
}
