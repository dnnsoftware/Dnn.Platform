// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Tests.Utilities
{
    using System;
    using System.Collections.Generic;

    using NUnit.Framework;

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
            ElementsMatch(expected, actual, matcher, string.Empty, new string[0]);
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
