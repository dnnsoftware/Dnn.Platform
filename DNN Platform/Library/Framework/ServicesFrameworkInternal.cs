using System;

namespace DotNetNuke.Framework
{
    internal class ServicesFrameworkInternal : ServiceLocator<IServiceFrameworkInternals, ServicesFrameworkInternal>
    {
        protected override Func<IServiceFrameworkInternals> GetFactory()
        {
            return () => new ServicesFrameworkImpl();
        }
    }
}