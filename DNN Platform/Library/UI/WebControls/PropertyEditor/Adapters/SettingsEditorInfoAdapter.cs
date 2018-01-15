#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2018
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
using System.Collections;
using System.Web.UI.WebControls;

using DotNetNuke.Common.Utilities;

#endregion

namespace DotNetNuke.UI.WebControls
{
    /// -----------------------------------------------------------------------------
    /// Project:    DotNetNuke
    /// Namespace:  DotNetNuke.UI.WebControls
    /// Class:      SettingsEditorInfoAdapter
    /// -----------------------------------------------------------------------------
    /// <summary>
    /// The SettingsEditorInfoAdapter control provides a factory for creating the
    /// appropriate EditInfo object
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
            DataMember = dataMember;
            DataSource = dataSource;
            FieldName = fieldName;
        }

        #region IEditorInfoAdapter Members

        public EditorInfo CreateEditControl()
        {

            var info = (SettingInfo) DataMember;
            var editInfo = new EditorInfo();

            //Get the Name of the property
            editInfo.Name = info.Name;

			editInfo.Category = string.Empty;

            //Get Value Field
            editInfo.Value = info.Value;

            //Get the type of the property
            editInfo.Type = info.Type.AssemblyQualifiedName;

            //Get Editor Field
            editInfo.Editor = info.Editor;

            //Get LabelMode Field
            editInfo.LabelMode = LabelMode.Left;

            //Get Required Field
            editInfo.Required = false;

            //Set ResourceKey Field
            editInfo.ResourceKey = editInfo.Name;

            //Get Style
            editInfo.ControlStyle = new Style();

            //Get Validation Expression Field
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

            var settings = (Hashtable) DataSource;
            IDictionaryEnumerator settingsEnumerator = settings.GetEnumerator();
            while (settingsEnumerator.MoveNext())
            {
                key = Convert.ToString(settingsEnumerator.Key);
                //Do we have the item in the Hashtable being changed
                if (key == name)
                {
					//Set the Value property to the new value
                    if ((!(ReferenceEquals(newValue, oldValue))) || changed)
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

        #endregion
    }
}
