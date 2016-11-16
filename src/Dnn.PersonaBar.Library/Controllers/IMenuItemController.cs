using System.Collections.Generic;
using Dnn.PersonaBar.Library.Model;

namespace Dnn.PersonaBar.Library.Controllers
{
    public interface IMenuItemController
    {
        /// <summary>
        /// Update menu item parameters.
        /// </summary>
        /// <param name="menuItem"></param>
        void UpdateParameters(MenuItem menuItem);

        /// <summary>
        /// whether the menu item visible in current context.
        /// </summary>
        /// <param name="menuItem"></param>
        /// <returns></returns>
        bool Visible(MenuItem menuItem);

        /// <summary>
        /// get menu settings.
        /// </summary>
        /// <param name="menuItem"></param>
        /// <returns></returns>
        IDictionary<string, object> GetSettings(MenuItem menuItem);
    }
}
