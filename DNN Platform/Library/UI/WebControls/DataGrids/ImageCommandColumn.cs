#region Copyright

// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2018
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
            get { return _editMode; }
            set { _editMode = value; }
        }


        /// <summary>
        /// Gets or sets the URL of the Image
        /// </summary>
        /// <value>A String</value>
        public string ImageURL
        {
            get
            {
                if (!string.IsNullOrEmpty(_imageURL))
                {
                    return _imageURL;
                }

                return IconController.IconURL(IconKey, IconSize, IconStyle);
            }
            set { _imageURL = value; }
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
            get { return _showImage; }
            set { _showImage = value; }
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
                template.ImageURL = ImageURL;
                if (!isDesignMode)
                {
                    template.CommandName = CommandName;
                    template.VisibleField = VisibleField;
                    template.KeyField = KeyField;
                }
            }
            template.EditMode = EditMode;
            template.NavigateURL = NavigateURL;
            template.NavigateURLFormatString = NavigateURLFormatString;
            template.OnClickJS = OnClickJS;
            template.ShowImage = ShowImage;
            template.Visible = Visible;

            template.Text = type == ListItemType.Header ? HeaderText : Text;

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
            ItemTemplate = CreateTemplate(ListItemType.Item);
            EditItemTemplate = CreateTemplate(ListItemType.EditItem);
            HeaderTemplate = CreateTemplate(ListItemType.Header);

            if (HttpContext.Current == null)
            {
                HeaderStyle.Font.Names = new[] {"Tahoma, Verdana, Arial"};
                HeaderStyle.Font.Size = new FontUnit("10pt");
                HeaderStyle.Font.Bold = true;
            }
            ItemStyle.HorizontalAlign = HorizontalAlign.Center;
            HeaderStyle.HorizontalAlign = HorizontalAlign.Center;
        }

        #endregion
    }
}