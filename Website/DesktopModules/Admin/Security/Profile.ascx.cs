#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2014
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

using System;
using DotNetNuke.Common.Lists;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Profile;
using DotNetNuke.Entities.Users;
using DotNetNuke.Framework;
using DotNetNuke.Services.Localization;
using DotNetNuke.UI.WebControls;

#endregion

namespace DesktopModules.Admin.Security
{
    /// -----------------------------------------------------------------------------
    /// <summary>
    /// The Profile UserModuleBase is used to register Users
    /// </summary>
    /// <remarks>
    /// </remarks>
    /// <history>
    /// 	[cnurse]	03/02/2006
    /// </history>
    /// -----------------------------------------------------------------------------
    public partial class DNNProfile : ProfileUserControlBase
    {
		#region Protected Properties

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets whether to display the Visibility controls
        /// </summary>
        /// <history>
        /// 	[cnurse]	08/11/2006  Created
        /// </history>
        /// -----------------------------------------------------------------------------
        protected bool ShowVisibility
        {
            get
            {
                object setting = GetSetting(PortalId, "Profile_DisplayVisibility");
                return Convert.ToBoolean(setting) && IsUser;
            }
        }

		#endregion

		#region Public Properties

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets and sets the EditorMode
        /// </summary>
        /// <history>
        /// 	[cnurse]	05/02/2006  Created
        /// </history>
        /// -----------------------------------------------------------------------------
        public PropertyEditorMode EditorMode
        {
            get
            {
                return ProfileProperties.EditMode;
            }
            set
            {
                ProfileProperties.EditMode = value;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets whether the User is valid
        /// </summary>
        /// <history>
        /// 	[cnurse]	05/18/2006  Created
        /// </history>
        /// -----------------------------------------------------------------------------
        public bool IsValid
        {
            get
            {
                return ProfileProperties.IsValid || IsAdmin;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets and sets whether the Update button
        /// </summary>
        /// <history>
        /// 	[cnurse]	05/18/2006  Created
        /// </history>
        /// -----------------------------------------------------------------------------
        public bool ShowUpdate
        {
            get
            {
                return actionsRow.Visible;
            }
            set
            {
                actionsRow.Visible = value;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets the UserProfile associated with this control
        /// </summary>
        /// <history>
        /// 	[cnurse]	03/02/2006  Created
        /// </history>
        /// -----------------------------------------------------------------------------
        public UserProfile UserProfile
        {
            get
            {
                UserProfile _Profile = null;
                if (User != null)
                {
                    _Profile = User.Profile;
                }
                return _Profile;
            }
        }

		#endregion

		#region Public Methods

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// DataBind binds the data to the controls
        /// </summary>
        /// <history>
        /// 	[cnurse]	03/01/2006  Created
        /// </history>
        /// -----------------------------------------------------------------------------
        public override void DataBind()
        {
		
            //Before we bind the Profile to the editor we need to "update" the visible data
            var properties = new ProfilePropertyDefinitionCollection();
			var imageType = new ListController().GetListEntryInfo("DataType", "Image");
            foreach (ProfilePropertyDefinition profProperty in UserProfile.ProfileProperties)
            {
                if (IsAdmin && !IsProfile)
                {
                    profProperty.Visible = true;
                }

                if (!profProperty.Deleted && (Request.IsAuthenticated || profProperty.DataType != imageType.EntryID))
                {
                    properties.Add(profProperty);
                }
            }

            ProfileProperties.User = User;
            ProfileProperties.ShowVisibility = ShowVisibility;
            ProfileProperties.DataSource = properties;
            ProfileProperties.DataBind();
        }

		#endregion

		#region Event Handlers

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Page_Init runs when the control is initialised
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <history>
        /// 	[cnurse]	03/01/2006  Created
        /// </history>
        /// -----------------------------------------------------------------------------
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            ID = "Profile.ascx";

            //Get the base Page
            var basePage = Page as PageBase;
            if (basePage != null)
            {
				//Check if culture is RTL
                ProfileProperties.LabelMode = basePage.PageCulture.TextInfo.IsRightToLeft ? LabelMode.Right : LabelMode.Left;
            }
            ProfileProperties.LocalResourceFile = LocalResourceFile;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Page_Load runs when the control is loaded
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <history>
        /// 	[cnurse]	03/01/2006  Created
        /// </history>
        /// -----------------------------------------------------------------------------
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            cmdUpdate.Click += cmdUpdate_Click;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// cmdUpdate_Click runs when the Update Button is clicked
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <history>
        /// 	[cnurse]	03/01/2006  Created
        /// </history>
        /// -----------------------------------------------------------------------------
        private void cmdUpdate_Click(object sender, EventArgs e)
        {
			if (IsUserOrAdmin == false && UserId == Null.NullInteger)
            {
                return;
            }

            if (IsValid)
            {
                var properties = (ProfilePropertyDefinitionCollection) ProfileProperties.DataSource;

                //Update User's profile
                User = ProfileController.UpdateUserProfile(User, properties);

                OnProfileUpdated(EventArgs.Empty);
                OnProfileUpdateCompleted(EventArgs.Empty);
            }
        }
		
		#endregion
    }
}