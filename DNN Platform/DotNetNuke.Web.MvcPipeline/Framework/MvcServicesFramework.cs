namespace DotNetNuke.Web.MvcPipeline.Framework
{
    using System;

    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Framework;

    /// <summary>Enables modules to support Services Framework features.</summary>
    public class MvcServicesFramework : ServiceLocator<IMvcServicesFramework, MvcServicesFramework>
    {
        public static string GetServiceFrameworkRoot()
        {
            var portalSettings = PortalSettings.Current;
            if (portalSettings == null)
            {
                return string.Empty;
            }

            var path = portalSettings.PortalAlias.HTTPAlias;
            var index = path.IndexOf('/');
            if (index > 0)
            {
                path = path.Substring(index);
                if (!path.EndsWith("/"))
                {
                    path += "/";
                }
            }
            else
            {
                path = "/";
            }

            return path;
        }

        /// <inheritdoc/>
        protected override Func<IMvcServicesFramework> GetFactory()
        {
            return () => new MvcServicesFrameworkImpl();
        }
    }
}
