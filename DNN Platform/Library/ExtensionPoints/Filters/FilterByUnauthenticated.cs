namespace DotNetNuke.ExtensionPoints.Filters
{
    public class FilterByUnauthenticated : IExtensionPointFilter
    {
        private readonly bool isAuthenticated;

        public FilterByUnauthenticated(bool isAuthenticated)
        {
            this.isAuthenticated = isAuthenticated;
        }

        public bool Condition(IExtensionPointData m)
        {
            return isAuthenticated || !m.DisableUnauthenticated;
        }
    }
}
