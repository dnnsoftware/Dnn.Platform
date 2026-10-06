// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.Mvc.Framework.ActionFilters
{
    using System;
    using System.Globalization;
    using System.Reflection;
    using System.Threading.Tasks;
    using System.Web.Mvc;

    using DotNetNuke.Entities.Modules.Actions;
    using DotNetNuke.Framework;
    using DotNetNuke.Web.Mvc.Framework.Controllers;

    public class ModuleActionItemsAttribute : ActionFilterAttribute
    {
        private const string MethodNameTemplate = "Get{0}Actions";

        public Type Type { get; set; }

        public string MethodName { get; set; }

        /// <inheritdoc />
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var controller = filterContext.Controller as IDnnController;
            var result = this.InvokeMethod(filterContext, controller, false);
            if (result is ModuleActionCollection moduleActions)
            {
                controller.ModuleActions = moduleActions;
            }
        }

        public async Task OnActionExecutingAsync(ActionExecutingContext filterContext)
        {
            var controller = filterContext.Controller as IDnnController;
            var result = this.InvokeMethod(filterContext, controller, true);
            if (result is ModuleActionCollection moduleActions)
            {
                controller.ModuleActions = moduleActions;
            }
            else if (result is Task<ModuleActionCollection> taskResult)
            {
                controller.ModuleActions = await taskResult;
            }
        }

        private static MethodInfo GetMethod(Type type, string methodName, bool supportsAsync)
        {
            var method = type.GetMethod(methodName);

            if (method == null)
            {
                throw new NotImplementedException($"The expected method to get the module actions cannot be found. Type: {type.FullName}, Method: {methodName}");
            }

            var returnType = method.ReturnType;
            if (returnType == typeof(ModuleActionCollection) || (supportsAsync && returnType == typeof(Task<ModuleActionCollection>)))
            {
                return method;
            }

            throw new InvalidOperationException("The method must return an object of type ModuleActionCollection");
        }

        private object InvokeMethod(ActionExecutingContext filterContext, IDnnController controller, bool supportsAsync)
        {
            Type type;
            string methodName;

            if (controller == null)
            {
                throw new InvalidOperationException("This attribute can only be applied to Controllers that implement IDnnController");
            }

            object instance;

            if (this.Type == null)
            {
                type = filterContext.Controller.GetType();
                instance = controller;
            }
            else
            {
                type = this.Type;
                instance = Reflection.CreateInstance(type);
            }

            if (string.IsNullOrEmpty(this.MethodName))
            {
                methodName = string.Format(CultureInfo.InvariantCulture, MethodNameTemplate, filterContext.ActionDescriptor.ActionName);
            }
            else
            {
                methodName = this.MethodName;
            }

            var method = GetMethod(type, methodName, supportsAsync);

            return method.Invoke(instance, null);
        }
    }
}
