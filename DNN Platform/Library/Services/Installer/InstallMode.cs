#region Usings

using System.ComponentModel;

#endregion

namespace DotNetNuke.Services.Installer
{
    [TypeConverter(typeof (EnumConverter))]
    public enum InstallMode
    {
        Install,
        ManifestOnly,
        UnInstall
    }
}
