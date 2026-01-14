// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.UI.WebControls
{
    using System;
    using System.Globalization;
    using System.Web.UI;

    using DotNetNuke.Common;
    using DotNetNuke.Common.Extensions;
    using DotNetNuke.Common.Lists;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Profile;
    using DotNetNuke.Security;

    /// <summary>The ProfileEditorControl control provides a Control to display Profile Properties.</summary>
    [ToolboxData("<{0}:ProfileEditorControl runat=server></{0}:ProfileEditorControl>")]
    public class ProfileEditorControl : CollectionEditorControl
    {
        /// <summary>Initializes a new instance of the <see cref="ProfileEditorControl"/> class.</summary>
        [Obsolete("Deprecated in DotNetNuke 10.0.0. Please use overload with IServiceProvider. Scheduled removal in v12.0.0.")]
        public ProfileEditorControl()
            : this(Globals.GetCurrentServiceProvider())
        {
        }

        /// <summary>Initializes a new instance of the <see cref="ProfileEditorControl"/> class.</summary>
        /// <param name="serviceProvider">The DI container.</param>
        public ProfileEditorControl(IServiceProvider serviceProvider)
            : base(serviceProvider)
        {
        }

        /// <summary>CreateEditor creates the control collection.</summary>
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
                        if (checkEditor.Editor is DNNCountryEditControl countryEdit)
                        {
                            if (editor.Editor.Category == countryEdit.Category)
                            {
                                country = Convert.ToString(countryEdit.Value, CultureInfo.InvariantCulture);
                            }
                        }
                    }

                    // Create a ListAttribute for the Region
                    string countryKey = "Unknown";
                    if (int.TryParse(country, out var entryId))
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
