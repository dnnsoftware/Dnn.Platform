// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.UI.WebControls
{
    using System;
    using System.Collections.Specialized;
    using System.Globalization;
    using System.Web.UI;
    using System.Web.UI.WebControls;

    using DotNetNuke.Common.Utilities;
    using DotNetNuke.UI.Utilities;
    using DotNetNuke.UI.WebControls;
    using Telerik.Web.UI;

    public class DnnGridImageCommandColumnTemplate : IBindableTemplate
    {
        private ImageCommandColumnEditMode _editMode = ImageCommandColumnEditMode.Command;
        private GridItemType _itemType = GridItemType.Item;
        private bool _showImage = true;
        private bool _visible = true;

        public DnnGridImageCommandColumnTemplate()
            : this(GridItemType.Item)
        {
        }

        public DnnGridImageCommandColumnTemplate(GridItemType itemType)
        {
            this.ItemType = itemType;
        }

        /// <summary>
        /// Gets or sets the CommandName for the Column.
        /// </summary>
        /// <value>A String.</value>
        public string CommandName { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether gets or sets the Design Mode of the Column.
        /// </summary>
        /// <value>A Boolean.</value>
        public bool DesignMode { get; set; }

        /// <summary>
        /// Gets or sets the CommandName for the Column.
        /// </summary>
        /// <value>A String.</value>
        public ImageCommandColumnEditMode EditMode
        {
            get { return this._editMode; }
            set { this._editMode = value; }
        }

        /// <summary>
        /// Gets or sets the URL of the Image.
        /// </summary>
        /// <value>A String.</value>
        public string ImageURL { get; set; }

        /// <summary>
        /// Gets or sets the type of Template to Create.
        /// </summary>
        /// <value>A String.</value>
        public GridItemType ItemType
        {
            get { return this._itemType; }
            set { this._itemType = value; }
        }

        /// <summary>
        /// Gets or sets the Key Field that provides a Unique key to the data Item.
        /// </summary>
        /// <value>A String.</value>
        public string KeyField { get; set; }

        /// <summary>
        /// Gets or sets the URL of the Link (unless DataBinding through KeyField).
        /// </summary>
        /// <value>A String.</value>
        public string NavigateURL { get; set; }

        /// <summary>
        /// Gets or sets the URL Formatting string.
        /// </summary>
        /// <value>A String.</value>
        public string NavigateURLFormatString { get; set; }

        /// <summary>
        /// Gets or sets javascript text to attach to the OnClick Event.
        /// </summary>
        /// <value>A String.</value>
        public string OnClickJs { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether gets or sets whether an Image is displayed.
        /// </summary>
        /// <remarks>Defaults to True.</remarks>
        /// <value>A Boolean.</value>
        public bool ShowImage
        {
            get { return this._showImage; }
            set { this._showImage = value; }
        }

        /// <summary>
        /// Gets or sets the Text (for Header/Footer Templates).
        /// </summary>
        /// <value>A String.</value>
        public string Text { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether an flag that indicates whether the buttons are visible (this is overridden if
        /// the VisibleField is set)
        /// changed.
        /// </summary>
        /// <value>A Boolean.</value>
        public bool Visible
        {
            get { return this._visible; }
            set { this._visible = value; }
        }

        /// <summary>
        /// Gets or sets an flag that indicates whether the buttons are visible.
        /// </summary>
        /// <value>A Boolean.</value>
        public string VisibleField { get; set; }

        /// <summary>
        /// InstantiateIn instantiates the template (implementation of ITemplate).
        /// </summary>
        /// <remarks>
        /// </remarks>
        ///     <param name="container">The parent container (DataGridItem).</param>
        public void InstantiateIn(Control container)
        {
            switch (this.ItemType)
            {
                case GridItemType.Item:
                case GridItemType.AlternatingItem:
                case GridItemType.SelectedItem:
                case GridItemType.EditItem:
                    if (this.EditMode == ImageCommandColumnEditMode.URL)
                    {
                        var hypLink = new HyperLink { ToolTip = this.Text };
                        if (!string.IsNullOrEmpty(this.ImageURL) && this.ShowImage)
                        {
                            var img = new Image { ImageUrl = this.DesignMode ? this.ImageURL.Replace("~/", "../../") : this.ImageURL };
                            hypLink.Controls.Add(img);
                            img.ToolTip = this.Text;
                        }
                        else
                        {
                            hypLink.Text = this.Text;
                        }

                        hypLink.DataBinding += this.ItemDataBinding;
                        container.Controls.Add(hypLink);
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(this.ImageURL) && this.ShowImage)
                        {
                            var colIcon = new ImageButton
                            { ImageUrl = this.DesignMode ? this.ImageURL.Replace("~/", "../../") : this.ImageURL, ToolTip = this.Text };
                            if (!string.IsNullOrEmpty(this.OnClickJs))
                            {
                                ClientAPI.AddButtonConfirm(colIcon, this.OnClickJs);
                            }

                            colIcon.CommandName = this.CommandName;
                            colIcon.DataBinding += this.ItemDataBinding;
                            container.Controls.Add(colIcon);
                        }

                        if (!string.IsNullOrEmpty(this.Text) && !this.ShowImage)
                        {
                            var colLink = new LinkButton { ToolTip = this.Text };
                            if (!string.IsNullOrEmpty(this.OnClickJs))
                            {
                                ClientAPI.AddButtonConfirm(colLink, this.OnClickJs);
                            }

                            colLink.CommandName = this.CommandName;
                            colLink.Text = this.Text;
                            colLink.DataBinding += this.ItemDataBinding;
                            container.Controls.Add(colLink);
                        }
                    }

                    break;
                case GridItemType.Footer:
                case GridItemType.Header:
                    container.Controls.Add(new LiteralControl(this.Text));
                    break;
            }
        }

        public IOrderedDictionary ExtractValues(Control container)
        {
            // do nothing we don't really support databinding
            // but the telerik grid trys to databind to all template columns regardless
            return new OrderedDictionary();
        }

        /// <summary>
        /// Gets whether theButton is visible.
        /// </summary>
        ///     <param name="container">The parent container (DataGridItem).</param>
        private bool GetIsVisible(GridItem container)
        {
            if (!string.IsNullOrEmpty(this.VisibleField))
            {
                return Convert.ToBoolean(DataBinder.Eval(container.DataItem, this.VisibleField));
            }

            return this.Visible;
        }

        /// <summary>
        /// Gets the value of the key.
        /// </summary>
        ///     <param name="container">The parent container (DataGridItem).</param>
        private int GetValue(GridItem container)
        {
            int keyValue = Null.NullInteger;
            if (!string.IsNullOrEmpty(this.KeyField))
            {
                keyValue = Convert.ToInt32(DataBinder.Eval(container.DataItem, this.KeyField));
            }

            return keyValue;
        }

        /// <summary>
        /// Item_DataBinding runs when an Item of type GridItemType.Item is being data-bound.
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <param name="sender"> The object that triggers the event.</param>
        /// <param name="e">An EventArgs object.</param>
        private void ItemDataBinding(object sender, EventArgs e)
        {
            GridItem container;
            int keyValue;
            if (this.EditMode == ImageCommandColumnEditMode.URL)
            {
                var hypLink = (HyperLink)sender;
                container = (GridItem)hypLink.NamingContainer;
                keyValue = this.GetValue(container);
                if (!string.IsNullOrEmpty(this.NavigateURLFormatString))
                {
                    hypLink.NavigateUrl = string.Format(this.NavigateURLFormatString, keyValue);
                }
                else
                {
                    hypLink.NavigateUrl = keyValue.ToString(CultureInfo.InvariantCulture);
                }
            }
            else
            {
                // Bind Image Button
                if (!string.IsNullOrEmpty(this.ImageURL) && this.ShowImage)
                {
                    var colIcon = (ImageButton)sender;
                    container = (GridItem)colIcon.NamingContainer;
                    keyValue = this.GetValue(container);
                    colIcon.CommandArgument = keyValue.ToString(CultureInfo.InvariantCulture);
                    colIcon.Visible = this.GetIsVisible(container);
                }

                if (!string.IsNullOrEmpty(this.Text) && !this.ShowImage)
                {
                    // Bind Link Button
                    var colLink = (LinkButton)sender;
                    container = (GridItem)colLink.NamingContainer;
                    keyValue = this.GetValue(container);
                    colLink.CommandArgument = keyValue.ToString(CultureInfo.InvariantCulture);
                    colLink.Visible = this.GetIsVisible(container);
                }
            }
        }
    }
}
