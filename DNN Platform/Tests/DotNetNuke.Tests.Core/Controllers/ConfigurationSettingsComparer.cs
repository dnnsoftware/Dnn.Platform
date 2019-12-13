using System;
using System.Collections.Generic;

using DotNetNuke.Entities;

namespace DotNetNuke.Tests.Core.Controllers
{
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
