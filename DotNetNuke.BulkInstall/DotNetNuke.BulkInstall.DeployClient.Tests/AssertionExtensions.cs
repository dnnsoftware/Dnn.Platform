// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Tests.BulkInstall.DeployClient
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http;
    using Shouldly;

    public static class AssertionExtensions
    {
        public static void ShouldHaveApiKeyHeader(this HttpRequestMessage request, string apiKey)
        {
            request.Headers.TryGetValues("x-api-key", out var apiKeys).ShouldBeTrue();
            apiKeys.ShouldHaveSingleItem().ShouldBe(apiKey);
        }

        public static void ShouldContainStringsInOrder(this string str, params string[] stringsToMatch)
        {
            str.ShouldContainStringsInOrder(onlyOnce: false, stringsToMatch);
        }

        public static void ShouldContainStringsInOrder(this string str, bool onlyOnce, params string[] stringsToMatch)
        {
            var lastIndex = 0;
            var matchedIndexes = new HashSet<int>();
            foreach (var stringToMatch in stringsToMatch)
            {
                str.Substring(lastIndex).ShouldContain(stringToMatch);
                lastIndex = str.IndexOf(stringToMatch, lastIndex, StringComparison.Ordinal);

                matchedIndexes.Add(lastIndex);

                lastIndex += stringToMatch.Length;
            }

            if (onlyOnce)
            {
                foreach (var stringToMatch in stringsToMatch)
                {
                    var allIndexes = str.FindAllIndexesOf(stringToMatch).ToList();
                    matchedIndexes.IsSupersetOf(allIndexes).ShouldBeTrue(customMessage: $"{stringToMatch} was found more than once.");
                }
            }
        }

        public static void ShouldNotContainStringsInOrder(this string str, params string[] stringsToMatch)
        {
            var lastIndex = 0;
            foreach (var stringToMatch in stringsToMatch)
            {
                if (!str.Substring(lastIndex).Contains(stringToMatch))
                {
                    return;
                }
                lastIndex = str.IndexOf(stringToMatch, lastIndex, StringComparison.Ordinal);
                lastIndex += stringToMatch.Length;
            }

            str.ShouldNotContain(stringsToMatch.Last());
        }

        private static IEnumerable<int> FindAllIndexesOf(this string str, string stringToMatch)
        {
            var lastIndex = 0;

            while (lastIndex != -1)
            {
                lastIndex = str.IndexOf(stringToMatch, lastIndex, StringComparison.Ordinal);

                if (lastIndex == -1)
                {
                    break;
                }

                yield return lastIndex;
                lastIndex += stringToMatch.Length;
            }
        }
    }
}
