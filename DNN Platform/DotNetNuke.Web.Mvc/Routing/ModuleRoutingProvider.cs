#region Copyright
// 
// DotNetNuke® - http://www.dnnsoftware.com
// Copyright (c) 2002-2017
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
using System.Web.Routing;
using DotNetNuke.ComponentModel;
using DotNetNuke.Entities.Modules;
using DotNetNuke.UI.Modules;

namespace DotNetNuke.Web.Mvc.Routing
{
    public abstract class ModuleRoutingProvider
    {
        public static ModuleRoutingProvider Instance()
        {
            var component = ComponentFactory.GetComponent<ModuleRoutingProvider>();

            if (component == null)
            {
                component = new StandardModuleRoutingProvider();
                ComponentFactory.RegisterComponentInstance<ModuleRoutingProvider>(component);
            }

            return component;
        }

        public abstract string GenerateUrl(RouteValueDictionary routeValues, ModuleInstanceContext moduleContext);

        public abstract RouteData GetRouteData(HttpContextBase httpContext, ModuleControlInfo moduleControl);
    }
}
