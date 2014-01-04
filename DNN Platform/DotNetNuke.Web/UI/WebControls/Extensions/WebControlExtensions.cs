using System;
using System.Drawing;
using System.Linq;
using System.Web.UI.WebControls;

namespace DotNetNuke.Web.UI.WebControls.Extensions
{
    public static class WebControlExtensions
    {
        public static void AddCssClass(this WebControl control, string cssClass)
        {
            if (string.IsNullOrEmpty(control.CssClass))
            {
                control.CssClass = cssClass;
            }
            else
            {
                var cssClasses = control.CssClass.Split(' ');
                var classExists = cssClasses.Any(@class => @class == cssClass);

                if (!classExists)
                {
                    control.CssClass += " " + cssClass;
                }
            }
        }

        public static void RemoveCssClass(this WebControl control, string cssClass)
        {
            if (!string.IsNullOrEmpty(control.CssClass))
            {
                var cssClasses = control.CssClass.Split(' ');
                control.CssClass = string.Join(" ", cssClasses.Where(@class => @class != cssClass).ToArray());
            }
        }

        public static Orientation Orientation(this Size size)
        {
            return size.Width > size.Height ?
                System.Web.UI.WebControls.Orientation.Horizontal : System.Web.UI.WebControls.Orientation.Vertical;
        }

    }

}
