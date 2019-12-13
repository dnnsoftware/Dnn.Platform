using DotNetNuke.Collections.Internal;

using NUnit.Framework;

namespace DotNetNuke.Tests.Core.Collections
{
    [TestFixture]
    public class ExclusiveLockSharedListTests : SharedListTests
    {
        internal override LockingStrategy LockingStrategy
        {
            get
            {
                return LockingStrategy.Exclusive;
            }
        }
    }
}
