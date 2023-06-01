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
    public async Task UploadPackageAsync(DeployInput options, string sessionId, Stream encryptedPackage, string packageName)
    {
        try
        {
            var fileName = Path.GetRelativePath(options.PackagesDirectoryPath, packageName);
            var form = new MultipartFormDataContent
            {
                { new StreamContent(encryptedPackage), "none", fileName },
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

        async Task<(HttpResponseMessage?, Exception?)> SendRequest()
        {
            using var request = new HttpRequestMessage
            {
                RequestUri = new Uri(options.GetTargetUri(), "DesktopModules/PolyDeploy/API/Remote/" + path),
                Method = method,
                Content = content,
            };

            request.Headers.Add("x-api-key", options.ApiKey);
            request.Headers.UserAgent.Add(new ProductInfoHeaderValue("PolyDeploy", DeployClientVersion.Replace(" ", "_")));

            try
            {
                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(Math.Max(100, options.InstallationStatusTimeout)));
                return (await this.httpClient.SendAsync(request, cts.Token), null);
            }
            catch (HttpRequestException requestException)
            {
                return (null, requestException);
            }
        }

        var (response, exception) = await SendRequest();
        while (exception != null || response?.IsSuccessStatusCode == false)
        {
            if (options.InstallationStatusTimeout <= this.stopwatch.Elapsed.TotalSeconds || content != null)
            {
                if (exception != null)
                {
                    throw exception;
                }

                Debug.Assert(response != null, nameof(response) + " != null");
                response.EnsureSuccessStatusCode();
            }
            else
            {
                (response, exception) = await SendRequest();
            }
        }

        return response;
    }

    private class ResponseJson
    {
        public string? Response { get; set; }
    }

    private class CreateSessionResponse
    {
        public string? Guid { get; set; }
    }
}
