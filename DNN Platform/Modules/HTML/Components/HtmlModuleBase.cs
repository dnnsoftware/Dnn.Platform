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

    public class HtmlModuleBase : PortalModuleBase
    {
        private HtmlModuleSettings _settings;

        public new HtmlModuleSettings Settings
        {
            get
            {
                if (this._settings == null)
                {
                    var repo = new HtmlModuleSettingsRepository();
                    this._settings = repo.GetSettings(this.ModuleConfiguration);
                }

                return this._settings;
            }

            set { this._settings = value; }
        }
    }
}
