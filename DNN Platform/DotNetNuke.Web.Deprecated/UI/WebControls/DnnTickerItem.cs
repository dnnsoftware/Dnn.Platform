﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
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
