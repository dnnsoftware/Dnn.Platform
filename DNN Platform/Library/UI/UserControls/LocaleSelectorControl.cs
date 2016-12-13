#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2017
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