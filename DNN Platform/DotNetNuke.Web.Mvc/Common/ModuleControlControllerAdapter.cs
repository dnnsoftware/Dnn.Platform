// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.Mvc.Common
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Common;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Framework;

    using Microsoft.Extensions.DependencyInjection;

    public class ModuleControlControllerAdapter : ServiceLocator<IModuleControlController, ModuleControlControllerAdapter>, IModuleControlController
    {
        private readonly IHostSettings hostSettings;

        /// <summary>Initializes a new instance of the <see cref="ModuleControlControllerAdapter"/> class.</summary>
        [Obsolete("Deprecated in DotNetNuke 10.2.3. Please use overload with IHostSettings. Scheduled removal in v12.0.0.")]
        public ModuleControlControllerAdapter()
            : this(null)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="ModuleControlControllerAdapter"/> class.</summary>
        /// <param name="hostSettings">The host settings.</param>
        public ModuleControlControllerAdapter(IHostSettings hostSettings)
        {
            this.hostSettings = hostSettings ?? Globals.GetCurrentServiceProvider().GetRequiredService<IHostSettings>();
        }

        /// <inheritdoc />
        public ModuleControlInfo GetModuleControlByControlKey(string controlKey, int moduleDefID)
        {
            return ModuleControlController.GetModuleControlByControlKey(this.hostSettings, controlKey, moduleDefID);
        }

        /// <inheritdoc />
        protected override Func<IModuleControlController> GetFactory()
        {
            return () => Globals.GetCurrentServiceProvider().GetRequiredService<IModuleControlController>();
        }
    }
}
