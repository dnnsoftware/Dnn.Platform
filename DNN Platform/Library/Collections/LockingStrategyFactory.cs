// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Collections.Internal
{
    using System;

    /// <summary>Factory class for LockingStrategies.</summary>
    internal class LockingStrategyFactory
    {
        /// <summary>Creates a new locking strategy.</summary>
        /// <param name="strategy">The <see cref="LockingStrategy"/> to instantiate.</param>
        /// <returns>An instance of <see cref="ILockStrategy"/> that uses the specified strategy.</returns>
        public static ILockStrategy Create(LockingStrategy strategy)
        {
            return strategy switch
            {
                LockingStrategy.ReaderWriter => new ReaderWriterLockStrategy(),
                LockingStrategy.Exclusive => new ExclusiveLockStrategy(),
                _ => throw new ArgumentOutOfRangeException(nameof(strategy), strategy, "Unexpected strategy, ReaderWriter and Exclusive are supported values."),
            };
        }
    }
}
