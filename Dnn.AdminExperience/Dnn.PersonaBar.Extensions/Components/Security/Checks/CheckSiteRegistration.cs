// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
using System;
using DotNetNuke.Entities.Portals;

namespace Dnn.PersonaBar.Security.Components.Checks
{
    public class CheckSiteRegistration : IAuditCheck
    {
        public string Id => "CheckSiteRegistration";

        public bool LazyLoad => false;

        public CheckResult Execute()
        {
            var result = new CheckResult(SeverityEnum.Unverified, Id);
            try
            {
                var portalController = new PortalController();
                result.Severity = SeverityEnum.Pass;
                foreach (PortalInfo portal in portalController.GetPortals())
                {
                    //check for public registration
                    if (portal.UserRegistration == 2)
                    {
                        result.Severity = SeverityEnum.Warning;
                        result.Notes.Add("Portal:" + portal.PortalName);
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }

            return result;
        }
    }
}
