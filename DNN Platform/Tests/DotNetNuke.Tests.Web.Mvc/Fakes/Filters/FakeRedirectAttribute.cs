// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Tests.Web.Mvc.Fakes.Filters
{
    using System.Web.Mvc;

    public abstract class FakeRedirectAttribute : ActionFilterAttribute
    {
        public static ActionResult Result = new EmptyResult();
    }
}
