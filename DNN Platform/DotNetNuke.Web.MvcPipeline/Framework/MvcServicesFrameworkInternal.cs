namespace DotNetNuke.Web.MvcPipeline.Framework
{
    using System;
    using DotNetNuke.Framework;

    internal class MvcServicesFrameworkInternal : ServiceLocator<IMvcServiceFrameworkInternals, MvcServicesFrameworkInternal>
    {
        /// <inheritdoc/>
        protected override Func<IMvcServiceFrameworkInternals> GetFactory()
        {
            return () => new MvcServicesFrameworkImpl();
        }
    }
}
