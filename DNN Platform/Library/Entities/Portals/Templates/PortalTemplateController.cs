namespace DotNetNuke.Entities.Portals.Templates
{
    using System;
    using System.Collections.Generic;

    using DotNetNuke.Framework;
    using DotNetNuke.Instrumentation;

    public class PortalTemplateController : ServiceLocator<IPortalTemplateController, PortalTemplateController>, IPortalTemplateController
    {
        protected override Func<IPortalTemplateController> GetFactory()
        {
            return () => new PortalTemplateController();
        }
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(PortalTemplateController));

        public void ApplyPortalTemplate(int portalId, IPortalTemplateInfo template, int administratorId, PortalTemplateModuleAction mergeTabs, bool isNewPortal)
        {
            var importer = new PortalTemplateImporter(template);
            importer.ParseTemplate(portalId, administratorId, mergeTabs, isNewPortal);
        }

        public Tuple<bool, string> ExportPortalTemplate(int portalId, string fileName, string description, bool isMultiLanguage, IEnumerable<string> locales, string localizationCulture, IEnumerable<int> exportTabIds, bool includeContent, bool includeFiles, bool includeModules, bool includeProfile, bool includeRoles)
        {
            var exporter = new PortalTemplateExporter();
            return exporter.ExportPortalTemplate(portalId, fileName, description, isMultiLanguage, locales, localizationCulture, exportTabIds, includeContent, includeFiles, includeModules, includeProfile, includeRoles);
        }
    }
}
