// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
#region Usings

using System;
using System.Web.UI.WebControls;

using DotNetNuke.Framework;
using DotNetNuke.Services.Localization;
using DotNetNuke.Services.Personalization;

#endregion

namespace DotNetNuke.UI.UserControls
{
    /// -----------------------------------------------------------------------------
    /// <summary>
    ///   LocaleSelectorControl is a user control that provides all the server code to manage
    ///   localisation selection
    /// </summary>
    /// -----------------------------------------------------------------------------
    public abstract class LocaleSelectorControl : UserControlBase
    {
        #region "Controls"

        protected DropDownList ddlPortalDefaultLanguage;
        protected Literal litStatus;
        protected RadioButtonList rbViewType;

        #endregion

        private string MyFileName = "LocaleSelectorControl.ascx";
        private string _ViewType = "";

        private CultureDropDownTypes DisplayType
        {
            get
            {
                switch (ViewType)
                {
                    case "NATIVE":
                        return CultureDropDownTypes.NativeName;
                    case "ENGLISH":
                        return CultureDropDownTypes.EnglishName;
                    default:
                        return CultureDropDownTypes.NativeName;
                }
            }
        }

        private string ViewType
        {
            get
            {
                if (string.IsNullOrEmpty(_ViewType))
                {
                    _ViewType = Convert.ToString(Personalization.GetProfile("LanguageEnabler", string.Format("ViewType{0}", PortalSettings.PortalId)));
                }
                if (string.IsNullOrEmpty(_ViewType))
                {
                    _ViewType = "NATIVE";
                }
                return _ViewType;
            }
        }

        #region "Public Methods"

        public void BindDefaultLanguageSelector()
        {
            if (Page.IsPostBack == false)
            {
                Localization.LoadCultureDropDownList(ddlPortalDefaultLanguage, DisplayType, PortalSettings.DefaultLanguage, true);
            }
        }

        #endregion

        #region "Public properties"

        public string CultureCode
        {
            get
            {
                return ddlPortalDefaultLanguage.SelectedValue;
            }
        }

        #endregion

        #region "Event Handlers"

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            rbViewType.SelectedIndexChanged += rbViewType_SelectedIndexChanged;

            if (!Page.IsPostBack)
            {
                ListItem item = default(ListItem);

                item = new ListItem(Localization.GetString("NativeName.Text", Localization.GetResourceFile(this, MyFileName)), "NATIVE");
                rbViewType.Items.Add(item);
                if (ViewType == "NATIVE")
                {
                    item.Selected = true;
                }
                item = new ListItem(Localization.GetString("EnglishName.Text", Localization.GetResourceFile(this, MyFileName)), "ENGLISH");
                rbViewType.Items.Add(item);
                if (ViewType == "ENGLISH")
                {
                    item.Selected = true;
                }
            }
        }

        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);

            if (Visible)
            {
                BindDefaultLanguageSelector();
            }
        }

        private void rbViewType_SelectedIndexChanged(object sender, EventArgs e)
        {
            _ViewType = rbViewType.SelectedValue;
        }

        #endregion
    }
}
