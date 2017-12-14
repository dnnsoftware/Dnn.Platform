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
using System.ComponentModel;
using System.Reflection;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

using DotNetNuke.Common.Utilities;

#endregion

namespace DotNetNuke.UI.WebControls
{
    /// -----------------------------------------------------------------------------
    /// Project:    DotNetNuke
    /// Namespace:  DotNetNuke.UI.WebControls
    /// Class:      CollectionEditorControl
    /// -----------------------------------------------------------------------------
    /// <summary>
    /// The CollectionEditorControl control provides a Control to display Collection
    /// Properties.
    /// </summary>
    /// <remarks>
    /// </remarks>
    /// -----------------------------------------------------------------------------
    [ToolboxData("<{0}:CollectionEditorControl runat=server></{0}:CollectionEditorControl>")]
    public class CollectionEditorControl : PropertyEditorControl
    {
		#region Protected Members

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets the Underlying DataSource
        /// </summary>
        /// <value>An IEnumerable</value>
        /// -----------------------------------------------------------------------------
        protected override IEnumerable UnderlyingDataSource
        {
            get
            {
                return (IEnumerable)DataSource;
            }
        }

		#endregion

		#region Public Properties

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets and sets the value of the Category
        /// </summary>
        /// <value>A string representing the Category of the Field</value>
        /// -----------------------------------------------------------------------------
        [Browsable(true), Category("Data"), DefaultValue(""), Description("Enter the name of the field that is data bound to the Category.")]
        public string CategoryDataField { get; set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets and sets the value of the Editor Type to use
        /// </summary>
        /// <value>A string representing the Editor Type of the Field</value>
        /// -----------------------------------------------------------------------------
        [Browsable(true), Category("Data"), DefaultValue(""), Description("Enter the name of the field that is data bound to the Editor Type.")]
        public string EditorDataField { get; set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets and sets the value of the Field that determines the length
        /// </summary>
        /// <value>A string representing the Name of the Field</value>
        /// -----------------------------------------------------------------------------
        [Browsable(true), Category("Data"), DefaultValue(""), Description("Enter the name of the field that determines the length.")]
        public string LengthDataField { get; set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets and sets the value of the Field that is bound to the Label
        /// </summary>
        /// <value>A string representing the Name of the Field</value>
        /// -----------------------------------------------------------------------------
        [Browsable(true), Category("Data"), DefaultValue(""), Description("Enter the name of the field that is data bound to the Label's Text property.")]
        public string NameDataField { get; set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets and sets the value of the Field that determines whether an item is required
        /// </summary>
        /// <value>A string representing the Name of the Field</value>
        /// -----------------------------------------------------------------------------
        [Browsable(true), Category("Data"), DefaultValue(""), Description("Enter the name of the field that determines whether an item is required.")]
        public string RequiredDataField { get; set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets and sets the value of the Field that is bound to the EditControl
        /// </summary>
        /// <value>A string representing the Name of the Field</value>
        /// -----------------------------------------------------------------------------
        [Browsable(true), Category("Data"), DefaultValue(""), Description("Enter the name of the field that is data bound to the EditControl's Type.")]
        public string TypeDataField { get; set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets and sets the value of the Field that is bound to the EditControl's 
        /// Expression DynamicContentValidator
        /// </summary>
        /// <value>A string representing the Name of the Field</value>
        /// -----------------------------------------------------------------------------
        [Browsable(true), Category("Data"), DefaultValue(""), Description("Enter the name of the field that is data bound to the EditControl's Expression DynamicContentValidator.")]
        public string ValidationExpressionDataField { get; set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets and sets the value of the Field that is bound to the EditControl
        /// </summary>
        /// <value>A string representing the Name of the Field</value>
        /// -----------------------------------------------------------------------------
        [Browsable(true), Category("Data"), DefaultValue(""), Description("Enter the name of the field that is data bound to the EditControl's Value property.")]
        public string ValueDataField { get; set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets and sets the value of the Field that determines whether the control is visible
        /// </summary>
        /// <value>A string representing the Name of the Field</value>
        /// -----------------------------------------------------------------------------
        [Browsable(true), Category("Data"), DefaultValue(""), Description("Enter the name of the field that determines whether the item is visble.")]
        public string VisibleDataField { get; set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets and sets the value of the Field that determines the visibility
        /// </summary>
        /// <value>A string representing the Name of the Field</value>
        /// -----------------------------------------------------------------------------
        [Browsable(true), Category("Data"), DefaultValue(""), Description("Enter the name of the field that determines the visibility.")]
        public string VisibilityDataField { get; set; }
		
		#endregion

		#region Private Methods

        private Hashtable GetFieldNames()
        {
            var fields = new Hashtable();
            fields.Add("Category", CategoryDataField);
            fields.Add("Editor", EditorDataField);
            fields.Add("Name", NameDataField);
            fields.Add("Required", RequiredDataField);
            fields.Add("Type", TypeDataField);
            fields.Add("ValidationExpression", ValidationExpressionDataField);
            fields.Add("Value", ValueDataField);
            fields.Add("ProfileVisibility", VisibilityDataField);
            fields.Add("Length", LengthDataField);

            return fields;
        }
		
		#endregion

		#region Protected Methods

        protected override void AddEditorRow(Table table, object obj)
        {
            AddEditorRow(table, NameDataField, new CollectionEditorInfoAdapter(obj, ID, NameDataField, GetFieldNames()));
        }

        protected override void AddEditorRow(Panel container, object obj)
        {
            AddEditorRow(container, NameDataField, new CollectionEditorInfoAdapter(obj, ID, NameDataField, GetFieldNames()));
        }

        protected override void AddEditorRow(object obj)
        {
            AddEditorRow(this, NameDataField, new CollectionEditorInfoAdapter(obj, ID, NameDataField, GetFieldNames()));
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// GetCategory gets the Category of an object
        /// </summary>
        /// -----------------------------------------------------------------------------
        protected override string GetCategory(object obj)
        {
            PropertyInfo objProperty;
            string _Category = Null.NullString;
			
			//Get Category Field
            if (!String.IsNullOrEmpty(CategoryDataField))
            {
                objProperty = obj.GetType().GetProperty(CategoryDataField);
                if (!(objProperty == null || (objProperty.GetValue(obj, null) == null)))
                {
                    _Category = Convert.ToString(objProperty.GetValue(obj, null));
                }
            }
            return _Category;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// GetGroups gets an array of Groups/Categories from the DataSource
        /// </summary>
        /// -----------------------------------------------------------------------------
        protected override string[] GetGroups(IEnumerable arrObjects)
        {
            var arrGroups = new ArrayList();
            PropertyInfo objProperty;

            foreach (object obj in arrObjects)
            {
				//Get Category Field
                if (!String.IsNullOrEmpty(CategoryDataField))
                {
                    objProperty = obj.GetType().GetProperty(CategoryDataField);
                    if (!((objProperty == null) || (objProperty.GetValue(obj, null) == null)))
                    {
                        string _Category = Convert.ToString(objProperty.GetValue(obj, null));

                        if (!arrGroups.Contains(_Category))
                        {
                            arrGroups.Add(_Category);
                        }
                    }
                }
            }
            var strGroups = new string[arrGroups.Count];
            for (int i = 0; i <= arrGroups.Count - 1; i++)
            {
                strGroups[i] = Convert.ToString(arrGroups[i]);
            }
            return strGroups;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// GetRowVisibility determines the Visibility of a row in the table
        /// </summary>
        /// <param name="obj">The property</param>
        /// -----------------------------------------------------------------------------
        protected override bool GetRowVisibility(object obj)
        {
            bool isVisible = true;
            PropertyInfo objProperty;
            objProperty = obj.GetType().GetProperty(VisibleDataField);
            if (!(objProperty == null || (objProperty.GetValue(obj, null) == null)))
            {
                isVisible = Convert.ToBoolean(objProperty.GetValue(obj, null));
            }
            if (!isVisible && EditMode == PropertyEditorMode.Edit)
            {
				//Check if property is required - as this will need to override visibility
                objProperty = obj.GetType().GetProperty(RequiredDataField);
                if (!(objProperty == null || (objProperty.GetValue(obj, null) == null)))
                {
                    isVisible = Convert.ToBoolean(objProperty.GetValue(obj, null));
                }
            }
            return isVisible;
        }
		
		#endregion
    }
}
