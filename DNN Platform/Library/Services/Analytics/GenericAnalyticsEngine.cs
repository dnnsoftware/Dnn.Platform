// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

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
            return this.ReplaceTokens(scriptTemplate);
        }
    }
}
