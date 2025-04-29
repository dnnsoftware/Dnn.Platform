// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.UI.WebControls;

using System;
using System.Collections;
using System.Reflection;
using System.Web.UI.WebControls;

using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Profile;
using DotNetNuke.Entities.Users;

/// Project:    DotNetNuke
/// Namespace:  DotNetNuke.UI.WebControls
/// Class:      CollectionEditorInfoFactory
/// <summary>The CollectionEditorInfoAdapter control provides an Adapter for Collection Onjects.</summary>
public class CollectionEditorInfoAdapter : IEditorInfoAdapter
{
    private readonly object dataSource;
    private readonly Hashtable fieldNames;
    private readonly string name;

    /// <summary>Initializes a new instance of the <see cref="CollectionEditorInfoAdapter"/> class.</summary>
    /// <param name="dataSource"></param>
    /// <param name="name"></param>
    /// <param name="fieldName"></param>
    /// <param name="fieldNames"></param>
    public CollectionEditorInfoAdapter(object dataSource, string name, string fieldName, Hashtable fieldNames)
    {
        this.dataSource = dataSource;
        this.fieldNames = fieldNames;
        this.name = name;
    }

    /// <inheritdoc/>
    public EditorInfo CreateEditControl()
    {
        return this.GetEditorInfo();
    }

    /// <inheritdoc/>
    public bool UpdateValue(PropertyEditorEventArgs e)
    {
        string nameDataField = Convert.ToString(this.fieldNames["Name"]);
        string valueDataField = Convert.ToString(this.fieldNames["Value"]);
        PropertyInfo objProperty;
        string propertyName = string.Empty;
        bool changed = e.Changed;
        string name = e.Name;
        object oldValue = e.OldValue;
        object newValue = e.Value;
        object stringValue = e.StringValue;
        bool isDirty = Null.NullBoolean;

        // Get the Name Property
        objProperty = this.dataSource.GetType().GetProperty(nameDataField);
        if (objProperty != null)
        {
            propertyName = Convert.ToString(objProperty.GetValue(this.dataSource, null));

            // Do we have the item in the IEnumerable Collection being changed
            propertyName = propertyName.Replace(" ", "_");
            if (propertyName == name)
            {
                // Get the Value Property
                objProperty = this.dataSource.GetType().GetProperty(valueDataField);

                // Set the Value property to the new value
                if ((!ReferenceEquals(newValue, oldValue)) || changed)
                {
                    if (objProperty.PropertyType.FullName == "System.String")
                    {
                        objProperty.SetValue(this.dataSource, stringValue, null);
                    }
                    else
                    {
                        objProperty.SetValue(this.dataSource, newValue, null);
                    }

                    isDirty = true;
                }
            }
        }

        return isDirty;
    }

    /// <inheritdoc/>
    public bool UpdateVisibility(PropertyEditorEventArgs e)
    {
        string nameDataField = Convert.ToString(this.fieldNames["Name"]);
        string dataField = Convert.ToString(this.fieldNames["ProfileVisibility"]);
        string name = e.Name;
        object newValue = e.Value;
        bool dirty = Null.NullBoolean;

        // Get the Name Property
        PropertyInfo property = this.dataSource.GetType().GetProperty(nameDataField);
        if (property != null)
        {
            string propertyName = Convert.ToString(property.GetValue(this.dataSource, null));

            // Do we have the item in the IEnumerable Collection being changed
            propertyName = propertyName.Replace(" ", "_");
            if (propertyName == name)
            {
                // Get the Value Property
                property = this.dataSource.GetType().GetProperty(dataField);

                // Set the Value property to the new value
                property.SetValue(this.dataSource, newValue, null);
                dirty = true;
            }
        }

        return dirty;
    }

