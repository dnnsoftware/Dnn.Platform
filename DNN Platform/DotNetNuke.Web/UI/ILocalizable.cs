namespace DotNetNuke.Web.UI
{
    public interface ILocalizable
    {
        string LocalResourceFile { get; set; }
        bool Localize { get; set; }

        void LocalizeStrings();
    }
}
