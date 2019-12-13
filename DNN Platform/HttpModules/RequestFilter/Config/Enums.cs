namespace DotNetNuke.HttpModules.RequestFilter
{
    public enum RequestFilterRuleType
    {
        Redirect,
        PermanentRedirect,
        NotFound
    }

    public enum RequestFilterOperatorType
    {
        Equal,
        NotEqual,
        Regex
    }
}
