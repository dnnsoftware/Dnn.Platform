// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.Mvc.Framework.ActionFilters
{
    using System;
    using System.Reflection;
    using System.Web.Mvc;

    using DotNetNuke.Entities.Modules.Actions;
    using DotNetNuke.Framework;
    using DotNetNuke.Web.Mvc.Framework.Controllers;

    public class ModuleActionItemsAttribute : ActionFilterAttribute
    {
        private const string MethodNameTemplate = "Get{0}Actions";

        public Type Type { get; set; }

        public string MethodName { get; set; }

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var controller = filterContext.Controller as IDnnController;
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
                methodName = string.Format(MethodNameTemplate, filterContext.ActionDescriptor.ActionName);
            }
            else
            {
                methodName = this.MethodName;
            }

            var method = this.GetMethod(type, methodName);

            controller.ModuleActions = method.Invoke(instance, null) as ModuleActionCollection;
        }

        private MethodInfo GetMethod(Type type, string methodName)
        {
            var method = type.GetMethod(methodName);

            if (method == null)
            {
                throw new NotImplementedException(string.Format("The expected method to get the module actions cannot be found. Type: {0}, Method: {1}", type.FullName, methodName));
            }

            var returnType = method.ReturnType.FullName;

            if (returnType != "DotNetNuke.Entities.Modules.Actions.ModuleActionCollection")
            {
                throw new InvalidOperationException("The method must return an object of type ModuleActionCollection");
            }

            return method;
        }
    }
}
