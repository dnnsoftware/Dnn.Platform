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
using System.Linq;
using System.Web.UI.WebControls;
using System.Web.UI;
using DotNetNuke.Common.Utilities;
using System.Web.UI.HtmlControls;
using DotNetNuke.Web.Client.ClientResourceManagement;
using DotNetNuke.Framework.JavaScriptLibraries;
using System.Collections.Generic;
using DotNetNuke.Common.Lists;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Framework;
using DotNetNuke.Services.Localization;
using DotNetNuke.Web.Client;

#endregion

namespace DotNetNuke.UI.WebControls
{	/// -----------------------------------------------------------------------------
	/// Project:    DotNetNuke
	/// Namespace:  DotNetNuke.UI.WebControls
	/// Class:      DNNRegionEditControl
	/// -----------------------------------------------------------------------------
	/// <summary>
	/// The DNNRegionEditControl control provides a standard UI component for editing
	/// Regions
	/// </summary>
	/// -----------------------------------------------------------------------------
	[ToolboxData("<{0}:DNNRegionEditControl runat=server></{0}:DNNRegionEditControl>")]
	public class DNNRegionEditControl : EditControl
	{

		#region Controls
		private DropDownList _Regions;
		private DropDownList Regions
		{
			get
			{
				if (_Regions == null)
				{
					_Regions = new DropDownList();
				}
				return _Regions;
			}
		}

		private TextBox _Region;
		private TextBox Region
		{
			get
			{
				if (_Region == null)
				{
					_Region = new TextBox();
				}
				return _Region;
			}
		}

		private HtmlInputHidden _InitialValue;
		private HtmlInputHidden RegionCode
		{
			get
			{
				if (_InitialValue == null)
				{
					_InitialValue = new HtmlInputHidden();
				}
				return _InitialValue;
			}
		}
		#endregion

		#region Properties
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

		/// <summary>
		/// The parent key of the List to display
		/// </summary>
		public string ParentKey { get; set; }

		private List<ListEntryInfo> _listEntries;
		/// <summary>
		/// Gets the ListEntryInfo objects associated witht the control
		/// </summary>
		protected IEnumerable<ListEntryInfo> ListEntries
		{
			get
			{
				if (_listEntries == null)
				{
					var listController = new ListController();
					_listEntries = listController.GetListEntryInfoItems("Region", ParentKey, PortalId).OrderBy(s => s.SortOrder).ThenBy(s => s.Text).ToList();
				}

				return _listEntries;
			}
		}

		protected int PortalId
		{
			get
			{
				return PortalController.GetEffectivePortalId(PortalSettings.Current.PortalId);
			}
		}
		#endregion

		#region Constructors
		public DNNRegionEditControl()
		{
			Init += DnnRegionControl_Init;
		}
		public DNNRegionEditControl(string type)
		{
			Init += DnnRegionControl_Init;
			SystemType = type;
		}
		#endregion

		#region Overrides
		/// -----------------------------------------------------------------------------
		/// <summary>
		/// OnAttributesChanged runs when the CustomAttributes property has changed.
		/// </summary>
		/// -----------------------------------------------------------------------------
		protected override void OnAttributesChanged()
		{
			//Get the List settings out of the "Attributes"
			if ((CustomAttributes != null))
			{
				foreach (Attribute attribute in CustomAttributes)
				{
					if (attribute is ListAttribute)
					{
						var listAtt = (ListAttribute)attribute;
						ParentKey = listAtt.ParentKey;
						_listEntries = null;
						break;
					}
				}
			}
		}

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

			Regions.ControlStyle.CopyFrom(ControlStyle);
			Regions.ID = ID + "_dropdown";
			Regions.Attributes.Add("data-editor", "DNNRegionEditControl_DropDown");
			Regions.Items.Add(new ListItem() { Text = "<" + Localization.GetString("Not_Specified", Localization.SharedResourceFile) + ">", Value = "" });
			Controls.Add(Regions);

			Region.ControlStyle.CopyFrom(ControlStyle);
			Region.ID = ID + "_text";
			Region.Attributes.Add("data-editor", "DNNRegionEditControl_Text");
			Controls.Add(Region);

			RegionCode.ID = ID + "_value";
			RegionCode.Attributes.Add("data-editor", "DNNRegionEditControl_Hidden");
			Controls.Add(RegionCode);

		}

		public override bool LoadPostData(string postDataKey, System.Collections.Specialized.NameValueCollection postCollection)
		{
			bool dataChanged = false;
			string presentValue = StringValue;
			string postedValue = postCollection[postDataKey + "_value"];
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
				Page.RegisterRequiresPostBack(RegionCode);
			}

		}

		protected override void RenderEditMode(HtmlTextWriter writer)
		{
			if (ListEntries != null && ListEntries.Any())
			{
				foreach (ListEntryInfo item in ListEntries)
				{
					Regions.Items.Add(new ListItem() { Text = item.Text, Value = item.EntryID.ToString() });
				}
			}
			ControlStyle.AddAttributesToRender(writer);
			writer.AddAttribute("data-name", Name);
			writer.AddAttribute("data-list", "Region");
			writer.AddAttribute("data-category", Category);
			writer.AddAttribute("data-required", Required.ToString().ToLower());
			writer.RenderBeginTag(HtmlTextWriterTag.Div);
			RenderChildren(writer);
			writer.RenderEndTag();
		}
		#endregion

		#region Page Events
		private void DnnRegionControl_Init(object sender, System.EventArgs e)
		{
			ServicesFramework.Instance.RequestAjaxAntiForgerySupport();
			ClientResourceManager.RegisterScript(this.Page, "~/Resources/Shared/components/CountriesRegions/dnn.CountriesRegions.js");
			ClientResourceManager.RegisterFeatureStylesheet(this.Page, "~/Resources/Shared/components/CountriesRegions/dnn.CountriesRegions.css");
			JavaScript.RequestRegistration(CommonJs.jQuery);
			JavaScript.RequestRegistration(CommonJs.jQueryUI);
		}

		#endregion

		#region Private Methods
		private void LoadControls()
		{
			RegionCode.Value = StringValue;
		}
		#endregion

	}
}
