// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.UI.WebControls
{
    using System;
    using System.Linq;
    using System.Web.UI;

    using DotNetNuke.Common.Lists;

    /// -----------------------------------------------------------------------------
    /// Project:    DotNetNuke
    /// Namespace:  DotNetNuke.UI.WebControls
    /// Class:      DNNCountryEditControl
    /// -----------------------------------------------------------------------------
    /// <summary>
    /// The DNNCountryEditControl control provides a standard UI component for editing
    /// Countries.
    /// </summary>
    /// -----------------------------------------------------------------------------
    [ToolboxData("<{0}:DNNCountryEditControl runat=server></{0}:DNNCountryEditControl>")]
    public class DNNCountryEditControl : DNNListEditControl
    {
        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Initializes a new instance of the <see cref="DNNCountryEditControl"/> class.
        /// Constructs a DNNCountryEditControl.
        /// </summary>
        /// -----------------------------------------------------------------------------
        public DNNCountryEditControl()
        {
            this.AutoPostBack = true;
            this.ListName = "Country";
            this.ParentKey = string.Empty;
            this.TextField = ListBoundField.Text;
            this.ValueField = ListBoundField.Id;
            this.ItemChanged += this.OnItemChanged;
            this.SortAlphabetically = true;
        }

        private void OnItemChanged(object sender, PropertyEditorEventArgs e)
        {
            var regionContainer = ControlUtilities.FindControl<Control>(this.Parent, "Region", true);
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
