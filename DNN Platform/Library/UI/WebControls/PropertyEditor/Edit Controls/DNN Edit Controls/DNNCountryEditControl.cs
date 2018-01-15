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
using System.Web.UI;
using DotNetNuke.Common.Lists;

#endregion

namespace DotNetNuke.UI.WebControls
{
	/// -----------------------------------------------------------------------------
	/// Project:    DotNetNuke
	/// Namespace:  DotNetNuke.UI.WebControls
	/// Class:      DNNCountryEditControl
	/// -----------------------------------------------------------------------------
	/// <summary>
	/// The DNNCountryEditControl control provides a standard UI component for editing
	/// Countries
	/// </summary>
	/// -----------------------------------------------------------------------------
	[ToolboxData("<{0}:DNNCountryEditControl runat=server></{0}:DNNCountryEditControl>")]
	public class DNNCountryEditControl : DNNListEditControl
	{
		/// -----------------------------------------------------------------------------
		/// <summary>
		/// Constructs a DNNCountryEditControl
		/// </summary>
		/// -----------------------------------------------------------------------------
		public DNNCountryEditControl()
		{
			AutoPostBack = true;
			ListName = "Country";
			ParentKey = "";
			TextField = ListBoundField.Text;
			ValueField = ListBoundField.Id;
			ItemChanged += OnItemChanged;
			SortAlphabetically = true;
		}

		void OnItemChanged(object sender, PropertyEditorEventArgs e)
		{
			var regionContainer = ControlUtilities.FindControl<Control>(Parent, "Region", true);
			if (regionContainer != null)
			{
				var regionControl = ControlUtilities.FindFirstDescendent<DNNRegionEditControl>(regionContainer);
				if (regionControl != null)
				{
					var listController = new ListController();
					var countries = listController.GetListEntryInfoItems("Country");
					foreach (var checkCountry in countries)
					{
						if (checkCountry.EntryID.ToString() == e.StringValue)
						{
							var attributes = new object[1];
							attributes[0] = new ListAttribute("Region", "Country." + checkCountry.Value, ListBoundField.Id, ListBoundField.Text);
							regionControl.CustomAttributes = attributes;
							break;
						}
					}
				}
			}
		}
	}
}
