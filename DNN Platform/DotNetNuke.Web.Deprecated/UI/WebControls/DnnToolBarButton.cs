// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
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
