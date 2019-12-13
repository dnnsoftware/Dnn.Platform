namespace DotNetNuke.ExtensionPoints.Filters
{
    public interface IExtensionPointFilter
    {
        bool Condition(IExtensionPointData m);
    }
}
