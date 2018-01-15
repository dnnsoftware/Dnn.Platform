#region Copyright

// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2018
// by DotNetNuke Corporation
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
