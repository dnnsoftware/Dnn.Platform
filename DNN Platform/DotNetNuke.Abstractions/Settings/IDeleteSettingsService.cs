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
        /// Deletes the specified setting.
        /// </summary>
        /// <param name="key">The key to delete from the settings.</param>
        /// <param name="clearCache">If true the cache will be reset and the settings will be reloaded.</param>
        void Delete(string key, bool clearCache);

        /// <summary>
        /// Deletes all the settings.
        /// </summary>
        /// <param name="clearCache">If true the cache will be reset and the settings will be reloaded.</param>
        void DeleteAll(bool clearCache);
    }
}
