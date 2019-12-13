// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
#region Usings

using Telerik.Web.UI;

#endregion

namespace DotNetNuke.Web.UI.WebControls
{
    public class DnnToolBarButton : RadToolBarButton
    {
        public DnnToolBarButton()
        {
        }

        public DnnToolBarButton(string text) : base(text)
        {
        }

        public DnnToolBarButton(string text, bool isChecked, string @group) : base(text, isChecked, @group)
        {
        }
    }
}
