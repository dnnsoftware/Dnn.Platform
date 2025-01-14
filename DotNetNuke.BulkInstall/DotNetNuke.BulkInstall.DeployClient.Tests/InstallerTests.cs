﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Tests.BulkInstall.DeployClient;

using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text.Json;

using DotNetNuke.BulkInstall.DeployClient;

public class InstallerTests
{
    public static TheoryData<bool, string> ApiData => new()
    {
        { true, "/DesktopModules/PolyDeploy/API/Remote/" },
        { false, "/DesktopModules/BulkInstall/API/Remote/" },
    };

    [Theory]
    [MemberData(nameof(ApiData))]
    public async Task StartSessionAsync_CallsCreateSessionApi(bool legacyApi, string baseUri)
    {
        var expectedSessionId = Guid.NewGuid().ToString().Replace("-", string.Empty);
        var targetUri = new Uri("https://polydeploy.example.com/");
        var options = TestHelpers.CreateDeployInput(targetUri.ToString(), Guid.NewGuid().ToString(), legacyApi: legacyApi);

        var handler = new FakeMessageHandler(
            new Uri(targetUri, baseUri + "CreateSession"),
            new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(JsonSerializer.Serialize(new { Guid = expectedSessionId })), });
        var installer = CreateInstaller(handler);

        var sessionId = await installer.StartSessionAsync(options);

        handler.Request.ShouldNotBeNull();
        handler.Request.ShouldHaveApiKeyHeader(options.ApiKey);

