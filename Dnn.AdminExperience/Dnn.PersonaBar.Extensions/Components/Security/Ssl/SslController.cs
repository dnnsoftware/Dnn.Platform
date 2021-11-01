// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.Extensions.Components.Security.Ssl
{
    using System;
    using System.Linq;

    using DotNetNuke.Data;
    using DotNetNuke.Framework;

    public class SslController : ServiceLocator<ISSlController, SslController>, ISSlController
    {
        /// <inheritdoc />
        public PortalStats GetPortalStats(int portalId)
        {
            using (IDataContext context = DataContext.Instance())
            {
                return context.ExecuteQuery<PortalStats>(System.Data.CommandType.StoredProcedure, "{databaseOwner}{objectQualifier}PersonaBar_GetPortalStats", portalId).FirstOrDefault();
            }
        }

        /// <inheritdoc />
        public void SetAllPortalTabsSecure(int portalId, bool secure)
        {
            using (IDataContext context = DataContext.Instance())
            {
                context.Execute(System.Data.CommandType.StoredProcedure, "{databaseOwner}{objectQualifier}PersonaBar_SetAllPortalTabsSecure", portalId, secure);
            }
        }

        protected override Func<ISSlController> GetFactory()
        {
            return () => new SslController();
        }
    }

    public interface ISSlController
    {
        /// <summary>
        /// Get statistics on the specified portal.
        /// </summary>
        /// <param name="portalId">Portal id of the portal.</param>
        /// <returns>PortalStats object.</returns>
        PortalStats GetPortalStats(int portalId);

        /// <summary>
        /// Sets all tabs of the portal to the specified secure value.
        /// </summary>
        /// <param name="portalId">The portal to update</param>
        /// <param name="secure">True to set all pages to secure, false to set them all to non secure.</param>
        void SetAllPortalTabsSecure(int portalId, bool secure);
    }
}
