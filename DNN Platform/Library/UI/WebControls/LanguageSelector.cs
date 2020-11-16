// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.UI.WebControls
{
    using System;
    using System.Collections;
    using System.Globalization;
    using System.Web.UI;
    using System.Web.UI.HtmlControls;
    using System.Web.UI.WebControls;

    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Services.Localization;

    /// <summary>
    /// Language Selector control.
    /// </summary>
    public class LanguageSelector : Control, INamingContainer
    {
        private Panel pnlControl;

        public enum LanguageItemStyle
        {
            FlagOnly = 1,
            FlagAndCaption = 2,
            CaptionOnly = 3,
        }

        public enum LanguageListDirection
        {
            Horizontal = 1,
            Vertical = 2,
        }

        /// <summary>
        /// Language Selection mode, offered to the user: single select or multiple select.
        /// </summary>
        public enum LanguageSelectionMode
        {
            Multiple = 1,
            Single = 2,
        }

        /// <summary>
        /// Selection object: Language ("de", "en") or Locale ("de-DE", "en-US").
        /// </summary>
        public enum LanguageSelectionObject
        {
            NeutralCulture = 1,
            SpecificCulture = 2,
        }

        /// <summary>
        /// Gets or sets selection mode (single, multiple).
        /// </summary>
        public LanguageSelectionMode SelectionMode
        {
            get
            {
                if (this.ViewState["SelectionMode"] == null)
                {
                    return LanguageSelectionMode.Single;
                }
                else
                {
                    return (LanguageSelectionMode)this.ViewState["SelectionMode"];
                }
            }

            set
            {
                if (this.SelectionMode != value)
                {
                    this.ViewState["SelectionMode"] = value;
                    if (this.Controls.Count > 0)
                    {
                        this.CreateChildControls(); // Recreate if already created
                    }
                }
            }
        }

        /// <summary>
        /// Gets or sets the type of objects to be selectable: NeutralCulture ("de") or SpecificCulture ("de-DE").
        /// </summary>
        public LanguageSelectionObject SelectionObject
        {
            get
            {
                if (this.ViewState["SelectionObject"] == null)
                {
                    return LanguageSelectionObject.SpecificCulture;
                }
                else
                {
                    return (LanguageSelectionObject)this.ViewState["SelectionObject"];
                }
            }

            set
            {
                if ((int)this.SelectionMode != (int)value)
                {
                    this.ViewState["SelectionObject"] = value;
                    if (this.Controls.Count > 0)
                    {
                        this.CreateChildControls(); // Recreate if already created
                    }
                }
            }
        }

        /// <summary>
        /// Gets or sets the style of the language items.
        /// </summary>
        public LanguageItemStyle ItemStyle
        {
            get
            {
                if (this.ViewState["ItemStyle"] == null)
                {
                    return LanguageItemStyle.FlagAndCaption;
                }
                else
                {
                    return (LanguageItemStyle)this.ViewState["ItemStyle"];
                }
            }

            set
            {
                if (this.ItemStyle != value)
                {
                    this.ViewState["ItemStyle"] = value;
                    if (this.Controls.Count > 0)
                    {
                        this.CreateChildControls(); // Recreate if already created
                    }
                }
            }
        }

        /// <summary>
        /// Gets or sets the direction of the language list.
        /// </summary>
        public LanguageListDirection ListDirection
        {
            get
            {
                if (this.ViewState["ListDirection"] == null)
                {
                    return LanguageListDirection.Vertical;
                }
                else
                {
                    return (LanguageListDirection)this.ViewState["ListDirection"];
                }
            }

            set
            {
                if (this.ListDirection != value)
                {
                    this.ViewState["ListDirection"] = value;
                    if (this.Controls.Count > 0)
                    {
                        this.CreateChildControls(); // Recreate if already created
                    }
                }
            }
        }

        /// <summary>
        /// Gets or sets the list of selected languages.
        /// </summary>
        public string[] SelectedLanguages
        {
            get
            {
                this.EnsureChildControls();
                var a = new ArrayList();
                if (this.GetCultures(this.SelectionObject == LanguageSelectionObject.SpecificCulture).Length < 2)
                {
                    // return single language
                    PortalSettings _Settings = PortalController.Instance.GetCurrentPortalSettings();
                    foreach (string strLocale in LocaleController.Instance.GetLocales(_Settings.PortalId).Keys)
                    {
                        a.Add(strLocale);
                    }
                }
                else
                {
                    // create list of selected languages
                    foreach (CultureInfo c in this.GetCultures(this.SelectionObject == LanguageSelectionObject.SpecificCulture))
                    {
                        if (this.SelectionMode == LanguageSelectionMode.Single)
                        {
                            if (((RadioButton)this.pnlControl.FindControl("opt" + c.Name)).Checked)
                            {
                                a.Add(c.Name);
                            }
                        }
                        else
                        {
                            if (((CheckBox)this.pnlControl.FindControl("chk" + c.Name)).Checked)
                            {
                                a.Add(c.Name);
                            }
                        }
                    }
                }

                return a.ToArray(typeof(string)) as string[];
            }

            set
            {
                this.EnsureChildControls();
                if (this.SelectionMode == LanguageSelectionMode.Single && value.Length > 1)
                {
                    throw new ArgumentException("Selection mode 'single' cannot have more than one selected item.");
                }

                foreach (CultureInfo c in this.GetCultures(this.SelectionObject == LanguageSelectionObject.SpecificCulture))
                {
                    if (this.SelectionMode == LanguageSelectionMode.Single)
                    {
                        ((RadioButton)this.pnlControl.FindControl("opt" + c.Name)).Checked = false;
                    }
                    else
                    {
                        ((CheckBox)this.pnlControl.FindControl("chk" + c.Name)).Checked = false;
                    }
                }

                foreach (string strLocale in value)
                {
                    if (this.SelectionMode == LanguageSelectionMode.Single)
                    {
                        Control ctl = this.pnlControl.FindControl("opt" + strLocale);
                        if (ctl != null)
                        {
                            ((RadioButton)ctl).Checked = true;
                        }
                    }
                    else
                    {
                        Control ctl = this.pnlControl.FindControl("chk" + strLocale);
                        if (ctl != null)
                        {
                            ((CheckBox)ctl).Checked = true;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Create Child Controls.
        /// </summary>
        protected override void CreateChildControls()
        {
            this.Controls.Clear();
            this.pnlControl = new Panel();
            this.pnlControl.CssClass = "dnnLangSelector";

            this.Controls.Add(this.pnlControl);
            this.pnlControl.Controls.Add(new LiteralControl("<ul>"));

            foreach (var c in this.GetCultures(this.SelectionObject == LanguageSelectionObject.SpecificCulture))
            {
                this.pnlControl.Controls.Add(new LiteralControl("<li>"));

                var lblLocale = new HtmlGenericControl("label");
                if (this.SelectionMode == LanguageSelectionMode.Single)
                {
                    var optLocale = new RadioButton();
                    optLocale.ID = "opt" + c.Name;
                    optLocale.GroupName = this.pnlControl.ID + "_Locale";
                    if (c.Name == Localization.SystemLocale)
                    {
                        optLocale.Checked = true;
                    }

                    this.pnlControl.Controls.Add(optLocale);
                    lblLocale.Attributes["for"] = optLocale.ClientID;
                }
                else
                {
                    var chkLocale = new CheckBox();
                    chkLocale.ID = "chk" + c.Name;
                    this.pnlControl.Controls.Add(chkLocale);
                    lblLocale.Attributes["for"] = chkLocale.ClientID;
                }

                this.pnlControl.Controls.Add(lblLocale);
                if (this.ItemStyle != LanguageItemStyle.CaptionOnly)
                {
                    var imgLocale = new Image();
                    imgLocale.ImageUrl = this.ResolveUrl("~/images/Flags/" + c.Name + ".gif");
                    imgLocale.AlternateText = c.DisplayName;
                    imgLocale.Style["vertical-align"] = "middle";
                    lblLocale.Controls.Add(imgLocale);
                }

                if (this.ItemStyle != LanguageItemStyle.FlagOnly)
                {
                    lblLocale.Controls.Add(new LiteralControl("&nbsp;" + c.DisplayName));
                }

                this.pnlControl.Controls.Add(new LiteralControl("</li>"));
            }

            this.pnlControl.Controls.Add(new LiteralControl("</ul>"));

            // Hide if not more than one language
            if (this.GetCultures(this.SelectionObject == LanguageSelectionObject.SpecificCulture).Length < 2)
            {
                this.Visible = false;
            }
        }

        /// <summary>
        /// retrieve the cultures, currently supported by the portal.
        /// </summary>
        /// <param name="specific">true: locales, false: neutral languages.</param>
        /// <returns>Array of cultures.</returns>
        private CultureInfo[] GetCultures(bool specific)
        {
            var a = new ArrayList();
            PortalSettings _Settings = PortalController.Instance.GetCurrentPortalSettings();
            foreach (string strLocale in LocaleController.Instance.GetLocales(_Settings.PortalId).Keys)
            {
                var c = new CultureInfo(strLocale);
                if (specific)
                {
                    a.Add(c);
                }
                else
                {
                    CultureInfo p = c.Parent;
                    if (!a.Contains(p))
                    {
                        a.Add(p);
                    }
                }
            }

            return (CultureInfo[])a.ToArray(typeof(CultureInfo));
        }
    }
}
