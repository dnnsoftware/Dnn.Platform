// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.UI.WebControls.Extensions
{
    using System.Drawing;
    using System.Linq;
    using System.Web.UI.WebControls;

    /// <summary>Extension methods for <see cref="WebControl"/> instances.</summary>
    public static class WebControlExtensions
    {
        /// <summary>Adds the <paramref name="cssClass"/> to the <paramref name="control"/>.</summary>
        /// <param name="control">The control.</param>
        /// <param name="cssClass">The CSS class to add.</param>
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

        /// <summary>Removes the <paramref name="cssClass"/> from the <paramref name="control"/>.</summary>
        /// <param name="control">The control.</param>
        /// <param name="cssClass">The CSS class.</param>
        public static void RemoveCssClass(this WebControl control, string cssClass)
        {
            if (!string.IsNullOrEmpty(control.CssClass))
            {
                var cssClasses = control.CssClass.Split(' ');
                control.CssClass = string.Join(" ", cssClasses.Where(@class => @class != cssClass).ToArray());
            }
        }

        /// <summary>Gets the orientation of the <paramref name="size"/>.</summary>
        /// <param name="size">The size.</param>
        /// <returns>The orientation.</returns>
        public static Orientation Orientation(this Size size)
        {
            return size.Width > size.Height ?
                System.Web.UI.WebControls.Orientation.Horizontal : System.Web.UI.WebControls.Orientation.Vertical;
        }
    }
}
