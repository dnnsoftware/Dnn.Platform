// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.Api.Auth.ApiTokens
{
    using DotNetNuke.Entities.Modules.Settings;

    public class ApiTokenSettings
    {
        [HostSetting(Prefix = "ApiTokens_")]
        public int MaximumTimespan { get; set; } = 1;

        [HostSetting(Prefix = "ApiTokens_")]
        public string MaximumTimespanMeasure { get; set; } = "y";

        public bool ApiTokensEnabled => ApiTokenAuthMessageHandler.IsEnabled;

        public static ApiTokenSettings GetSettings()
        {
            var repo = new ApiTokenSettingsRepository();
            return repo.GetSettings(-1);
        }

        public void SaveSettings()
        {
            var repo = new ApiTokenSettingsRepository();
            repo.SaveSettings(-1, this);
        }
    }
}
