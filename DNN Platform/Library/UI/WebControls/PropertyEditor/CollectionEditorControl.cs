// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.UI.WebControls
{
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.Reflection;
    using System.Web.UI;
    using System.Web.UI.HtmlControls;
    using System.Web.UI.WebControls;

    using DotNetNuke.Common.Utilities;

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
        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets and sets the value of the Category.
        /// </summary>
        /// <value>A string representing the Category of the Field.</value>
        /// -----------------------------------------------------------------------------
        [Browsable(true)]
        [Category("Data")]
        [DefaultValue("")]
        [Description("Enter the name of the field that is data bound to the Category.")]
        public string CategoryDataField { get; set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets and sets the value of the Editor Type to use.
        /// </summary>
        /// <value>A string representing the Editor Type of the Field.</value>
        /// -----------------------------------------------------------------------------
        [Browsable(true)]
        [Category("Data")]
        [DefaultValue("")]
        [Description("Enter the name of the field that is data bound to the Editor Type.")]
        public string EditorDataField { get; set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets and sets the value of the Field that determines the length.
        /// </summary>
        /// <value>A string representing the Name of the Field.</value>
        /// -----------------------------------------------------------------------------
        [Browsable(true)]
        [Category("Data")]
        [DefaultValue("")]
        [Description("Enter the name of the field that determines the length.")]
        public string LengthDataField { get; set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets and sets the value of the Field that is bound to the Label.
        /// </summary>
        /// <value>A string representing the Name of the Field.</value>
        /// -----------------------------------------------------------------------------
        [Browsable(true)]
        [Category("Data")]
        [DefaultValue("")]
        [Description("Enter the name of the field that is data bound to the Label's Text property.")]
        public string NameDataField { get; set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets and sets the value of the Field that determines whether an item is required.
        /// </summary>
        /// <value>A string representing the Name of the Field.</value>
        /// -----------------------------------------------------------------------------
        [Browsable(true)]
        [Category("Data")]
        [DefaultValue("")]
        [Description("Enter the name of the field that determines whether an item is required.")]
        public string RequiredDataField { get; set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets and sets the value of the Field that is bound to the EditControl.
        /// </summary>
        /// <value>A string representing the Name of the Field.</value>
        /// -----------------------------------------------------------------------------
        [Browsable(true)]
        [Category("Data")]
        [DefaultValue("")]
        [Description("Enter the name of the field that is data bound to the EditControl's Type.")]
        public string TypeDataField { get; set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets and sets the value of the Field that is bound to the EditControl's
        /// Expression DynamicContentValidator.
        /// </summary>
        /// <value>A string representing the Name of the Field.</value>
        /// -----------------------------------------------------------------------------
        [Browsable(true)]
        [Category("Data")]
        [DefaultValue("")]
        [Description("Enter the name of the field that is data bound to the EditControl's Expression DynamicContentValidator.")]
        public string ValidationExpressionDataField { get; set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets and sets the value of the Field that is bound to the EditControl.
        /// </summary>
        /// <value>A string representing the Name of the Field.</value>
        /// -----------------------------------------------------------------------------
        [Browsable(true)]
        [Category("Data")]
        [DefaultValue("")]
        [Description("Enter the name of the field that is data bound to the EditControl's Value property.")]
        public string ValueDataField { get; set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets and sets the value of the Field that determines whether the control is visible.
        /// </summary>
        /// <value>A string representing the Name of the Field.</value>
        /// -----------------------------------------------------------------------------
        [Browsable(true)]
        [Category("Data")]
        [DefaultValue("")]
        [Description("Enter the name of the field that determines whether the item is visble.")]
        public string VisibleDataField { get; set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets and sets the value of the Field that determines the visibility.
        /// </summary>
        /// <value>A string representing the Name of the Field.</value>
        /// -----------------------------------------------------------------------------
        [Browsable(true)]
        [Category("Data")]
        [DefaultValue("")]
        [Description("Enter the name of the field that determines the visibility.")]
        public string VisibilityDataField { get; set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets the Underlying DataSource.
        /// </summary>
        /// <value>An IEnumerable.</value>
        /// -----------------------------------------------------------------------------
        protected override IEnumerable UnderlyingDataSource
        {
            get
            {
                return (IEnumerable)this.DataSource;
            }
        }

        protected override void AddEditorRow(Table table, object obj)
        {
            this.AddEditorRow(table, this.NameDataField, new CollectionEditorInfoAdapter(obj, this.ID, this.NameDataField, this.GetFieldNames()));
        }

        protected override void AddEditorRow(Panel container, object obj)
        {
            this.AddEditorRow(container, this.NameDataField, new CollectionEditorInfoAdapter(obj, this.ID, this.NameDataField, this.GetFieldNames()));
        }

        protected override void AddEditorRow(object obj)
        {
            this.AddEditorRow(this, this.NameDataField, new CollectionEditorInfoAdapter(obj, this.ID, this.NameDataField, this.GetFieldNames()));
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// GetCategory gets the Category of an object.
        /// </summary>
        /// <returns></returns>
        /// -----------------------------------------------------------------------------
        protected override string GetCategory(object obj)
        {
            PropertyInfo objProperty;
            string _Category = Null.NullString;

            // Get Category Field
            if (!string.IsNullOrEmpty(this.CategoryDataField))
            {
                objProperty = obj.GetType().GetProperty(this.CategoryDataField);
                if (!(objProperty == null || (objProperty.GetValue(obj, null) == null)))
                {
                    _Category = Convert.ToString(objProperty.GetValue(obj, null));
                }
            }

            return _Category;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// GetGroups gets an array of Groups/Categories from the DataSource.
        /// </summary>
        /// <returns></returns>
        /// -----------------------------------------------------------------------------
        protected override string[] GetGroups(IEnumerable arrObjects)
        {
            var arrGroups = new ArrayList();
            PropertyInfo objProperty;

            foreach (object obj in arrObjects)
            {
                // Get Category Field
                if (!string.IsNullOrEmpty(this.CategoryDataField))
                {
                    objProperty = obj.GetType().GetProperty(this.CategoryDataField);
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
        /// GetRowVisibility determines the Visibility of a row in the table.
        /// </summary>
        /// <param name="obj">The property.</param>
        /// <returns></returns>
        /// -----------------------------------------------------------------------------
        protected override bool GetRowVisibility(object obj)
        {
            bool isVisible = true;
            PropertyInfo objProperty;
            objProperty = obj.GetType().GetProperty(this.VisibleDataField);
            if (!(objProperty == null || (objProperty.GetValue(obj, null) == null)))
            {
                isVisible = Convert.ToBoolean(objProperty.GetValue(obj, null));
            }

            if (!isVisible && this.EditMode == PropertyEditorMode.Edit)
            {
                // Check if property is required - as this will need to override visibility
                objProperty = obj.GetType().GetProperty(this.RequiredDataField);
                if (!(objProperty == null || (objProperty.GetValue(obj, null) == null)))
                {
                    isVisible = Convert.ToBoolean(objProperty.GetValue(obj, null));
                }
            }

            return isVisible;
        }

        private Hashtable GetFieldNames()
        {
            var fields = new Hashtable();
            fields.Add("Category", this.CategoryDataField);
            fields.Add("Editor", this.EditorDataField);
            fields.Add("Name", this.NameDataField);
            fields.Add("Required", this.RequiredDataField);
            fields.Add("Type", this.TypeDataField);
            fields.Add("ValidationExpression", this.ValidationExpressionDataField);
            fields.Add("Value", this.ValueDataField);
            fields.Add("ProfileVisibility", this.VisibilityDataField);
            fields.Add("Length", this.LengthDataField);

            return fields;
        }
    }
}
