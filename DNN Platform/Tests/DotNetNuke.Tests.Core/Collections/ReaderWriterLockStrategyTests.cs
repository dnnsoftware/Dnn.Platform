// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Tests.Core.Collections
{
    using DotNetNuke.Collections.Internal;
    using NUnit.Framework;

    [TestFixture]
    public class ReaderWriterLockStrategyTests : LockStrategyTests
    {
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

        internal override ILockStrategy GetLockStrategy()
        {
            return new ReaderWriterLockStrategy();
        }
    }
}
