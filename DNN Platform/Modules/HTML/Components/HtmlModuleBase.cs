// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

using DotNetNuke.Entities.Modules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DotNetNuke.Modules.Html.Components
{
    public class HtmlModuleBase : PortalModuleBase
    {
        private HtmlModuleSettings _settings;
        public new HtmlModuleSettings Settings
        {
            get
            {
                if (_settings == null)
                {
                    var repo = new HtmlModuleSettingsRepository();
                    _settings = repo.GetSettings(this.ModuleConfiguration);
                }
                return _settings;
            }
            set { _settings = value; }
        }
    }
}
