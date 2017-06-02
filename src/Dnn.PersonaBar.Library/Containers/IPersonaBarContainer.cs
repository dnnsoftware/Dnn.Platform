using System.Collections.Generic;
using System.Web.UI;
using Dnn.PersonaBar.Library.Model;

namespace Dnn.PersonaBar.Library.Containers
{
    /// <summary>
    /// Persona Bar Container.
    /// </summary>
    public interface IPersonaBarContainer
    {
        /// <summary>
        /// Items allowed to defined in root level.
        /// </summary>
        IList<string> RootItems { get; } 

        /// <summary>
        /// Indicate whether persona bar is available.
        /// </summary>
        bool Visible { get; }

        /// <summary>
        /// Initialize the persona bar container.
        /// </summary>
        /// <param name="personaBarControl">The Persona Bar Container control.</param>
        void Initialize(UserControl personaBarControl);

        /// <summary>
        /// Get Persona Bar Settings.
        /// </summary>
        /// <returns></returns>
        IDictionary<string, object> GetConfiguration();

        void FilterMenu(PersonaBarMenu menu);
    }
}
