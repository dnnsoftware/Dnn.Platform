// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
using System.Web.Mvc;

namespace DotNetNuke.Tests.Web.Mvc.Fakes.Filters
{
    public abstract class FakeRedirectAttribute : ActionFilterAttribute
    {
        public static ActionResult Result = new EmptyResult();
    }
}
