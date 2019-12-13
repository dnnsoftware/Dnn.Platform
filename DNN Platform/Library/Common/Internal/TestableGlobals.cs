using System;
using DotNetNuke.Framework;

namespace DotNetNuke.Common.Internal
{
    public class TestableGlobals : ServiceLocator<IGlobals, TestableGlobals>
    {
        protected override Func<IGlobals> GetFactory()
        {
            return () => new GlobalsImpl();
        }
    }
}
