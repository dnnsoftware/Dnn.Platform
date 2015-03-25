#region Copyright
// 
// DotNetNuke® - http://www.dnnsoftware.com
// Copyright (c) 2002-2014
// by DNN Corporation
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
using System.Web.Mvc;
using DotNetNuke.Common;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Tabs;
using DotNetNuke.Entities.Users;
using DotNetNuke.Services.Localization;
using DotNetNuke.UI.Modules;
using DotNetNuke.Web.Mvc.Framework.Controllers;

namespace DotNetNuke.Web.Mvc.Helpers
{
    public class DnnHelper
    {
        public DnnHelper(ViewContext viewContext)
        {
            Requires.NotNull("viewContext", viewContext);

            var controller = viewContext.Controller as IDnnController;

            if (controller == null)
            {
                throw new InvalidOperationException("The DnnHelper class can only be used in Views that inherit from DnnWebViewPage");
            }

            ModuleContext = controller.ModuleContext;
        }

        public ModuleInfo ActiveModule
        {
            get { return (ModuleContext == null) ? null : ModuleContext.Configuration; }
        }

        public TabInfo ActivePage
        {
            get { return (PortalSettings == null) ? null : PortalSettings.ActiveTab; }
        }

        public string LocalResourceFile { get; set; }

        public string LocalizeString(string key)
        {
            return Localization.GetString(key, LocalResourceFile);
        }

        public ModuleInstanceContext ModuleContext { get; set; }

        public PortalSettings PortalSettings
        {
            get { return (ModuleContext == null) ? null : ModuleContext.PortalSettings; }
        }

        public UserInfo User
        {
            get { return (PortalSettings == null) ? null : PortalSettings.UserInfo; }
        }
    }
}