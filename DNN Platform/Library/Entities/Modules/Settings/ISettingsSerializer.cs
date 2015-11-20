namespace DotNetNuke.Entities.Modules.Settings
{
    public interface ISettingsSerializer<T>
    {
        string Serialize(T value);
        T Deserialize(string value);
    }
}
