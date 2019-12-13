using DotNetNuke.Collections.Internal;

using NUnit.Framework;

namespace DotNetNuke.Tests.Core.Collections
{
    [TestFixture]
    public class ExclusiveLockSharedDictionaryTests : SharedDictionaryTests
    {
        public override LockingStrategy LockingStrategy
        {
            get
            {
                return LockingStrategy.Exclusive;
            }
        }
    }
}
