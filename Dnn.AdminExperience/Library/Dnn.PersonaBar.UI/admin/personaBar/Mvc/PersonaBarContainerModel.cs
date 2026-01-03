// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace Dnn.PersonaBar.UI.Controllers
{
    /// <summary>
    /// Represents the model for the Persona Bar container.
    /// </summary>
    public class PersonaBarContainerModel
    {
        /// <summary>
        /// Gets the settings for the Persona Bar.
        /// </summary>
        public string PersonaBarSettings { get; internal set; }

        /// <summary>
        /// Gets the build number of the application.
        /// </summary>
        public string BuildNumber { get; internal set; }

        /// <summary>
        /// Gets the application path.
        /// </summary>
        public string AppPath { get; internal set; }

        /// <summary>
        /// Gets a value indicating whether the Persona Bar is visible.
        /// </summary>
        public bool Visible { get; internal set; }
    }
}
