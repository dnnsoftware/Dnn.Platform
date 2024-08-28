// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.Styles.Services
{
    using System.Threading;
    using System.Web.Http;

    using Dnn.PersonaBar.Library;
    using Dnn.PersonaBar.Library.Attributes;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Instrumentation;

    /// <summary>
    /// REST API Controller for the Styles module.
    /// </summary>
    /// <seealso cref="Dnn.PersonaBar.Library.PersonaBarApiController" />
    [MenuPermission(MenuName = Components.Constants.MenuName)]
    public class StylesController : PersonaBarApiController
    {
        /// <summary>
        /// Gets the styles.
        /// </summary>
        /// <returns>The current portal style settings.</returns>
        [HttpGet]
        public IHttpActionResult GetStyles()
        {
            var repo = new PortalStylesRepository();
            var settings = repo.GetSettings(this.PortalId);
            return this.Ok(settings);
        }
    }
}
