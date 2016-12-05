﻿#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2016
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

using System.ComponentModel.Composition;

using DotNetNuke.ExtensionPoints;
using DotNetNuke.Services.Localization;

namespace DotNetNuke.Modules.UrlManagement.Components.ExtensionPoints.UserControls
{
    [Export(typeof(IEditPagePanelExtensionPoint))]
    [ExportMetadata("Module", "HostSettings")]
    [ExportMetadata("Name", "FriendlyUrlsExtensionPoint")]
    [ExportMetadata("Group", "FriendlyUrls")]
    [ExportMetadata("Priority", 2)]
    public class FriendlyUrlsExtensionPoint : IEditPagePanelExtensionPoint
    {
        private const string _localResourceFile = "DesktopModules/Admin/UrlManagement/App_LocalResources/FriendlyUrls.ascx.resx";

        public string EditPagePanelId
        {
            get { return "Panel-FriendlyUrl-Extension"; }
        }

        public string CssClass
        {
            get { return ""; }
        }

        public string UserControlSrc
        {
            get { return "~/DesktopModules/Admin/UrlManagement/FriendlyUrls.ascx"; }
        }

        public string Text
        {
            get { return Localization.GetString("FriendlyUrl", _localResourceFile); }
        }

        public string Icon
        {
            get { return ""; }
        }

        public int Order
        {
            get { return 0; }
        }

        public bool Visible
        {
            get { return true; }
        }
    }
}