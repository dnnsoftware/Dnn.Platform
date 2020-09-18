// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.UI.WebControls
{
    using System;
    using System.Collections;
    using System.Reflection;
    using System.Web.UI.WebControls;

    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Profile;
    using DotNetNuke.Entities.Users;

    /// -----------------------------------------------------------------------------
    /// Project:    DotNetNuke
    /// Namespace:  DotNetNuke.UI.WebControls
    /// Class:      CollectionEditorInfoFactory
    /// -----------------------------------------------------------------------------
    /// <summary>
    /// The CollectionEditorInfoAdapter control provides an Adapter for Collection Onjects.
    /// </summary>
    /// <remarks>
    /// </remarks>
    /// -----------------------------------------------------------------------------
    public class CollectionEditorInfoAdapter : IEditorInfoAdapter
    {
        private readonly object DataSource;
        private readonly Hashtable FieldNames;
        private readonly string Name;

        public CollectionEditorInfoAdapter(object dataSource, string name, string fieldName, Hashtable fieldNames)
        {
            this.DataSource = dataSource;
            this.FieldNames = fieldNames;
            this.Name = name;
        }

        public EditorInfo CreateEditControl()
        {
            return this.GetEditorInfo();
        }

        public bool UpdateValue(PropertyEditorEventArgs e)
        {
            string NameDataField = Convert.ToString(this.FieldNames["Name"]);
            string ValueDataField = Convert.ToString(this.FieldNames["Value"]);
            PropertyInfo objProperty;
            string PropertyName = string.Empty;
            bool changed = e.Changed;
            string name = e.Name;
            object oldValue = e.OldValue;
            object newValue = e.Value;
            object stringValue = e.StringValue;
            bool _IsDirty = Null.NullBoolean;

            // Get the Name Property
            objProperty = this.DataSource.GetType().GetProperty(NameDataField);
            if (objProperty != null)
            {
                PropertyName = Convert.ToString(objProperty.GetValue(this.DataSource, null));

                // Do we have the item in the IEnumerable Collection being changed
                PropertyName = PropertyName.Replace(" ", "_");
                if (PropertyName == name)
                {
                    // Get the Value Property
                    objProperty = this.DataSource.GetType().GetProperty(ValueDataField);

                    // Set the Value property to the new value
                    if ((!ReferenceEquals(newValue, oldValue)) || changed)
                    {
                        if (objProperty.PropertyType.FullName == "System.String")
                        {
                            objProperty.SetValue(this.DataSource, stringValue, null);
                        }
                        else
                        {
                            objProperty.SetValue(this.DataSource, newValue, null);
                        }

                        _IsDirty = true;
                    }
                }
            }

            return _IsDirty;
        }

        public bool UpdateVisibility(PropertyEditorEventArgs e)
        {
            string nameDataField = Convert.ToString(this.FieldNames["Name"]);
            string dataField = Convert.ToString(this.FieldNames["ProfileVisibility"]);
            string name = e.Name;
            object newValue = e.Value;
            bool dirty = Null.NullBoolean;

            // Get the Name Property
            PropertyInfo property = this.DataSource.GetType().GetProperty(nameDataField);
            if (property != null)
            {
                string propertyName = Convert.ToString(property.GetValue(this.DataSource, null));

                // Do we have the item in the IEnumerable Collection being changed
                propertyName = propertyName.Replace(" ", "_");
                if (propertyName == name)
                {
                    // Get the Value Property
                    property = this.DataSource.GetType().GetProperty(dataField);

                    // Set the Value property to the new value
                    property.SetValue(this.DataSource, newValue, null);
                    dirty = true;
                }
            }

            return dirty;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// GetEditorInfo builds an EditorInfo object for a propoerty.
        /// </summary>
        /// -----------------------------------------------------------------------------
        private EditorInfo GetEditorInfo()
        {
            string CategoryDataField = Convert.ToString(this.FieldNames["Category"]);
            string EditorDataField = Convert.ToString(this.FieldNames["Editor"]);
            string NameDataField = Convert.ToString(this.FieldNames["Name"]);
            string RequiredDataField = Convert.ToString(this.FieldNames["Required"]);
            string TypeDataField = Convert.ToString(this.FieldNames["Type"]);
            string ValidationExpressionDataField = Convert.ToString(this.FieldNames["ValidationExpression"]);
            string ValueDataField = Convert.ToString(this.FieldNames["Value"]);
            string VisibilityDataField = Convert.ToString(this.FieldNames["ProfileVisibility"]);
            string MaxLengthDataField = Convert.ToString(this.FieldNames["Length"]);

            var editInfo = new EditorInfo();
            PropertyInfo property;

            // Get the Name of the property
            editInfo.Name = string.Empty;
            if (!string.IsNullOrEmpty(NameDataField))
            {
                property = this.DataSource.GetType().GetProperty(NameDataField);
                if (!(property == null || (property.GetValue(this.DataSource, null) == null)))
                {
                    editInfo.Name = Convert.ToString(property.GetValue(this.DataSource, null));
                }
            }

            // Get the Category of the property
            editInfo.Category = string.Empty;

            // Get Category Field
            if (!string.IsNullOrEmpty(CategoryDataField))
            {
                property = this.DataSource.GetType().GetProperty(CategoryDataField);
                if (!(property == null || (property.GetValue(this.DataSource, null) == null)))
                {
                    editInfo.Category = Convert.ToString(property.GetValue(this.DataSource, null));
                }
            }

            // Get Value Field
            editInfo.Value = string.Empty;
            if (!string.IsNullOrEmpty(ValueDataField))
            {
                property = this.DataSource.GetType().GetProperty(ValueDataField);
                if (!(property == null || (property.GetValue(this.DataSource, null) == null)))
                {
                    editInfo.Value = Convert.ToString(property.GetValue(this.DataSource, null));
                }
            }

            // Get the type of the property
            editInfo.Type = "System.String";
            if (!string.IsNullOrEmpty(TypeDataField))
            {
                property = this.DataSource.GetType().GetProperty(TypeDataField);
                if (!(property == null || (property.GetValue(this.DataSource, null) == null)))
                {
                    editInfo.Type = Convert.ToString(property.GetValue(this.DataSource, null));
                }
            }

            // Get Editor Field
            editInfo.Editor = "DotNetNuke.UI.WebControls.TextEditControl, DotNetNuke";
            if (!string.IsNullOrEmpty(EditorDataField))
            {
                property = this.DataSource.GetType().GetProperty(EditorDataField);
                if (!(property == null || (property.GetValue(this.DataSource, null) == null)))
                {
                    editInfo.Editor = EditorInfo.GetEditor(Convert.ToInt32(property.GetValue(this.DataSource, null)));
                }
            }

            // Get LabelMode Field
            editInfo.LabelMode = LabelMode.Left;

            // Get Required Field
            editInfo.Required = false;
            if (!string.IsNullOrEmpty(RequiredDataField))
            {
                property = this.DataSource.GetType().GetProperty(RequiredDataField);
                if (!((property == null) || (property.GetValue(this.DataSource, null) == null)))
                {
                    editInfo.Required = Convert.ToBoolean(property.GetValue(this.DataSource, null));
                }
            }

            // Set ResourceKey Field
            editInfo.ResourceKey = editInfo.Name;
            editInfo.ResourceKey = string.Format("{0}_{1}", this.Name, editInfo.Name);

            // Set Style
            editInfo.ControlStyle = new Style();

            // Get Visibility Field
            editInfo.ProfileVisibility = new ProfileVisibility
            {
                VisibilityMode = UserVisibilityMode.AllUsers,
            };
            if (!string.IsNullOrEmpty(VisibilityDataField))
            {
                property = this.DataSource.GetType().GetProperty(VisibilityDataField);
                if (!(property == null || (property.GetValue(this.DataSource, null) == null)))
                {
                    editInfo.ProfileVisibility = (ProfileVisibility)property.GetValue(this.DataSource, null);
                }
            }

            // Get Validation Expression Field
            editInfo.ValidationExpression = string.Empty;
            if (!string.IsNullOrEmpty(ValidationExpressionDataField))
            {
                property = this.DataSource.GetType().GetProperty(ValidationExpressionDataField);
                if (!(property == null || (property.GetValue(this.DataSource, null) == null)))
                {
                    editInfo.ValidationExpression = Convert.ToString(property.GetValue(this.DataSource, null));
                }
            }

            // Get Length Field
            if (!string.IsNullOrEmpty(MaxLengthDataField))
            {
                property = this.DataSource.GetType().GetProperty(MaxLengthDataField);
                if (!(property == null || (property.GetValue(this.DataSource, null) == null)))
                {
                    int length = Convert.ToInt32(property.GetValue(this.DataSource, null));
                    var attributes = new object[1];
                    attributes[0] = new MaxLengthAttribute(length);
                    editInfo.Attributes = attributes;
                }
            }

            // Remove spaces from name
            editInfo.Name = editInfo.Name.Replace(" ", "_");
            return editInfo;
        }
    }
}
