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
using System.Web.Routing;

namespace DotNetNuke.Web.Mvc.Helpers
{
    public class DnnUrlHelper : UrlHelper
    {
        public DnnUrlHelper()
        {
        }

        public DnnUrlHelper(RequestContext requestContext)
            : base(requestContext, RouteTable.Routes)
        {
        }

        public DnnUrlHelper(RequestContext requestContext, RouteCollection routeCollection)
            :base(requestContext, routeCollection)
        {
        }

        private string CleanUrl(string url)
        {
            if (!String.IsNullOrEmpty(url))
            {
                return url.Replace("//", "/");
            }
            return url;
        }

        /// <summary>
        /// Generates a string to a fully qualified URL to an action method.
        /// </summary>
        /// 
        /// <returns>
        /// A string to a fully qualified URL to an action method.
        /// </returns>
        public override string Action()
        {
            return CleanUrl(base.Action());
        }

        /// <summary>
        /// Generates a fully qualified URL to an action method by using the specified action name.
        /// </summary>
        /// 
        /// <returns>
        /// The fully qualified URL to an action method.
        /// </returns>
        /// <param name="actionName">The name of the action method.</param>
        public override string Action(string actionName)
        {
            return CleanUrl(base.Action(actionName));
        }

        /// <summary>
        /// Generates a fully qualified URL to an action method by using the specified action name and route values.
        /// </summary>
        /// 
        /// <returns>
        /// The fully qualified URL to an action method.
        /// </returns>
        /// <param name="actionName">The name of the action method.</param><param name="routeValues">An object that contains the parameters for a route. The parameters are retrieved through reflection by examining the properties of the object. The object is typically created by using object initializer syntax.</param>
        public override string Action(string actionName, object routeValues)
        {
            return CleanUrl(base.Action(actionName, routeValues));
        }

        /// <summary>
        /// Generates a fully qualified URL to an action method for the specified action name and route values.
        /// </summary>
        /// 
        /// <returns>
        /// The fully qualified URL to an action method.
        /// </returns>
        /// <param name="actionName">The name of the action method.</param><param name="routeValues">An object that contains the parameters for a route.</param>
        public override string Action(string actionName, RouteValueDictionary routeValues)
        {
            return CleanUrl(base.Action(actionName, routeValues));
        }

        /// <summary>
        /// Generates a fully qualified URL to an action method by using the specified action name and controller name.
        /// </summary>
        /// 
        /// <returns>
        /// The fully qualified URL to an action method.
        /// </returns>
        /// <param name="actionName">The name of the action method.</param><param name="controllerName">The name of the controller.</param>
        public override string Action(string actionName, string controllerName)
        {
            return CleanUrl(base.Action(actionName, controllerName));
        }

        /// <summary>
        /// Generates a fully qualified URL to an action method by using the specified action name, controller name, and route values.
        /// </summary>
        /// 
        /// <returns>
        /// The fully qualified URL to an action method.
        /// </returns>
        /// <param name="actionName">The name of the action method.</param><param name="controllerName">The name of the controller.</param><param name="routeValues">An object that contains the parameters for a route. The parameters are retrieved through reflection by examining the properties of the object. The object is typically created by using object initializer syntax.</param>
        public override string Action(string actionName, string controllerName, object routeValues)
        {
            return CleanUrl(base.Action(actionName, controllerName, routeValues));
        }

        /// <summary>
        /// Generates a fully qualified URL to an action method by using the specified action name, controller name, and route values.
        /// </summary>
        /// 
        /// <returns>
        /// The fully qualified URL to an action method.
        /// </returns>
        /// <param name="actionName">The name of the action method.</param><param name="controllerName">The name of the controller.</param><param name="routeValues">An object that contains the parameters for a route.</param>
        public override string Action(string actionName, string controllerName, RouteValueDictionary routeValues)
        {
            return CleanUrl(base.Action(actionName, controllerName, routeValues));
        }

        /// <summary>
        /// Generates a fully qualified URL for an action method by using the specified action name, controller name, route values, and protocol to use.
        /// </summary>
        /// 
        /// <returns>
        /// The fully qualified URL to an action method.
        /// </returns>
        /// <param name="actionName">The name of the action method.</param><param name="controllerName">The name of the controller.</param><param name="routeValues">An object that contains the parameters for a route.</param><param name="protocol">The protocol for the URL, such as "http" or "https".</param>
        public override string Action(string actionName, string controllerName, RouteValueDictionary routeValues, string protocol)
        {
            return CleanUrl(base.Action(actionName, controllerName, routeValues, protocol));
        }

        /// <summary>
        /// Generates a fully qualified URL to an action method by using the specified action name, controller name, route values, and protocol to use.
        /// </summary>
        /// 
        /// <returns>
        /// The fully qualified URL to an action method.
        /// </returns>
        /// <param name="actionName">The name of the action method.</param><param name="controllerName">The name of the controller.</param><param name="routeValues">An object that contains the parameters for a route. The parameters are retrieved through reflection by examining the properties of the object. The object is typically created by using object initializer syntax.</param><param name="protocol">The protocol for the URL, such as "http" or "https".</param>
        public override string Action(string actionName, string controllerName, object routeValues, string protocol)
        {
            return CleanUrl(base.Action(actionName, controllerName, routeValues, protocol));
        }

        /// <summary>
        /// Generates a fully qualified URL for an action method by using the specified action name, controller name, route values, protocol to use and host name.
        /// </summary>
        /// 
        /// <returns>
        /// The fully qualified URL to an action method.
        /// </returns>
        /// <param name="actionName">The name of the action method.</param><param name="controllerName">The name of the controller.</param><param name="routeValues">An object that contains the parameters for a route.</param><param name="protocol">The protocol for the URL, such as "http" or "https".</param><param name="hostName">The host name for the URL.</param>
        public override string Action(string actionName, string controllerName, RouteValueDictionary routeValues, string protocol, string hostName)
        {
            return CleanUrl(base.Action(actionName, controllerName, routeValues, protocol, hostName));
        }
    }
}
