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
            Assert.IsInstanceOf<EmptyResult>(result);
        }

        public static void IsUnauthorized(ActionResult result)
        {
            Assert.IsInstanceOf<HttpUnauthorizedResult>(result);
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
            Assert.IsNotNull(castResult);
            return castResult;
        }

        private static void StringsEqualOrBothNullOrEmpty(string expected, string actual)
        {
            if (string.IsNullOrEmpty(expected))
            {
                Assert.IsTrue(string.IsNullOrEmpty(actual));
            }
            else
            {
                Assert.AreEqual(expected, actual);
            }
        }
    }
}
