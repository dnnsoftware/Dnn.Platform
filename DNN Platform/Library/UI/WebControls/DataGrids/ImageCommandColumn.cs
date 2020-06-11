﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

#region Usings

using System;
using System.Web;
using System.Web.UI.WebControls;
using DotNetNuke.Entities.Icons;

#endregion

namespace DotNetNuke.UI.WebControls
{
    /// <summary>
    /// The ImageCommandColumn control provides an Image Command (or Hyperlink) column
    /// for a Data Grid
    /// </summary>
    public class ImageCommandColumn : TemplateColumn
    {
        private ImageCommandColumnEditMode _editMode = ImageCommandColumnEditMode.Command;
        private bool _showImage = true;

        #region "Public Properties"

        private string _imageURL = string.Empty;

        /// <summary>
        /// Gets or sets the CommandName for the Column
        /// </summary>
        /// <value>A String</value>
        public string CommandName { get; set; }


        /// <summary>
        /// EditMode for the Column
        /// </summary>
        /// <value>A String</value>
        public ImageCommandColumnEditMode EditMode
        {
            get { return this._editMode; }
            set { this._editMode = value; }
        }


        /// <summary>
        /// Gets or sets the URL of the Image
        /// </summary>
        /// <value>A String</value>
        public string ImageURL
        {
            get
            {
                if (!string.IsNullOrEmpty(this._imageURL))
                {
                    return this._imageURL;
                }

                return IconController.IconURL(this.IconKey, this.IconSize, this.IconStyle);
            }
            set { this._imageURL = value; }
        }


        /// <summary>
        /// The Icon Key to obtain ImageURL
        /// </summary>
        /// <value>A String</value>
        public string IconKey { get; set; }


        /// <summary>
        /// The Icon Siz to obtain ImageURL
        /// </summary>
        /// <value>A String</value>
        public string IconSize { get; set; }


        /// <summary>
        /// The Icon Style to obtain ImageURL
        /// </summary>
        /// <value>A String</value>
        public string IconStyle { get; set; }


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
// ReSharper disable InconsistentNaming
        public string OnClickJS { get; set; }
// ReSharper restore InconsistentNaming


        /// <summary>
        /// Gets or sets whether an Image is displayed
        /// </summary>
        /// <remarks>Defaults to True</remarks>
        /// <value>A Boolean</value>
        public bool ShowImage
        {
            get { return this._showImage; }
            set { this._showImage = value; }
        }


        /// <summary>
        /// Gets or sets the Text (for Header/Footer Templates)
        /// </summary>
        /// <value>A String</value>
        public string Text { get; set; }


        /// <summary>
        /// An flag that indicates whether the buttons are visible.
        /// </summary>
        /// <value>A Boolean</value>
        public string VisibleField { get; set; }


        /// <summary>
        /// Creates a ImageCommandColumnTemplate
        /// </summary>
        /// <returns>A ImageCommandColumnTemplate</returns>
        private ImageCommandColumnTemplate CreateTemplate(ListItemType type)
        {
            bool isDesignMode = HttpContext.Current == null;
            var template = new ImageCommandColumnTemplate(type);
            if (type != ListItemType.Header)
            {
                template.ImageURL = this.ImageURL;
                if (!isDesignMode)
                {
                    template.CommandName = this.CommandName;
                    template.VisibleField = this.VisibleField;
                    template.KeyField = this.KeyField;
                }
            }
            template.EditMode = this.EditMode;
            template.NavigateURL = this.NavigateURL;
            template.NavigateURLFormatString = this.NavigateURLFormatString;
            template.OnClickJS = this.OnClickJS;
            template.ShowImage = this.ShowImage;
            template.Visible = this.Visible;

            template.Text = type == ListItemType.Header ? this.HeaderText : this.Text;

            //Set Design Mode to True
            template.DesignMode = isDesignMode;

            return template;
        }

        #endregion

        #region "Public Methods"

        /// <summary>
        /// Initialises the Column
        /// </summary>
        public override void Initialize()
        {
            this.ItemTemplate = this.CreateTemplate(ListItemType.Item);
            this.EditItemTemplate = this.CreateTemplate(ListItemType.EditItem);
            this.HeaderTemplate = this.CreateTemplate(ListItemType.Header);

            if (HttpContext.Current == null)
            {
                this.HeaderStyle.Font.Names = new[] {"Tahoma, Verdana, Arial"};
                this.HeaderStyle.Font.Size = new FontUnit("10pt");
                this.HeaderStyle.Font.Bold = true;
            }
            this.ItemStyle.HorizontalAlign = HorizontalAlign.Center;
            this.HeaderStyle.HorizontalAlign = HorizontalAlign.Center;
        }

        #endregion
    }
}
