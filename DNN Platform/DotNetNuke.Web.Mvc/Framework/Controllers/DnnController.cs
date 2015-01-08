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
using DotNetNuke.Web.Mvc.Framework.ActionResults;
using DotNetNuke.Web.Mvc.Framework.Modules;
using DotNetNuke.Web.Mvc.Routing;

namespace DotNetNuke.Web.Mvc.Framework.Controllers
{
    public abstract class DnnController : DnnControllerBase, IDnnController
    {
        private readonly ResultCapturingActionInvoker _actionInvoker;

        protected DnnController()
        {
            _actionInvoker = new ResultCapturingActionInvoker();
            ActionInvoker = _actionInvoker;
        }

        protected override RedirectToRouteResult RedirectToAction(string actionName, string controllerName, RouteValueDictionary routeValues)
        {

            return RedirectToRoute(actionName, controllerName, routeValues, false);
        }

        protected override RedirectToRouteResult RedirectToActionPermanent(string actionName, string controllerName, RouteValueDictionary routeValues)
        {
            return RedirectToRoute(actionName, controllerName, routeValues, true);
        }

        protected internal RedirectToRouteResult RedirectToDefaultRoute()
        {
            return new DnnRedirecttoRouteResult(String.Empty, String.Empty, String.Empty, null, false);
        }

        protected override RedirectToRouteResult RedirectToRoute(string routeName, RouteValueDictionary routeValues)
        {
            return new DnnRedirecttoRouteResult(String.Empty, String.Empty, routeName, routeValues, false);
        }

        private DnnRedirecttoRouteResult RedirectToRoute(string actionName, string controllerName, RouteValueDictionary routeValues, bool permanent)
        {
            var values = RouteData != null
                ? RouteValuesHelpers.MergeRouteValues(actionName, controllerName, RouteData.Values, routeValues, true)
                : RouteValuesHelpers.MergeRouteValues(actionName, controllerName, null, routeValues, true);

            var result = new DnnRedirecttoRouteResult(actionName, controllerName, String.Empty, values, false);

            return result;
        }

        protected override RedirectToRouteResult RedirectToRoutePermanent(string routeName, RouteValueDictionary routeValues)
        {
            return new DnnRedirecttoRouteResult(String.Empty, String.Empty, routeName, routeValues, true);
        }

        public ActionResult ResultOfLastExecute
        {
            get { return _actionInvoker.ResultOfLastInvoke; }
        }

    }
}