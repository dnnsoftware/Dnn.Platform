// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Tests.Core.Collections
{
    using System.Threading;

    using DotNetNuke.Collections.Internal;
    using NUnit.Framework;

    [TestFixture]
    public class ReaderWriterSlimLockTests
    {
        [Test]
        public void DoubleDisposeAllowed()
        {
            using (var l = new ReaderWriterLockSlim())
            {
                var scl = new ReaderWriterSlimLock(l);

                scl.Dispose();
                scl.Dispose();

                // no exception on 2nd dispose
            }
        }
    }
}
