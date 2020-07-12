// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.UI.WebControls
{
    using System.Web;
    using System.Web.UI.WebControls;

    using DotNetNuke.Entities.Icons;
    using DotNetNuke.UI.WebControls;
    using Telerik.Web.UI;

    public class DnnGridImageCommandColumn : DnnGridTemplateColumn
    {
        private ImageCommandColumnEditMode _editMode = ImageCommandColumnEditMode.Command;
        private bool _showImage = true;

        private string _imageURL = string.Empty;

        /// <summary>
        /// Gets or sets the CommandName for the Column.
        /// </summary>
        /// <value>A String.</value>
        public string CommandName { get; set; }

        /// <summary>
        /// Gets or sets editMode for the Column.
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
        /// Gets or sets the Icon Key to obtain ImageURL.
        /// </summary>
        /// <value>A String.</value>
        public string IconKey { get; set; }

        /// <summary>
        /// Gets or sets the Icon Siz to obtain ImageURL.
        /// </summary>
        /// <value>A String.</value>
        public string IconSize { get; set; }

        /// <summary>
        /// Gets or sets the Icon Style to obtain ImageURL.
        /// </summary>
        /// <value>A String.</value>
        public string IconStyle { get; set; }

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
        /// Gets or sets an flag that indicates whether the buttons are visible.
        /// </summary>
        /// <value>A Boolean.</value>
        public string VisibleField { get; set; }

        /// <summary>
        /// Initialises the Column.
        /// </summary>
        public override void Initialize()
        {
            this.ItemTemplate = this.CreateTemplate(GridItemType.Item);
            this.EditItemTemplate = this.CreateTemplate(GridItemType.EditItem);
            this.HeaderTemplate = this.CreateTemplate(GridItemType.Header);

            if (HttpContext.Current == null)
            {
                this.HeaderStyle.Font.Names = new[] { "Tahoma, Verdana, Arial" };
                this.HeaderStyle.Font.Size = new FontUnit("10pt");
                this.HeaderStyle.Font.Bold = true;
            }

            this.ItemStyle.HorizontalAlign = HorizontalAlign.Center;
            this.HeaderStyle.HorizontalAlign = HorizontalAlign.Center;
        }

        /// <summary>
        /// Creates a ImageCommandColumnTemplate.
        /// </summary>
        /// <returns>A ImageCommandColumnTemplate.</returns>
        private DnnGridImageCommandColumnTemplate CreateTemplate(GridItemType type)
        {
            bool isDesignMode = HttpContext.Current == null;
            var template = new DnnGridImageCommandColumnTemplate(type);
            if (type != GridItemType.Header)
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
            template.OnClickJs = this.OnClickJs;
            template.ShowImage = this.ShowImage;
            template.Visible = this.Visible;

            template.Text = type == GridItemType.Header ? this.HeaderText : this.Text;

            // Set Design Mode to True
            template.DesignMode = isDesignMode;

            return template;
        }
    }
}
