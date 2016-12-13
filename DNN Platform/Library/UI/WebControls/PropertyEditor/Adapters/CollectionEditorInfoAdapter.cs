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
using System.Collections;
using System.Reflection;
using System.Web.UI.WebControls;

using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Profile;
using DotNetNuke.Entities.Users;

#endregion

namespace DotNetNuke.UI.WebControls
{
    /// -----------------------------------------------------------------------------
    /// Project:    DotNetNuke
    /// Namespace:  DotNetNuke.UI.WebControls
    /// Class:      CollectionEditorInfoFactory
    /// -----------------------------------------------------------------------------
    /// <summary>
    /// The CollectionEditorInfoAdapter control provides an Adapter for Collection Onjects
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
            DataSource = dataSource;
            FieldNames = fieldNames;
            Name = name;
        }

        #region IEditorInfoAdapter Members

        public EditorInfo CreateEditControl()
        {
            return GetEditorInfo();
        }

        public bool UpdateValue(PropertyEditorEventArgs e)
        {
            string NameDataField = Convert.ToString(FieldNames["Name"]);
            string ValueDataField = Convert.ToString(FieldNames["Value"]);
            PropertyInfo objProperty;
            string PropertyName = "";
            bool changed = e.Changed;
            string name = e.Name;
            object oldValue = e.OldValue;
            object newValue = e.Value;
            object stringValue = e.StringValue;
            bool _IsDirty = Null.NullBoolean;
			
			//Get the Name Property
            objProperty = DataSource.GetType().GetProperty(NameDataField);
            if (objProperty != null)
            {
                PropertyName = Convert.ToString(objProperty.GetValue(DataSource, null));
				//Do we have the item in the IEnumerable Collection being changed
                PropertyName = PropertyName.Replace(" ", "_");
                if (PropertyName == name)
                {
					//Get the Value Property
                    objProperty = DataSource.GetType().GetProperty(ValueDataField);
					
					//Set the Value property to the new value
                    if ((!(ReferenceEquals(newValue, oldValue))) || changed)
                    {
                        if (objProperty.PropertyType.FullName == "System.String")
                        {
                            objProperty.SetValue(DataSource, stringValue, null);
                        }
                        else
                        {
                            objProperty.SetValue(DataSource, newValue, null);
                        }
                        _IsDirty = true;
                    }
                }
            }
            return _IsDirty;
        }

        public bool UpdateVisibility(PropertyEditorEventArgs e)
        {
            string nameDataField = Convert.ToString(FieldNames["Name"]);
            string dataField = Convert.ToString(FieldNames["ProfileVisibility"]);
            string name = e.Name;
            object newValue = e.Value;
            bool dirty = Null.NullBoolean;
			
			//Get the Name Property
            PropertyInfo property = DataSource.GetType().GetProperty(nameDataField);
            if (property != null)
            {
                string propertyName = Convert.ToString(property.GetValue(DataSource, null));
				//Do we have the item in the IEnumerable Collection being changed
                propertyName = propertyName.Replace(" ", "_");
                if (propertyName == name)
                {
					//Get the Value Property
                    property = DataSource.GetType().GetProperty(dataField);
					//Set the Value property to the new value
                    property.SetValue(DataSource, newValue, null);
                    dirty = true;
                }
            }
            return dirty;
        }

        #endregion

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// GetEditorInfo builds an EditorInfo object for a propoerty
        /// </summary>
        /// -----------------------------------------------------------------------------
        private EditorInfo GetEditorInfo()
        {
            string CategoryDataField = Convert.ToString(FieldNames["Category"]);
            string EditorDataField = Convert.ToString(FieldNames["Editor"]);
            string NameDataField = Convert.ToString(FieldNames["Name"]);
            string RequiredDataField = Convert.ToString(FieldNames["Required"]);
            string TypeDataField = Convert.ToString(FieldNames["Type"]);
            string ValidationExpressionDataField = Convert.ToString(FieldNames["ValidationExpression"]);
            string ValueDataField = Convert.ToString(FieldNames["Value"]);
            string VisibilityDataField = Convert.ToString(FieldNames["ProfileVisibility"]);
            string MaxLengthDataField = Convert.ToString(FieldNames["Length"]);

            var editInfo = new EditorInfo();
            PropertyInfo property;

            //Get the Name of the property
            editInfo.Name = string.Empty;
            if (!String.IsNullOrEmpty(NameDataField))
            {
                property = DataSource.GetType().GetProperty(NameDataField);
                if (!(property == null || (property.GetValue(DataSource, null) == null)))
                {
                    editInfo.Name = Convert.ToString(property.GetValue(DataSource, null));
                }
            }
			
            //Get the Category of the property
            editInfo.Category = string.Empty;
			
			//Get Category Field
            if (!String.IsNullOrEmpty(CategoryDataField))
            {
                property = DataSource.GetType().GetProperty(CategoryDataField);
                if (!(property == null || (property.GetValue(DataSource, null) == null)))
                {
                    editInfo.Category = Convert.ToString(property.GetValue(DataSource, null));
                }
            }
            
			//Get Value Field
			editInfo.Value = string.Empty;
            if (!String.IsNullOrEmpty(ValueDataField))
            {
                property = DataSource.GetType().GetProperty(ValueDataField);
                if (!(property == null || (property.GetValue(DataSource, null) == null)))
                {
                    editInfo.Value = Convert.ToString(property.GetValue(DataSource, null));
                }
            }
            
			//Get the type of the property
			editInfo.Type = "System.String";
            if (!String.IsNullOrEmpty(TypeDataField))
            {
                property = DataSource.GetType().GetProperty(TypeDataField);
                if (!(property == null || (property.GetValue(DataSource, null) == null)))
                {
                    editInfo.Type = Convert.ToString(property.GetValue(DataSource, null));
                }
            }
            
			//Get Editor Field
			editInfo.Editor = "DotNetNuke.UI.WebControls.TextEditControl, DotNetNuke";
            if (!String.IsNullOrEmpty(EditorDataField))
            {
                property = DataSource.GetType().GetProperty(EditorDataField);
                if (!(property == null || (property.GetValue(DataSource, null) == null)))
                {
                    editInfo.Editor = EditorInfo.GetEditor(Convert.ToInt32(property.GetValue(DataSource, null)));
                }
            }
			
            //Get LabelMode Field
            editInfo.LabelMode = LabelMode.Left;

            //Get Required Field
            editInfo.Required = false;
            if (!String.IsNullOrEmpty(RequiredDataField))
            {
                property = DataSource.GetType().GetProperty(RequiredDataField);
                if (!((property == null) || (property.GetValue(DataSource, null) == null)))
                {
                    editInfo.Required = Convert.ToBoolean(property.GetValue(DataSource, null));
                }
            }
			
            //Set ResourceKey Field
            editInfo.ResourceKey = editInfo.Name;
            editInfo.ResourceKey = string.Format("{0}_{1}", Name, editInfo.Name);

            //Set Style
            editInfo.ControlStyle = new Style();

            //Get Visibility Field
            editInfo.ProfileVisibility = new ProfileVisibility
                                             {
                                                 VisibilityMode = UserVisibilityMode.AllUsers
                                             };
            if (!String.IsNullOrEmpty(VisibilityDataField))
            {
                property = DataSource.GetType().GetProperty(VisibilityDataField);
                if (!(property == null || (property.GetValue(DataSource, null) == null)))
                {
                    editInfo.ProfileVisibility = (ProfileVisibility)property.GetValue(DataSource, null);
                }
            }
			
            //Get Validation Expression Field
            editInfo.ValidationExpression = string.Empty;
            if (!String.IsNullOrEmpty(ValidationExpressionDataField))
            {
                property = DataSource.GetType().GetProperty(ValidationExpressionDataField);
                if (!(property == null || (property.GetValue(DataSource, null) == null)))
                {
                    editInfo.ValidationExpression = Convert.ToString(property.GetValue(DataSource, null));
                }
            }
			
			//Get Length Field
            if (!String.IsNullOrEmpty(MaxLengthDataField))
            {
                property = DataSource.GetType().GetProperty(MaxLengthDataField);
                if (!(property == null || (property.GetValue(DataSource, null) == null)))
                {
                    int length = Convert.ToInt32(property.GetValue(DataSource, null));
                    var attributes = new object[1];
                    attributes[0] = new MaxLengthAttribute(length);
                    editInfo.Attributes = attributes;
                }
            }
			
			//Remove spaces from name
            editInfo.Name = editInfo.Name.Replace(" ", "_");
            return editInfo;
        }
    }
}
