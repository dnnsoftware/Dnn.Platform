// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
#region Usings

using System.Drawing;

using Telerik.Web.UI;

#endregion

namespace DotNetNuke.Web.UI.WebControls
{
    public class DnnColorPickerItem : ColorPickerItem
    {
        public DnnColorPickerItem()
        {
        }

        public DnnColorPickerItem(Color value) : base(value)
        {
        }

        public DnnColorPickerItem(Color value, string title) : base(value, title)
        {
        }
    }
}
