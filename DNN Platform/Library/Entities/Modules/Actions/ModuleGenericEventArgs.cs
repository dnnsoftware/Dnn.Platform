// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Entities.Modules.Actions
{
    /// <summary>
    /// Event arguments for a generic module event.
    /// </summary>
    public class ModuleGenericEventArgs : ModuleEventArgs
    {
        /// <summary>
        /// Gets or sets the name of the event.
        /// </summary>
        public string EventName { get; set; }

        /// <summary>
        /// Gets or sets the payload for the event.
        /// </summary>
        public object Payload { get; set; }
    }
}
