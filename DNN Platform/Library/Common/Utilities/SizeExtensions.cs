// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
using System.Drawing;
using System.Web.UI.WebControls;

namespace DotNetNuke.Common.Utilities
{

    public static class SizeExtensions
    {
        public static Orientation Orientation(this Size size)
        {
            return size.Width > size.Height
                       ? System.Web.UI.WebControls.Orientation.Horizontal
                       : System.Web.UI.WebControls.Orientation.Vertical;
        }
    }
}
