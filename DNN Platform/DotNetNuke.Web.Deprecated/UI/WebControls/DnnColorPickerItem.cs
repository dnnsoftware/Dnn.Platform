// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Web.UI.WebControls
{
    using System;
    using System.Drawing;

    using Telerik.Web.UI;

    [Obsolete("Telerik support will be removed in DNN Platform 10.0.0.  You will need to find an alternative solution")]
    public class DnnColorPickerItem : ColorPickerItem
    {
        public DnnColorPickerItem()
        {
        }

        public DnnColorPickerItem(Color value)
            : base(value)
        {
        }

        public DnnColorPickerItem(Color value, string title)
            : base(value, title)
        {
        }
    }
}
