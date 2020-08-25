// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.UI.WebControls
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Web.UI;

    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Services.Localization;

    /// <summary>
    /// The DNNLocaleEditControl control provides a standard UI component for selecting
    /// a Locale.
    /// </summary>
    [ToolboxData("<{0}:DNNLocaleEditControl runat=server></{0}:DNNLocaleEditControl>")]
    public class DNNLocaleEditControl : TextEditControl, IPostBackEventHandler
    {
        private string _DisplayMode = "Native";
        private LanguagesListType _ListType = LanguagesListType.Enabled;

        protected LanguagesListType ListType
        {
            get
            {
                return this._ListType;
            }
        }

        protected string DisplayMode
        {
            get
            {
                return this._DisplayMode;
            }
        }

        protected PortalSettings PortalSettings
        {
            get
            {
                return PortalController.Instance.GetCurrentPortalSettings();
            }
        }

        public void RaisePostBackEvent(string eventArgument)
        {
            this._DisplayMode = eventArgument;
        }

        /// <summary>
        /// OnAttributesChanged runs when the CustomAttributes property has changed.
        /// </summary>
        protected override void OnAttributesChanged()
        {
            // Get the List settings out of the "Attributes"
            if (this.CustomAttributes != null)
            {
                foreach (Attribute attribute in this.CustomAttributes)
                {
                    var listAtt = attribute as LanguagesListTypeAttribute;
                    if (listAtt != null)
                    {
                        this._ListType = listAtt.ListType;
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// RenderViewMode renders the View (readonly) mode of the control.
        /// </summary>
        /// <param name="writer">A HtmlTextWriter.</param>
        protected override void RenderViewMode(HtmlTextWriter writer)
        {
            Locale locale = LocaleController.Instance.GetLocale(this.StringValue);

            this.ControlStyle.AddAttributesToRender(writer);
            writer.RenderBeginTag(HtmlTextWriterTag.Div);
            if (locale != null)
            {
                writer.Write(locale.Text);
            }

            writer.RenderEndTag();
        }

        /// <summary>
        /// RenderEditMode renders the Edit mode of the control.
        /// </summary>
        /// <param name="writer">A HtmlTextWriter.</param>
        protected override void RenderEditMode(HtmlTextWriter writer)
        {
            // Render div
            writer.AddAttribute(HtmlTextWriterAttribute.Class, "dnnLeft");
            writer.RenderBeginTag(HtmlTextWriterTag.Div);

            IList<CultureInfo> cultures = new List<CultureInfo>();
            switch (this.ListType)
            {
                case LanguagesListType.All:
                    var culturesArray = CultureInfo.GetCultures(CultureTypes.SpecificCultures);
                    Array.Sort(culturesArray, new CultureInfoComparer(this.DisplayMode));
                    cultures = culturesArray.ToList();
                    break;
                case LanguagesListType.Supported:
                    cultures = LocaleController.Instance.GetLocales(Null.NullInteger).Values
                        .Select(c => CultureInfo.GetCultureInfo(c.Code))
                        .ToList();
                    break;
                case LanguagesListType.Enabled:
                    cultures = LocaleController.Instance.GetLocales(this.PortalSettings.PortalId).Values
                        .Select(c => CultureInfo.GetCultureInfo(c.Code))
                        .ToList();
                    break;
            }

            var promptValue = this.StringValue == Null.NullString && cultures.Count > 1 && !this.Required;

            // Render the Select Tag
            writer.AddAttribute(HtmlTextWriterAttribute.Name, this.UniqueID);
            writer.AddAttribute(HtmlTextWriterAttribute.Id, this.ClientID);
            if (promptValue)
            {
                writer.AddAttribute(HtmlTextWriterAttribute.Onchange, "onLocaleChanged(this)");
            }

            writer.RenderBeginTag(HtmlTextWriterTag.Select);

            // Render None selected option
            // Add the Value Attribute
            writer.AddAttribute(HtmlTextWriterAttribute.Value, Null.NullString);

            if (this.StringValue == Null.NullString)
            {
                // Add the Selected Attribute
                writer.AddAttribute(HtmlTextWriterAttribute.Selected, "selected");
            }

            writer.RenderBeginTag(HtmlTextWriterTag.Option);
            writer.Write(Localization.GetString("Not_Specified", Localization.SharedResourceFile));
            writer.RenderEndTag();

            foreach (var culture in cultures)
            {
                this.RenderOption(writer, culture);
            }

            // Close Select Tag
            writer.RenderEndTag();

            if (promptValue)
            {
                writer.WriteBreak();

                writer.AddAttribute(HtmlTextWriterAttribute.Class, "dnnFormMessage dnnFormError");
                writer.RenderBeginTag(HtmlTextWriterTag.Span);
                writer.Write(Localization.GetString("LanguageNotSelected", Localization.SharedResourceFile));
                writer.RenderEndTag();

                writer.AddAttribute(HtmlTextWriterAttribute.Type, "text/javascript");
                writer.RenderBeginTag(HtmlTextWriterTag.Script);
                writer.Write(@"
function onLocaleChanged(element){
    var $this = $(element);
    var $error = $this.next().next();
    var value = $this.val();
    value ? $error.hide() : $error.show();
};
");
                writer.RenderEndTag();
            }

            // Render break
            writer.Write("<br />");

            // Render Span
            writer.AddAttribute(HtmlTextWriterAttribute.Class, "dnnFormRadioButtons");
            writer.RenderBeginTag(HtmlTextWriterTag.Span);

            // Render Button Row
            this.RenderModeButtons(writer);

            // close span
            writer.RenderEndTag();

            // close div
            writer.RenderEndTag();
        }

        private bool IsSelected(string locale)
        {
            return locale == this.StringValue;
        }

        private void RenderModeButtons(HtmlTextWriter writer)
        {
            writer.AddAttribute(HtmlTextWriterAttribute.Type, "radio");
            writer.AddAttribute("aria-label", "Mode");
            if (this.DisplayMode == "English")
            {
                writer.AddAttribute(HtmlTextWriterAttribute.Checked, "checked");
            }
            else
            {
                writer.AddAttribute(HtmlTextWriterAttribute.Onclick, this.Page.ClientScript.GetPostBackEventReference(this, "English"));
            }

            writer.RenderBeginTag(HtmlTextWriterTag.Input);
            writer.RenderEndTag();
            writer.Write(Localization.GetString("EnglishName", Localization.GlobalResourceFile));

            // writer.Write("<br />");
            writer.AddAttribute(HtmlTextWriterAttribute.Type, "radio");
            writer.AddAttribute("aria-label", "Mode");
            if (this.DisplayMode == "Native")
            {
                writer.AddAttribute(HtmlTextWriterAttribute.Checked, "checked");
            }
            else
            {
                writer.AddAttribute(HtmlTextWriterAttribute.Onclick, this.Page.ClientScript.GetPostBackEventReference(this, "Native"));
            }

            writer.RenderBeginTag(HtmlTextWriterTag.Input);
            writer.RenderEndTag();

            writer.Write(Localization.GetString("NativeName", Localization.GlobalResourceFile));
        }

        private void RenderOption(HtmlTextWriter writer, CultureInfo culture)
        {
            string localeName;

            if (this.DisplayMode == "Native")
            {
                localeName = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(culture.NativeName);
            }
            else
            {
                localeName = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(culture.EnglishName);
            }

            // Add the Value Attribute
            writer.AddAttribute(HtmlTextWriterAttribute.Value, culture.Name);

            if (this.IsSelected(culture.Name))
            {
                writer.AddAttribute(HtmlTextWriterAttribute.Selected, "selected");
            }

            // Render Option Tag
            writer.RenderBeginTag(HtmlTextWriterTag.Option);
            writer.Write(localeName);
            writer.RenderEndTag();
        }
    }
}
