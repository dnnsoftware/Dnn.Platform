// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
using System;
using System.Collections.Generic;
using System.Web.Mvc;

namespace DotNetNuke.Web.Mvc.Framework.Modules
{
    public class ResultCapturingActionInvoker : ControllerActionInvoker
    {
        public ActionResult ResultOfLastInvoke { get; set; }
        protected override ActionExecutedContext InvokeActionMethodWithFilters(ControllerContext controllerContext, IList<IActionFilter> filters, ActionDescriptor actionDescriptor, IDictionary<string, object> parameters)
        {
            var context = base.InvokeActionMethodWithFilters(controllerContext, filters, actionDescriptor, parameters);
            ResultOfLastInvoke = context.Result;
            return context;
        }

        protected override ExceptionContext InvokeExceptionFilters(ControllerContext controllerContext, IList<IExceptionFilter> filters, Exception exception)
        {            
            var context = base.InvokeExceptionFilters(controllerContext, filters, exception);
            ResultOfLastInvoke = context.Result;            
            return context;
        }
        
        protected override void InvokeActionResult(ControllerContext controllerContext, ActionResult actionResult)
        {
            //Do not invoke the action.  Instead, store it for later retrieval            
            if(ResultOfLastInvoke == null) ResultOfLastInvoke = actionResult;
        }
    }
}
