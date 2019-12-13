namespace DotNetNuke.UI.Modules
{
    /// -----------------------------------------------------------------------------
    /// Project	 : DotNetNuke
    /// Namespace: DotNetNuke.UI.Modules
    /// Class	 : ISettingsControl
    /// -----------------------------------------------------------------------------
    /// <summary>
    /// ISettingsControl provides a common Interface for Module Settings Controls
    /// </summary>
    /// -----------------------------------------------------------------------------
    public interface ISettingsControl : IModuleControl
    {
        void LoadSettings();

        void UpdateSettings();
    }
}
