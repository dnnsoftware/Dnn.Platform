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
