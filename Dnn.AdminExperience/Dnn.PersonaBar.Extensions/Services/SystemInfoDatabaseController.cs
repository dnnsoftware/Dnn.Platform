﻿// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Dnn.PersonaBar.Library;
using Dnn.PersonaBar.Library.Attributes;
using Dnn.PersonaBar.Servers.Components.DatabaseServer;
using DotNetNuke.Instrumentation;

namespace Dnn.PersonaBar.Servers.Services
{
    [MenuPermission(Scope = ServiceScope.Host)]
    public class SystemInfoDatabaseController : PersonaBarApiController
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(SystemInfoDatabaseController));
        private readonly DatabaseController _databaseController = new DatabaseController();
        
        [HttpGet]
        public HttpResponseMessage GetDatabaseServerInfo()
        {
            try
            {
                var dbInfo = _databaseController.GetDbInfo();
                var dbBackups = _databaseController.GetDbBackups().Select(b => new
                {
                    name = b.Name,
                    startDate = b.StartDate,
                    finishDate = b.FinishDate,
                    size = b.Size,
                    backupType = b.BackupType
                });
                var dbFileInfo = _databaseController.GetDbFileInfo().Select(f => new
                {
                    name = f.Name,
                    size = f.Megabytes,
                    fileType = f.FileType,
                    fileName = f.ShortFileName
                });

                return Request.CreateResponse(HttpStatusCode.OK, new
                {
                    productVersion = dbInfo.ProductVersion,
                    servicePack = dbInfo.ServicePack,
                    productEdition = dbInfo.ProductEdition,
                    softwarePlatform = dbInfo.SoftwarePlatform,
                    backups = dbBackups,
                    files = dbFileInfo
                });
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }
    }
}
