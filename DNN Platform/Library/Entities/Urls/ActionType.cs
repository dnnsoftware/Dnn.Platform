namespace DotNetNuke.Entities.Urls
{
    public enum ActionType
    {
        IgnoreRequest,
        Continue,
        Redirect302Now,
        Redirect301,
        CheckFor301,
        Redirect302,
        Output404,
        Output500
    }
}
