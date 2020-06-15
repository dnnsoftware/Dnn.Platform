// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.Mvc.Framework.ActionFilters
{
    using System;
    using System.Web.Mvc;

    using DotNetNuke.Services.Exceptions;
    using DotNetNuke.Web.Mvc.Framework.Controllers;

    public class DnnHandleErrorAttribute : HandleErrorAttribute
    {
        public override void OnException(ExceptionContext filterContext)
        {
            var controller = filterContext.Controller as IDnnController;

            if (controller == null)
            {
                throw new InvalidOperationException("This attribute can only be applied to Controllers that implement IDnnController");
            }

            this.LogException(filterContext.Exception);
        }

        protected virtual void LogException(Exception exception)
        {
            Exceptions.LogException(exception);
        }
    }
}
