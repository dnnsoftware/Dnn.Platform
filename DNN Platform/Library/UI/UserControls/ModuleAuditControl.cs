// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.UI.UserControls
{
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

    public abstract class ModuleAuditControl : UserControl
    {
        private const string MyFileName = "ModuleAuditControl.ascx";
        protected Label lblCreatedBy;
        protected Label lblUpdatedBy;

        private static readonly Regex CheckDateColumnRegex = new Regex(@"^-?\d+$", RegexOptions.Compiled);
        private string _systemUser;

        public ModuleAuditControl()
        {
            this.LastModifiedDate = string.Empty;
            this.LastModifiedByUser = string.Empty;
            this.CreatedByUser = string.Empty;
            this.CreatedDate = string.Empty;
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

                    this.ViewState["Entity"] = entity;
                }
                else
                {
                    this.ViewState["Entity"] = null;
                }
            }
        }

        private string DisplayMode => (this.Request.QueryString["Display"] ?? string.Empty).ToLowerInvariant();

        private EntityInfo Model
        {
            get { return this.ViewState["Entity"] as EntityInfo; }
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            try
            {
                if (this.Model != null)
                {
                    this.CreatedByUser = this.Model.CreatedByUserID.ToString();
                    this.CreatedDate = this.Model.CreatedOnDate.ToString();
                    this.LastModifiedByUser = this.Model.LastModifiedByUserID.ToString();
                    this.LastModifiedDate = this.Model.LastModifiedOnDate.ToString();
                }

                // check to see if updated check is redundant
                var isCreatorAndUpdater = this.CreatedByUser == this.LastModifiedByUser &&
                    Globals.NumberMatchRegex.IsMatch(this.CreatedByUser) && Globals.NumberMatchRegex.IsMatch(this.LastModifiedByUser);

                this._systemUser = Localization.GetString("SystemUser", Localization.GetResourceFile(this, MyFileName));
                var displayMode = this.DisplayMode;
                if (displayMode != "editor" && displayMode != "settings")
                {
                    this.ShowCreatedString();
                    this.ShowUpdatedString(isCreatorAndUpdater);
                }
            }
            catch (Exception exc) // Module failed to load
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

        private void ShowCreatedString()
        {
            if (CheckDateColumnRegex.IsMatch(this.CreatedByUser))
            {
                if (int.Parse(this.CreatedByUser) == Null.NullInteger)
                {
                    this.CreatedByUser = this._systemUser;
                }
                else
                {
                    // contains a UserID
                    UserInfo userInfo = UserController.GetUserById(PortalController.Instance.GetCurrentPortalSettings().PortalId, int.Parse(this.CreatedByUser));
                    if (userInfo != null)
                    {
                        this.CreatedByUser = userInfo.DisplayName;
                    }
                }
            }

            string createdString = Localization.GetString("CreatedBy", Localization.GetResourceFile(this, MyFileName));
            this.lblCreatedBy.Text = string.Format(createdString, this.CreatedByUser, this.CreatedDate);
        }

        private void ShowUpdatedString(bool isCreatorAndUpdater)
        {
            // check to see if audit contains update information
            if (string.IsNullOrEmpty(this.LastModifiedDate))
            {
                return;
            }

            if (CheckDateColumnRegex.IsMatch(this.LastModifiedByUser))
            {
                if (isCreatorAndUpdater)
                {
                    this.LastModifiedByUser = this.CreatedByUser;
                }
                else if (int.Parse(this.LastModifiedByUser) == Null.NullInteger)
                {
                    this.LastModifiedByUser = this._systemUser;
                }
                else
                {
                    // contains a UserID
                    UserInfo userInfo = UserController.GetUserById(PortalController.Instance.GetCurrentPortalSettings().PortalId, int.Parse(this.LastModifiedByUser));
                    if (userInfo != null)
                    {
                        this.LastModifiedByUser = userInfo.DisplayName;
                    }
                }
            }

            string updatedByString = Localization.GetString("UpdatedBy", Localization.GetResourceFile(this, MyFileName));
            this.lblUpdatedBy.Text = string.Format(updatedByString, this.LastModifiedByUser, this.LastModifiedDate);
        }

        [Serializable]
        private class EntityInfo
        {
            public int CreatedByUserID { get; set; }

            public DateTime CreatedOnDate { get; set; }

            public int LastModifiedByUserID { get; set; }

            public DateTime LastModifiedOnDate { get; set; }
        }
    }
}
