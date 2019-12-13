using System;
using DotNetNuke.Framework;

namespace DotNetNuke.Services.Localization.Internal
{
    public class TestableLocalization : ServiceLocator<ILocalization, TestableLocalization>
    {
        protected override Func<ILocalization> GetFactory()
        {
            return () => new LocalizationImpl();
        }
    }
}
