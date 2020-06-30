// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
// ReSharper disable CheckNamespace
namespace DotNetNuke.UI.WebControls

// ReSharper restore CheckNamespace
{
    using System;
    using System.Web.UI;

    using DotNetNuke.Common.Lists;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Profile;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Security;

    /// -----------------------------------------------------------------------------
    /// Project:    DotNetNuke
    /// Namespace:  DotNetNuke.UI.WebControls
    /// Class:      ProfileEditorControl
    /// -----------------------------------------------------------------------------
    /// <summary>
    /// The ProfileEditorControl control provides a Control to display Profile
    /// Properties.
    /// </summary>
    /// <remarks>
    /// </remarks>
    /// -----------------------------------------------------------------------------
    [ToolboxData("<{0}:ProfileEditorControl runat=server></{0}:ProfileEditorControl>")]
    public class ProfileEditorControl : CollectionEditorControl
    {
        /// -----------------------------------------------------------------------------
        /// <summary>
        /// CreateEditor creates the control collection.
        /// </summary>
        /// -----------------------------------------------------------------------------
        protected override void CreateEditor()
        {
            this.CategoryDataField = "PropertyCategory";
            this.EditorDataField = "DataType";
            this.NameDataField = "PropertyName";
            this.RequiredDataField = "Required";
            this.ValidationExpressionDataField = "ValidationExpression";
            this.ValueDataField = "PropertyValue";
            this.VisibleDataField = "Visible";
            this.VisibilityDataField = "ProfileVisibility";
            this.LengthDataField = "Length";

            base.CreateEditor();

            foreach (FieldEditorControl editor in this.Fields)
            {
                // Check whether Field is readonly
                string fieldName = editor.Editor.Name;
                ProfilePropertyDefinitionCollection definitions = editor.DataSource as ProfilePropertyDefinitionCollection;
                ProfilePropertyDefinition definition = definitions[fieldName];

                if (definition != null && definition.ReadOnly && (editor.Editor.EditMode == PropertyEditorMode.Edit))
                {
                    PortalSettings ps = PortalController.Instance.GetCurrentPortalSettings();
                    if (!PortalSecurity.IsInRole(ps.AdministratorRoleName))
                    {
                        editor.Editor.EditMode = PropertyEditorMode.View;
                    }
                }

                // We need to wire up the RegionControl to the CountryControl
                if (editor.Editor is DNNRegionEditControl)
                {
                    string country = null;

                    foreach (FieldEditorControl checkEditor in this.Fields)
                    {
                        if (checkEditor.Editor is DNNCountryEditControl)
                        {
                            if (editor.Editor.Category == checkEditor.Editor.Category)
                            {
                                var countryEdit = (DNNCountryEditControl)checkEditor.Editor;
                                country = Convert.ToString(countryEdit.Value);
                            }
                        }
                    }

                    // Create a ListAttribute for the Region
                    string countryKey = "Unknown";
                    int entryId;
                    if (int.TryParse(country, out entryId))
                    {
                        ListController lc = new ListController();
                        ListEntryInfo item = lc.GetListEntryInfo(entryId);
                        if (item != null)
                        {
                            countryKey = item.Value;
                        }
                    }

                    countryKey = "Country." + countryKey;
                    var attributes = new object[1];
                    attributes[0] = new ListAttribute("Region", countryKey, ListBoundField.Id, ListBoundField.Text);
                    editor.Editor.CustomAttributes = attributes;
                }
            }
        }
    }
}
