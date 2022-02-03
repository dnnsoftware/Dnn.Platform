namespace PolyDeploy.DeployClient.Tests
{
    using System.Net.Http;
    using Shouldly;

    public static class AssertionExtensions
    {
        public static void ShouldHaveApiKeyHeader(this HttpRequestMessage request, string apiKey)
        {
            request.Headers.TryGetValues("x-api-key", out var apiKeys).ShouldBeTrue();
            apiKeys.ShouldHaveSingleItem().ShouldBe(apiKey);
        }
    }
}
