// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
using System.Web.Mvc;

namespace DotNetNuke.Tests.Web.Mvc.Fakes.Filters
{
    public class FakeHandleExceptionRedirectAttribute : FakeRedirectAttribute, IExceptionFilter
    {        
        public static bool IsExceptionHandled { get; set; }
        public void OnException(ExceptionContext filterContext)
        {
            filterContext.Result = Result;
            filterContext.ExceptionHandled = IsExceptionHandled;
        }
    }
}
