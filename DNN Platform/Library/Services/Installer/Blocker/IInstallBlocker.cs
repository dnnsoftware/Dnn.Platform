namespace DotNetNuke.Services.Installer.Blocker
{
    /// <summary>
    /// This interface ...
    /// </summary>
    public interface IInstallBlocker
    {
        void RegisterInstallBegining();
        void RegisterInstallEnd();
        bool IsInstallInProgress();
    }
}
