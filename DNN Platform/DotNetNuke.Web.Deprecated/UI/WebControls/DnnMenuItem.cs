// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
#region Usings

using Telerik.Web.UI;

#endregion

namespace DotNetNuke.Web.UI.WebControls
{
    public class DnnMenuItem : RadMenuItem
    {
        public DnnMenuItem()
        {
        }

        public DnnMenuItem(string text) : base(text)
        {
        }

        public DnnMenuItem(string text, string navigateUrl) : base(text, navigateUrl)
        {
        }
    }
}
