// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.UI.Services
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

    using Dnn.PersonaBar.Library;
    using Dnn.PersonaBar.Library.Attributes;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Services.Exceptions;
    using DotNetNuke.Services.Localization;
    using Newtonsoft.Json;

    [MenuPermission(Scope = ServiceScope.Regular)]
    public class LocalizationController : PersonaBarApiController
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
            var cacheKey = $"PersonaBarResources{culture}";
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

            var expired = resourceFiles.Select(file => new FileInfo(file.Value))
                .Any(resourceFile => resourceFile.LastWriteTime > lastModifiedTime || resourceFile.CreationTime > lastModifiedTime);
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
            foreach (var resourcesFile in resourceFiles)
            {
                var key = resourcesFile.Key;
                var relativePath = resourcesFile.Value.Replace(Globals.ApplicationMapPath, "~").Replace("\\", "/");
                if (File.Exists(HttpContext.Current.Server.MapPath(relativePath)))
                {
                    var keys = this.GetLocalizedDictionary(relativePath, culture);
                    resources.Add(key, keys);
                }
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

        private IDictionary<string, string> GetLocalizedDictionary(string relativePath, string culture)
        {
            var localizedDict = Dnn.PersonaBar.Library.Controllers.LocalizationController.Instance.GetLocalizedDictionary(relativePath, culture);
            if (!culture.Equals(Localization.SystemLocale, StringComparison.InvariantCultureIgnoreCase))
            {
                var fallbackCulture = this.GetFallbackCulture(culture);
                var folder = Path.GetDirectoryName(relativePath)?.Replace("\\", "/");
                var fileName = Path.GetFileNameWithoutExtension(relativePath)?
                                .ToLowerInvariant().Replace("." + culture.ToLowerInvariant(), string.Empty);
                var culturePart = fallbackCulture.Equals(Localization.SystemLocale, StringComparison.InvariantCultureIgnoreCase)
                                    ? string.Empty : "." + fallbackCulture;
                var fallbackFilePath = $"{folder}//{fileName}{culturePart}.resx";
                if (!File.Exists(HttpContext.Current.Server.MapPath(fallbackFilePath)))
                {
                    fallbackFilePath = $"{folder}//{fileName}.resx";
                }

                if (File.Exists(HttpContext.Current.Server.MapPath(fallbackFilePath)))
                {
                    var fallbackDict = Dnn.PersonaBar.Library.Controllers.LocalizationController.Instance.GetLocalizedDictionary(fallbackFilePath, culture);
                    foreach (var kvp in fallbackDict)
                    {
                        if (!localizedDict.ContainsKey(kvp.Key))
                        {
                            localizedDict.Add(kvp.Key, kvp.Value);
                        }
                    }
                }
            }

            return localizedDict;
        }

        private string GetFallbackCulture(string culture)
        {
            var locale = LocaleController.Instance.GetLocale(this.PortalId, culture);
            if (locale != null && !string.IsNullOrEmpty(locale.Fallback))
            {
                return locale.Fallback;
            }

            return Localization.SystemLocale;
        }

        private string GetResourcesJsonFilePath(string culture)
        {
            var path = Path.Combine(Constants.PersonaBarRelativePath, "Resources", $"LocalResources.{culture}.resources");
            return HttpContext.Current.Server.MapPath(path);
        }

        private IDictionary<string, string> GetAllResourceFiles(string culture)
        {
            var physicalPath = HttpContext.Current.Server.MapPath(Constants.PersonaBarRelativePath);
            var allFiles = Directory.GetFiles(physicalPath, "*.resx", SearchOption.AllDirectories);
            var resourceFiles = new Dictionary<string, string>();
            foreach (var resourceFile in allFiles)
            {
                var key = Path.GetFileNameWithoutExtension(resourceFile);
                var folder = Path.GetDirectoryName(resourceFile);
                if (string.IsNullOrEmpty(key) || string.IsNullOrEmpty(folder))
                {
                    continue;
                }

                if (key.Contains("."))
                {
                    key = key.Substring(0, key.IndexOf(".", StringComparison.InvariantCultureIgnoreCase));
                }

                if (resourceFiles.ContainsKey(key))
                {
                    continue;
                }

                var filePath = Path.Combine(folder, key + ".resx");
                if (!culture.Equals(Localization.SystemLocale, StringComparison.InvariantCultureIgnoreCase))
                {
                    var cultureSpecificFileName = $"{key}.{culture}.resx";
                    var cultureSpecificFile = allFiles.FirstOrDefault(f =>
                    {
                        var name = Path.GetFileName(f);
                        return !string.IsNullOrEmpty(name)
                                    && name.Equals(cultureSpecificFileName, StringComparison.InvariantCultureIgnoreCase);
                    });

                    if (string.IsNullOrEmpty(cultureSpecificFile))
                    {
                        var fallbackCulture = this.GetFallbackCulture(culture);
                        cultureSpecificFileName = $"{key}.{fallbackCulture}.resx";
                        cultureSpecificFile = allFiles.FirstOrDefault(f =>
                        {
                            var name = Path.GetFileName(f);
                            return !string.IsNullOrEmpty(name)
                                        && name.Equals(cultureSpecificFileName, StringComparison.InvariantCultureIgnoreCase);
                        });
                    }

                    if (!string.IsNullOrEmpty(cultureSpecificFile))
                    {
                        filePath = cultureSpecificFile;
                    }
                }

                resourceFiles.Add(key, filePath);
            }

            return resourceFiles;
        }
    }

    [Serializable]
    public class CacheDto
    {}
}
