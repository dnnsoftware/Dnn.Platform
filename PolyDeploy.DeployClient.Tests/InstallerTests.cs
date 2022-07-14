namespace PolyDeploy.DeployClient.Tests
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Net;
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
            var options = TestHelpers.CreateDeployInput(targetUri.ToString(), Guid.NewGuid().ToString());

            var handler = new FakeMessageHandler(
                new Uri(targetUri, "/DesktopModules/PolyDeploy/API/Remote/CreateSession"),
                new HttpResponseMessage(System.Net.HttpStatusCode.OK) { Content = new StringContent(JsonSerializer.Serialize(new { Guid = expectedSessionId })), });
            var client = new HttpClient(handler);

            var installer = new Installer(client);

            var sessionId = await installer.StartSessionAsync(options);

            handler.Request.ShouldNotBeNull();
            handler.Request.ShouldHaveApiKeyHeader(options.ApiKey);

            sessionId.ShouldBe(expectedSessionId);
        }

        [Fact]
        public async Task StartSessionAsync_CallsSessionPostApi_NotFound_ThrowsHttpRequestException()
        {
            var targetUri = new Uri("https://polydeploy.example.com/");
            var options = TestHelpers.CreateDeployInput(targetUri.ToString(), Guid.NewGuid().ToString());

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
            var options = TestHelpers.CreateDeployInput(targetUri.ToString(), Guid.NewGuid().ToString());

            var handler = new FakeMessageHandler(
                new Uri(targetUri, $"/DesktopModules/PolyDeploy/API/Remote/AddPackages?sessionGuid={sessionId}"),
                null);
            var client = new HttpClient(handler);

            var installer = new Installer(client);

            await installer.UploadPackageAsync(options, sessionId, new MemoryStream(Encoding.UTF8.GetBytes("XYZ")), "Jamestown_install_5.5.7.zip");

            handler.Request.ShouldNotBeNull();
            handler.Request.Method.ShouldBe(HttpMethod.Post);
            handler.Request.ShouldHaveApiKeyHeader(options.ApiKey);
            var formContent = handler.Request.Content.ShouldBeOfType<MultipartFormDataContent>();
            var innerContent = formContent.ShouldHaveSingleItem();
            (await innerContent.ReadAsStringAsync()).ShouldBe("XYZ");
        }

        [Fact]
        public async Task InstallPackagesAsync_DoesGetInstall()
        {
            var sessionId = Guid.NewGuid().ToString().Replace("-", string.Empty);
            var targetUri = new Uri("https://polydeploy.example.com/");
            var options = TestHelpers.CreateDeployInput(targetUri.ToString(), Guid.NewGuid().ToString());

            var handler = new FakeMessageHandler(
                new Uri(targetUri, $"/DesktopModules/PolyDeploy/API/Remote/Install?sessionGuid={sessionId}"),
                null);
            var client = new HttpClient(handler);

            var installer = new Installer(client);

            await installer.InstallPackagesAsync(options, sessionId);

            handler.Request.ShouldNotBeNull();
            handler.Request.Method.ShouldBe(HttpMethod.Get);
            handler.Request.ShouldHaveApiKeyHeader(options.ApiKey);
        }

        [Fact]
        public async Task GetSessionAsync_DeserializesInProgressResponse()
        {
            var sessionId = Guid.NewGuid().ToString().Replace("-", string.Empty);
            var targetUri = new Uri("https://polydeploy.example.com/");
            var options = TestHelpers.CreateDeployInput(targetUri.ToString());

            var response = @"{
                ""SessionID"":219,
                ""Guid"":""1F85AA2130F94D038397B54C93C461DA"",
                ""Status"":1,
                ""Response"":""{
                    \""0\"":{
                        \""Name\"":\""zip\"",
                        \""Packages\"":[{
                            \""Name\"":\""pack\"",
                            \""Dependencies\"":[{
                                \""IsPackageDependency\"":true,
                                \""PackageName\"":\""dep\"",
                                \""DependencyVersion\"":\""2.2.2\""
                            }],
                            \""VersionStr\"":\""1.1.1\"",
                            \""CanInstall\"":true
                        }],
                        \""Failures\"":[\""err\""],
                        \""Attempted\"":false,
                        \""Success\"":false,
                        \""CanInstall\"":true
                    }
                }"",
                ""LastUsed"":""2022-02-03T16:31:57.953""
            }".Replace('\r', ' ').Replace('\n', ' ');

            var handler = new FakeMessageHandler(
                new Uri(targetUri, $"/DesktopModules/PolyDeploy/API/Remote/GetSession?sessionGuid={sessionId}"),
                new HttpResponseMessage(System.Net.HttpStatusCode.OK) { Content = new StringContent(response), });
            var client = new HttpClient(handler);

            var installer = new Installer(client);

            var session = await installer.GetSessionAsync(options, sessionId);

            handler.Request.ShouldNotBeNull();
            handler.Request.Method.ShouldBe(HttpMethod.Get);

            session.Status.ShouldBe(SessionStatus.InProgess);
            var responseItem = session.Responses.ShouldHaveSingleItem();
            responseItem.Key.ShouldBe(0);
            var sessionResponse = responseItem.Value.ShouldNotBeNull();
            sessionResponse.Name.ShouldBe("zip");
            var failure = sessionResponse.Failures.ShouldHaveSingleItem();
            failure.ShouldBe("err");
            var package = sessionResponse.Packages.ShouldHaveSingleItem();
            package.ShouldNotBeNull();
            package.Name.ShouldBe("pack");
            var dependency = package.Dependencies.ShouldHaveSingleItem();
            dependency.ShouldNotBeNull();
            dependency.IsPackageDependency.ShouldBeTrue();
            dependency.PackageName.ShouldBe("dep");
            dependency.DependencyVersion.ShouldBe("2.2.2");
            package.VersionStr.ShouldBe("1.1.1");
            package.CanInstall.ShouldBeTrue();
            sessionResponse.Attempted.ShouldBeFalse();
            sessionResponse.Success.ShouldBeFalse();
            sessionResponse.CanInstall.ShouldBeTrue();
        }

        private class FakeMessageHandler : HttpMessageHandler
        {
            public FakeMessageHandler(Uri uri, HttpResponseMessage? response)
            {
                this.Uri = uri;
                this.Response = response;
            }

            public Uri Uri { get; set; }
            public HttpRequestMessage? Request { get; set; }
            public HttpResponseMessage? Response { get; set; }

            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                this.Request = request;
                request.RequestUri.ShouldBe(this.Uri);

                return Task.FromResult(this.Response ?? new HttpResponseMessage(HttpStatusCode.NoContent));
            }
        }
    }
}
