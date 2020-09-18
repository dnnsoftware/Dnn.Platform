// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.Servers.Services
{
    using System;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Web.Http;

    using Dnn.PersonaBar.Library;
    using Dnn.PersonaBar.Library.Attributes;
    using Dnn.PersonaBar.Servers.Components.DatabaseServer;
    using DotNetNuke.Instrumentation;

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
                var dbInfo = this._databaseController.GetDbInfo();
                var dbBackups = this._databaseController.GetDbBackups().Select(b => new
                {
                    name = b.Name,
                    startDate = b.StartDate,
                    finishDate = b.FinishDate,
                    size = b.Size,
                    backupType = b.BackupType
                });
                var dbFileInfo = this._databaseController.GetDbFileInfo().Select(f => new
                {
                    name = f.Name,
                    size = f.Megabytes,
                    fileType = f.FileType,
                    fileName = f.ShortFileName
                });

                return this.Request.CreateResponse(HttpStatusCode.OK, new
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
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }
    }
}