        sessionId.ShouldBe(expectedSessionId);
    }

    [Theory]
    [MemberData(nameof(ApiData))]
    public async Task StartSessionAsync_CallsSessionPostApi_NotFound_ThrowsHttpRequestException(bool legacyApi, string baseUri)
    {
        var targetUri = new Uri("https://polydeploy.example.com/");
        var options = TestHelpers.CreateDeployInput(targetUri.ToString(), Guid.NewGuid().ToString(), legacyApi: legacyApi);

        var handler = new FakeMessageHandler(
            new Uri(targetUri, baseUri + "CreateSession"),
            new HttpResponseMessage(HttpStatusCode.NotFound));

        var installer = CreateInstaller(handler);

        var exception = await Should.ThrowAsync<InstallerException>(() => installer.StartSessionAsync(options));
        exception.Message.ShouldBe("An Error Occurred While Starting the Deployment Session");
        exception.InnerException.ShouldBeAssignableTo<HttpRequestException>();
    }

    [Theory]
    [MemberData(nameof(ApiData))]
    public async Task UploadPackageAsync_CallsAddPackagesPostApiUsesCorrectSessionId(bool legacyApi, string baseUri)
    {
        var sessionId = Guid.NewGuid().ToString().Replace("-", string.Empty);
        var targetUri = new Uri("https://polydeploy.example.com/");
        var options = TestHelpers.CreateDeployInput(targetUri.ToString(), Guid.NewGuid().ToString(), legacyApi: legacyApi);

        var handler = new FakeMessageHandler(
            new Uri(targetUri, $"{baseUri}AddPackages?sessionGuid={sessionId}"),
            null);
        var installer = CreateInstaller(handler);

        var result = installer.UploadPackage(options, sessionId, new MemoryStream("XYZ"u8.ToArray()), "Jamestown_install_5.5.7.zip");
        await result.UploadTask;

        handler.Request.ShouldNotBeNull();
        handler.Request.Method.ShouldBe(HttpMethod.Post);
        handler.Request.ShouldHaveApiKeyHeader(options.ApiKey);
        var formContent = handler.Request.Content.ShouldBeOfType<MultipartFormDataContent>();
        var innerContent = formContent.ShouldHaveSingleItem();
        var disposition = innerContent.Headers.ContentDisposition.ShouldNotBeNull();
        disposition.FileName.ShouldBe("Jamestown_install_5.5.7.zip");
        (await innerContent.ReadAsStringAsync()).ShouldBe("XYZ");
    }

    [Theory]
    [MemberData(nameof(ApiData))]
    public async Task UploadPackageAsync_CallsAddPackagesPostApiUsesCorrectFileName(bool legacyApi, string baseUri)
    {
        var sessionId = Guid.NewGuid().ToString().Replace("-", string.Empty);
        var targetUri = new Uri("https://polydeploy.example.com/");
        var options = TestHelpers.CreateDeployInput(targetUri.ToString(), Guid.NewGuid().ToString(), legacyApi: legacyApi);

        var handler = new FakeMessageHandler(
            new Uri(targetUri, $"{baseUri}AddPackages?sessionGuid={sessionId}"),
            null);
        var installer = CreateInstaller(handler);

        var result = installer.UploadPackage(options, sessionId, new MemoryStream("XYZ"u8.ToArray()), @"modules\Jamestown_install_5.5.7.zip");
        await result.UploadTask;

        var formContent = handler.Request.ShouldNotBeNull().Content.ShouldBeOfType<MultipartFormDataContent>();
        var innerContent = formContent.ShouldHaveSingleItem();
        var disposition = innerContent.Headers.ContentDisposition.ShouldNotBeNull();
        disposition.FileName.ShouldBe("Jamestown_install_5.5.7.zip");
    }

    [Theory]
    [MemberData(nameof(ApiData))]
    public async Task UploadPackageAsync_WhenApiErrors_ThrowWrappedException(bool legacyApi, string baseUri)
    {
        var sessionId = Guid.NewGuid().ToString().Replace("-", string.Empty);
        var targetUri = new Uri("https://polydeploy.example.com/");
        var options = TestHelpers.CreateDeployInput(targetUri.ToString(), Guid.NewGuid().ToString(), legacyApi: legacyApi);

        var handler = new FakeMessageHandler(
            new Uri(targetUri, $"{baseUri}AddPackages?sessionGuid={sessionId}"),
            new HttpResponseMessage(HttpStatusCode.InternalServerError));
        var installer = CreateInstaller(handler);

        var exception = await Should.ThrowAsync<InstallerException>(() =>
        {
            return installer.UploadPackage(
                options,
                sessionId,
                new MemoryStream("XYZ"u8.ToArray()),
                "Jamestown_install_5.5.7.zip").UploadTask;
        });

        exception.Message.ShouldBe("An Error Occurred While Uploading the Packages");
    }

    [Theory]
    [MemberData(nameof(ApiData))]
    public async Task InstallPackagesAsync_WhenAPIErrors_ThrowsWrappedException(bool legacyApi, string baseUri)
    {
        var sessionId = Guid.NewGuid().ToString().Replace("-", string.Empty);
        var targetUri = new Uri("https://polydeploy.example.com/");
        var options = TestHelpers.CreateDeployInput(targetUri.ToString(), Guid.NewGuid().ToString(), legacyApi: legacyApi);

        var handler = new FakeMessageHandler(
            new Uri(targetUri, $"{baseUri}Install?sessionGuid={sessionId}"),
            new HttpResponseMessage(HttpStatusCode.InternalServerError));
        var installer = CreateInstaller(handler);

        var exception = await Should.ThrowAsync<InstallerException>(() => installer.InstallPackagesAsync(options, sessionId));

        exception.Message.ShouldBe("An Error Occurred While Installing the Packages");
    }

    [Theory]
    [MemberData(nameof(ApiData))]
    public async Task InstallPackagesAsync_DoesGetInstall(bool legacyApi, string baseUri)
    {
        var sessionId = Guid.NewGuid().ToString().Replace("-", string.Empty);
        var targetUri = new Uri("https://polydeploy.example.com/");
        var options = TestHelpers.CreateDeployInput(targetUri.ToString(), Guid.NewGuid().ToString(), legacyApi: legacyApi);

        var handler = new FakeMessageHandler(
            new Uri(targetUri, $"{baseUri}Install?sessionGuid={sessionId}"),
            null);
        var installer = CreateInstaller(handler);

        await installer.InstallPackagesAsync(options, sessionId);

        handler.Request.ShouldNotBeNull();
        handler.Request.Method.ShouldBe(HttpMethod.Get);
        handler.Request.ShouldHaveApiKeyHeader(options.ApiKey);
    }

    [Theory]
    [MemberData(nameof(ApiData))]
    public async Task GetSessionAsync_DeserializesInProgressResponse(bool legacyApi, string baseUri)
    {
        var sessionId = Guid.NewGuid().ToString().Replace("-", string.Empty);
        var targetUri = new Uri("https://polydeploy.example.com/");
        var options = TestHelpers.CreateDeployInput(targetUri.ToString(), legacyApi: legacyApi);

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
            new Uri(targetUri, $"{baseUri}GetSession?sessionGuid={sessionId}"),
            new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(response), });

        var installer = CreateInstaller(handler);

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

    [Theory]
    [MemberData(nameof(ApiData))]
    public async Task GetSessionAsync_TimeoutResponse_Exception(bool legacyApi, string baseUri)
    {
        var sessionId = Guid.NewGuid().ToString().Replace("-", string.Empty);
        var targetUri = new Uri("https://polydeploy.example.com/");
        var options = TestHelpers.CreateDeployInput(targetUri.ToString(), installationStatusTimeout: 5, legacyApi: legacyApi);
        var stopwatch = new TestStopwatch(TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(2), TimeSpan.FromSeconds(6));

        var handler = new FakeMessageHandler(
            new Uri(targetUri, $"{baseUri}GetSession?sessionGuid={sessionId}"),
            new HttpResponseMessage(HttpStatusCode.NotFound));
        var installer = CreateInstaller(handler, stopwatch);

        var exception = await Should.ThrowAsync<InstallerException>(() => installer.GetSessionAsync(options, sessionId));
        exception.Message.ShouldBe("An Error Occurred Getting the Status of the Deployment Session");
        exception.InnerException.ShouldBeAssignableTo<HttpRequestException>();
        handler.Requests.Count.ShouldBe(3);
    }

    [Theory]
    [MemberData(nameof(ApiData))]
    public async Task GetSessionAsync_GoodResponseAfterNotFound_Succeeds(bool legacyApi, string baseUri)
    {
        var sessionId = Guid.NewGuid().ToString().Replace("-", string.Empty);
        var targetUri = new Uri("https://polydeploy.example.com/");
        var options = TestHelpers.CreateDeployInput(targetUri.ToString(), installationStatusTimeout: 5, legacyApi: legacyApi);
        var stopwatch = new TestStopwatch(TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(2), TimeSpan.FromSeconds(6));

        var handler = new FakeMessageHandler(
            new Uri(targetUri, $"{baseUri}GetSession?sessionGuid={sessionId}"),
            new HttpResponseMessage(HttpStatusCode.NotFound));
        handler.Responses.Enqueue(new HttpResponseMessage(HttpStatusCode.NotFound));
        handler.Responses.Enqueue(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(@"{""Status"":1,""Response"":null}") });
        var installer = CreateInstaller(handler, stopwatch);

        await Should.NotThrowAsync(() => installer.GetSessionAsync(options, sessionId));
        handler.Requests.Count.ShouldBe(3);
    }

    [Theory]
    [MemberData(nameof(ApiData))]
    public async Task GetSessionAsync_GoodResponseAfterHttpException_Succeeds(bool legacyApi, string baseUri)
    {
        var sessionId = Guid.NewGuid().ToString().Replace("-", string.Empty);
        var targetUri = new Uri("https://polydeploy.example.com/");
        var options = TestHelpers.CreateDeployInput(targetUri.ToString(), installationStatusTimeout: 5, legacyApi: legacyApi);
        var stopwatch = new TestStopwatch(TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(2), TimeSpan.FromSeconds(6));

        var handler = new FakeMessageHandler(
            new Uri(targetUri, $"{baseUri}GetSession?sessionGuid={sessionId}"),
            new HttpResponseMessage(HttpStatusCode.NotFound));
        handler.ExceptionsBeforeResponses.Enqueue(new HttpRequestException("An error occurred while sending the request.", new IOException("Unable to read the data from the transport connection: An existing connection was forcibly closed by the remote host.", new SocketException())));
        handler.Responses.Enqueue(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(@"{""Status"":1,""Response"":null}") });
        var installer = CreateInstaller(handler, stopwatch);

        await Should.NotThrowAsync(() => installer.GetSessionAsync(options, sessionId));
        handler.Requests.Count.ShouldBe(2);
    }

    [Theory]
    [MemberData(nameof(ApiData))]
    public async Task UploadPackageAsync_DoesNotRetryAfterFailure(bool legacyApi, string baseUri)
    {
        var sessionId = Guid.NewGuid().ToString().Replace("-", string.Empty);
        var targetUri = new Uri("https://polydeploy.example.com/");
        var options = TestHelpers.CreateDeployInput(targetUri.ToString(), installationStatusTimeout: 5, legacyApi: legacyApi);
        var stopwatch = new TestStopwatch(TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(2), TimeSpan.FromSeconds(6));

        var handler = new FakeMessageHandler(
            new Uri(targetUri, $"{baseUri}AddPackages?sessionGuid={sessionId}"),
            new HttpResponseMessage(HttpStatusCode.NotFound));
        handler.Responses.Enqueue(new HttpResponseMessage(HttpStatusCode.NotFound));
        handler.Responses.Enqueue(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(@"{""Status"":1,""Response"":null}") });
        var installer = CreateInstaller(handler, stopwatch);

        await Should.ThrowAsync<InstallerException>(() =>
        {
            return installer.UploadPackage(
                options,
                sessionId,
                new MemoryStream("XYZ"u8.ToArray()),
                "Jamestown_install_5.5.7.zip").UploadTask;
        });
        handler.Requests.Count.ShouldBe(1);
    }

    [Theory]
    [MemberData(nameof(ApiData))]
    public async Task UploadPackageAsync_TracksUploadProgress(bool legacyApi, string baseUri)
    {
        var sessionId = Guid.NewGuid().ToString().Replace("-", string.Empty);
        var targetUri = new Uri("https://polydeploy.example.com/");
        var options = TestHelpers.CreateDeployInput(targetUri.ToString(), installationStatusTimeout: 5, legacyApi: legacyApi);
        var stopwatch = new TestStopwatch(TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(2), TimeSpan.FromSeconds(6));

        var handler = new FakeMessageHandler(
            new Uri(targetUri, $"{baseUri}AddPackages?sessionGuid={sessionId}"),
            new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("""{"Status":1,"Response":null}""") });
        var installer = CreateInstaller(handler, stopwatch);

        var progressResults = new List<double>();

        var buffer = new byte[500_000];

        var upload = installer.UploadPackage(
            options,
            sessionId,
            new MemoryStream(buffer),
            "Jamestown_install_5.5.7.zip");

        upload.OnProgress += (_, progress) => { progressResults.Add(progress); };
        await upload.UploadTask;

        progressResults.Last().ShouldBe(1);
        progressResults.Count.ShouldBeGreaterThan(1);
    }

    [Theory]
    [MemberData(nameof(ApiData))]
    public async Task StartSessionAsync_SetsUserAgentHeader(bool legacyApi, string baseUri)
    {
        var expectedSessionId = Guid.NewGuid().ToString().Replace("-", string.Empty);
        var targetUri = new Uri("https://polydeploy.example.com/");
        var options = TestHelpers.CreateDeployInput(targetUri.ToString(), Guid.NewGuid().ToString(), legacyApi: legacyApi);

        var handler = new FakeMessageHandler(
            new Uri(targetUri, baseUri + "CreateSession"),
            new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(JsonSerializer.Serialize(new { Guid = expectedSessionId })), });
        var installer = CreateInstaller(handler);

        var sessionId = await installer.StartSessionAsync(options);

        var request = handler.Request.ShouldNotBeNull();
        var userAgent = request.Headers.UserAgent.ShouldHaveSingleItem();
        var product = userAgent.Product.ShouldNotBeNull();
        product.Name.ShouldBe("PolyDeploy");
        product.Version.ShouldBe(Assembly.GetExecutingAssembly().GetCustomAttribute<AssemblyInformationalVersionAttribute>()!.InformationalVersion.Replace(" ", "_"));

        sessionId.ShouldBe(expectedSessionId);
    }

    private static Installer CreateInstaller(HttpMessageHandler? messageHandler = null, IStopwatch? stopwatch = null)
    {
        messageHandler ??= new FakeMessageHandler(A.Dummy<Uri>(), null);
        return new Installer(
            new HttpClient(messageHandler),
            stopwatch ?? new TestStopwatch());
    }

    private class FakeMessageHandler : HttpMessageHandler
    {
        public FakeMessageHandler(Uri uri, HttpResponseMessage? response)
        {
            this.Uri = uri;
            this.Response = response;
            if (response != null)
            {
                this.Responses.Enqueue(response);
            }
        }

        public Uri Uri { get; set; }

        public HttpRequestMessage? Request { get; set; }

        public HttpResponseMessage? Response { get; set; }

        public List<HttpRequestMessage> Requests { get; } = new List<HttpRequestMessage>();

        public Queue<HttpResponseMessage> Responses { get; } = new Queue<HttpResponseMessage>();

        public Queue<Exception> ExceptionsBeforeResponses { get; } = new Queue<Exception>();

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (this.ExceptionsBeforeResponses.TryDequeue(out var exception))
            {
                throw exception;
            }

            this.Request = await CloneRequest(request);
            this.Requests.Add(this.Request);
            request.RequestUri.ShouldBe(this.Uri);

            if (!this.Responses.TryDequeue(out var response))
            {
                response = this.Response;
            }

            return response ?? new HttpResponseMessage(HttpStatusCode.NoContent);
        }

        private static async Task<HttpRequestMessage> CloneRequest(HttpRequestMessage httpRequestMessage)
        {
            var httpRequestMessageClone = new HttpRequestMessage(httpRequestMessage.Method, httpRequestMessage.RequestUri);
            if (httpRequestMessage.Content != null)
            {
                if (httpRequestMessage.Content is MultipartFormDataContent formContent)
                {
                    var contentClone = new MultipartFormDataContent();
                    httpRequestMessageClone.Content = contentClone;
                    foreach (var content in formContent)
                    {
                        contentClone.Add(await CloneContent(content), content.Headers.ContentDisposition?.Name ?? string.Empty, content.Headers.ContentDisposition?.FileName ?? string.Empty);
                    }
                }
                else
                {
                    var contentClone = await CloneContent(httpRequestMessage.Content);
                    httpRequestMessageClone.Content = contentClone;
                    foreach (var header in httpRequestMessage.Content.Headers)
                    {
                        httpRequestMessageClone.Content.Headers.Add(header.Key, header.Value);
                    }
                }
            }

            httpRequestMessageClone.Version = httpRequestMessage.Version;

            foreach (var props in httpRequestMessage.Options)
            {
                httpRequestMessageClone.Options.Set(new HttpRequestOptionsKey<object?>(props.Key), props.Value);
            }

            foreach (var header in httpRequestMessage.Headers)
            {
                httpRequestMessageClone.Headers.TryAddWithoutValidation(header.Key, header.Value);
            }

            return httpRequestMessageClone;
        }

        private static async Task<StreamContent> CloneContent(HttpContent httpContent)
        {
            var ms = new MemoryStream();
            await httpContent.CopyToAsync(ms);
            ms.Position = 0;
            return new StreamContent(ms);
        }
    }
}
