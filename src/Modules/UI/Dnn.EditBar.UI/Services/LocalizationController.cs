#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2017
// by DotNetNuke Corporation
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated 
// documentation files (the "Software"), to deal in the Software without restriction, including without limitation 
// the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and 
// to permit persons to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or substantial portions 
// of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED 
// TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL 
// THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF 
// CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
// DEALINGS IN THE SOFTWARE.
#endregion

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

namespace Dnn.EditBar.UI.Services
{
    [DnnAuthorize]
    public class LocalizationController : DnnApiController
    {
        private static object _threadLocker = new object();

        #region Public API methods
        
        /// <summary>
        /// Retrieve a list of CMX related Localization Keys with it's values for the current culture. 
        /// </summary>
        [HttpGet]
        public HttpResponseMessage GetTable(string culture)
        {
            try
            {
                var resources = GetResourcesFromFile(culture);
                if (resources == null)
                {
                    lock (_threadLocker)
                    {
                        resources = GetResourcesFromFile(culture);
                        if (resources == null)
                        {
                            resources = GenerateJsonFile(culture);
                        }
                    }
                }

                return Request.CreateResponse(HttpStatusCode.OK, resources);
            }
            catch (Exception ex)
            {
                Exceptions.LogException(ex);
                return Request.CreateErrorResponse(HttpStatusCode.NotFound, ex.Message);
            }
        }

        #endregion

        #region Private Methods

        private IDictionary<string, IDictionary<string, string>> GetResourcesFromFile(string culture)
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

        private bool Expired(string culture)
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
                .Any(resourceFile => resourceFile.LastWriteTime > lastModifiedTime);
            if (!expired)
            {
                DataCache.SetCache(cacheKey, new CacheDto(), TimeSpan.FromMinutes(5));
            }

            return expired;
        }

        private string GetJsonFileContent(string culture)
        {
            var path = GetResourcesJsonFilePath(culture);
            return File.Exists(path) ? File.ReadAllText(path, Encoding.UTF8) : null;
        }

        private IDictionary<string, IDictionary<string, string>> GenerateJsonFile(string culture)
        {
            var resources = new Dictionary<string, IDictionary<string, string>>();
            var resourceFiles = GetAllResourceFiles(culture);
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
            var filePath = GetResourcesJsonFilePath(culture);
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

        #endregion
    }

    [Serializable]
    public class CacheDto
    {

    }
}