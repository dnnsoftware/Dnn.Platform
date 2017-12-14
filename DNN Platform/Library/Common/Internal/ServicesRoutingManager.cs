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
using DotNetNuke.Instrumentation;
using DotNetNuke.Services.Cache;

namespace DotNetNuke.Common.Internal
{
    public static class ServicesRoutingManager
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof (ServicesRoutingManager));

        public static void RegisterServiceRoutes()
        {
            const string unableToRegisterServiceRoutes = "Unable to register service routes";

            try
            {
                //new ServicesRoutingManager().RegisterRoutes();
                var instance = Activator.CreateInstance("DotNetNuke.Web", "DotNetNuke.Web.Api.Internal.ServicesRoutingManager");

                var method = instance.Unwrap().GetType().GetMethod("RegisterRoutes");
                method.Invoke(instance.Unwrap(), new object[0]);

                var instanceMvc = Activator.CreateInstance("DotNetNuke.Web.Mvc", "DotNetNuke.Web.Mvc.Routing.MvcRoutingManager");

                var methodMvc = instanceMvc.Unwrap().GetType().GetMethod("RegisterRoutes");
                methodMvc.Invoke(instanceMvc.Unwrap(), new object[0]);
            }
            catch (Exception e)
            {
                Logger.Error(unableToRegisterServiceRoutes, e);
            }
        }

        public static void ReRegisterServiceRoutesWhileSiteIsRunning()
        {
            //by clearing a "fake" key on the caching provider we can echo this
            //command to all the members of a web farm
            //the caching provider will call to make the actual registration of new routes
            CachingProvider.Instance().Clear("ServiceFrameworkRoutes", "-1");
        }
    }
}