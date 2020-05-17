﻿// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Dnn.PersonaBar.ConfigConsole.Services.Dto;
using Dnn.PersonaBar.Library;
using Dnn.PersonaBar.Library.Attributes;
using DotNetNuke.Instrumentation;
using DotNetNuke.Web.Api;

namespace Dnn.PersonaBar.ConfigConsole.Services
{
    [MenuPermission(Scope = ServiceScope.Host)]
    public class ConfigConsoleController : PersonaBarApiController
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(ConfigConsoleController));
        private Components.ConfigConsoleController _controller = new Components.ConfigConsoleController();

        /// GET: api/ConfigConsole/GetConfigFilesList
        /// <summary>
        /// Gets list of config files
        /// </summary>
        /// <param></param>
        /// <returns>List of config files</returns>
        [HttpGet]
        public HttpResponseMessage GetConfigFilesList()
        {
            try
            {
                var configFileList = _controller.GetConfigFilesList().ToList();

                var response = new
                {
                    Success = true,
                    Results = configFileList,
                    TotalResults = configFileList.Count()
                };

                return Request.CreateResponse(HttpStatusCode.OK, response);
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        /// GET: api/ConfigConsole/GetConfigFile
        /// <summary>
        /// Gets content of a specific config file
        /// </summary>
        /// <param name="fileName">Name of a config file</param>
        /// <returns>Content of a config file</returns>
        [HttpGet]
        public HttpResponseMessage GetConfigFile(string fileName)
        {
            try
            {
                var fileContent = _controller.GetConfigFile(fileName);

                var response = new
                {
                    FileName = fileName,
                    FileContent = fileContent
                };

                return Request.CreateResponse(HttpStatusCode.OK, response);
            }
            catch (ArgumentException exc)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, exc.Message);
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        /// POST: api/ConfigConsole/UpdateConfigFile
        /// <summary>
        /// Updates a config file
        /// </summary>
        /// <param name="configFileDto">Content of config file</param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public HttpResponseMessage UpdateConfigFile(ConfigFileDto configFileDto)
        {
            try
            {
                _controller.UpdateConfigFile(configFileDto.FileName, configFileDto.FileContent);
                return Request.CreateResponse(HttpStatusCode.OK, new { Success = true });
            }
            catch (ArgumentException exc)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, exc.Message);
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        /// POST: api/ConfigConsole/MergeConfigFile
        /// <summary>
        /// Merges config files
        /// </summary>
        /// <param name="configFileDto">Content of config file</param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public HttpResponseMessage MergeConfigFile(ConfigFileDto configFileDto)
        {
            try
            {
                _controller.MergeConfigFile(configFileDto.FileContent);
                return Request.CreateResponse(HttpStatusCode.OK, new { Success = true });
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }
    }
}
