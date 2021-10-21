namespace PolyDeploy.DeployClient.Tests
{
    using System;
    using System.Net.Http;
    using System.Text.Json;
    using System.Threading;
    using System.Threading.Tasks;
    using Shouldly;
    using Xunit;

    public class InstallerTests
    {
        [Fact]
        public async Task StartSessionAsync_CallsCreateSessionApi()
        {
            var expectedSessionId = Guid.NewGuid().ToString().Replace("-", string.Empty);
            var targetUri = new Uri("https://polydeploy.example.com/");
            var options = new DeployInput(targetUri.ToString());

            var handler = new FakeMessageHandler(
                new Uri(targetUri, "/DesktopModules/PolyDeploy/API/Remote/CreateSession"),
                new HttpResponseMessage(System.Net.HttpStatusCode.OK) { Content = new StringContent(JsonSerializer.Serialize(new { Guid = expectedSessionId })), });
            var client = new HttpClient(handler);

            var installer = new Installer(client);

            var sessionId = await installer.StartSessionAsync(options);

            sessionId.ShouldBe(expectedSessionId);
        }

        [Fact]
        public async Task StartSessionAsync_CallsSessionPostApi_NotFound_ThrowsHttpRequestException()
        {
            var expectedSessionId = Guid.NewGuid().ToString().Replace("-", string.Empty);
            var targetUri = new Uri("https://polydeploy.example.com/");
            var options = new DeployInput(targetUri.ToString());

            var handler = new FakeMessageHandler(
                new Uri(targetUri, "/DesktopModules/PolyDeploy/API/Remote/CreateSession"),
                new HttpResponseMessage(System.Net.HttpStatusCode.NotFound));
            var client = new HttpClient(handler);

            var installer = new Installer(client);

            await Should.ThrowAsync<HttpRequestException>(() => installer.StartSessionAsync(options));
        }

        private class FakeMessageHandler : HttpMessageHandler
        {
            public FakeMessageHandler(Uri uri, HttpResponseMessage response)
            {
                this.Uri = uri;
                this.Response = response;
            }

            public Uri Uri { get; set; }
            public HttpRequestMessage? Request { get; set; }
            public HttpResponseMessage Response { get; set; }

            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                this.Request = request;
                request.RequestUri.ShouldBe(this.Uri);
                return Task.FromResult(this.Response);
            }
        }
    }
}
