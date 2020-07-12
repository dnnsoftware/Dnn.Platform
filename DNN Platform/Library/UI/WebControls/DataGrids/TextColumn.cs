// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.UI.WebControls
{
    using System.Web;
    using System.Web.UI.WebControls;

    /// -----------------------------------------------------------------------------
    /// Project:    DotNetNuke
    /// Namespace:  DotNetNuke.UI.WebControls
    /// Class:      TextColumn
    /// -----------------------------------------------------------------------------
    /// <summary>
    /// The TextColumn control provides a custom Text Column.
    /// </summary>
    /// -----------------------------------------------------------------------------
    public class TextColumn : TemplateColumn
    {
        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the Data Field is the field that binds to the Text Column.
        /// </summary>
        /// <value>A String.</value>
        /// -----------------------------------------------------------------------------
        public string DataField { get; set; }

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
        /// Initialises the Column.
        /// </summary>
        /// -----------------------------------------------------------------------------
        public override void Initialize()
        {
            this.ItemTemplate = this.CreateTemplate(ListItemType.Item);
            this.EditItemTemplate = this.CreateTemplate(ListItemType.EditItem);
            this.HeaderTemplate = this.CreateTemplate(ListItemType.Header);
            if (HttpContext.Current == null)
            {
                this.ItemStyle.Font.Names = new[] { "Tahoma, Verdana, Arial" };
                this.ItemStyle.Font.Size = new FontUnit("10pt");
                this.ItemStyle.HorizontalAlign = HorizontalAlign.Left;
                this.HeaderStyle.Font.Names = new[] { "Tahoma, Verdana, Arial" };
                this.HeaderStyle.Font.Size = new FontUnit("10pt");
                this.HeaderStyle.Font.Bold = true;
                this.HeaderStyle.HorizontalAlign = HorizontalAlign.Left;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Creates a TextColumnTemplate.
        /// </summary>
        /// <returns>A TextColumnTemplate.</returns>
        /// -----------------------------------------------------------------------------
        private TextColumnTemplate CreateTemplate(ListItemType type)
        {
            bool isDesignMode = false;
            if (HttpContext.Current == null)
            {
                isDesignMode = true;
            }

            var template = new TextColumnTemplate(type);
            if (type != ListItemType.Header)
            {
                template.DataField = this.DataField;
            }

            template.Width = this.Width;
            if (type == ListItemType.Header)
            {
                template.Text = this.HeaderText;
            }
            else
            {
                template.Text = this.Text;
            }

            template.DesignMode = isDesignMode;
            return template;
        }
    }
}
