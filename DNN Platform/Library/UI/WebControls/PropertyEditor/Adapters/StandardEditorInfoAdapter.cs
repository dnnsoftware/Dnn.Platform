// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.UI.WebControls
{
    using System;
    using System.ComponentModel;
    using System.Reflection;
    using System.Web.UI.WebControls;

    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Profile;
    using DotNetNuke.Entities.Users;

    /// <summary>The StandardEditorInfoAdapter control provides an Adapter for standard datasources.</summary>
    public class StandardEditorInfoAdapter : IEditorInfoAdapter
    {
        private readonly object dataSource;
        private readonly string fieldName;

        /// <summary>Initializes a new instance of the <see cref="StandardEditorInfoAdapter"/> class.</summary>
        /// <param name="dataSource">The data source object.</param>
        /// <param name="fieldName">The field/property name.</param>
        public StandardEditorInfoAdapter(object dataSource, string fieldName)
        {
            this.dataSource = dataSource;
            this.fieldName = fieldName;
        }

        /// <inheritdoc/>
        public EditorInfo CreateEditControl()
        {
            EditorInfo editInfo = null;
            PropertyInfo objProperty = GetProperty(this.dataSource, this.fieldName);
            if (objProperty != null)
            {
                editInfo = GetEditorInfo(this.dataSource, objProperty);
            }

            return editInfo;
        }

        /// <inheritdoc/>
        public bool UpdateValue(PropertyEditorEventArgs e)
        {
            bool changed = e.Changed;
            object oldValue = e.OldValue;
            object newValue = e.Value;
            bool isDirty = Null.NullBoolean;
            if (this.dataSource != null)
            {
                PropertyInfo objProperty = this.dataSource.GetType().GetProperty(e.Name);
                if (objProperty != null)
                {
                    if ((!ReferenceEquals(newValue, oldValue)) || changed)
                    {
                        objProperty.SetValue(this.dataSource, newValue, null);
                        isDirty = true;
                    }
                }
            }

            return isDirty;
        }

        /// <inheritdoc/>
        public bool UpdateVisibility(PropertyEditorEventArgs e)
        {
            return false;
        }

        /// <summary>GetEditorInfo builds an EditorInfo object for a property.</summary>
        private static EditorInfo GetEditorInfo(object dataSource, PropertyInfo objProperty)
        {
            var editInfo = new EditorInfo();

            // Get the Name of the property
            editInfo.Name = objProperty.Name;

            // Get the value of the property
            editInfo.Value = objProperty.GetValue(dataSource, null);

            // Get the type of the property
            editInfo.Type = objProperty.PropertyType.AssemblyQualifiedName;

            // Get the Custom Attributes for the property
            editInfo.Attributes = objProperty.GetCustomAttributes(true);

            // Get Category Field
            editInfo.Category = string.Empty;
            object[] categoryAttributes = objProperty.GetCustomAttributes(typeof(CategoryAttribute), true);
            if (categoryAttributes.Length > 0)
            {
                var category = (CategoryAttribute)categoryAttributes[0];
                editInfo.Category = category.Category;
            }

            // Get EditMode Field
            if (!objProperty.CanWrite)
            {
                editInfo.EditMode = PropertyEditorMode.View;
            }
            else
            {
                object[] readOnlyAttributes = objProperty.GetCustomAttributes(typeof(IsReadOnlyAttribute), true);
                if (readOnlyAttributes.Length > 0)
                {
                    var readOnlyMode = (IsReadOnlyAttribute)readOnlyAttributes[0];
                    if (readOnlyMode.IsReadOnly)
                    {
                        editInfo.EditMode = PropertyEditorMode.View;
                    }
                }
            }

            // Get Editor Field
            editInfo.Editor = "UseSystemType";
            object[] editorAttributes = objProperty.GetCustomAttributes(typeof(EditorAttribute), true);
            if (editorAttributes.Length > 0)
            {
                EditorAttribute editor = null;
                for (int i = 0; i <= editorAttributes.Length - 1; i++)
                {
                    if (((EditorAttribute)editorAttributes[i]).EditorBaseTypeName.Contains("DotNetNuke.UI.WebControls.EditControl", StringComparison.Ordinal))
                    {
                        editor = (EditorAttribute)editorAttributes[i];
                        break;
                    }
                }

                if (editor != null)
                {
                    editInfo.Editor = editor.EditorTypeName;
                }
            }

            // Get Required Field
            editInfo.Required = false;
            object[] requiredAttributes = objProperty.GetCustomAttributes(typeof(RequiredAttribute), true);
            if (requiredAttributes.Length > 0)
            {
                // The property may contain multiple edit mode types, so make sure we only use DotNetNuke editors.
                var required = (RequiredAttribute)requiredAttributes[0];
                if (required.Required)
                {
                    editInfo.Required = true;
                }
            }

            // Get Css Style
            editInfo.ControlStyle = new Style();
            object[] styleAttributes = objProperty.GetCustomAttributes(typeof(ControlStyleAttribute), true);
            if (styleAttributes.Length > 0)
            {
                var attribute = (ControlStyleAttribute)styleAttributes[0];
                editInfo.ControlStyle.CssClass = attribute.CssClass;
                editInfo.ControlStyle.Height = attribute.Height;
                editInfo.ControlStyle.Width = attribute.Width;
            }

            // Get LabelMode Field
            editInfo.LabelMode = LabelMode.Left;
            object[] labelModeAttributes = objProperty.GetCustomAttributes(typeof(LabelModeAttribute), true);
            if (labelModeAttributes.Length > 0)
            {
                var mode = (LabelModeAttribute)labelModeAttributes[0];
                editInfo.LabelMode = mode.Mode;
            }

            // Set ResourceKey Field
            editInfo.ResourceKey = $"{dataSource.GetType().Name}_{objProperty.Name}";

            // Get Validation Expression Field
            editInfo.ValidationExpression = string.Empty;
            object[] regExAttributes = objProperty.GetCustomAttributes(typeof(RegularExpressionValidatorAttribute), true);
            if (regExAttributes.Length > 0)
            {
                var regExAttribute = (RegularExpressionValidatorAttribute)regExAttributes[0];
                editInfo.ValidationExpression = regExAttribute.Expression;
            }

            // Set Visibility
            editInfo.ProfileVisibility = new ProfileVisibility
            {
                VisibilityMode = UserVisibilityMode.AllUsers,
            };

            return editInfo;
        }

        /// <summary>GetProperty returns the property that is being "bound" to.</summary>
        private static PropertyInfo GetProperty(object dataSource, string fieldName)
        {
            if (dataSource != null)
            {
                BindingFlags bindings = BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static;
                PropertyInfo objProperty = dataSource.GetType().GetProperty(fieldName, bindings);
                return objProperty;
            }
            else
            {
                return null;
            }
        }
    }
}
