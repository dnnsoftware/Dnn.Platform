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

using System.Web;
using DotNetNuke.Common;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Tabs;
using DotNetNuke.Entities.Users;
using DotNetNuke.Web.Mvc.Framework.Modules;

namespace DotNetNuke.Web.Mvc.Framework
{
    public class SiteContext
    {
        public SiteContext(HttpContextBase httpContext)
        {
            Requires.NotNull("httpContext", httpContext);
            HttpContext = httpContext;
        }

        public ModuleRequestResult ActiveModuleRequest { get; set; }

        public TabInfo ActivePage { get; set; }

        public PortalInfo ActiveSite { get; set; }

        public PortalAliasInfo ActiveSiteAlias { get; set; }

        public HttpContextBase HttpContext { get; private set; }

        public UserInfo User { get; private set; }

        public void SetUser(UserInfo user)
        {
            User = user;
            HttpContext.Items.Add("UserInfo", user);
        }

        //public string CurrentTheme
        //{
        //    get
        //    {
        //        if (_currentTheme == null && DnnMvcApplication.Container != null)
        //        {
        //            _currentTheme = DnnMvcApplication.Container
        //                                               .GetExportedObjectOrDefault<string>(WebContractNames.AppDefaultTheme);
        //        }
        //        return _currentTheme;
        //    }
        //    set { _currentTheme = value; }
        //}
    }
}