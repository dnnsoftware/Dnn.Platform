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
using System.Web.UI;
using System.Web.UI.WebControls;

using DotNetNuke.Common.Utilities;

#endregion

namespace DotNetNuke.UI.WebControls
{
    /// -----------------------------------------------------------------------------
    /// Project:    DotNetNuke
    /// Namespace:  DotNetNuke.UI.WebControls
    /// Class:      TextColumnTemplate
    /// -----------------------------------------------------------------------------
    /// <summary>
    /// The TextColumnTemplate provides a Template for the TextColumn
    /// </summary>
    /// -----------------------------------------------------------------------------
    public class TextColumnTemplate : ITemplate
    {
		#region "Private Members"

        private ListItemType mItemType = ListItemType.Item;
		
		#endregion

		#region "Constructors"

        public TextColumnTemplate() : this(ListItemType.Item)
        {
        }

        public TextColumnTemplate(ListItemType itemType)
        {
            ItemType = itemType;
        }
		
		#endregion

		#region "Public Properties"

		/// -----------------------------------------------------------------------------
		/// <summary>
		/// The Data Field is the field that binds to the Text Column
		/// </summary>
		/// <value>A String</value>
		/// -----------------------------------------------------------------------------
        public string DataField { get; set; }

		/// -----------------------------------------------------------------------------
		/// <summary>
		/// Gets or sets the Design Mode of the Column
		/// </summary>
		/// <value>A Boolean</value>
		/// -----------------------------------------------------------------------------
        public bool DesignMode { get; set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// The type of Template to Create
        /// </summary>
        /// <value>A String</value>
        /// -----------------------------------------------------------------------------
        public ListItemType ItemType
        {
            get
            {
                return mItemType;
            }
            set
            {
                mItemType = value;
            }
        }

		/// -----------------------------------------------------------------------------
		/// <summary>
		/// Gets or sets the Text (for Header/Footer Templates)
		/// </summary>
		/// <value>A String</value>
		/// -----------------------------------------------------------------------------
        public string Text { get; set; }

		/// -----------------------------------------------------------------------------
		/// <summary>
		/// Gets or sets the Width of the Column
		/// </summary>
		/// <value>A Unit</value>
		/// -----------------------------------------------------------------------------
        public Unit Width { get; set; }
		
		#endregion

        #region ITemplate Members

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// InstantiateIn instantiates the template (implementation of ITemplate)
        /// </summary>
        /// <remarks>
        /// </remarks>
        ///	<param name="container">The parent container (DataGridItem)</param>
        /// -----------------------------------------------------------------------------
        public void InstantiateIn(Control container)
        {
            switch (ItemType)
            {
                case ListItemType.Item:
                case ListItemType.AlternatingItem:
                case ListItemType.SelectedItem:
                    var lblText = new Label();
                    lblText.Width = Width;
                    lblText.DataBinding += Item_DataBinding;
                    container.Controls.Add(lblText);
                    break;
                case ListItemType.EditItem:
                    var txtText = new TextBox();
                    txtText.Width = Width;
                    txtText.DataBinding += Item_DataBinding;
                    container.Controls.Add(txtText);
                    break;
                case ListItemType.Footer:
                case ListItemType.Header:
                    container.Controls.Add(new LiteralControl(Text));
                    break;
            }
        }

        #endregion

		#region "Private Methods"

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets the value of the Data Field
        /// </summary>
        ///	<param name="container">The parent container (DataGridItem)</param>
        /// -----------------------------------------------------------------------------
        private string GetValue(DataGridItem container)
        {
            string itemValue = Null.NullString;
            if (!String.IsNullOrEmpty(DataField))
            {
                if (DesignMode)
                {
                    itemValue = "DataBound to " + DataField;
                }
                else
                {
                    if (container.DataItem != null)
                    {
                        object evaluation = DataBinder.Eval(container.DataItem, DataField);
                        if ((evaluation != null))
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
        /// Item_DataBinding runs when an Item of type ListItemType.Item is being data-bound
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <param name="sender"> The object that triggers the event</param>
        /// <param name="e">An EventArgs object</param>
        /// -----------------------------------------------------------------------------
        private void Item_DataBinding(object sender, EventArgs e)
        {
            DataGridItem container;
            int keyValue = Null.NullInteger;
            switch (ItemType)
            {
                case ListItemType.Item:
                case ListItemType.AlternatingItem:
                case ListItemType.SelectedItem:
                    var lblText = (Label) sender;
                    container = (DataGridItem) lblText.NamingContainer;
                    lblText.Text = GetValue(container);
                    break;
                case ListItemType.EditItem:
                    var txtText = (TextBox) sender;
                    container = (DataGridItem) txtText.NamingContainer;
                    txtText.Text = GetValue(container);
                    break;
            }
        }
		
		#endregion
    }
}
