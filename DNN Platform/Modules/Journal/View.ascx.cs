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
using DotNetNuke.Common;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Users;
using DotNetNuke.Entities.Users.Social;
using DotNetNuke.Framework;
using DotNetNuke.Framework.JavaScriptLibraries;
using DotNetNuke.Services.Exceptions;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Web.Client.ClientResourceManagement;
using DotNetNuke.Modules.Journal.Components;
using DotNetNuke.Security.Roles;

namespace DotNetNuke.Modules.Journal {

    /// -----------------------------------------------------------------------------
    /// <summary>
    /// The ViewJournal class displays the content
    /// </summary>
    /// -----------------------------------------------------------------------------
    public partial class View : JournalModuleBase {

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

        #region Event Handlers

        override protected void OnInit(EventArgs e) 
        {
            JavaScript.RequestRegistration(CommonJs.DnnPlugins);
            JavaScript.RequestRegistration(CommonJs.jQueryFileUpload);
            ServicesFramework.Instance.RequestAjaxAntiForgerySupport();
            JavaScript.RequestRegistration(CommonJs.Knockout);
            
            ClientResourceManager.RegisterScript(Page, "~/DesktopModules/Journal/Scripts/journal.js");
            ClientResourceManager.RegisterScript(Page, "~/DesktopModules/Journal/Scripts/journalcomments.js");
			ClientResourceManager.RegisterScript(Page, "~/DesktopModules/Journal/Scripts/mentionsInput.js");
			ClientResourceManager.RegisterScript(Page, "~/Resources/Shared/Scripts/json2.js");

            var isAdmin = UserInfo.IsInRole(RoleController.Instance.GetRoleById(PortalId, PortalSettings.AdministratorRoleId).RoleName);
            if (!Request.IsAuthenticated || (!UserInfo.IsSuperUser && !isAdmin && UserInfo.IsInRole("Unverified Users")))
            {
                ShowEditor = false;
            } 
            else
            {
                ShowEditor = EditorEnabled;
            }

            if (Settings.ContainsKey(Constants.DefaultPageSize))
            {
                PageSize = Convert.ToInt16(Settings[Constants.DefaultPageSize]);
            }
            if (Settings.ContainsKey(Constants.MaxCharacters))
            {
                MaxMessageLength = Convert.ToInt16(Settings[Constants.MaxCharacters]);
            }
            if (Settings.ContainsKey(Constants.AllowPhotos))
            {
                AllowPhotos = Convert.ToBoolean(Settings[Constants.AllowPhotos]);
            }
            if (Settings.ContainsKey(Constants.AllowFiles))
            {
                AllowFiles = Convert.ToBoolean(Settings[Constants.AllowFiles]);
            }
            ctlJournalList.Enabled = true;
            ctlJournalList.ProfileId = -1;
            ctlJournalList.PageSize = PageSize;
            ctlJournalList.ModuleId = ModuleId;
            
            ModuleInfo moduleInfo = ModuleContext.Configuration;

            foreach (var module in ModuleController.Instance.GetTabModules(TabId).Values) 
            {
                if (module.ModuleDefinition.FriendlyName == "Social Groups") 
                {
                    if (GroupId == -1 && FilterMode == JournalMode.Auto) 
                    {
                        ShowEditor = false;
                        ctlJournalList.Enabled = false;
                    }

                    if (GroupId > 0) 
                    {
                        RoleInfo roleInfo = RoleController.Instance.GetRoleById(moduleInfo.OwnerPortalID, GroupId);
                        if (roleInfo != null) 
                        {
                            if (UserInfo.IsInRole(roleInfo.RoleName)) 
                            {
                                ShowEditor = true;
                                CanComment = true;
                                IsGroup = true;
                            } else 
                            {
                                ShowEditor = false;
                                CanComment = false;
                            }
                            
                            if (!roleInfo.IsPublic && !ShowEditor) 
                            {
                                ctlJournalList.Enabled = false;                               
                            }
                            if (roleInfo.IsPublic && !ShowEditor) 
                            {
                                ctlJournalList.Enabled = true;
                            }
                            if (roleInfo.IsPublic && ShowEditor) 
                            {
                                ctlJournalList.Enabled = true;
                            }
                            if (roleInfo.IsPublic)
                            {
                                IsPublicGroup = true;
                            }
                        } 
                        else 
                        {
                            ShowEditor = false;
                            ctlJournalList.Enabled = false;
                        }
                    }
                   
                }
            }

            if (!String.IsNullOrEmpty(Request.QueryString["userId"])) 
            {
                ctlJournalList.ProfileId = Convert.ToInt32(Request.QueryString["userId"]);
                if (!UserInfo.IsSuperUser && !isAdmin && ctlJournalList.ProfileId != UserId)
                {
                    ShowEditor = ShowEditor && Utilities.AreFriends(UserController.GetUserById(PortalId, ctlJournalList.ProfileId), UserInfo);                    
                }
            } 
            else if (GroupId > 0) 
            {
                ctlJournalList.SocialGroupId = Convert.ToInt32(Request.QueryString["groupId"]);
            }
            
            InitializeComponent();
            base.OnInit(e);
        }

        private void InitializeComponent() {
            Load += Page_Load;
        }
        
        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Page_Load runs when the control is loaded
        /// </summary>
        /// -----------------------------------------------------------------------------
        private void Page_Load(object sender, EventArgs e) {
            try 
            {
                BaseUrl = Globals.ApplicationPath;
                BaseUrl = BaseUrl.EndsWith("/") ? BaseUrl : BaseUrl + "/";
                BaseUrl += "DesktopModules/Journal/";

                ProfilePage = Common.Globals.NavigateURL(PortalSettings.UserTabId, string.Empty, new[] {"userId=xxx"});

                if (!String.IsNullOrEmpty(Request.QueryString["userId"])) 
                {
                    Pid = Convert.ToInt32(Request.QueryString["userId"]);
                    ctlJournalList.ProfileId = Pid;                    
                } 
                else if (GroupId > 0) 
                {
                    Gid = GroupId;
                    ctlJournalList.SocialGroupId = GroupId;                    
                }
                ctlJournalList.PageSize = PageSize;
            } 
            catch (Exception exc) //Module failed to load
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }
        #endregion
    }
}
