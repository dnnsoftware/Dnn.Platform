#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2018
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
using System.Text.RegularExpressions;
using System.Web.UI;
using System.Web.UI.WebControls;
using DotNetNuke.Common;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using DotNetNuke.Services.Exceptions;
using DotNetNuke.Services.Localization;

#endregion

namespace DotNetNuke.UI.UserControls
{
    public abstract class ModuleAuditControl : UserControl
    {
        private const string MyFileName = "ModuleAuditControl.ascx";
        private string _systemUser;
        protected Label lblCreatedBy;
        protected Label lblUpdatedBy;

        private static readonly Regex CheckDateColumnRegex = new Regex(@"^-?\d+$", RegexOptions.Compiled);

        private string DisplayMode => (Request.QueryString["Display"] ?? "").ToLowerInvariant();

        [Serializable]
		private class EntityInfo
		{
			public int CreatedByUserID { get; set; }
			public DateTime CreatedOnDate { get; set; }
			public int LastModifiedByUserID { get; set; }
			public DateTime LastModifiedOnDate { get; set; }
		}

        public ModuleAuditControl()
        {
            LastModifiedDate = String.Empty;
            LastModifiedByUser = String.Empty;
            CreatedByUser = String.Empty;
            CreatedDate = String.Empty;
        }

        public string CreatedDate { private get; set; }

        public string CreatedByUser { private get; set; }

        public string LastModifiedByUser { private get; set; }

        public string LastModifiedDate { private get; set; }

		public BaseEntityInfo Entity
		{
			set
			{
				if (value != null)
				{
					var entity = new EntityInfo();
					entity.CreatedByUserID = value.CreatedByUserID;
					entity.CreatedOnDate = value.CreatedOnDate;
					entity.LastModifiedByUserID = value.LastModifiedByUserID;
					entity.LastModifiedOnDate = value.LastModifiedOnDate;

					ViewState["Entity"] = entity;
				}
				else
				{
					ViewState["Entity"] = null;
				}
			}
		}

	    private EntityInfo Model
	    {
		    get { return ViewState["Entity"] as EntityInfo; }
	    }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            try
            {
				if (Model != null)
                {
					CreatedByUser = Model.CreatedByUserID.ToString();
					CreatedDate = Model.CreatedOnDate.ToString();
					LastModifiedByUser = Model.LastModifiedByUserID.ToString();
					LastModifiedDate = Model.LastModifiedOnDate.ToString();
                }

                //check to see if updated check is redundant
                var isCreatorAndUpdater = CreatedByUser == LastModifiedByUser &&
                    Globals.NumberMatchRegex.IsMatch(CreatedByUser) && Globals.NumberMatchRegex.IsMatch(LastModifiedByUser);

                _systemUser = Localization.GetString("SystemUser", Localization.GetResourceFile(this, MyFileName));
                var displayMode = DisplayMode;
                if (displayMode != "editor" && displayMode != "settings")
                {
                    ShowCreatedString();
                    ShowUpdatedString(isCreatorAndUpdater);
                }
            }
            catch (Exception exc) //Module failed to load
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

        private void ShowCreatedString()
        {
            if (CheckDateColumnRegex.IsMatch(CreatedByUser))
            {
                if (int.Parse(CreatedByUser) == Null.NullInteger)
                {
                    CreatedByUser = _systemUser;
                }
                else
                {
                    //contains a UserID
                    UserInfo userInfo = UserController.GetUserById(PortalController.Instance.GetCurrentPortalSettings().PortalId, int.Parse(CreatedByUser));
                    if (userInfo != null)
                    {
                        CreatedByUser = userInfo.DisplayName;
                    }
                }
            }
            string createdString = Localization.GetString("CreatedBy", Localization.GetResourceFile(this, MyFileName));
            lblCreatedBy.Text = string.Format(createdString, CreatedByUser, CreatedDate);

        }

        private void ShowUpdatedString(bool isCreatorAndUpdater)
        {
            //check to see if audit contains update information
            if (string.IsNullOrEmpty(LastModifiedDate))
            {
                return;
            }

            if (CheckDateColumnRegex.IsMatch(LastModifiedByUser))
            {
                if (isCreatorAndUpdater)
                {
                    LastModifiedByUser = CreatedByUser;
                }
                else if (int.Parse(LastModifiedByUser) == Null.NullInteger)
                {
                    LastModifiedByUser = _systemUser;
                }
                else
                {
                    //contains a UserID
                    UserInfo userInfo = UserController.GetUserById(PortalController.Instance.GetCurrentPortalSettings().PortalId, int.Parse(LastModifiedByUser));
                    if (userInfo != null)
                    {
                        LastModifiedByUser = userInfo.DisplayName;
                    }
                }
            }

            string updatedByString = Localization.GetString("UpdatedBy", Localization.GetResourceFile(this, MyFileName));
            lblUpdatedBy.Text = string.Format(updatedByString, LastModifiedByUser, LastModifiedDate);
        }
    }
}