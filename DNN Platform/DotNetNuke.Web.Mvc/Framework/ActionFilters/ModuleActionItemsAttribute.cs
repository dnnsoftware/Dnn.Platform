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

using System;
using System.Reflection;
using System.Web.Mvc;
using DotNetNuke.Entities.Modules.Actions;
using DotNetNuke.Framework;
using DotNetNuke.Web.Mvc.Framework.Controllers;

namespace DotNetNuke.Web.Mvc.Framework.ActionFilters
{
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

            if(controller == null)
            {
                throw new InvalidOperationException("This attribute can only be applied to Controllers that implement IDnnController");
            }

            object instance;

            if (Type == null)
            {
                type = filterContext.Controller.GetType();
                instance = controller;
            }
            else
            {
                type = Type;
                instance = Reflection.CreateInstance(type);
            }

            if (String.IsNullOrEmpty(MethodName))
            {
                methodName = String.Format(MethodNameTemplate, filterContext.ActionDescriptor.ActionName);
            }
            else
            {
                methodName = MethodName;
            }

            var method = GetMethod(type, methodName);

            controller.ModuleActions = method.Invoke(instance, null) as ModuleActionCollection;
        }

        private MethodInfo GetMethod(Type type, string methodName)
        {
            var method = type.GetMethod(methodName);

            if (method == null)
            {
                throw new NotImplementedException(String.Format("The expected method to get the module actions cannot be found. Type: {0}, Method: {1}", type.FullName, methodName));
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
