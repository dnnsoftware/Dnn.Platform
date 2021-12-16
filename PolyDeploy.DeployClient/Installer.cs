namespace PolyDeploy.DeployClient
{
    using System;
    using System.IO;
    using System.Net.Http;
    using System.Text.Json;
    using System.Threading.Tasks;

    public class Installer : IInstaller
    {
        private readonly HttpClient httpClient;

        public Installer(HttpClient httpClient)
        {
            this.httpClient = httpClient;
        }

        public Task<object> GetSessionAsync(DeployInput options, string sessionId)
        {
            throw new System.NotImplementedException();
        }

        public Task InstallPackagesAsync(DeployInput options, string sessionId)
        {
            throw new System.NotImplementedException();
        }

        public async Task<string> StartSessionAsync(DeployInput options)
        {
            var requestUri = new Uri(options.GetTargetUri(), "DesktopModules/PolyDeploy/API/Remote/CreateSession");
            this.httpClient.DefaultRequestHeaders.Add("x-api-key", options.ApiKey);
            var responseStream = await this.httpClient.GetStreamAsync(requestUri);
            var response = await JsonSerializer.DeserializeAsync<CreateSessionResponse>(responseStream);
            if (string.IsNullOrWhiteSpace(response?.Guid))
            {
                throw new InvalidOperationException("Received an empty response trying to create PolyDeploy session");
            }

            return response.Guid;
        }

        public async Task UploadPackageAsync(DeployInput options, string sessionId, Stream encryptedPackage, string packageName)
        {
            var requestUri = new Uri(options.GetTargetUri(), $"DesktopModules/PolyDeploy/API/Remote/AddPackages?sessionGuid={sessionId}");

            var form = new MultipartFormDataContent();
            form.Add(new StreamContent(encryptedPackage), "none", packageName);

            await this.httpClient.PostAsync(requestUri, form);
        }

        private class CreateSessionResponse
        {
            public string? Guid { get; set; }
        }
    }
}