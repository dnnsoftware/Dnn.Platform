// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.UI.WebControls
{
    using System;
    using System.Web.UI;
    using System.Web.UI.WebControls;

    using DotNetNuke.Common.Utilities;

    /// -----------------------------------------------------------------------------
    /// Project:    DotNetNuke
    /// Namespace:  DotNetNuke.UI.WebControls
    /// Class:      TextColumnTemplate
    /// -----------------------------------------------------------------------------
    /// <summary>
    /// The TextColumnTemplate provides a Template for the TextColumn.
    /// </summary>
    /// -----------------------------------------------------------------------------
    public class TextColumnTemplate : ITemplate
    {
        private ListItemType mItemType = ListItemType.Item;

        public TextColumnTemplate()
            : this(ListItemType.Item)
        {
        }

        public TextColumnTemplate(ListItemType itemType)
        {
            this.ItemType = itemType;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the Data Field is the field that binds to the Text Column.
        /// </summary>
        /// <value>A String.</value>
        /// -----------------------------------------------------------------------------
        public string DataField { get; set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets a value indicating whether gets or sets the Design Mode of the Column.
        /// </summary>
        /// <value>A Boolean.</value>
        /// -----------------------------------------------------------------------------
        public bool DesignMode { get; set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the type of Template to Create.
        /// </summary>
        /// <value>A String.</value>
        /// -----------------------------------------------------------------------------
        public ListItemType ItemType
        {
            get
            {
                return this.mItemType;
            }

            set
            {
                this.mItemType = value;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the Text (for Header/Footer Templates).
        /// </summary>
        /// <value>A String.</value>
        /// -----------------------------------------------------------------------------
        public string Text { get; set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the Width of the Column.
        /// </summary>
        /// <value>A Unit.</value>
        /// -----------------------------------------------------------------------------
        public Unit Width { get; set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// InstantiateIn instantiates the template (implementation of ITemplate).
        /// </summary>
        /// <remarks>
        /// </remarks>
        ///     <param name="container">The parent container (DataGridItem).</param>
        /// -----------------------------------------------------------------------------
        public void InstantiateIn(Control container)
        {
            switch (this.ItemType)
            {
                case ListItemType.Item:
                case ListItemType.AlternatingItem:
                case ListItemType.SelectedItem:
                    var lblText = new Label();
                    lblText.Width = this.Width;
                    lblText.DataBinding += this.Item_DataBinding;
                    container.Controls.Add(lblText);
                    break;
                case ListItemType.EditItem:
                    var txtText = new TextBox();
                    txtText.Width = this.Width;
                    txtText.DataBinding += this.Item_DataBinding;
                    container.Controls.Add(txtText);
                    break;
                case ListItemType.Footer:
                case ListItemType.Header:
                    container.Controls.Add(new LiteralControl(this.Text));
                    break;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets the value of the Data Field.
        /// </summary>
        ///     <param name="container">The parent container (DataGridItem).</param>
        /// -----------------------------------------------------------------------------
        private string GetValue(DataGridItem container)
        {
            string itemValue = Null.NullString;
            if (!string.IsNullOrEmpty(this.DataField))
            {
                if (this.DesignMode)
                {
                    itemValue = "DataBound to " + this.DataField;
                }
                else
                {
                    if (container.DataItem != null)
                    {
                        object evaluation = DataBinder.Eval(container.DataItem, this.DataField);
                        if (evaluation != null)
                        {
                            itemValue = evaluation.ToString();
                        }
                    }
                }
            }

            return itemValue;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Item_DataBinding runs when an Item of type ListItemType.Item is being data-bound.
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <param name="sender"> The object that triggers the event.</param>
        /// <param name="e">An EventArgs object.</param>
        /// -----------------------------------------------------------------------------
        private void Item_DataBinding(object sender, EventArgs e)
        {
            DataGridItem container;
            int keyValue = Null.NullInteger;
            switch (this.ItemType)
            {
                case ListItemType.Item:
                case ListItemType.AlternatingItem:
                case ListItemType.SelectedItem:
                    var lblText = (Label)sender;
                    container = (DataGridItem)lblText.NamingContainer;
                    lblText.Text = this.GetValue(container);
                    break;
                case ListItemType.EditItem:
                    var txtText = (TextBox)sender;
                    container = (DataGridItem)txtText.NamingContainer;
                    txtText.Text = this.GetValue(container);
                    break;
            }
        }
    }
}
