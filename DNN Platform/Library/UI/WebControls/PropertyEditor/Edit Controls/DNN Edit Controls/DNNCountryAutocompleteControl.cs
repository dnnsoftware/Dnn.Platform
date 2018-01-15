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
using System.Linq;
using System.Web.UI.WebControls;
using System.Web.UI;
using DotNetNuke.Common.Lists;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Framework;
using DotNetNuke.Framework.JavaScriptLibraries;
using DotNetNuke.Web.Client;
using DotNetNuke.Web.Client.ClientResourceManagement;

#endregion

namespace DotNetNuke.UI.WebControls
{

	[ToolboxData("<{0}:DnnCountryAutocompleteControl runat=server></{0}:DnnCountryAutocompleteControl>")]
	public class DnnCountryAutocompleteControl : EditControl
	{

		#region " Controls "
		private TextBox _CountryName;
		private TextBox CountryName
		{
			get
			{
				if (_CountryName == null)
				{
					_CountryName = new TextBox();
				}
				return _CountryName;
			}
		}

		private HiddenField _CountryId;
		private HiddenField CountryId
		{
			get
			{
				if (_CountryId == null)
				{
					_CountryId = new HiddenField();
				}
				return _CountryId;
			}
		}

		public override string EditControlClientId
		{
			get
			{
				EnsureChildControls();
				return CountryName.ClientID;
			}
		}

		#endregion

		#region " Properties "
		protected override string StringValue
		{
			get
			{
				string strValue = Null.NullString;
				if (Value != null)
				{
					strValue = Convert.ToString(Value);
				}
				return strValue;
			}
			set { this.Value = value; }
		}

		protected string OldStringValue
		{
			get { return Convert.ToString(OldValue); }
		}
		#endregion

		#region " Constructors "
		public DnnCountryAutocompleteControl()
		{
			Init += DnnCountryRegionControl_Init;
		}
		public DnnCountryAutocompleteControl(string type)
		{
			Init += DnnCountryRegionControl_Init;
			SystemType = type;
		}
		#endregion

		#region " Overrides "
		protected override void OnDataChanged(EventArgs e)
		{
			PropertyEditorEventArgs args = new PropertyEditorEventArgs(Name);
			args.Value = StringValue;
			args.OldValue = OldStringValue;
			args.StringValue = StringValue;
			base.OnValueChanged(args);
		}

		protected override void CreateChildControls()
		{
			base.CreateChildControls();

			CountryName.ControlStyle.CopyFrom(ControlStyle);
			CountryName.ID = ID + "_name";
			CountryName.Attributes.Add("data-name", Name);
			CountryName.Attributes.Add("data-list", "Country");
			CountryName.Attributes.Add("data-category", Category);
			CountryName.Attributes.Add("data-editor", "DnnCountryAutocompleteControl");
			CountryName.Attributes.Add("data-required", Required.ToString().ToLower());
			Controls.Add(CountryName);

			CountryId.ID = ID + "_id";
			Controls.Add(CountryId);

		}

		public override bool LoadPostData(string postDataKey, System.Collections.Specialized.NameValueCollection postCollection)
		{
			bool dataChanged = false;
			string presentValue = StringValue;
			string postedValue = postCollection[postDataKey + "_id"];
			if (!presentValue.Equals(postedValue))
			{
				Value = postedValue;
				dataChanged = true;
			}
			return dataChanged;
		}

		protected override void OnPreRender(System.EventArgs e)
		{
			base.OnPreRender(e);

			LoadControls();

			if (Page != null & EditMode == PropertyEditorMode.Edit)
			{
				Page.RegisterRequiresPostBack(this);
				Page.RegisterRequiresPostBack(CountryId);
			}

		}

		protected override void RenderEditMode(HtmlTextWriter writer)
		{
			RenderChildren(writer);
		}
		#endregion

		#region " Page Events "
		private void DnnCountryRegionControl_Init(object sender, System.EventArgs e)
		{
			ServicesFramework.Instance.RequestAjaxAntiForgerySupport();
			ClientResourceManager.RegisterScript(this.Page, "~/Resources/Shared/components/CountriesRegions/dnn.CountriesRegions.js");
			ClientResourceManager.RegisterFeatureStylesheet(this.Page, "~/Resources/Shared/components/CountriesRegions/dnn.CountriesRegions.css");
			JavaScript.RequestRegistration(CommonJs.jQuery);
			JavaScript.RequestRegistration(CommonJs.jQueryUI);
		}

		#endregion

		#region " Private Methods "
		private void LoadControls()
		{

			CountryName.Text = StringValue;
			int countryId = -1;
			string countryCode = StringValue;
			if (!string.IsNullOrEmpty(StringValue) && int.TryParse(StringValue, out countryId))
			{
				var listController = new ListController();
				var c = listController.GetListEntryInfo(countryId);
				CountryName.Text = c.Text;
				countryCode = c.Value;
			}
			CountryId.Value = StringValue;

			var regionControl2 = ControlUtilities.FindFirstDescendent<DNNRegionEditControl>(Page, c => IsCoupledRegionControl(c));
			if (regionControl2 != null)
			{
				regionControl2.ParentKey = "Country." + countryCode;
			}

		}

		private bool IsCoupledRegionControl(Control ctr)
		{
			if (ctr is DNNRegionEditControl)
			{
				var c = (DNNRegionEditControl)ctr;
				if (c.Category == this.Category)
				{
					return true;
				}
			}
			return false;
		}
		#endregion

	}
}
