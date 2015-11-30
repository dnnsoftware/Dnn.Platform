namespace DotNetNuke.Entities.Modules.Settings
{
    public interface ISettingsRepository<T> where T : class
    {
        T GetSettings(ModuleInfo moduleContext);
        void SaveSettings(ModuleInfo moduleContext, T settings);
    }
}
