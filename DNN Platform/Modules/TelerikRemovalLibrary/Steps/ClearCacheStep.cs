// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.Modules.TelerikRemovalLibrary
{
    using System;

    using DotNetNuke.Instrumentation;

    /// <inheritdoc/>
    internal class ClearCacheStep : StepBase, IClearCacheStep
    {
        private readonly IDataCache dataCache;

        /// <summary>
        /// Initializes a new instance of the <see cref="ClearCacheStep"/> class.
        /// </summary>
        /// <param name="loggerSource">An instance of <see cref="ILoggerSource"/>.</param>
        /// <param name="dataCache">An instance of <see cref="IDataCache"/>.</param>
        public ClearCacheStep(ILoggerSource loggerSource, IDataCache dataCache)
            : base(loggerSource)
        {
            this.dataCache = dataCache ??
                throw new ArgumentNullException(nameof(dataCache));

            this.Quiet = true;
        }

        /// <inheritdoc/>
        public override string Name => "Clear cache";

        /// <inheritdoc/>
        protected override void ExecuteInternal()
        {
            this.dataCache.ClearCache();
            this.Success = true;
        }
    }
}
