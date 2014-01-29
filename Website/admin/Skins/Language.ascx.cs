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
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web.UI.WebControls;

using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Tabs;
using DotNetNuke.Framework;
using DotNetNuke.Security;
using DotNetNuke.Services.Exceptions;
using DotNetNuke.Services.Localization;

#endregion

namespace DotNetNuke.UI.Skins.Controls
{
    /// <summary>
    /// The Language skinobject allows the visitor to select the language of the page
    /// </summary>
    /// <remarks></remarks>
    public partial class Language : SkinObjectBase
    {
		#region Private Members

        private const string MyFileName = "Language.ascx";
        private string _SelectedItemTemplate;
        private string _alternateTemplate;
        private string _commonFooterTemplate;
        private string _commonHeaderTemplate;
        private string _footerTemplate;
        private string _headerTemplate;
        private string _itemTemplate;
        private string _localResourceFile;
        private LanguageTokenReplace _localTokenReplace;
        private string _separatorTemplate;
        private bool _showMenu = true;
		
		#endregion

		#region Public Properties

        public string AlternateTemplate
        {
            get
            {
                if (string.IsNullOrEmpty(_alternateTemplate))
                {
                    _alternateTemplate = Localization.GetString("AlternateTemplate.Default", LocalResourceFile, TemplateCulture);
                }
                return _alternateTemplate;
            }
            set
            {
                _alternateTemplate = value;
            }
        }

        public string CommonFooterTemplate
        {
            get
            {
                if (string.IsNullOrEmpty(_commonFooterTemplate))
                {
                    _commonFooterTemplate = Localization.GetString("CommonFooterTemplate.Default", LocalResourceFile, TemplateCulture);
                }
                return _commonFooterTemplate;
            }
            set
            {
                _commonFooterTemplate = value;
            }
        }

        public string CommonHeaderTemplate
        {
            get
            {
                if (string.IsNullOrEmpty(_commonHeaderTemplate))
                {
                    _commonHeaderTemplate = Localization.GetString("CommonHeaderTemplate.Default", LocalResourceFile, TemplateCulture);
                }
                return _commonHeaderTemplate;
            }
            set
            {
                _commonHeaderTemplate = value;
            }
        }

        public string CssClass { get; set; }

        public string FooterTemplate
        {
            get
            {
                if (string.IsNullOrEmpty(_footerTemplate))
                {
                    _footerTemplate = Localization.GetString("FooterTemplate.Default", LocalResourceFile, TemplateCulture);
                }
                return _footerTemplate;
            }
            set
            {
                _footerTemplate = value;
            }
        }

        public string HeaderTemplate
        {
            get
            {
                if (string.IsNullOrEmpty(_headerTemplate))
                {
                    _headerTemplate = Localization.GetString("HeaderTemplate.Default", LocalResourceFile, TemplateCulture);
                }
                return _headerTemplate;
            }
            set
            {
                _headerTemplate = value;
            }
        }

        public string ItemTemplate
        {
            get
            {
                if (string.IsNullOrEmpty(_itemTemplate))
                {
                    _itemTemplate = Localization.GetString("ItemTemplate.Default", LocalResourceFile, TemplateCulture);
                }
                return _itemTemplate;
            }
            set
            {
                _itemTemplate = value;
            }
        }

        public string SelectedItemTemplate
        {
            get
            {
                if (string.IsNullOrEmpty(_SelectedItemTemplate))
                {
                    _SelectedItemTemplate = Localization.GetString("SelectedItemTemplate.Default", LocalResourceFile, TemplateCulture);
                }
                return _SelectedItemTemplate;
            }
            set
            {
                _SelectedItemTemplate = value;
            }
        }

        public string SeparatorTemplate
        {
            get
            {
                if (string.IsNullOrEmpty(_separatorTemplate))
                {
                    _separatorTemplate = Localization.GetString("SeparatorTemplate.Default", LocalResourceFile, TemplateCulture);
                }
                return _separatorTemplate;
            }
            set
            {
                _separatorTemplate = value;
            }
        }

        public bool ShowLinks { get; set; }

        public bool ShowMenu
        {
            get
            {
                if ((_showMenu == false) && (ShowLinks == false))
                {
					//this is to make sure that at least one type of selector will be visible if multiple languages are enabled
                    _showMenu = true;
                }
                return _showMenu;
            }
            set
            {
                _showMenu = value;
            }
        }

        public bool UseCurrentCultureForTemplate { get; set;  }

		#endregion

		#region Protected Properties

        protected string CurrentCulture
        {
            get
            {
                return CultureInfo.CurrentCulture.ToString();
            }
        }

        protected string TemplateCulture
        {
            get
            {
                return (UseCurrentCultureForTemplate) ? CurrentCulture : "en-US";
            }
        }

        
        protected string LocalResourceFile
        {
            get
            {
                if (string.IsNullOrEmpty(_localResourceFile))
                {
                    _localResourceFile = Localization.GetResourceFile(this, MyFileName);
                }
                return _localResourceFile;
            }
        }

        protected LanguageTokenReplace LocalTokenReplace
        {
            get
            {
                if (_localTokenReplace == null)
                {
                    _localTokenReplace = new LanguageTokenReplace {resourceFile = LocalResourceFile};
                }
                return _localTokenReplace;
            }
        }

		#endregion

		#region Private Methods

