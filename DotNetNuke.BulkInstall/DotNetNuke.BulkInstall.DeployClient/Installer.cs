// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.BulkInstall.DeployClient;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text.Json;

/// <summary>The <see cref="IInstaller"/> implementation, using <see cref="HttpClient"/>, with retries based on <see cref="DeployInput.InstallationStatusTimeout"/>.</summary>
public class Installer : IInstaller
{
    private static readonly string DeployClientVersion = Assembly.GetExecutingAssembly().GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion ?? string.Empty;
    private readonly HttpClient httpClient;
    private readonly IStopwatch stopwatch;

    /// <summary>Initializes a new instance of the <see cref="Installer"/> class.</summary>
    /// <param name="httpClient">The HTTP client.</param>
    /// <param name="stopwatch">The stopwatch.</param>
    public Installer(HttpClient httpClient, IStopwatch stopwatch)
    {
        this.httpClient = httpClient;
        this.stopwatch = stopwatch;
    }

    /// <inheritdoc/>
    public async Task<Session> GetSessionAsync(DeployInput options, string sessionId)
    {
        try
        {
            using var response = await this.SendRequestAsync(options, HttpMethod.Get, $"GetSession?sessionGuid={sessionId}");

            var responseString = await response.Content.ReadAsStringAsync();
            var responseBody = JsonSerializer.Deserialize<Session>(responseString);
            if (responseBody == null)
            {
                throw new InvalidOperationException("Received an empty response trying to get a PolyDeploy session");
            }

            var responseJson = JsonSerializer.Deserialize<ResponseJson>(responseString);
            if (!string.IsNullOrWhiteSpace(responseJson?.Response))
            {
                responseBody.Responses = JsonSerializer.Deserialize<SortedList<int, SessionResponse?>>(responseJson.Response);
            }

            return responseBody;
        }
        catch (Exception ex)
        {
            throw new InstallerException("An Error Occurred Getting the Status of the Deployment Session", ex);
        }
    }

    /// <inheritdoc/>
    public async Task InstallPackagesAsync(DeployInput options, string sessionId)
    {
        try
        {
            using var response = await this.SendRequestAsync(options, HttpMethod.Get, $"Install?sessionGuid={sessionId}");
        }
        catch (Exception e)
        {
            throw new InstallerException("An Error Occurred While Installing the Packages", e);
        }
    }

    /// <inheritdoc/>
    public async Task<string> StartSessionAsync(DeployInput options)
    {
        try
        {
            using var response = await this.SendRequestAsync(options, HttpMethod.Get, "CreateSession");
            var responseStream = await response.Content.ReadAsStreamAsync();
            var responseBody = await JsonSerializer.DeserializeAsync<CreateSessionResponse>(responseStream);
            if (string.IsNullOrWhiteSpace(responseBody?.Guid))
            {
                throw new InvalidOperationException("Received an empty response trying to create PolyDeploy session");
            }

            return responseBody.Guid;
        }
        catch (Exception e)
        {
            throw new InstallerException("An Error Occurred While Starting the Deployment Session", e);
        }
    }

    /// <inheritdoc/>
    public UploadPackageResult UploadPackage(
        DeployInput options,
        string sessionId,
        Stream encryptedPackage,
        string packageName)
    {
        UploadPackageResult? result = null;
        var task = this.UploadPackageImplementationAsync(
            options,
            sessionId,
            encryptedPackage,
            packageName,
            onProgress: progress => result?.TriggerProgress(progress));
        result = new UploadPackageResult(task, packageName, encryptedPackage);
        return result;
    }

    private async Task UploadPackageImplementationAsync(
        DeployInput options,
        string sessionId,
        Stream encryptedPackage,
        string packageName,
        Action<double>? onProgress = null)
    {
        try
        {
            var fileName = Path.GetFileName(packageName);
            var form = new MultipartFormDataContent
            {
                { new StreamContent(new ProgressStream(encryptedPackage, onRead: onProgress)), "none", fileName },
            };

            using var response = await this.SendRequestAsync(options, HttpMethod.Post, $"AddPackages?sessionGuid={sessionId}", form);
        }
        catch (Exception e)
        {
            throw new InstallerException("An Error Occurred While Uploading the Packages", e);
        }
    }

    private async Task<HttpResponseMessage> SendRequestAsync(DeployInput options, HttpMethod method, string path, HttpContent? content = null)
    {
        this.stopwatch.StartNew();

        var result = await SendRequest();
        while (result.Exception != null || result.Response.IsSuccessStatusCode == false)
        {
            if (options.InstallationStatusTimeout <= this.stopwatch.Elapsed.TotalSeconds || content != null)
            {
                if (result.Exception != null)
                {
                    throw result.Exception;
                }

                result.Response.EnsureSuccessStatusCode();
            }
            else
            {
                result = await SendRequest();
            }
        }

        return result.Response;

        async Task<WebResult> SendRequest()
        {
            var basePath = options.LegacyApi ? "DesktopModules/PolyDeploy/API/Remote/" : "/DesktopModules/BulkInstall/API/Remote/";
            using var request = new HttpRequestMessage();
            request.RequestUri = new Uri(options.GetTargetUri(), basePath + path);
            request.Method = method;
            request.Content = content;

            request.Headers.Add("x-api-key", options.ApiKey);
            request.Headers.UserAgent.Add(new ProductInfoHeaderValue("PolyDeploy", DeployClientVersion.Replace(" ", "_")));

            try
            {
                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(Math.Max(100, options.InstallationStatusTimeout)));
                return new WebResult(await this.httpClient.SendAsync(request, cts.Token));
            }
            catch (HttpRequestException requestException)
            {
                return new WebResult(requestException);
            }
        }
    }

    private class ResponseJson
    {
        public string? Response { get; set; }
    }

    private class CreateSessionResponse
    {
        public string? Guid { get; set; }
    }

    private class WebResult
    {
        private HttpResponseMessage? response;

        public WebResult(HttpResponseMessage response)
        {
            this.response = response;
        }

        public WebResult(HttpRequestException exception)
        {
            this.Exception = exception;
        }

        public HttpResponseMessage Response => this.response ?? throw new InvalidOperationException("Response must have a value.");

        public HttpRequestException? Exception { get; }
    }

    private class ProgressStream(Stream innerStream, Action<double>? onRead = null) : Stream
    {
        public override bool CanRead => innerStream.CanRead;

        public override bool CanSeek => innerStream.CanSeek;

        public override bool CanWrite => innerStream.CanWrite;

        public override long Length => innerStream.Length;

        public double Percentage => (double)this.Position / this.Length;

        public override long Position
        {
            get => innerStream.Position;
            set => innerStream.Position = value;
        }

        public override void Flush() => innerStream.Flush();

        public override int Read(byte[] buffer, int offset, int count)
        {
            var result = innerStream.Read(buffer, offset, count);
            if (result > 0)
            {
                onRead?.Invoke(this.Percentage);
            }

            return result;
        }

        public override long Seek(long offset, SeekOrigin origin) => innerStream.Seek(offset, origin);

        public override void SetLength(long value) => innerStream.SetLength(value);

        public override void Write(byte[] buffer, int offset, int count) => innerStream.Write(buffer, offset, count);

        public override async ValueTask DisposeAsync()
        {
            await innerStream.DisposeAsync();
            await base.DisposeAsync();
        }

        protected override void Dispose(bool disposing)
        {
            innerStream.Dispose();
            base.Dispose(disposing);
        }
    }
}
