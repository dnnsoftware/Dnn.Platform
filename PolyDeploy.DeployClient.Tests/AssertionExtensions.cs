namespace PolyDeploy.DeployClient.Tests
{
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

        public static void ShouldContainStringsInOrder(this string str, IEnumerable<string> stringsToMatch)
        {
            var lastIndex = 0;
            foreach (var stringToMatch in stringsToMatch)
            {
                str.Substring(lastIndex).ShouldContain(stringToMatch);
                lastIndex = str.IndexOf(stringToMatch);
            }
        }
    }
}