        private string parseTemplate(string template, string locale)
        {
            string strReturnValue = template;
            try
            {
                if (!string.IsNullOrEmpty(locale))
                {
					//for non data items use locale
                    LocalTokenReplace.Language = locale;
                }
                else
                {
					//for non data items use page culture
                    LocalTokenReplace.Language = CurrentCulture;
                }
				
				//perform token replacements
                strReturnValue = LocalTokenReplace.ReplaceEnvironmentTokens(strReturnValue);
            }
            catch (Exception ex)
            {
                Exceptions.ProcessPageLoadException(ex, Request.RawUrl);
            }
            return strReturnValue;
        }

        private void handleCommonTemplates()
        {
            if (string.IsNullOrEmpty(CommonHeaderTemplate))
            {
                litCommonHeaderTemplate.Visible = false;
            }
            else
            {
                litCommonHeaderTemplate.Text = parseTemplate(CommonHeaderTemplate, CurrentCulture);
            }
            if (string.IsNullOrEmpty(CommonFooterTemplate))
            {
                litCommonFooterTemplate.Visible = false;
            }
            else
            {
                litCommonFooterTemplate.Text = parseTemplate(CommonFooterTemplate, CurrentCulture);
            }
        }

		private bool LocaleIsAvailable(Locale locale)
		{
			var tab = PortalSettings.ActiveTab;
			if (tab.DefaultLanguageTab != null)
			{
				tab = tab.DefaultLanguageTab;
			}

			return new TabController().GetTabByCulture(tab.TabID, tab.PortalID, locale) != null;
		}

		#endregion

		#region Event Handlers

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            selectCulture.SelectedIndexChanged += selectCulture_SelectedIndexChanged;
            rptLanguages.ItemDataBound += rptLanguages_ItemDataBound;

            try
            {
                if (ShowLinks)
                {
                    var locales = new Dictionary<string, Locale>();
                    foreach (Locale loc in LocaleController.Instance.GetLocales(PortalSettings.PortalId).Values)
                    {
                        string defaultRoles = PortalController.GetPortalSetting(string.Format("DefaultTranslatorRoles-{0}", loc.Code), PortalSettings.PortalId, "Administrators");
                        if (!PortalSettings.ContentLocalizationEnabled ||
							(LocaleIsAvailable(loc) &&
								(PortalSecurity.IsInRoles(PortalSettings.AdministratorRoleName) || loc.IsPublished || PortalSecurity.IsInRoles(defaultRoles))))
                        {
                            locales.Add(loc.Code, loc);
                        }
                    }
                    if (locales.Count > 1)
                    {
                        rptLanguages.DataSource = locales.Values;
                        rptLanguages.DataBind();
                    }
                    else
                    {
                        rptLanguages.Visible = false;
                    }
                }
                if (!Page.IsPostBack)
                {
                    if (ShowMenu)
                    {
                        if (!String.IsNullOrEmpty(CssClass))
                        {
                            selectCulture.CssClass = CssClass;
                        }
                        Localization.LoadCultureDropDownList(selectCulture, CultureDropDownTypes.NativeName, CurrentCulture);

                        //only show language selector if more than one language
                        if (selectCulture.Items.Count <= 1)
                        {
                            selectCulture.Visible = false;
                        }
                    }
                    else
                    {
                        selectCulture.Visible = false;
                    }
                    handleCommonTemplates();
                }
            }
            catch (Exception ex)
            {
                Exceptions.ProcessPageLoadException(ex, Request.RawUrl);
            }
        }

        private void selectCulture_SelectedIndexChanged(object sender, EventArgs e)
        {
			//Redirect to same page to update all controls for newly selected culture
            LocalTokenReplace.Language = selectCulture.SelectedItem.Value;
            Response.Redirect(LocalTokenReplace.ReplaceEnvironmentTokens("[URL]"));
        }

        /// <summary>
        /// Binds data to repeater. a template is used to render the items
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <history>
        ///      [erikvb]  20070814	  added
        /// </history>
        protected void rptLanguages_ItemDataBound(object sender, RepeaterItemEventArgs e)
        {
            try
            {
                var litTemplate = e.Item.FindControl("litItemTemplate") as Literal;
                if (litTemplate != null)
                {
					//load proper template for this Item
                    string strTemplate = "";
                    switch (e.Item.ItemType)
                    {
                        case ListItemType.Item:
                            strTemplate = ItemTemplate;
                            break;
                        case ListItemType.AlternatingItem:
                            if (!string.IsNullOrEmpty(AlternateTemplate))
                            {
                                strTemplate = AlternateTemplate;
                            }
                            else
                            {
                                strTemplate = ItemTemplate;
                            }
                            break;
                        case ListItemType.Header:
                            strTemplate = HeaderTemplate;
                            break;
                        case ListItemType.Footer:
                            strTemplate = FooterTemplate;
                            break;
                        case ListItemType.Separator:
                            strTemplate = SeparatorTemplate;
                            break;
                    }
                    if (string.IsNullOrEmpty(strTemplate))
                    {
                        litTemplate.Visible = false;
                    }
                    else
                    {
                        if (e.Item.DataItem != null)
                        {
                            var locale = e.Item.DataItem as Locale;
                            if (locale != null && (e.Item.ItemType == ListItemType.Item || e.Item.ItemType == ListItemType.AlternatingItem))
                            {
                                if (locale.Code == CurrentCulture && !string.IsNullOrEmpty(SelectedItemTemplate))
                                {
                                    strTemplate = SelectedItemTemplate;
                                }
                                litTemplate.Text = parseTemplate(strTemplate, locale.Code);
                            }
                        }
                        else
                        {
							//for non data items use page culture
                            litTemplate.Text = parseTemplate(strTemplate, CurrentCulture);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Exceptions.ProcessPageLoadException(ex, Request.RawUrl);
            }
        }
		
		#endregion
    }
}