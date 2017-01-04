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

using System;
using System.Collections.Specialized;
using System.Globalization;
using System.Web.UI;
using System.Web.UI.WebControls;
using DotNetNuke.Common.Utilities;
using DotNetNuke.UI.Utilities;
using DotNetNuke.UI.WebControls;

namespace DotNetNuke.Web.UI.WebControls
{
    public class DnnGridImageCommandColumnTemplate : IBindableTemplate
    {
        private ImageCommandColumnEditMode _editMode = ImageCommandColumnEditMode.Command;
        private ListItemType _itemType = ListItemType.Item;
        private bool _showImage = true;
        private bool _visible = true;

        public DnnGridImageCommandColumnTemplate()
            : this(ListItemType.Item)
        {
        }

        public DnnGridImageCommandColumnTemplate(ListItemType itemType)
        {
            ItemType = itemType;
        }


        /// <summary>
        /// Gets or sets the CommandName for the Column
        /// </summary>
        /// <value>A String</value>
        public string CommandName { get; set; }


        /// <summary>
        /// Gets or sets the Design Mode of the Column
        /// </summary>
        /// <value>A Boolean</value>
        public bool DesignMode { get; set; }


        /// <summary>
        /// Gets or sets the CommandName for the Column
        /// </summary>
        /// <value>A String</value>
        public ImageCommandColumnEditMode EditMode
        {
            get { return _editMode; }
            set { _editMode = value; }
        }


        /// <summary>
        /// Gets or sets the URL of the Image
        /// </summary>
        /// <value>A String</value>
        public string ImageURL { get; set; }


        /// <summary>
        /// The type of Template to Create
        /// </summary>
        /// <value>A String</value>
        public ListItemType ItemType
        {
            get { return _itemType; }
            set { _itemType = value; }
        }


        /// <summary>
        /// The Key Field that provides a Unique key to the data Item
        /// </summary>
        /// <value>A String</value>
        public string KeyField { get; set; }


        /// <summary>
        /// Gets or sets the URL of the Link (unless DataBinding through KeyField)
        /// </summary>
        /// <value>A String</value>
        public string NavigateURL { get; set; }


        /// <summary>
        /// Gets or sets the URL Formatting string
        /// </summary>
        /// <value>A String</value>
        public string NavigateURLFormatString { get; set; }


        /// <summary>
        /// Javascript text to attach to the OnClick Event
        /// </summary>
        /// <value>A String</value>
        public string OnClickJs { get; set; }


        /// <summary>
        /// Gets or sets whether an Image is displayed
        /// </summary>
        /// <remarks>Defaults to True</remarks>
        /// <value>A Boolean</value>
        public bool ShowImage
        {
            get { return _showImage; }
            set { _showImage = value; }
        }


        /// <summary>
        /// Gets or sets the Text (for Header/Footer Templates)
        /// </summary>
        /// <value>A String</value>
        public string Text { get; set; }


        /// <summary>
        /// An flag that indicates whether the buttons are visible (this is overridden if
        /// the VisibleField is set)
        /// changed
        /// </summary>
        /// <value>A Boolean</value>
        public bool Visible
        {
            get { return _visible; }
            set { _visible = value; }
        }


        /// <summary>
        /// An flag that indicates whether the buttons are visible.
        /// </summary>
        /// <value>A Boolean</value>
        public string VisibleField { get; set; }

        #region ITemplate Members

