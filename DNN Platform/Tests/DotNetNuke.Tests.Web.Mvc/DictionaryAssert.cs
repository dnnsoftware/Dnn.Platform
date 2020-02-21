// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
using System.Collections.Generic;
using System.Web.Routing;
using NUnit.Framework;

namespace DotNetNuke.Tests.Web.Mvc
{
    public static class DictionaryAssert
    {
        public static void ContainsEntries(object expected, IDictionary<string, object> actual)
        {
            ContainsEntries(new RouteValueDictionary(expected), actual);
        }

        public static void ContainsEntries(IDictionary<string, object> expected, IDictionary<string, object> actual)
        {
            foreach (KeyValuePair<string, object> pair in expected)
            {
                Assert.IsTrue(actual.ContainsKey(pair.Key));
                Assert.AreEqual(pair.Value, actual[pair.Key]);
            }
        }
    }
}
