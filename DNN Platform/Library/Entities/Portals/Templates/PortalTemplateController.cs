namespace DotNetNuke.Entities.Portals.Templates
{
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Framework;
    using DotNetNuke.Instrumentation;
    using DotNetNuke.Services.Localization;
    using System;

    public class PortalTemplateController : ServiceLocator<IPortalTemplateController, PortalTemplateController>, IPortalTemplateController
    {
        protected override Func<IPortalTemplateController> GetFactory()
        {
            return () => new PortalTemplateController();
        }
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(PortalTemplateController));

        public string ExportPortalTemplate(UserInfo userInfo, out bool success)
        {
            throw new NotImplementedException();
        }

        public void ParseTemplate(int portalId, IPortalTemplateInfo template, int administratorId, PortalTemplateModuleAction mergeTabs, bool isNewPortal, out LocaleCollection localeCollection)
        {
            throw new NotImplementedException();
        }
    }
}
