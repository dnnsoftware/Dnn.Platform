// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DesktopModules.Admin.Security
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using DotNetNuke.Common.Lists;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Entities.Profile;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Framework;
    using DotNetNuke.UI.Skins.Controls;
    using DotNetNuke.UI.WebControls;

    using MembershipProvider = DotNetNuke.Security.Membership.MembershipProvider;

    /// -----------------------------------------------------------------------------
    /// <summary>
    /// The Profile UserModuleBase is used to register Users.
    /// </summary>
    /// <remarks>
    /// </remarks>
    /// -----------------------------------------------------------------------------
    public partial class DNNProfile : ProfileUserControlBase
    {
        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets a value indicating whether gets whether the User is valid.
        /// </summary>
        /// -----------------------------------------------------------------------------
        public bool IsValid
        {
            get
            {
                return this.ProfileProperties.IsValid || this.IsAdmin;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets the UserProfile associated with this control.
        /// </summary>
        /// -----------------------------------------------------------------------------
        public UserProfile UserProfile
        {
            get
            {
                UserProfile _Profile = null;
                if (this.User != null)
                {
                    _Profile = this.User.Profile;
                }

                return _Profile;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets and sets the EditorMode.
        /// </summary>
        /// -----------------------------------------------------------------------------
        public PropertyEditorMode EditorMode
        {
            get
            {
                return this.ProfileProperties.EditMode;
            }

            set
            {
                this.ProfileProperties.EditMode = value;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets a value indicating whether gets and sets whether the Update button.
        /// </summary>
        /// -----------------------------------------------------------------------------
        public bool ShowUpdate
        {
            get
            {
                return this.actionsRow.Visible;
            }

            set
            {
                this.actionsRow.Visible = value;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets a value indicating whether gets whether to display the Visibility controls.
        /// </summary>
        /// -----------------------------------------------------------------------------
        protected bool ShowVisibility
        {
            get
            {
                object setting = GetSetting(this.PortalId, "Profile_DisplayVisibility");
                return Convert.ToBoolean(setting) && this.IsUser;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// DataBind binds the data to the controls.
        /// </summary>
        /// -----------------------------------------------------------------------------
        public override void DataBind()
        {
            // Before we bind the Profile to the editor we need to "update" the visible data
            var properties = new ProfilePropertyDefinitionCollection();
            var imageType = new ListController().GetListEntryInfo("DataType", "Image");
            foreach (ProfilePropertyDefinition profProperty in this.UserProfile.ProfileProperties)
            {
                if (this.IsAdmin && !this.IsProfile)
                {
                    profProperty.Visible = true;
                }

                if (!profProperty.Deleted && (this.Request.IsAuthenticated || profProperty.DataType != imageType.EntryID))
                {
                    properties.Add(profProperty);
                }
            }

            this.ProfileProperties.User = this.User;
            this.ProfileProperties.ShowVisibility = this.ShowVisibility;
            this.ProfileProperties.DataSource = properties;
            this.ProfileProperties.DataBind();
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Page_Init runs when the control is initialised.
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// -----------------------------------------------------------------------------
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            this.ID = "Profile.ascx";

            // Get the base Page
            var basePage = this.Page as PageBase;
            if (basePage != null)
            {
                // Check if culture is RTL
                this.ProfileProperties.LabelMode = basePage.PageCulture.TextInfo.IsRightToLeft ? LabelMode.Right : LabelMode.Left;
            }

            this.ProfileProperties.LocalResourceFile = this.LocalResourceFile;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Page_Load runs when the control is loaded.
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// -----------------------------------------------------------------------------
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            this.cmdUpdate.Click += this.cmdUpdate_Click;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// cmdUpdate_Click runs when the Update Button is clicked.
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// -----------------------------------------------------------------------------
        private void cmdUpdate_Click(object sender, EventArgs e)
        {
            if (this.IsUserOrAdmin == false && this.UserId == Null.NullInteger)
            {
                return;
            }

            if (this.IsValid)
            {
                if (this.User.UserID == this.PortalSettings.AdministratorId)
                {
                    // Clear the Portal Cache
                    DataCache.ClearPortalCache(this.UserPortalID, true);
                }

                // Update DisplayName to conform to Format
                this.UpdateDisplayName();

                if (this.PortalSettings.Registration.RequireUniqueDisplayName)
                {
                    var usersWithSameDisplayName = (List<UserInfo>)MembershipProvider.Instance().GetUsersBasicSearch(this.PortalId, 0, 2, "DisplayName", true, "DisplayName", this.User.DisplayName);
                    if (usersWithSameDisplayName.Any(user => user.UserID != this.User.UserID))
                    {
                        this.AddModuleMessage("DisplayNameNotUnique", ModuleMessage.ModuleMessageType.RedError, true);
                        return;
                    }
                }

                var properties = (ProfilePropertyDefinitionCollection)this.ProfileProperties.DataSource;

                // Update User's profile
                this.User = ProfileController.UpdateUserProfile(this.User, properties);

                this.OnProfileUpdated(EventArgs.Empty);
                this.OnProfileUpdateCompleted(EventArgs.Empty);
            }
        }

        private void UpdateDisplayName()
        {
            if (!string.IsNullOrEmpty(this.PortalSettings.Registration.DisplayNameFormat))
            {
                this.User.UpdateDisplayName(this.PortalSettings.Registration.DisplayNameFormat);
            }
        }
    }
}