    /// <summary>GetEditorInfo builds an EditorInfo object for a propoerty.</summary>
    private EditorInfo GetEditorInfo()
    {
        string categoryDataField = Convert.ToString(this.fieldNames["Category"]);
        string editorDataField = Convert.ToString(this.fieldNames["Editor"]);
        string nameDataField = Convert.ToString(this.fieldNames["Name"]);
        string requiredDataField = Convert.ToString(this.fieldNames["Required"]);
        string typeDataField = Convert.ToString(this.fieldNames["Type"]);
        string validationExpressionDataField = Convert.ToString(this.fieldNames["ValidationExpression"]);
        string valueDataField = Convert.ToString(this.fieldNames["Value"]);
        string visibilityDataField = Convert.ToString(this.fieldNames["ProfileVisibility"]);
        string maxLengthDataField = Convert.ToString(this.fieldNames["Length"]);

        var editInfo = new EditorInfo();
        PropertyInfo property;

        // Get the Name of the property
        editInfo.Name = string.Empty;
        if (!string.IsNullOrEmpty(nameDataField))
        {
            property = this.dataSource.GetType().GetProperty(nameDataField);
            if (!(property == null || (property.GetValue(this.dataSource, null) == null)))
            {
                editInfo.Name = Convert.ToString(property.GetValue(this.dataSource, null));
            }
        }

        // Get the Category of the property
        editInfo.Category = string.Empty;

        // Get Category Field
        if (!string.IsNullOrEmpty(categoryDataField))
        {
            property = this.dataSource.GetType().GetProperty(categoryDataField);
            if (!(property == null || (property.GetValue(this.dataSource, null) == null)))
            {
                editInfo.Category = Convert.ToString(property.GetValue(this.dataSource, null));
            }
        }

        // Get Value Field
        editInfo.Value = string.Empty;
        if (!string.IsNullOrEmpty(valueDataField))
        {
            property = this.dataSource.GetType().GetProperty(valueDataField);
            if (!(property == null || (property.GetValue(this.dataSource, null) == null)))
            {
                editInfo.Value = Convert.ToString(property.GetValue(this.dataSource, null));
            }
        }

        // Get the type of the property
        editInfo.Type = "System.String";
        if (!string.IsNullOrEmpty(typeDataField))
        {
            property = this.dataSource.GetType().GetProperty(typeDataField);
            if (!(property == null || (property.GetValue(this.dataSource, null) == null)))
            {
                editInfo.Type = Convert.ToString(property.GetValue(this.dataSource, null));
            }
        }

        // Get Editor Field
        editInfo.Editor = "DotNetNuke.UI.WebControls.TextEditControl, DotNetNuke";
        if (!string.IsNullOrEmpty(editorDataField))
        {
            property = this.dataSource.GetType().GetProperty(editorDataField);
            if (!(property == null || (property.GetValue(this.dataSource, null) == null)))
            {
                editInfo.Editor = EditorInfo.GetEditor(Convert.ToInt32(property.GetValue(this.dataSource, null)));
            }
        }

        // Get LabelMode Field
        editInfo.LabelMode = LabelMode.Left;

        // Get Required Field
        editInfo.Required = false;
        if (!string.IsNullOrEmpty(requiredDataField))
        {
            property = this.dataSource.GetType().GetProperty(requiredDataField);
            if (!((property == null) || (property.GetValue(this.dataSource, null) == null)))
            {
                editInfo.Required = Convert.ToBoolean(property.GetValue(this.dataSource, null));
            }
        }

        // Set ResourceKey Field
        editInfo.ResourceKey = editInfo.Name;
        editInfo.ResourceKey = string.Format("{0}_{1}", this.name, editInfo.Name);

        // Set Style
        editInfo.ControlStyle = new Style();

        // Get Visibility Field
        editInfo.ProfileVisibility = new ProfileVisibility
        {
            VisibilityMode = UserVisibilityMode.AllUsers,
        };
        if (!string.IsNullOrEmpty(visibilityDataField))
        {
            property = this.dataSource.GetType().GetProperty(visibilityDataField);
            if (!(property == null || (property.GetValue(this.dataSource, null) == null)))
            {
                editInfo.ProfileVisibility = (ProfileVisibility)property.GetValue(this.dataSource, null);
            }
        }

        // Get Validation Expression Field
        editInfo.ValidationExpression = string.Empty;
        if (!string.IsNullOrEmpty(validationExpressionDataField))
        {
            property = this.dataSource.GetType().GetProperty(validationExpressionDataField);
            if (!(property == null || (property.GetValue(this.dataSource, null) == null)))
            {
                editInfo.ValidationExpression = Convert.ToString(property.GetValue(this.dataSource, null));
            }
        }

        // Get Length Field
        if (!string.IsNullOrEmpty(maxLengthDataField))
        {
            property = this.dataSource.GetType().GetProperty(maxLengthDataField);
            if (!(property == null || (property.GetValue(this.dataSource, null) == null)))
            {
                int length = Convert.ToInt32(property.GetValue(this.dataSource, null));
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
