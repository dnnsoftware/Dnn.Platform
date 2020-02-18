// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
namespace DotNetNuke.Web.UI.WebControls
{
    public class DnnFieldLabel : DnnLabel
    {
        public override void LocalizeStrings()
        {
            base.LocalizeStrings();
            Text = Text + Utilities.GetLocalizedString("FieldSuffix.Text");
        }
    }
}
