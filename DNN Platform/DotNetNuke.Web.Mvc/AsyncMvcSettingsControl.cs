// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.Mvc
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    using DotNetNuke.Entities.Modules;
    using DotNetNuke.UI.Modules;

    public class AsyncMvcSettingsControl : AsyncMvcHostControl, IAsyncSettingsControl
    {
        public AsyncMvcSettingsControl()
            : base("Settings")
        {
            this.ExecuteModuleImmediately = false;
        }

        /// <inheritdoc/>
        public void LoadSettings()
        {
            throw new NotSupportedException("Async controls need to call LoadSettingsAsync.");
        }

        /// <inheritdoc/>
        public Task LoadSettingsAsync(CancellationToken cancellationToken)
        {
            return this.ExecuteModuleAsync(cancellationToken);
        }

        /// <inheritdoc/>
        public void UpdateSettings()
        {
            throw new NotSupportedException("Async controls need to call UpdateSettingsAsync.");
        }

        /// <inheritdoc/>
        public async Task UpdateSettingsAsync(CancellationToken cancellationToken)
        {
            await this.ExecuteModuleAsync(cancellationToken);

            ModuleController.Instance.UpdateModule(this.ModuleContext.Configuration);
        }
    }
}
