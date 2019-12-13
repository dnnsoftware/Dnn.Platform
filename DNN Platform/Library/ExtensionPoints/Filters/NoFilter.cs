namespace DotNetNuke.ExtensionPoints.Filters
{
    public class NoFilter : IExtensionPointFilter
    {        
        public bool Condition(IExtensionPointData m)
        {
            return true;
        }
    }
}
