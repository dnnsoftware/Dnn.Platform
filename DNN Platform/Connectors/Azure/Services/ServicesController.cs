#region Copyright
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2017
// by DotNetNuke Corporation
// All Rights Reserved
#endregion

using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using DotNetNuke.Providers.FolderProviders.AzureFolderProvider;
using DotNetNuke.Services.Exceptions;
using DotNetNuke.Web.Api;
using Microsoft.WindowsAzure.StorageClient;

namespace Dnn.AzureConnector.Services
{
    [DnnAuthorize]
    public class ServicesController : DnnApiController
    {
        #region API

        [HttpGet]
        public HttpResponseMessage GetAllContainers(int id)
        {
            try
            {
                var containers = new List<string>();
                var folderProvider = new AzureFolderProvider();
                var folderMapping = Components.AzureConnector.FindAzureFolderMappingStatic(PortalSettings.PortalId, id, false);
                if (folderMapping != null)
                {
                    containers = folderProvider.GetAllContainers(folderMapping);
                }

                return Request.CreateResponse(HttpStatusCode.OK, containers);
            }
            catch (StorageClientException ex)
            {
                Exceptions.LogException(ex);
                var message = ex.Message;
                return Request.CreateResponse(HttpStatusCode.InternalServerError, new { Message = message });
            }
            catch (Exception ex)
            {
                Exceptions.LogException(ex);
                const string message = "An error has occurred connecting to the Azure account.";
                return Request.CreateResponse(HttpStatusCode.InternalServerError, new { Message = message });
            }
        }

        [HttpGet]
        public HttpResponseMessage GetFolderMappingId()
        {
            return Request.CreateResponse(HttpStatusCode.OK, Components.AzureConnector.FindAzureFolderMappingStatic(PortalSettings.PortalId).FolderMappingID);
        }

        #endregion
    }
}