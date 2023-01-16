// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Maintenance.Telerik.Steps
{
    using System;

    using DotNetNuke.Instrumentation;
    using DotNetNuke.Maintenance.Shims;
    using DotNetNuke.Maintenance.Telerik.Removal;

    /// <inheritdoc/>
    internal class ClearCacheStep : StepBase, IClearCacheStep
    {
        private readonly IDataCache dataCache;

        /// <summary>Initializes a new instance of the <see cref="ClearCacheStep"/> class.</summary>
        /// <param name="loggerSource">An instance of <see cref="ILoggerSource"/>.</param>
        /// <param name="localizer">An instance of <see cref="ILocalizer"/>.</param>
        /// <param name="dataCache">An instance of <see cref="IDataCache"/>.</param>
        public ClearCacheStep(ILoggerSource loggerSource, ILocalizer localizer, IDataCache dataCache)
            : base(loggerSource, localizer)
        {
            this.dataCache = dataCache ??
                throw new ArgumentNullException(nameof(dataCache));

            this.Quiet = true;
        }

        /// <inheritdoc/>
        public override string Name => this.Localize("UninstallStepClearCache");

        /// <inheritdoc/>
        protected override void ExecuteInternal()
        {
            this.dataCache.ClearCache();
            this.Success = true;
        }
    }
}
