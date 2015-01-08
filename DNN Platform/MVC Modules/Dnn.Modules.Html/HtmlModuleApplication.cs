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

using System.Web.Routing;
using DotNetNuke.Web.Mvc.Framework.Modules;
using DotNetNuke.Web.Mvc.Routing;

namespace DotNetNuke.Web.Mvc.Html
{
    public class HtmlModuleApplication : ModuleApplication
    {
        public const string HtmlControllersNamespace = " DotNetNuke.Web.Mvc.Html.Controllers";

        public override string DefaultActionName
        {
            get { return "Index"; }
        }

        public override string DefaultControllerName
        {
            get { return "Html"; }
        }

        protected override string FolderPath
        {
            get { return "Html"; }
        }

        public override string ModuleName
        {
            get { return "DNN_HTML"; }
        }

        protected override void RegisterRoutes(RouteCollection routes)
        {
            routes.RegisterDefaultRoute(DefaultControllerName, new[] { HtmlControllersNamespace });
        }

    }
}
