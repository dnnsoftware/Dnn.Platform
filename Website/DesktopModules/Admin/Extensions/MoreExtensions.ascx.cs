#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2014
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
#region Usings

using System;
using System.Web.UI;

using DotNetNuke.Common;
using DotNetNuke.Framework;
using DotNetNuke.UI.Modules;
using DotNetNuke.Services.Localization;
using DotNetNuke.Web.Client.ClientResourceManagement;

#endregion

namespace DotNetNuke.Modules.Admin.Extensions
{
    public partial class MoreExtensions : ModuleUserControlBase
    {

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            jQuery.RequestRegistration();
            ClientResourceManager.RegisterScript(this.Page, "~/Resources/Shared/Scripts/jquery/jquery.tmpl.js");
            ClientResourceManager.RegisterScript(this.Page, "~/Resources/Shared/Scripts/json2.js");
            ClientResourceManager.RegisterScript(this.Page, "~/DesktopModules/Admin/Extensions/Scripts/Gallery.js");
        }

        protected bool IsDebugEnabled()
        {
            return (System.Web.HttpContext.Current != null && System.Web.HttpContext.Current.IsDebuggingEnabled);
        }

    }
}