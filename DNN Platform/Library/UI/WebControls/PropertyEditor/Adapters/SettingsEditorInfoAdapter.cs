// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.UI.WebControls
{
    using System;
    using System.Collections;
    using System.Globalization;
    using System.Web.UI.WebControls;

    using DotNetNuke.Common.Utilities;

    /// <summary>
    /// The SettingsEditorInfoAdapter control provides a factory for creating the
    /// appropriate EditInfo object.
    /// </summary>
    public class SettingsEditorInfoAdapter : IEditorInfoAdapter
    {
        private readonly object dataMember;
        private readonly object dataSource;
        private string fieldName;

        /// <summary>Initializes a new instance of the <see cref="SettingsEditorInfoAdapter"/> class.</summary>
        /// <param name="dataSource">The data source <see cref="Hashtable"/>.</param>
        /// <param name="dataMember">The <see cref="SettingInfo"/> instance.</param>
        /// <param name="fieldName">The field name.</param>
        public SettingsEditorInfoAdapter(object dataSource, object dataMember, string fieldName)
        {
            this.dataMember = dataMember;
            this.dataSource = dataSource;
            this.fieldName = fieldName;
        }

        /// <inheritdoc/>
        public EditorInfo CreateEditControl()
        {
            var info = (SettingInfo)this.dataMember;
            var editInfo = new EditorInfo();

            // Get the Name of the property
            editInfo.Name = info.Name;

            editInfo.Category = string.Empty;

            // Get Value Field
            editInfo.Value = info.Value;

            // Get the type of the property
            editInfo.Type = info.Type.AssemblyQualifiedName;

            // Get Editor Field
            editInfo.Editor = info.Editor;

            // Get LabelMode Field
            editInfo.LabelMode = LabelMode.Left;

            // Get Required Field
            editInfo.Required = false;

            // Set ResourceKey Field
            editInfo.ResourceKey = editInfo.Name;

            // Get Style
            editInfo.ControlStyle = new Style();

            // Get Validation Expression Field
            editInfo.ValidationExpression = string.Empty;

            return editInfo;
        }

        /// <inheritdoc/>
        public bool UpdateValue(PropertyEditorEventArgs e)
        {
            string name = e.Name;
            bool changed = e.Changed;
            object oldValue = e.OldValue;
            object newValue = e.Value;
            bool isDirty = Null.NullBoolean;

            var settings = (Hashtable)this.dataSource;
            var settingsEnumerator = settings.GetEnumerator();
            while (settingsEnumerator.MoveNext())
            {
                var key = Convert.ToString(settingsEnumerator.Key, CultureInfo.InvariantCulture);

                // Do we have the item in the Hashtable being changed
                if (key == name)
                {
                    // Set the Value property to the new value
                    if ((!ReferenceEquals(newValue, oldValue)) || changed)
                    {
                        settings[key] = newValue;
                        isDirty = true;
                        break;
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
    }
}
