namespace Dnn.PersonaBar.Pages.Components
{
    /// <summary>
    /// Theme controller
    /// </summary>
    public interface IDefaultPortalThemeController
    {
        /// <summary>
        /// Returns the default current portal container
        /// </summary>
        /// <returns></returns>
        string GetDefaultPortalContainer();

        /// <summary>
        /// Returns the default current portal layout
        /// </summary>
        /// <returns></returns>
        string GetDefaultPortalLayout();
    }
}
