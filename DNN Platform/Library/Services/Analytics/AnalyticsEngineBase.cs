// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
#region Usings

using DotNetNuke.Entities.Users;
using DotNetNuke.Services.Analytics.Config;
using DotNetNuke.Services.Tokens;

#endregion

namespace DotNetNuke.Services.Analytics
{
    public abstract class AnalyticsEngineBase
    {
        public abstract string EngineName { get; }

        public abstract string RenderScript(string scriptTemplate);

        public string ReplaceTokens(string s)
        {
            var tokenizer = new TokenReplace();
            tokenizer.AccessingUser = UserController.Instance.GetCurrentUserInfo();
            tokenizer.DebugMessages = false;
            return (tokenizer.ReplaceEnvironmentTokens(s));
        }

        public AnalyticsConfiguration GetConfig()
        {
            return AnalyticsConfiguration.GetConfig(EngineName);
        }

        public virtual string RenderCustomScript(AnalyticsConfiguration config)
        {
            return "";
        }
    }
}
