// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.Mvc.Framework.Modules
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Web.Mvc;
    using System.Web.Mvc.Async;

    using DotNetNuke.Web.Mvc.Framework.ActionFilters;

    public class ResultCapturingActionInvoker : AsyncControllerActionInvoker
    {
        public ActionResult ResultOfLastInvoke { get; set; }

        /// <inheritdoc />
        protected override IAsyncResult BeginInvokeActionMethodWithFilters(ControllerContext controllerContext, IList<IActionFilter> filters, ActionDescriptor actionDescriptor, IDictionary<string, object> parameters, AsyncCallback callback, object state)
        {
            var moduleActionsFilter = filters.OfType<ModuleActionItemsAttribute>().ToList();
            if (moduleActionsFilter.Count == 0)
            {
                return base.BeginInvokeActionMethodWithFilters(controllerContext, filters, actionDescriptor, parameters, callback, state);
            }

            var tcs = new TaskCompletionSource<bool>(state);
            var filterContext = new ActionExecutingContext(controllerContext, actionDescriptor, parameters);
            var task = Task.CompletedTask;
            foreach (var filter in moduleActionsFilter)
            {
                task = task.ContinueWith(_ => filter.OnActionExecutingAsync(filterContext));
            }

            task.ContinueWith(t =>
            {
                if (t.IsFaulted)
                {
                    tcs.TrySetException(t.Exception.InnerExceptions);
                }
                else if (t.IsCanceled)
                {
                    tcs.TrySetCanceled();
                }
                else
                {
                    tcs.TrySetResult(true);
                }

                IAsyncResult BeginDelegate(AsyncCallback innerCallback, object innerState) => base.BeginInvokeActionMethodWithFilters(controllerContext, [.. filters.Where(f => f is not ModuleActionItemsAttribute)], actionDescriptor, parameters, innerCallback, innerState);
                return Task.Factory.FromAsync(BeginDelegate, ar => callback(ar), state);
            });
            return tcs.Task;
        }

        /// <inheritdoc />
        protected override ActionExecutedContext InvokeActionMethodWithFilters(ControllerContext controllerContext, IList<IActionFilter> filters, ActionDescriptor actionDescriptor, IDictionary<string, object> parameters)
        {
            var context = base.InvokeActionMethodWithFilters(controllerContext, filters, actionDescriptor, parameters);
            this.ResultOfLastInvoke = context.Result;
            return context;
        }

        /// <inheritdoc />
        protected override ActionExecutedContext EndInvokeActionMethodWithFilters(IAsyncResult asyncResult)
        {
            var context = base.EndInvokeActionMethodWithFilters(asyncResult);
            this.ResultOfLastInvoke = context.Result;
            return context;
        }

        /// <inheritdoc />
        protected override ExceptionContext InvokeExceptionFilters(ControllerContext controllerContext, IList<IExceptionFilter> filters, Exception exception)
        {
            var context = base.InvokeExceptionFilters(controllerContext, filters, exception);
            this.ResultOfLastInvoke = context.Result;
            return context;
        }

        /// <inheritdoc />
        protected override void InvokeActionResult(ControllerContext controllerContext, ActionResult actionResult)
        {
            // Do not invoke the action.  Instead, store it for later retrieval
            if (this.ResultOfLastInvoke == null)
            {
                this.ResultOfLastInvoke = actionResult;
            }
        }
    }
}
