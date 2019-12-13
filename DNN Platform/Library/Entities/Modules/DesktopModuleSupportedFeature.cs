using System;

namespace DotNetNuke.Entities.Modules
{
    /// -----------------------------------------------------------------------------
    /// Project	 : DotNetNuke
    /// Namespace: DotNetNuke.Entities.Modules
    /// Class	 : DesktopModuleSupportedFeature
    /// -----------------------------------------------------------------------------
    /// <summary>
    /// The DesktopModuleSupportedFeature enum provides an enumeration of Supported
    /// Features
    /// </summary>
    /// -----------------------------------------------------------------------------
    [Flags]
    public enum DesktopModuleSupportedFeature
    {
        IsPortable = 1,
        IsSearchable = 2,
        IsUpgradeable = 4
    }
}
