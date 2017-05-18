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
using DotNetNuke.UI.Utilities;

#endregion

namespace DotNetNuke.UI.WebControls
{
    /// -----------------------------------------------------------------------------
    /// Project:    DotNetNuke
    /// Namespace:  DotNetNuke.UI.WebControls
    /// Class:      ImageCommandColumnTemplate
    /// -----------------------------------------------------------------------------
    /// <summary>
    /// The ImageCommandColumnTemplate provides a Template for the ImageCommandColumn
    /// </summary>
    /// -----------------------------------------------------------------------------
    public class ImageCommandColumnTemplate : ITemplate
    {
		#region "Private Members"

        private ImageCommandColumnEditMode mEditMode = ImageCommandColumnEditMode.Command;
        private ListItemType mItemType = ListItemType.Item;
        private bool mShowImage = true;
        private bool mVisible = true;
		
		#endregion

		#region "Constructors"

        public ImageCommandColumnTemplate() : this(ListItemType.Item)
        {
        }

        public ImageCommandColumnTemplate(ListItemType itemType)
        {
            ItemType = itemType;
        }
		
		#endregion

		#region "Public Properties"

		/// -----------------------------------------------------------------------------
		/// <summary>
		/// Gets or sets the CommandName for the Column
		/// </summary>
		/// <value>A String</value>
		/// -----------------------------------------------------------------------------
        public string CommandName { get; set; }

		/// -----------------------------------------------------------------------------
		/// <summary>
		/// Gets or sets the Design Mode of the Column
		/// </summary>
		/// <value>A Boolean</value>
		/// -----------------------------------------------------------------------------
        public bool DesignMode { get; set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the CommandName for the Column
        /// </summary>
        /// <value>A String</value>
        /// -----------------------------------------------------------------------------
        public ImageCommandColumnEditMode EditMode
        {
            get
            {
                return mEditMode;
            }
            set
            {
                mEditMode = value;
            }
        }

		/// -----------------------------------------------------------------------------
		/// <summary>
		/// Gets or sets the URL of the Image
		/// </summary>
		/// <value>A String</value>
		/// -----------------------------------------------------------------------------
        public string ImageURL { get; set; }

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
		/// The Key Field that provides a Unique key to the data Item
		/// </summary>
		/// <value>A String</value>
		/// -----------------------------------------------------------------------------
        public string KeyField { get; set; }

		/// -----------------------------------------------------------------------------
		/// <summary>
		/// Gets or sets the URL of the Link (unless DataBinding through KeyField)
		/// </summary>
		/// <value>A String</value>
		/// -----------------------------------------------------------------------------
        public string NavigateURL { get; set; }

		/// -----------------------------------------------------------------------------
		/// <summary>
		/// Gets or sets the URL Formatting string
		/// </summary>
		/// <value>A String</value>
		/// -----------------------------------------------------------------------------
        public string NavigateURLFormatString { get; set; }

		/// -----------------------------------------------------------------------------
		/// <summary>
		/// Javascript text to attach to the OnClick Event
		/// </summary>
		/// <value>A String</value>
		/// -----------------------------------------------------------------------------
        public string OnClickJS { get; set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets whether an Image is displayed
        /// </summary>
        /// <remarks>Defaults to True</remarks>
        /// <value>A Boolean</value>
        /// -----------------------------------------------------------------------------
        public bool ShowImage
        {
            get
            {
                return mShowImage;
            }
            set
            {
                mShowImage = value;
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
        /// An flag that indicates whether the buttons are visible (this is overridden if
        /// the VisibleField is set)
        /// changed
        /// </summary>
        /// <value>A Boolean</value>
        /// -----------------------------------------------------------------------------
        public bool Visible
        {
            get
            {
                return mVisible;
            }
            set
            {
                mVisible = value;
            }
        }

		/// -----------------------------------------------------------------------------
		/// <summary>
		/// An flag that indicates whether the buttons are visible.
		/// </summary>
		/// <value>A Boolean</value>
		/// -----------------------------------------------------------------------------
        public string VisibleField { get; set; }

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
                case ListItemType.EditItem:
                    if (EditMode == ImageCommandColumnEditMode.URL)
                    {
                        var hypLink = new HyperLink();
                        hypLink.ToolTip = Text;
                        if (!String.IsNullOrEmpty(ImageURL) && ShowImage)
                        {
                            var img = new Image();
                            if (DesignMode)
                            {
                                img.ImageUrl = ImageURL.Replace("~/", "../../");
                            }
                            else
                            {
                                img.ImageUrl = ImageURL;
                            }
                            hypLink.Controls.Add(img);
                            img.ToolTip = Text;
                        }
                        else
                        {
                            hypLink.Text = Text;
                        }
                        hypLink.DataBinding += Item_DataBinding;
                        container.Controls.Add(hypLink);
                    }
                    else
                    {
                        if (!String.IsNullOrEmpty(ImageURL) && ShowImage)
                        {
                            var colIcon = new ImageButton();
                            if (DesignMode)
                            {
                                colIcon.ImageUrl = ImageURL.Replace("~/", "../../");
                            }
                            else
                            {
                                colIcon.ImageUrl = ImageURL;
                            }
                            colIcon.ToolTip = Text;
                            if (!String.IsNullOrEmpty(OnClickJS))
                            {
                                ClientAPI.AddButtonConfirm(colIcon, OnClickJS);
                            }
                            colIcon.CommandName = CommandName;
                            colIcon.DataBinding += Item_DataBinding;
                            container.Controls.Add(colIcon);
                        }
                        if (!String.IsNullOrEmpty(Text) && !ShowImage)
                        {
                            var colLink = new LinkButton();
                            colLink.ToolTip = Text;
                            if (!String.IsNullOrEmpty(OnClickJS))
                            {
                                ClientAPI.AddButtonConfirm(colLink, OnClickJS);
                            }
                            colLink.CommandName = CommandName;
                            colLink.Text = Text;
                            colLink.DataBinding += Item_DataBinding;
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

        #endregion
		
		#endregion
		
		#region "Private Methods"

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets whether theButton is visible
        /// </summary>
        ///	<param name="container">The parent container (DataGridItem)</param>
        /// -----------------------------------------------------------------------------
        private bool GetIsVisible(DataGridItem container)
        {
            bool isVisible;
            if (!String.IsNullOrEmpty(VisibleField))
            {
                isVisible = Convert.ToBoolean(DataBinder.Eval(container.DataItem, VisibleField));
            }
            else
            {
                isVisible = Visible;
            }
            return isVisible;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets the value of the key
        /// </summary>
        ///	<param name="container">The parent container (DataGridItem)</param>
        /// -----------------------------------------------------------------------------
        private int GetValue(DataGridItem container)
        {
            int keyValue = Null.NullInteger;
            if (!String.IsNullOrEmpty(KeyField))
            {
                keyValue = Convert.ToInt32(DataBinder.Eval(container.DataItem, KeyField));
            }
            return keyValue;
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
                    hypLink.NavigateUrl = keyValue.ToString();
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
                    colIcon.CommandArgument = keyValue.ToString();
                    colIcon.Visible = GetIsVisible(container);
                }
                if (!String.IsNullOrEmpty(Text) && !ShowImage)
                {
					//Bind Link Button
                    var colLink = (LinkButton) sender;
                    container = (DataGridItem) colLink.NamingContainer;
                    keyValue = GetValue(container);
                    colLink.CommandArgument = keyValue.ToString();
                    colLink.Visible = GetIsVisible(container);
                }
            }
        }
		
		#endregion
    }
}
