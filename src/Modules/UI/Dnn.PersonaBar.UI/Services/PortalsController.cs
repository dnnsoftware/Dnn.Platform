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
