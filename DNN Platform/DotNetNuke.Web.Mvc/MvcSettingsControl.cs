// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.Mvc
{
    using System.Threading;
    using System.Threading.Tasks;

    using DotNetNuke.Entities.Modules;
    using DotNetNuke.UI.Modules;

    public class MvcSettingsControl : MvcHostControl, IAsyncSettingsControl
    {
        public MvcSettingsControl()
            : base("Settings")
        {
            this.ExecuteModuleImmediately = false;
        }

        /// <inheritdoc />
        public void LoadSettings()
        {
            // TODO: This should now throw as control needs to always be executed asynchronously.
            // throw new NotSupportedException();
            this.ExecuteModule();
        }

        /// <inheritdoc />
        public Task LoadSettingsAsync(CancellationToken cancellationToken)
        {
            return this.ExecuteModuleAsync(cancellationToken);
        }

        /// <inheritdoc/>
        public void UpdateSettings()
        {
            // TODO: This should now throw as control needs to always be executed asynchronously.
            // throw new NotSupportedException();
            this.ExecuteModule();

            ModuleController.Instance.UpdateModule(this.ModuleContext.Configuration);
        }

        /// <inheritdoc/>
        public async Task UpdateSettingsAsync(CancellationToken cancellationToken)
        {
            await this.ExecuteModuleAsync(cancellationToken);

            ModuleController.Instance.UpdateModule(this.ModuleContext.Configuration);
        }
    }
}
