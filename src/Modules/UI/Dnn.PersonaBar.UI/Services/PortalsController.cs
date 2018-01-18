#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2018
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
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Dnn.PersonaBar.Library;
using Dnn.PersonaBar.Library.Attributes;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Instrumentation;

namespace Dnn.PersonaBar.UI.Services
{
    /// <summary>
    /// Service to perform portal operations.
    /// </summary>
    [MenuPermission(Scope = ServiceScope.Regular)]
    public class PortalsController : PersonaBarApiController
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(PortalsController));


        /// GET: api/Portals/GetPortals
        /// <summary>
        /// Gets portals
        /// </summary>
        /// <param></param>
        /// <param name="addAll">Add all portals item in list.</param>
        /// <returns>List of portals</returns>
        [HttpGet]
        public HttpResponseMessage GetPortals(bool addAll = false)
        {
            try
            {
                var portals = PortalController.Instance.GetPortals().OfType<PortalInfo>();
                if (!UserInfo.IsSuperUser)
                {
                    portals = portals.Where(portal => portal.PortalID == PortalId);
                }

                var availablePortals = portals.Select(v => new
                {
                    v.PortalID,
                    v.PortalName
                }).ToList();

                if (addAll)
                {
                    availablePortals.Insert(0, new
                    {
                        PortalID = -1,
                        PortalName =
                            DotNetNuke.Services.Localization.Localization.GetString("AllSites", Constants.SharedResources)
                    });
                }

                var response = new
                {
                    Success = true,
                    Results = availablePortals,
                    TotalResults = availablePortals.Count
                };

                return Request.CreateResponse(HttpStatusCode.OK, response);
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }
    }
}
