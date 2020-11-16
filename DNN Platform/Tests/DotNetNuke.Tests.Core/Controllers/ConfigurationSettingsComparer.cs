// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Tests.Core.Controllers
{
    using System;
    using System.Collections.Generic;

    using DotNetNuke.Entities;

    public class ConfigurationSettingsComparer : EqualityComparer<ConfigurationSetting>
    {
        public override bool Equals(ConfigurationSetting x, ConfigurationSetting y)
        {
            if (x.Key == y.Key && x.Value == y.Value && x.IsSecure == y.IsSecure)
            {
                return true;
            }

            return false;
        }

        public override int GetHashCode(ConfigurationSetting obj)
        {
            throw new NotImplementedException();
        }
    }
}
