// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.UI.WebControls.Extensions
{
    using System;
    using System.Drawing;
    using System.Linq;
    using System.Web.UI.WebControls;

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
