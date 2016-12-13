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
using System.Collections.Generic;
using System.Globalization;
using System.Web.UI;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Services.Localization;

#endregion

namespace DotNetNuke.UI.WebControls
{

	/// <summary>
	/// The DNNLocaleEditControl control provides a standard UI component for selecting
	/// a Locale
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
				return _ListType;
			}
		}

		protected string DisplayMode
		{
			get
			{
				return _DisplayMode;
			}
		}

		protected PortalSettings PortalSettings
		{
			get
			{
				return PortalController.Instance.GetCurrentPortalSettings();
			}
		}

		#region IPostBackEventHandler Members

		public void RaisePostBackEvent(string eventArgument)
		{
			_DisplayMode = eventArgument;
		}

		#endregion

		private bool IsSelected(string locale)
		{
			return locale == StringValue;
		}

		private void RenderModeButtons(HtmlTextWriter writer)
		{
			writer.AddAttribute(HtmlTextWriterAttribute.Type, "radio");
			if (DisplayMode == "English")
			{
				writer.AddAttribute(HtmlTextWriterAttribute.Checked, "checked");
			}
			else
			{
				writer.AddAttribute(HtmlTextWriterAttribute.Onclick, Page.ClientScript.GetPostBackEventReference(this, "English"));
			}
			writer.RenderBeginTag(HtmlTextWriterTag.Input);
			writer.RenderEndTag();
			writer.Write(Localization.GetString("EnglishName", Localization.GlobalResourceFile));
			//writer.Write("<br />");

			writer.AddAttribute(HtmlTextWriterAttribute.Type, "radio");
			if (DisplayMode == "Native")
			{
				writer.AddAttribute(HtmlTextWriterAttribute.Checked, "checked");
			}
			else
			{
				writer.AddAttribute(HtmlTextWriterAttribute.Onclick, Page.ClientScript.GetPostBackEventReference(this, "Native"));
			}
			writer.RenderBeginTag(HtmlTextWriterTag.Input);
			writer.RenderEndTag();

			writer.Write(Localization.GetString("NativeName", Localization.GlobalResourceFile));
		}

		private void RenderOption(HtmlTextWriter writer, CultureInfo culture)
		{
			string localeName;

			if (DisplayMode == "Native")
			{
				localeName = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(culture.NativeName);
			}
			else
			{
				localeName = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(culture.EnglishName);
			}
			
			//Add the Value Attribute
			writer.AddAttribute(HtmlTextWriterAttribute.Value, culture.Name);

			if (IsSelected(culture.Name))
			{
				writer.AddAttribute(HtmlTextWriterAttribute.Selected, "selected");
			}
			
			//Render Option Tag
			writer.RenderBeginTag(HtmlTextWriterTag.Option);
			writer.Write(localeName);
			writer.RenderEndTag();
		}

		#region Protected Methods

		/// <summary>
		/// OnAttributesChanged runs when the CustomAttributes property has changed.
		/// </summary>
		protected override void OnAttributesChanged()
		{
			//Get the List settings out of the "Attributes"
			if ((CustomAttributes != null))
			{
				foreach (Attribute attribute in CustomAttributes)
				{
					var listAtt = attribute as LanguagesListTypeAttribute;
					if (listAtt != null)
					{
						_ListType = listAtt.ListType;
						break;
					}
				}
			}
		}

		/// <summary>
		/// RenderViewMode renders the View (readonly) mode of the control
		/// </summary>
		/// <param name="writer">A HtmlTextWriter.</param>
		protected override void RenderViewMode(HtmlTextWriter writer)
		{
			Locale locale = LocaleController.Instance.GetLocale(StringValue);

			ControlStyle.AddAttributesToRender(writer);
			writer.RenderBeginTag(HtmlTextWriterTag.Div);
			if (locale != null)
			{
				writer.Write(locale.Text);
			}
			writer.RenderEndTag();
		}

		/// <summary>
		/// RenderEditMode renders the Edit mode of the control
		/// </summary>
		/// <param name="writer">A HtmlTextWriter.</param>
		protected override void RenderEditMode(HtmlTextWriter writer)
		{
			//Render div
			writer.AddAttribute(HtmlTextWriterAttribute.Class, "dnnLeft");
			writer.RenderBeginTag(HtmlTextWriterTag.Div);

			//Render the Select Tag
			writer.AddAttribute(HtmlTextWriterAttribute.Name, UniqueID);
			writer.AddAttribute(HtmlTextWriterAttribute.Id, ClientID);
			writer.RenderBeginTag(HtmlTextWriterTag.Select);

			//Render None selected option
			//Add the Value Attribute
			writer.AddAttribute(HtmlTextWriterAttribute.Value, Null.NullString);

			if (StringValue == Null.NullString)
			{
				//Add the Selected Attribute
				writer.AddAttribute(HtmlTextWriterAttribute.Selected, "selected");
			}
			writer.RenderBeginTag(HtmlTextWriterTag.Option);
			writer.Write(Localization.GetString("Not_Specified", Localization.SharedResourceFile));
			writer.RenderEndTag();

			int languageCount = 0;
			switch (ListType)
			{
				case LanguagesListType.All:
					CultureInfo[] cultures = CultureInfo.GetCultures(CultureTypes.SpecificCultures);
					Array.Sort(cultures, new CultureInfoComparer(DisplayMode));

					foreach (CultureInfo culture in cultures)
					{
						RenderOption(writer, culture);
					}
					languageCount = cultures.Length;
					break;
				case LanguagesListType.Supported:
					Dictionary<string, Locale> cultures1 = LocaleController.Instance.GetLocales(Null.NullInteger);
					foreach (Locale language in cultures1.Values)
					{
                        RenderOption(writer, CultureInfo.GetCultureInfo(language.Code));
					}
					languageCount = cultures1.Count;
					break;
				case LanguagesListType.Enabled:
					Dictionary<string, Locale> cultures2 = LocaleController.Instance.GetLocales(PortalSettings.PortalId);
					foreach (Locale language in cultures2.Values)
					{
                        RenderOption(writer, CultureInfo.GetCultureInfo(language.Code));
					}
					languageCount = cultures2.Count;
					break;
			}
			//Close Select Tag
			writer.RenderEndTag();

			if (StringValue == Null.NullString && languageCount > 1)
			{
				writer.WriteBreak();

				writer.AddAttribute(HtmlTextWriterAttribute.Class, "dnnFormMessage dnnFormError");
				writer.RenderBeginTag(HtmlTextWriterTag.Span);
				writer.Write(Localization.GetString("LanguageNotSelected", Localization.SharedResourceFile));
				writer.RenderEndTag();
			}

			//Render break
			writer.Write("<br />");

			//Render Span
			writer.AddAttribute(HtmlTextWriterAttribute.Class, "dnnFormRadioButtons");
			writer.RenderBeginTag(HtmlTextWriterTag.Span);

			//Render Button Row
			RenderModeButtons(writer);

			//close span
			writer.RenderEndTag();

			//close div
			writer.RenderEndTag();
		}
		
		#endregion
	}
}
