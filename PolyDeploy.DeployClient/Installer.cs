namespace PolyDeploy.DeployClient
{
    using System.Net.Http.Headers;
    using System.Reflection;
    using System;
    using System.Collections.Generic;
    using System.Text.Json;
    
    public class Installer : IInstaller
    {
        private readonly HttpClient httpClient;

        private readonly IStopwatch stopwatch;

        private static readonly string DeployClientVersion = Assembly.GetEntryAssembly()?.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion ?? string.Empty;

        public Installer(HttpClient httpClient, IStopwatch stopwatch)
        {
            this.httpClient = httpClient;
            this.stopwatch = stopwatch;
        }

        public async Task<Session> GetSessionAsync(DeployInput options, string sessionId)
        {
            try
            {
                using var response =
                    await this.SendRequestAsync(options, HttpMethod.Get, $"GetSession?sessionGuid={sessionId}");

                var responseString = await response.Content.ReadAsStringAsync();
                var responseBody = JsonSerializer.Deserialize<Session>(responseString);
                if (responseBody == null)
                {
                    throw new InvalidOperationException(
                        "Received an empty response trying to get a PolyDeploy session");
                }

                var responseJson = JsonSerializer.Deserialize<ResponseJson>(responseString);
                if (!string.IsNullOrWhiteSpace(responseJson?.Response))
                {
                    responseBody.Responses =
                        JsonSerializer.Deserialize<SortedList<int, SessionResponse?>>(responseJson.Response);
                }

                return responseBody;
            }
            catch (Exception ex)
            {
                throw new InstallerException("An Error Occurred Getting the Status of the Deployment Session", ex);
            }
        }

        private class ResponseJson
        {
            public string? Response { get; set; }
        }

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

            async Task<HttpResponseMessage> SendRequest()
            {
                using var request = new HttpRequestMessage
                {
                    RequestUri = new Uri(options.GetTargetUri(), "DesktopModules/PolyDeploy/API/Remote/" + path),
                    Method = method,
                    Content = content,
                };

                request.Headers.Add("x-api-key", options.ApiKey);
                request.Headers.UserAgent.Add(new ProductInfoHeaderValue("PolyDeploy", DeployClientVersion));

                return await this.httpClient.SendAsync(request);
            }

            var response = await SendRequest();
            while (!response.IsSuccessStatusCode)
            {
                if (options.InstallationStatusTimeout <= stopwatch.Elapsed.TotalSeconds || content != null)
                {
                    response.EnsureSuccessStatusCode();
                }
                else
                {
                    response = await SendRequest();
                }
            }
            return response;
        }

        private class CreateSessionResponse
        {
            public string? Guid { get; set; }
        }
    }
}
