namespace DotNetNuke.Abstractions.Settings
{
    /// <summary>
    /// The Delete Settings Service is used by implementations
    /// of the <see cref="ISettingsService"/> to de-couple
    /// delete business rules. The <see cref="IDeleteSettingsService"/>
    /// is not designed to work with Dependency Injection, you
    /// will need to resolve the implementation of <see cref="ISettingsService"/>.
    /// </summary>
    public interface IDeleteSettingsService
    {
        /// <summary>
        /// Deletes the setting.
        /// </summary>
        /// <param name="key">The setting key to delete.</param>
        void Delete(string key);

        void Delete(string key, bool clearCache);

        void DeleteAll();

        void DeleteAll(bool clearCache);
    }
}
