// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

#region Usings

using System;

#endregion

namespace DotNetNuke.Collections.Internal
{
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
