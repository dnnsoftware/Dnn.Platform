// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.UI.Modules.Html5
{
    using System;
    using System.Web.UI;

    using DotNetNuke.Abstractions.ClientResources;
    using DotNetNuke.Abstractions.Modules;
    using DotNetNuke.Common;
    using DotNetNuke.Entities.Modules;
    using Microsoft.Extensions.DependencyInjection;

    /// <summary>Module control factory for HTML modules.</summary>
    public class Html5ModuleControlFactory : BaseModuleControlFactory
    {
        private readonly IBusinessControllerProvider businessControllerProvider;
        private readonly IClientResourceController clientResourceController;

        /// <summary>Initializes a new instance of the <see cref="Html5ModuleControlFactory"/> class.</summary>
        [Obsolete("Deprecated in DotNetNuke 10.0.0. Please use overload with IBusinessControllerProvider. Scheduled removal in v12.0.0.")]
        public Html5ModuleControlFactory()
            : this(null, null)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="Html5ModuleControlFactory"/> class.</summary>
        /// <param name="businessControllerProvider">The business controller provider.</param>
        [Obsolete("Deprecated in DotNetNuke 10.0.0. Please use overload with IClientResourceController. Scheduled removal in v12.0.0.")]
        public Html5ModuleControlFactory(IBusinessControllerProvider businessControllerProvider)
            : this(businessControllerProvider, null)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="Html5ModuleControlFactory"/> class.</summary>
        /// <param name="businessControllerProvider">The business controller provider.</param>
        /// <param name="clientResourceController">The client resource controller.</param>
        public Html5ModuleControlFactory(IBusinessControllerProvider businessControllerProvider, IClientResourceController clientResourceController)
        {
            this.businessControllerProvider = businessControllerProvider ?? Globals.GetCurrentServiceProvider().GetRequiredService<IBusinessControllerProvider>();
            this.clientResourceController = clientResourceController ?? Globals.GetCurrentServiceProvider().GetRequiredService<IClientResourceController>();
        }

        /// <inheritdoc/>
        public override int Priority => 100;

        /// <inheritdoc/>
        public override bool SupportsControl(ModuleInfo moduleConfiguration, string controlSrc)
        {
            return controlSrc.EndsWith(".html", StringComparison.OrdinalIgnoreCase) ||
                   controlSrc.EndsWith(".htm", StringComparison.OrdinalIgnoreCase);
        }

        /// <inheritdoc/>
        public override Control CreateControl(TemplateControl containerControl, string controlKey, string controlSrc)
        {
            return new Html5HostControl("~/" + controlSrc, this.businessControllerProvider, this.clientResourceController);
        }

        /// <inheritdoc/>
        public override Control CreateModuleControl(TemplateControl containerControl, ModuleInfo moduleConfiguration)
        {
            return this.CreateControl(containerControl, string.Empty, moduleConfiguration.ModuleControl.ControlSrc);
        }

        /// <inheritdoc/>
        public override Control CreateSettingsControl(TemplateControl containerControl, ModuleInfo moduleConfiguration, string controlSrc)
        {
            return this.CreateControl(containerControl, string.Empty, controlSrc);
        }
    }
}
