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
