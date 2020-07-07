// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.AzureConnector.Services
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Net.Http;
    using System.Web.Http;

    using DotNetNuke.Providers.FolderProviders.AzureFolderProvider;
    using DotNetNuke.Services.Exceptions;
    using DotNetNuke.Web.Api;
    using Microsoft.WindowsAzure.Storage;

    [DnnAuthorize]
    public class ServicesController : DnnApiController
    {
        [HttpGet]
        public HttpResponseMessage GetAllContainers(int id)
        {
            try
            {
                var containers = new List<string>();
                var folderProvider = new AzureFolderProvider();
                var folderMapping = Components.AzureConnector.FindAzureFolderMappingStatic(this.PortalSettings.PortalId, id, false);
                if (folderMapping != null)
                {
                    containers = folderProvider.GetAllContainers(folderMapping);
                }

                return this.Request.CreateResponse(HttpStatusCode.OK, containers);
            }
            catch (StorageException ex)
            {
                Exceptions.LogException(ex);
                var message = ex.RequestInformation.HttpStatusMessage ?? ex.Message;
                return this.Request.CreateResponse(HttpStatusCode.InternalServerError, new { Message = message });
            }
            catch (Exception ex)
            {
                Exceptions.LogException(ex);
                const string message = "An error has occurred connecting to the Azure account.";
                return this.Request.CreateResponse(HttpStatusCode.InternalServerError, new { Message = message });
            }
        }

        [HttpGet]
        public HttpResponseMessage GetFolderMappingId()
        {
            return this.Request.CreateResponse(HttpStatusCode.OK, Components.AzureConnector.FindAzureFolderMappingStatic(this.PortalSettings.PortalId).FolderMappingID);
        }
    }
}
