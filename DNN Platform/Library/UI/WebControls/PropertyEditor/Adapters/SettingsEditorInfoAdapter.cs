// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.UI.WebControls
{
    using System;
    using System.Collections;
    using System.Web.UI.WebControls;

    using DotNetNuke.Common.Utilities;

    /// -----------------------------------------------------------------------------
    /// Project:    DotNetNuke
    /// Namespace:  DotNetNuke.UI.WebControls
    /// Class:      SettingsEditorInfoAdapter
    /// -----------------------------------------------------------------------------
    /// <summary>
    /// The SettingsEditorInfoAdapter control provides a factory for creating the
    /// appropriate EditInfo object.
    /// </summary>
    /// <remarks>
    /// </remarks>
    /// -----------------------------------------------------------------------------
    public class SettingsEditorInfoAdapter : IEditorInfoAdapter
    {
        private readonly object DataMember;
        private readonly object DataSource;
        private string FieldName;

        public SettingsEditorInfoAdapter(object dataSource, object dataMember, string fieldName)
        {
            this.DataMember = dataMember;
            this.DataSource = dataSource;
            this.FieldName = fieldName;
        }

        public EditorInfo CreateEditControl()
        {
            var info = (SettingInfo)this.DataMember;
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

        public bool UpdateValue(PropertyEditorEventArgs e)
        {
            string key;
            string name = e.Name;
            bool changed = e.Changed;
            object oldValue = e.OldValue;
            object newValue = e.Value;
            object stringValue = e.StringValue;
            bool _IsDirty = Null.NullBoolean;

            var settings = (Hashtable)this.DataSource;
            IDictionaryEnumerator settingsEnumerator = settings.GetEnumerator();
            while (settingsEnumerator.MoveNext())
            {
                key = Convert.ToString(settingsEnumerator.Key);

                // Do we have the item in the Hashtable being changed
                if (key == name)
                {
                    // Set the Value property to the new value
                    if ((!ReferenceEquals(newValue, oldValue)) || changed)
                    {
                        settings[key] = newValue;
                        _IsDirty = true;
                        break;
                    }
                }
            }

            return _IsDirty;
        }

        public bool UpdateVisibility(PropertyEditorEventArgs e)
        {
            return false;
        }
    }
}
