// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.Library.Containers
{
    using System.Collections.Generic;
    using System.Web.UI;

    using Dnn.PersonaBar.Library.Model;

    /// <summary>
    /// Persona Bar Container.
    /// </summary>
    public interface IPersonaBarContainer
    {
        /// <summary>
        /// Gets items allowed to defined in root level.
        /// </summary>
        IList<string> RootItems { get; }

        /// <summary>
        /// Gets a value indicating whether indicate whether persona bar is available.
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
