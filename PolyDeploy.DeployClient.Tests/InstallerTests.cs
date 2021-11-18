namespace PolyDeploy.DeployClient.Tests
{
    using System;
    using System.IO;
    using System.Net.Http;
    using System.Text;
    using System.Text.Json;
    using System.Threading;
    using System.Threading.Tasks;
    using FakeItEasy;
    using Shouldly;
    using Xunit;

    public class InstallerTests
    {
        [Fact]
        public async Task StartSessionAsync_CallsCreateSessionApi()
        {
            var expectedSessionId = Guid.NewGuid().ToString().Replace("-", string.Empty);
            var targetUri = new Uri("https://polydeploy.example.com/");
            var options = new DeployInput(targetUri.ToString(), A.Dummy<string>());

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
            var options = new DeployInput(targetUri.ToString(), A.Dummy<string>());

            var handler = new FakeMessageHandler(
                new Uri(targetUri, "/DesktopModules/PolyDeploy/API/Remote/CreateSession"),
                new HttpResponseMessage(System.Net.HttpStatusCode.NotFound));
            var client = new HttpClient(handler);

            var installer = new Installer(client);

            await Should.ThrowAsync<HttpRequestException>(() => installer.StartSessionAsync(options));
        }

        [Fact]
        public async Task UploadPackageAsync_CallsAddPackagesPostApiWithUsesCorrectSessionId()
        {
            var sessionId = Guid.NewGuid().ToString().Replace("-", string.Empty);
            var targetUri = new Uri("https://polydeploy.example.com/");
            var options = new DeployInput(targetUri.ToString(), A.Dummy<string>());

            var handler = new FakeMessageHandler(
                new Uri(targetUri, $"/DesktopModules/PolyDeploy/API/Remote/AddPackages?sessionGuid={sessionId}"),
                new HttpResponseMessage(System.Net.HttpStatusCode.NotFound));
            var client = new HttpClient(handler);

            var installer = new Installer(client);

            await installer.UploadPackageAsync(options, sessionId, new MemoryStream(Encoding.UTF8.GetBytes("XYZ")), "Jamestown_install_5.5.7.zip");

            handler.Request.ShouldNotBeNull();
            handler.Request.Method.ShouldBe(HttpMethod.Post);
            var formContent = handler.Request.Content.ShouldBeOfType<MultipartFormDataContent>();
            var innerContent = formContent.ShouldHaveSingleItem();
            (await innerContent.ReadAsStringAsync()).ShouldBe("XYZ");
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
