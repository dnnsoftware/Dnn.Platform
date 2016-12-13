#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2017
// by DotNetNuke Corporation
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated 
// documentation files (the "Software"), to deal in the Software without restriction, including without limitation 
// the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and 
// to permit persons to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or substantial portions 
// of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED 
// TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL 
// THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF 
// CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
// DEALINGS IN THE SOFTWARE.
#endregion

using System;

using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Controllers;
using DotNetNuke.ExtensionPoints;
using DotNetNuke.UI.Modules;

namespace DotNetNuke.Modules.UrlManagement
{
    public partial class FriendlyUrls : ModuleUserControlBase, IEditPagePanelControlActions
    {
        public void BindAction(int portalId, int tabId, int moduleId)
        {
            basicUrlPanel.Visible = Config.GetFriendlyUrlProvider() != "advanced";
            chkUseFriendlyUrls.Checked = Entities.Host.Host.UseFriendlyUrls;
        }

        public void CancelAction(int portalId, int tabId, int moduleId)
        {
        }

        public void SaveAction(int portalId, int tabId, int moduleId)
        {
            HostController.Instance.Update("UseFriendlyUrls", chkUseFriendlyUrls.Checked ? "Y" : "N", false);
        }
    }
}