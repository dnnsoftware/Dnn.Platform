// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace Dnn.EditBar.UI.Services
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
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
        private static readonly HashSet<string> KnownCultureNames = new HashSet<string>(
            CultureInfo.GetCultures(CultureTypes.AllCultures).Select(c => c.Name).Where(n => !string.IsNullOrEmpty(n)),
            StringComparer.OrdinalIgnoreCase);

        private static object threadLocker = new object();

        /// <summary>Retrieve a list of CMX related Localization Keys with its values for the current culture.</summary>
        /// <param name="culture">The culture name.</param>
        /// <returns>A response with a collection of resources.</returns>
        [HttpGet]
        public HttpResponseMessage GetTable(string culture)
        {
            try
            {
                var resources = GetResourcesFromFile(culture);
                if (resources == null)
                {
                    lock (threadLocker)
                    {
                        resources = GetResourcesFromFile(culture);
                        if (resources == null)
                        {
                            resources = GenerateJsonFile(culture);
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

        private static IDictionary<string, IDictionary<string, string>> GetResourcesFromFile(string culture)
        {
            if (!Expired(culture))
            {
                var jsonFileContent = GetJsonFileContent(culture);
                return jsonFileContent != null
                    ? JsonConvert.DeserializeObject<IDictionary<string, IDictionary<string, string>>>(jsonFileContent)
                    : null;
            }

            return null;
        }

        private static bool Expired(string culture)
        {
            var cacheKey = $"EditBarResources{culture}";
            if (DataCache.GetCache(cacheKey) != null)
            {
                return false;
            }

            var jsonFilePath = GetResourcesJsonFilePath(culture);
            var jsonFile = new FileInfo(jsonFilePath);
            if (!jsonFile.Exists)
            {
                return true;
            }

            var lastModifiedTime = jsonFile.LastWriteTime;
            var resourceFiles = GetAllResourceFiles(culture);

            var expired = resourceFiles.Select(file => new FileInfo(file))
                .Any(resourceFile => resourceFile.LastWriteTime > lastModifiedTime || resourceFile.CreationTime > lastModifiedTime);
            if (!expired)
            {
                DataCache.SetCache(cacheKey, new CacheDto(), TimeSpan.FromMinutes(5));
            }

            return expired;
        }

        private static string GetJsonFileContent(string culture)
        {
            var path = GetResourcesJsonFilePath(culture);
            return File.Exists(path) ? File.ReadAllText(path, Encoding.UTF8) : null;
        }

        private static Dictionary<string, IDictionary<string, string>> GenerateJsonFile(string culture)
        {
            var resources = new Dictionary<string, IDictionary<string, string>>();
            var resourceFiles = GetAllResourceFiles(culture);
            var editBarResourcesPath = Path.Combine(Constants.EditBarRelativePath, "App_LocalResources");
            var neutralCulture = GetNeutralCulture(culture);
            foreach (var resourcesFile in resourceFiles.Where(file => !IsCultureSpecificFile(file)))
            {
                // Values from the base resx are overridden by the neutral culture file (e.g. Common.tr.resx)
                // and then by the specific culture file (e.g. Common.tr-TR.resx), when they exist.
                var key = Path.GetFileNameWithoutExtension(resourcesFile);
                var keys = GetResourceValues(editBarResourcesPath, key + ".resx", culture);
                foreach (var localizedCulture in new[] { neutralCulture, culture }.Where(c => !string.IsNullOrEmpty(c)).Distinct(StringComparer.OrdinalIgnoreCase))
                {
                    var localizedFileName = $"{key}.{localizedCulture}.resx";
                    if (File.Exists(Path.Combine(Path.GetDirectoryName(resourcesFile), localizedFileName)))
                    {
                        foreach (var kvp in GetResourceValues(editBarResourcesPath, localizedFileName, culture))
                        {
                            keys[kvp.Key] = kvp.Value;
                        }
                    }
                }

                resources.Add(key, keys);
            }

            var content = JsonConvert.SerializeObject(resources);
            var filePath = GetResourcesJsonFilePath(culture);
            var folderPath = Path.GetDirectoryName(filePath);
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            File.WriteAllText(filePath, content, Encoding.UTF8);

            return resources;
        }

        private static string GetResourcesJsonFilePath(string culture)
        {
            var path = Path.Combine(Constants.EditBarRelativePath, "Resources", $"LocalResources.{culture}.resources");
            return HttpContext.Current.Server.MapPath(path);
        }

        private static string[] GetAllResourceFiles(string culture)
        {
            var editBarResourcesPath = Path.Combine(Constants.EditBarRelativePath, "App_LocalResources");
            var physicalPath = HttpContext.Current.Server.MapPath(editBarResourcesPath);
            return Directory.GetFiles(physicalPath, "*.resx");
        }

        private static Dictionary<string, string> GetResourceValues(string resourcesPath, string fileName, string culture)
        {
            var relativePath = Path.Combine(resourcesPath, fileName);
            return EditBar.UI.Controllers.LocalizationController.Instance.GetLocalizedDictionary(relativePath, culture);
        }

        private static bool IsCultureSpecificFile(string resourcesFile)
        {
            var name = Path.GetFileNameWithoutExtension(resourcesFile);
            var separatorIndex = name.LastIndexOf('.');
            return separatorIndex > 0 && KnownCultureNames.Contains(name.Substring(separatorIndex + 1));
        }

        private static string GetNeutralCulture(string culture)
        {
            try
            {
                var cultureInfo = CultureInfo.GetCultureInfo(culture);
                return cultureInfo.IsNeutralCulture ? cultureInfo.Name : cultureInfo.Parent.Name;
            }
            catch (CultureNotFoundException)
            {
                return null;
            }
        }
    }
}
