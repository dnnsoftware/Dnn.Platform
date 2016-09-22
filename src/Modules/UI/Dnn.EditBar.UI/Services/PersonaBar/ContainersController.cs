#region Copyright
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2016
// by DotNetNuke Corporation
// All Rights Reserved
#endregion

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using Dnn.PersonaBar.Library;
using Dnn.PersonaBar.Library.Common;
using Dnn.PersonaBar.Library.Containers;
using Dnn.PersonaBar.UI.Common;
using DotNetNuke.Application;
using DotNetNuke.Services.Exceptions;
using DotNetNuke.Web.Api;

namespace Dnn.PersonaBar.UI.Services.PersonaBar
{
    [DnnAuthorize]
    public class ContainersController : PersonaBarApiController
    {
        #region Public API methods
        
        /// <summary>
        /// Retrieve a list of extensions for menu. 
        /// </summary>
        [HttpGet]
        public HttpResponseMessage GetConfiguration()
        {
            try
            {
                return Request.CreateResponse(HttpStatusCode.OK, PersonaBarContainer.Instance.GetConfiguration());
            }
            catch (Exception ex)
            {
                Exceptions.LogException(ex);
                return Request.CreateErrorResponse(HttpStatusCode.NotFound, ex.Message);
            }
        }

        #endregion
    }
}