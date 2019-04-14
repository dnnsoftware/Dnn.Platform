using System;
using DotNetNuke.Common.Lists;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Profile;

namespace Dnn.PersonaBar.Security.Components.Checks
{
    public class CheckBiography : IAuditCheck
    {
        public string Id => "CheckBiography";

        public bool LazyLoad => false;

        public CheckResult Execute()
        {
            var result = new CheckResult(SeverityEnum.Unverified, Id);
            try
            {
                var portalController = new PortalController();
                var controller = new ListController();

                var richTextDataType = controller.GetListEntryInfo("DataType", "RichText");
                result.Severity = SeverityEnum.Pass;
                foreach (PortalInfo portal in portalController.GetPortals())
                {
                    var pd = ProfileController.GetPropertyDefinitionByName(portal.PortalID, "Biography");
                    if (pd != null && pd.DataType == richTextDataType.EntryID)
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