using System.ComponentModel;

namespace DotNetNuke.ExtensionPoints
{
    public interface IExtensionPointData
    {
        string Module { get; }

        string Name { get; }

        string Group { get; }

        int Priority { get; }

        [DefaultValue(false)]
        bool DisableOnHost { get; }

        [DefaultValue(false)]
        bool DisableUnauthenticated { get; }
    }
}
