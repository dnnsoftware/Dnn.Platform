// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Collections.Internal
{
    using System;

    internal class LockingStrategyFactory
    {
        public static ILockStrategy Create(LockingStrategy strategy)
        {
            switch (strategy)
            {
                case LockingStrategy.ReaderWriter:

                    return new ReaderWriterLockStrategy();
                case LockingStrategy.Exclusive:

                    return new ExclusiveLockStrategy();
                default:

                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}
