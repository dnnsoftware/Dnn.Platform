// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

#region Usings

using System.Web;
using System.Web.UI.WebControls;

#endregion

namespace DotNetNuke.UI.WebControls
{
    /// -----------------------------------------------------------------------------
    /// Project:    DotNetNuke
    /// Namespace:  DotNetNuke.UI.WebControls
    /// Class:      TextColumn
    /// -----------------------------------------------------------------------------
    /// <summary>
    /// The TextColumn control provides a custom Text Column
    /// </summary>
    /// -----------------------------------------------------------------------------
    public class TextColumn : TemplateColumn
    {
 /// -----------------------------------------------------------------------------
 /// <summary>
 /// The Data Field is the field that binds to the Text Column
 /// </summary>
 /// <value>A String</value>
 /// -----------------------------------------------------------------------------
        public string DataField { get; set; }

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

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Creates a TextColumnTemplate
        /// </summary>
        /// <returns>A TextColumnTemplate</returns>
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
                template.DataField = DataField;
            }
            template.Width = Width;
            if (type == ListItemType.Header)
            {
                template.Text = HeaderText;
            }
            else
            {
                template.Text = Text;
            }
            template.DesignMode = isDesignMode;
            return template;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Initialises the Column
        /// </summary>
        /// -----------------------------------------------------------------------------
        public override void Initialize()
        {
            ItemTemplate = CreateTemplate(ListItemType.Item);
            EditItemTemplate = CreateTemplate(ListItemType.EditItem);
            HeaderTemplate = CreateTemplate(ListItemType.Header);
            if (HttpContext.Current == null)
            {
                ItemStyle.Font.Names = new[] {"Tahoma, Verdana, Arial"};
                ItemStyle.Font.Size = new FontUnit("10pt");
                ItemStyle.HorizontalAlign = HorizontalAlign.Left;
                HeaderStyle.Font.Names = new[] {"Tahoma, Verdana, Arial"};
                HeaderStyle.Font.Size = new FontUnit("10pt");
                HeaderStyle.Font.Bold = true;
                HeaderStyle.HorizontalAlign = HorizontalAlign.Left;
            }
        }
    }
}
