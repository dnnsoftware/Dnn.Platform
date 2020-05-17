// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
using System;
using System.Collections.Generic;

using NUnit.Framework;

namespace DotNetNuke.Tests.Utilities
{
    public static class EnumerableAssert
    {
        public static void ElementsAreEqual<T>(IEnumerable<T> expected, IEnumerable<T> actual)
        {
            ElementsMatch(expected, actual, (e, a) => Equals(e, a));
        }

        public static void ElementsAreEqual<T>(IEnumerable<T> expected, IEnumerable<T> actual, string message)
        {
            ElementsMatch(expected, actual, (e, a) => Equals(e, a), message);
        }

        public static void ElementsAreEqual<T>(IEnumerable<T> expected, IEnumerable<T> actual, string message, params object[] args)
        {
            ElementsMatch(expected, actual, (e, a) => Equals(e, a), message, args);
        }

        public static void ElementsMatch<TExpected, TActual>(IEnumerable<TExpected> expected, IEnumerable<TActual> actual, Func<TExpected, TActual, bool> matcher)
        {
            ElementsMatch(expected, actual, matcher, String.Empty, new string[0]);
        }

        public static void ElementsMatch<TExpected, TActual>(IEnumerable<TExpected> expected, IEnumerable<TActual> actual, Func<TExpected, TActual, bool> matcher, string message)
        {
            ElementsMatch(expected, actual, matcher, message, new string[0]);
        }

        public static void ElementsMatch<TExpected, TActual>(IEnumerable<TExpected> expected, IEnumerable<TActual> actual, Func<TExpected, TActual, bool> matcher, string message, params object[] args)
        {
            IEnumerator<TExpected> expectedEnumerator = expected.GetEnumerator();
            IEnumerator<TActual> actualEnumerator = actual.GetEnumerator();

            while (actualEnumerator.MoveNext())
            {
                Assert.IsTrue(expectedEnumerator.MoveNext());
                Assert.IsTrue(matcher(expectedEnumerator.Current, actualEnumerator.Current), message, args);
            }
        }
    }
}
