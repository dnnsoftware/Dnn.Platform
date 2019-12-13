using System.Collections.Generic;
using Dnn.PersonaBar.Library.Model;

namespace Dnn.PersonaBar.Library.Repository
{
    /// <summary>
    /// Interface responsible for persona bar extensions.
    /// </summary>
    public interface IPersonaBarExtensionRepository
    {
        /// <summary>
        /// save extension.
        /// </summary>
        /// <param name="extension"></param>
        void SaveExtension(PersonaBarExtension extension);

        /// <summary>
        /// delete extension.
        /// </summary>
        /// <param name="extension"></param>
        void DeleteExtension(PersonaBarExtension extension);

        /// <summary>
        /// delete extension.
        /// </summary>
        /// <param name="identifier"></param>
        void DeleteExtension(string identifier);

        /// <summary>
        /// get persona bar extensions.
        /// </summary>
        /// <returns></returns>
        IList<PersonaBarExtension> GetExtensions();

        /// <summary>
        /// get persona bar extensions for menu.
        /// </summary>
        /// <returns></returns>
        IList<PersonaBarExtension> GetExtensions(int menuId);

    }
}
