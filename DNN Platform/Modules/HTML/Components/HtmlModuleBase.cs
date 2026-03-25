// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Modules.Html.Components
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web;

    using DotNetNuke.Entities.Modules;

    using Microsoft.Extensions.DependencyInjection;

    public class HtmlModuleBase : PortalModuleBase
    {
        private HtmlModuleSettings settings;

        /// <summary>Initializes a new instance of the <see cref="HtmlModuleBase"/> class.</summary>
        [Obsolete("Deprecated in DotNetNuke 10.2.4. Please use overload with IHostSettings. Scheduled removal in v12.0.0.")]
        public HtmlModuleBase()
            : this(null)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="HtmlModuleBase"/> class.</summary>
        /// <param name="settingsRepository">The settings repository.</param>
        public HtmlModuleBase(HtmlModuleSettingsRepository settingsRepository)
        {
            this.SettingsRepository = settingsRepository ?? this.DependencyProvider.GetRequiredService<HtmlModuleSettingsRepository>();
        }

        public new HtmlModuleSettings Settings
        {
            get => this.settings ??= this.SettingsRepository.GetSettings(this.ModuleConfiguration);
            set => this.settings = value;
        }

        /// <summary>Gets the settings repository.</summary>
        protected HtmlModuleSettingsRepository SettingsRepository { get; }
    }
}
