// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Tests.Web.Mvc
{
    using System;
    using System.Web.Mvc;
    using System.Web.Routing;

    using DotNetNuke.Web.Mvc.Framework.ActionResults;
    using NUnit.Framework;

    public static class ResultAssert
    {
        public static void IsEmpty(ActionResult result)
        {
            Assert.That(result, Is.InstanceOf<EmptyResult>());
        }

        public static void IsUnauthorized(ActionResult result)
        {
            Assert.That(result, Is.InstanceOf<HttpUnauthorizedResult>());
        }

        public static void IsView(ActionResult result, string viewName)
        {
            IsView(result, viewName, string.Empty, new RouteValueDictionary());
        }

        public static void IsView(ActionResult result, string viewName, string masterName, RouteValueDictionary expectedViewData)
        {
            ViewResult viewResult = result.AssertCast<ViewResult>();
            StringsEqualOrBothNullOrEmpty(viewName, viewResult.ViewName);
            StringsEqualOrBothNullOrEmpty(viewName, viewResult.ViewName);

            DictionaryAssert.ContainsEntries(expectedViewData, viewResult.ViewData);
        }

        private static TCast AssertCast<TCast>(this ActionResult result)
            where TCast : class
        {
            var castResult = result as TCast;
            Assert.That(castResult, Is.Not.Null);
            return castResult;
        }

        private static void StringsEqualOrBothNullOrEmpty(string expected, string actual)
        {
            if (string.IsNullOrEmpty(expected))
            {
                Assert.That(string.IsNullOrEmpty(actual), Is.True);
            }
            else
            {
                Assert.That(actual, Is.EqualTo(expected));
            }
        }
    }
}
