// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.Security.Components.Checks
{
    using System;

    using DotNetNuke.Common.Lists;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Profile;

    public class CheckBiography : IAuditCheck
    {
        public string Id => "CheckBiography";

        public bool LazyLoad => false;

        public CheckResult Execute()
        {
            var result = new CheckResult(SeverityEnum.Unverified, this.Id);
            try
            {
                var portalController = new PortalController();
                var controller = new ListController();

                var richTextDataType = controller.GetListEntryInfo("DataType", "RichText");
                result.Severity = SeverityEnum.Pass;
                foreach (PortalInfo portal in portalController.GetPortals())
                {
                    var pd = ProfileController.GetPropertyDefinitionByName(portal.PortalID, "Biography");
                    if (pd != null && pd.DataType == richTextDataType.EntryID && !pd.Deleted)
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
