#region Usings

using System.ComponentModel;

#endregion

namespace DotNetNuke.Services.Installer
{
    [TypeConverter(typeof (EnumConverter))]
    public enum InstallFileType
    {
        AppCode,
        Ascx,
        Assembly,
        CleanUp,
        Language,
        Manifest,
        Other,
        Resources,
        Script
    }
}
