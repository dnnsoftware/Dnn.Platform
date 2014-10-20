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
using System.Linq;
using System.Web.UI.WebControls;
using System.Web.UI;
using DotNetNuke.Common.Utilities;

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
	/// <history>
	///     [cnurse]	05/04/2006	created
	/// </history>
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

		private HiddenField _InitialValue;
		private HiddenField RegionCode
		{
			get
			{
				if (_InitialValue == null)
				{
					_InitialValue = new HiddenField();
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
		#endregion

		#region Constructors
		public DNNRegionEditControl()
		{
			Load += DnnRegionControl_Load;
			Init += DnnRegionControl_Init;
		}
		public DNNRegionEditControl(string type)
		{
			Load += DnnRegionControl_Load;
			Init += DnnRegionControl_Init;
			SystemType = type;
		}
		#endregion

		#region Overrides
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
			Controls.Add(Regions);

			Region.ControlStyle.CopyFrom(ControlStyle);
			Region.ID = ID + "_text";
			Controls.Add(Region);

			RegionCode.ID = ID + "_value";
			Controls.Add(RegionCode);

		}

		public override bool LoadPostData(string postDataKey, System.Collections.Specialized.NameValueCollection postCollection)
		{
			EnsureChildControls();
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
			RenderChildren(writer);
		}
		#endregion

		#region Page Events
		private void DnnRegionControl_Init(object sender, System.EventArgs e)
		{
			//DotNetNuke.Web.Client.ClientResourceManagement.ClientResourceManager.RegisterScript(Page, ResolveUrl("~/DesktopModules/Albatros/Registration/js/countryregionbox.js"), 70);
			//DotNetNuke.Framework.jQuery.RequestRegistration();
			//DotNetNuke.Framework.jQuery.RequestUIRegistration();
		}


		private void DnnRegionControl_Load(object sender, System.EventArgs e)
		{
			//string script = string.Format(new XCData("\r\n  <script type='text/javascript'>\r\n   var dnnRegionBoxId = '{0}';\r\n  </script>\r\n  ").Value, this.ClientID);
			//DotNetNuke.UI.Utilities.ClientAPI.RegisterStartUpScript(Page, "DnnRegionControl", script);
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
