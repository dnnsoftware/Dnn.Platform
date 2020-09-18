// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.EditBar.UI.Services
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Text;
    using System.Web;
    using System.Web.Http;

    using Dnn.EditBar.Library;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Services.Exceptions;
    using DotNetNuke.Web.Api;
    using Newtonsoft.Json;

    [DnnAuthorize]
    public class LocalizationController : DnnApiController
    {
        private static object _threadLocker = new object();

        /// <summary>
        /// Retrieve a list of CMX related Localization Keys with it's values for the current culture.
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public HttpResponseMessage GetTable(string culture)
        {
            try
            {
                var resources = this.GetResourcesFromFile(culture);
                if (resources == null)
                {
                    lock (_threadLocker)
                    {
                        resources = this.GetResourcesFromFile(culture);
                        if (resources == null)
                        {
                            resources = this.GenerateJsonFile(culture);
                        }
                    }
                }

                return this.Request.CreateResponse(HttpStatusCode.OK, resources);
            }
            catch (Exception ex)
            {
                Exceptions.LogException(ex);
                return this.Request.CreateErrorResponse(HttpStatusCode.NotFound, ex.Message);
            }
        }

        private IDictionary<string, IDictionary<string, string>> GetResourcesFromFile(string culture)
        {
            if (!this.Expired(culture))
            {
                var jsonFileContent = this.GetJsonFileContent(culture);
                return jsonFileContent != null
                    ? JsonConvert.DeserializeObject<IDictionary<string, IDictionary<string, string>>>(jsonFileContent)
                    : null;
            }

            return null;
        }

        private bool Expired(string culture)
        {
            var cacheKey = $"EditBarResources{culture}";
            if (DataCache.GetCache(cacheKey) != null)
            {
                return false;
            }

            var jsonFilePath = this.GetResourcesJsonFilePath(culture);
            var jsonFile = new FileInfo(jsonFilePath);
            if (!jsonFile.Exists)
            {
                return true;
            }

            var lastModifiedTime = jsonFile.LastWriteTime;
            var resourceFiles = this.GetAllResourceFiles(culture);

            var expired = resourceFiles.Select(file => new FileInfo(file))
                .Any(resourceFile => resourceFile.LastWriteTime > lastModifiedTime);
            if (!expired)
            {
                DataCache.SetCache(cacheKey, new CacheDto(), TimeSpan.FromMinutes(5));
            }

            return expired;
        }

        private string GetJsonFileContent(string culture)
        {
            var path = this.GetResourcesJsonFilePath(culture);
            return File.Exists(path) ? File.ReadAllText(path, Encoding.UTF8) : null;
        }

        private IDictionary<string, IDictionary<string, string>> GenerateJsonFile(string culture)
        {
            var resources = new Dictionary<string, IDictionary<string, string>>();
            var resourceFiles = this.GetAllResourceFiles(culture);
            var editBarResourcesPath = Path.Combine(Constants.EditBarRelativePath, "App_LocalResources");
            foreach (var resourcesFile in resourceFiles)
            {
                var key = Path.GetFileNameWithoutExtension(resourcesFile);
                var filename = Path.GetFileName(resourcesFile);
                var relativePath = Path.Combine(editBarResourcesPath, filename);
                var keys = EditBar.UI.Controllers.LocalizationController.Instance.GetLocalizedDictionary(relativePath, culture);
                resources.Add(key, keys);
            }

            var content = JsonConvert.SerializeObject(resources);
            var filePath = this.GetResourcesJsonFilePath(culture);
            var folderPath = Path.GetDirectoryName(filePath);
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            File.WriteAllText(filePath, content, Encoding.UTF8);

            return resources;
        }

        private string GetResourcesJsonFilePath(string culture)
        {
            var path = Path.Combine(Constants.EditBarRelativePath, "Resources", $"LocalResources.{culture}.resources");
            return HttpContext.Current.Server.MapPath(path);
        }

        private IList<string> GetAllResourceFiles(string culture)
        {
            var editBarResourcesPath = Path.Combine(Constants.EditBarRelativePath, "App_LocalResources");
            var physicalPath = HttpContext.Current.Server.MapPath(editBarResourcesPath);
            return Directory.GetFiles(physicalPath, "*.resx");
        }
    }

    [Serializable]
    public class CacheDto
    {}
}
