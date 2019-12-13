using DotNetNuke.Collections.Internal;

using NUnit.Framework;

namespace DotNetNuke.Tests.Core.Collections
{
    [TestFixture]
    public class ReaderWriterLockStrategyTests : LockStrategyTests
    {
        internal override ILockStrategy GetLockStrategy()
        {
            return new ReaderWriterLockStrategy();
        }

        [Test] // no ExpectedException attribute
        public override void DoubleReadLockThrows()
        {
            base.DoubleReadLockThrows();
        }

        [Test] // no ExpectedException attribute
        public override void DoubleWriteLockThrows()
        {
            base.DoubleWriteLockThrows();
        }
    }
}
