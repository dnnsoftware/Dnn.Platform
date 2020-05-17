﻿// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
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
