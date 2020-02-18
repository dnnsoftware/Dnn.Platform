// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
namespace DotNetNuke.Services.Analytics
{
    public class GenericAnalyticsEngine : AnalyticsEngineBase
    {
        public override string EngineName
        {
            get
            {
                return "GenericAnalytics";
            }
        }

        public override string RenderScript(string scriptTemplate)
        {
            return ReplaceTokens(scriptTemplate);
        }
    }
}
