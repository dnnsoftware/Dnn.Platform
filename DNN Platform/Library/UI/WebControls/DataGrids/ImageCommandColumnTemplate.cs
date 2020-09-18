// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.UI.WebControls
{
    using System;
    using System.Web.UI;
    using System.Web.UI.WebControls;

    using DotNetNuke.Common.Utilities;
    using DotNetNuke.UI.Utilities;

    /// -----------------------------------------------------------------------------
    /// Project:    DotNetNuke
    /// Namespace:  DotNetNuke.UI.WebControls
    /// Class:      ImageCommandColumnTemplate
    /// -----------------------------------------------------------------------------
    /// <summary>
    /// The ImageCommandColumnTemplate provides a Template for the ImageCommandColumn.
    /// </summary>
    /// -----------------------------------------------------------------------------
    public class ImageCommandColumnTemplate : ITemplate
    {
        private ImageCommandColumnEditMode mEditMode = ImageCommandColumnEditMode.Command;
        private ListItemType mItemType = ListItemType.Item;
        private bool mShowImage = true;
        private bool mVisible = true;

        public ImageCommandColumnTemplate()
            : this(ListItemType.Item)
        {
        }

        public ImageCommandColumnTemplate(ListItemType itemType)
        {
            this.ItemType = itemType;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the CommandName for the Column.
        /// </summary>
        /// <value>A String.</value>
        /// -----------------------------------------------------------------------------
        public string CommandName { get; set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets a value indicating whether gets or sets the Design Mode of the Column.
        /// </summary>
        /// <value>A Boolean.</value>
        /// -----------------------------------------------------------------------------
        public bool DesignMode { get; set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the CommandName for the Column.
        /// </summary>
        /// <value>A String.</value>
        /// -----------------------------------------------------------------------------
        public ImageCommandColumnEditMode EditMode
        {
            get
            {
                return this.mEditMode;
            }

            set
            {
                this.mEditMode = value;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the URL of the Image.
        /// </summary>
        /// <value>A String.</value>
        /// -----------------------------------------------------------------------------
        public string ImageURL { get; set; }

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
        /// Gets or sets the Key Field that provides a Unique key to the data Item.
        /// </summary>
        /// <value>A String.</value>
        /// -----------------------------------------------------------------------------
        public string KeyField { get; set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the URL of the Link (unless DataBinding through KeyField).
        /// </summary>
        /// <value>A String.</value>
        /// -----------------------------------------------------------------------------
        public string NavigateURL { get; set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the URL Formatting string.
        /// </summary>
        /// <value>A String.</value>
        /// -----------------------------------------------------------------------------
        public string NavigateURLFormatString { get; set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets javascript text to attach to the OnClick Event.
        /// </summary>
        /// <value>A String.</value>
        /// -----------------------------------------------------------------------------
        public string OnClickJS { get; set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets a value indicating whether gets or sets whether an Image is displayed.
        /// </summary>
        /// <remarks>Defaults to True.</remarks>
        /// <value>A Boolean.</value>
        /// -----------------------------------------------------------------------------
        public bool ShowImage
        {
            get
            {
                return this.mShowImage;
            }

            set
            {
                this.mShowImage = value;
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
        /// Gets or sets a value indicating whether an flag that indicates whether the buttons are visible (this is overridden if
        /// the VisibleField is set)
        /// changed.
        /// </summary>
        /// <value>A Boolean.</value>
        /// -----------------------------------------------------------------------------
        public bool Visible
        {
            get
            {
                return this.mVisible;
            }

            set
            {
                this.mVisible = value;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets an flag that indicates whether the buttons are visible.
        /// </summary>
        /// <value>A Boolean.</value>
        /// -----------------------------------------------------------------------------
        public string VisibleField { get; set; }

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
                case ListItemType.EditItem:
                    if (this.EditMode == ImageCommandColumnEditMode.URL)
                    {
                        var hypLink = new HyperLink();
                        hypLink.ToolTip = this.Text;
                        if (!string.IsNullOrEmpty(this.ImageURL) && this.ShowImage)
                        {
                            var img = new Image();
                            if (this.DesignMode)
                            {
                                img.ImageUrl = this.ImageURL.Replace("~/", "../../");
                            }
                            else
                            {
                                img.ImageUrl = this.ImageURL;
                            }

                            hypLink.Controls.Add(img);
                            img.ToolTip = this.Text;
                        }
                        else
                        {
                            hypLink.Text = this.Text;
                        }

                        hypLink.DataBinding += this.Item_DataBinding;
                        container.Controls.Add(hypLink);
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(this.ImageURL) && this.ShowImage)
                        {
                            var colIcon = new ImageButton();
                            if (this.DesignMode)
                            {
                                colIcon.ImageUrl = this.ImageURL.Replace("~/", "../../");
                            }
                            else
                            {
                                colIcon.ImageUrl = this.ImageURL;
                            }

                            colIcon.ToolTip = this.Text;
                            if (!string.IsNullOrEmpty(this.OnClickJS))
                            {
                                ClientAPI.AddButtonConfirm(colIcon, this.OnClickJS);
                            }

                            colIcon.CommandName = this.CommandName;
                            colIcon.DataBinding += this.Item_DataBinding;
                            container.Controls.Add(colIcon);
                        }

                        if (!string.IsNullOrEmpty(this.Text) && !this.ShowImage)
                        {
                            var colLink = new LinkButton();
                            colLink.ToolTip = this.Text;
                            if (!string.IsNullOrEmpty(this.OnClickJS))
                            {
                                ClientAPI.AddButtonConfirm(colLink, this.OnClickJS);
                            }

                            colLink.CommandName = this.CommandName;
                            colLink.Text = this.Text;
                            colLink.DataBinding += this.Item_DataBinding;
                            container.Controls.Add(colLink);
                        }
                    }

                    break;
                case ListItemType.Footer:
                case ListItemType.Header:
                    container.Controls.Add(new LiteralControl(this.Text));
                    break;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets whether theButton is visible.
        /// </summary>
        ///     <param name="container">The parent container (DataGridItem).</param>
        /// -----------------------------------------------------------------------------
        private bool GetIsVisible(DataGridItem container)
        {
            bool isVisible;
            if (!string.IsNullOrEmpty(this.VisibleField))
            {
                isVisible = Convert.ToBoolean(DataBinder.Eval(container.DataItem, this.VisibleField));
            }
            else
            {
                isVisible = this.Visible;
            }

            return isVisible;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets the value of the key.
        /// </summary>
        ///     <param name="container">The parent container (DataGridItem).</param>
        /// -----------------------------------------------------------------------------
        private int GetValue(DataGridItem container)
        {
            int keyValue = Null.NullInteger;
            if (!string.IsNullOrEmpty(this.KeyField))
            {
                keyValue = Convert.ToInt32(DataBinder.Eval(container.DataItem, this.KeyField));
            }

            return keyValue;
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
            if (this.EditMode == ImageCommandColumnEditMode.URL)
            {
                var hypLink = (HyperLink)sender;
                container = (DataGridItem)hypLink.NamingContainer;
                keyValue = this.GetValue(container);
                if (!string.IsNullOrEmpty(this.NavigateURLFormatString))
                {
                    hypLink.NavigateUrl = string.Format(this.NavigateURLFormatString, keyValue);
                }
                else
                {
                    hypLink.NavigateUrl = keyValue.ToString();
                }
            }
            else
            {
                // Bind Image Button
                if (!string.IsNullOrEmpty(this.ImageURL) && this.ShowImage)
                {
                    var colIcon = (ImageButton)sender;
                    container = (DataGridItem)colIcon.NamingContainer;
                    keyValue = this.GetValue(container);
                    colIcon.CommandArgument = keyValue.ToString();
                    colIcon.Visible = this.GetIsVisible(container);
                }

                if (!string.IsNullOrEmpty(this.Text) && !this.ShowImage)
                {
                    // Bind Link Button
                    var colLink = (LinkButton)sender;
                    container = (DataGridItem)colLink.NamingContainer;
                    keyValue = this.GetValue(container);
                    colLink.CommandArgument = keyValue.ToString();
                    colLink.Visible = this.GetIsVisible(container);
                }
            }
        }
    }
}
