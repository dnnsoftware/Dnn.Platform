// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
#region Usings

using Telerik.Web.UI;

#endregion

namespace DotNetNuke.Web.UI.WebControls
{
    public class DnnTickerItem : RadTickerItem
    {
        public DnnTickerItem()
        {
        }

        public DnnTickerItem(string text) : base(text)
        {
        }

        public DnnTickerItem(string text, string navigateUrl) : base(text, navigateUrl)
        {
        }
    }
}
