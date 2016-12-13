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
using System.Web.UI;

using DotNetNuke.Common.Lists;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Profile;
using DotNetNuke.Entities.Users;
using DotNetNuke.Security;

#endregion

// ReSharper disable CheckNamespace
namespace DotNetNuke.UI.WebControls
// ReSharper restore CheckNamespace
{
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
		#region Protected Methods

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// CreateEditor creates the control collection.
        /// </summary>
        /// -----------------------------------------------------------------------------
        protected override void CreateEditor()
        {
            CategoryDataField = "PropertyCategory";
            EditorDataField = "DataType";
            NameDataField = "PropertyName";
            RequiredDataField = "Required";
            ValidationExpressionDataField = "ValidationExpression";
            ValueDataField = "PropertyValue";
            VisibleDataField = "Visible";
            VisibilityDataField = "ProfileVisibility";
            LengthDataField = "Length";

            base.CreateEditor();

            foreach (FieldEditorControl editor in Fields)
            {
                //Check whether Field is readonly
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

                //We need to wire up the RegionControl to the CountryControl
                if (editor.Editor is DNNRegionEditControl)
                {
                    string country = null;

                    foreach (FieldEditorControl checkEditor in Fields)
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
					
                    //Create a ListAttribute for the Region
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
		
		#endregion
    }
}
