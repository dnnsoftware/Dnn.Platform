// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.Modules.TelerikRemovalLibrary.Impl
{
    /// <inheritdoc/>
    internal class ClearCacheStep : StepBase, IClearCacheStep
    {
        private readonly IDataCache dataCache;

        /// <summary>
        /// Initializes a new instance of the <see cref="ClearCacheStep"/> class.
        /// </summary>
        /// <param name="dataCache">An instance of <see cref="IDataCache"/>.</param>
        public ClearCacheStep(IDataCache dataCache)
        {
            this.dataCache = dataCache ??
                throw new System.ArgumentNullException(nameof(dataCache));
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