        /// <summary>
        /// InstantiateIn instantiates the template (implementation of ITemplate)
        /// </summary>
        /// <remarks>
        /// </remarks>
        ///	<param name="container">The parent container (DataGridItem)</param>
        public void InstantiateIn(Control container)
        {
            switch (ItemType)
            {
                case ListItemType.Item:
                case ListItemType.AlternatingItem:
                case ListItemType.SelectedItem:
                case ListItemType.EditItem:
                    if (EditMode == ImageCommandColumnEditMode.URL)
                    {
                        var hypLink = new HyperLink {ToolTip = Text};
                        if (!String.IsNullOrEmpty(ImageURL) && ShowImage)
                        {
                            var img = new Image {ImageUrl = DesignMode ? ImageURL.Replace("~/", "../../") : ImageURL};
                            hypLink.Controls.Add(img);
                            img.ToolTip = Text;
                        }
                        else
                        {
                            hypLink.Text = Text;
                        }
                        hypLink.DataBinding += ItemDataBinding;
                        container.Controls.Add(hypLink);
                    }
                    else
                    {
                        if (!String.IsNullOrEmpty(ImageURL) && ShowImage)
                        {
                            var colIcon = new ImageButton
                                {ImageUrl = DesignMode ? ImageURL.Replace("~/", "../../") : ImageURL, ToolTip = Text};
                            if (!String.IsNullOrEmpty(OnClickJs))
                            {
                                ClientAPI.AddButtonConfirm(colIcon, OnClickJs);
                            }
                            colIcon.CommandName = CommandName;
                            colIcon.DataBinding += ItemDataBinding;
                            container.Controls.Add(colIcon);
                        }
                        if (!String.IsNullOrEmpty(Text) && !ShowImage)
                        {
                            var colLink = new LinkButton {ToolTip = Text};
                            if (!String.IsNullOrEmpty(OnClickJs))
                            {
                                ClientAPI.AddButtonConfirm(colLink, OnClickJs);
                            }
                            colLink.CommandName = CommandName;
                            colLink.Text = Text;
                            colLink.DataBinding += ItemDataBinding;
                            container.Controls.Add(colLink);
                        }
                    }
                    break;
                case ListItemType.Footer:
                case ListItemType.Header:
                    container.Controls.Add(new LiteralControl(Text));
                    break;
            }
        }
        
        public IOrderedDictionary ExtractValues(Control container)
        {
            //do nothing we don't really support databinding
            //but the telerik grid trys to databind to all template columns regardless
            return new OrderedDictionary();
        }

        #endregion

        /// <summary>
        /// Gets whether theButton is visible
        /// </summary>
        ///	<param name="container">The parent container (DataGridItem)</param>
        private bool GetIsVisible(DataGridItem container)
        {
            if (!String.IsNullOrEmpty(VisibleField))
            {
                return Convert.ToBoolean(DataBinder.Eval(container.DataItem, VisibleField));
            }

            return Visible;
        }


        /// <summary>
        /// Gets the value of the key
        /// </summary>
        ///	<param name="container">The parent container (DataGridItem)</param>
        private int GetValue(DataGridItem container)
        {
            int keyValue = Null.NullInteger;
            if (!String.IsNullOrEmpty(KeyField))
            {
                keyValue = Convert.ToInt32(DataBinder.Eval(container.DataItem, KeyField));
            }
            return keyValue;
        }


        /// <summary>
        /// Item_DataBinding runs when an Item of type GridItemType.Item is being data-bound
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <param name="sender"> The object that triggers the event</param>
        /// <param name="e">An EventArgs object</param>
        private void ItemDataBinding(object sender, EventArgs e)
        {
            DataGridItem container;
            int keyValue;
            if (EditMode == ImageCommandColumnEditMode.URL)
            {
                var hypLink = (HyperLink) sender;
                container = (DataGridItem) hypLink.NamingContainer;
                keyValue = GetValue(container);
                if (!String.IsNullOrEmpty(NavigateURLFormatString))
                {
                    hypLink.NavigateUrl = string.Format(NavigateURLFormatString, keyValue);
                }
                else
                {
                    hypLink.NavigateUrl = keyValue.ToString(CultureInfo.InvariantCulture);
                }
            }
            else
            {
                //Bind Image Button
                if (!String.IsNullOrEmpty(ImageURL) && ShowImage)
                {
                    var colIcon = (ImageButton) sender;
                    container = (DataGridItem) colIcon.NamingContainer;
                    keyValue = GetValue(container);
                    colIcon.CommandArgument = keyValue.ToString(CultureInfo.InvariantCulture);
                    colIcon.Visible = GetIsVisible(container);
                }
                if (!String.IsNullOrEmpty(Text) && !ShowImage)
                {
                    //Bind Link Button
                    var colLink = (LinkButton) sender;
                    container = (DataGridItem) colLink.NamingContainer;
                    keyValue = GetValue(container);
                    colLink.CommandArgument = keyValue.ToString(CultureInfo.InvariantCulture);
                    colLink.Visible = GetIsVisible(container);
                }
            }
        }
    }
}