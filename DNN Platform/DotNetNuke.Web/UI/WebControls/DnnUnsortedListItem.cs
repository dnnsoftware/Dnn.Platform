using System;
using System.Web.UI;
using System.Web.UI.WebControls;


namespace DotNetNuke.Web.UI.WebControls
{
    /// <summary>
    /// Creates a control that render one item in a list ($lt;li> control).
    /// </summary>
    /// <remarks></remarks>
    public class DnnUnsortedListItem : WebControl
    {

        public DnnUnsortedListItem() : base(HtmlTextWriterTag.Li)
        {
        }

        public void AddControls(params Control[] childControls)
        {
            foreach (var childControl in childControls)
            {
                if (childControl != null)
                {
                    Controls.Add(childControl);
                }
            }
        }

        protected override void AddAttributesToRender(HtmlTextWriter writer)
        {
            writer.AddAttribute(HtmlTextWriterAttribute.Id, ClientID);
            if (!string.IsNullOrEmpty(CssClass))
            {
                writer.AddAttribute(HtmlTextWriterAttribute.Class, CssClass);
            }
            if (!string.IsNullOrEmpty(ToolTip))
            {
                writer.AddAttribute(HtmlTextWriterAttribute.Title, ToolTip);
            }
        }

    }

}
