// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.Library.AppEvents
{
    /// <summary>
    /// This interface defines methods that need to be called at various points during
    /// the application lifecycle. All modules that need to have any housekeeping applied
    /// during these events, need to create a concrete class that extends this interface.
    /// The main application start event will enumerate these (through reflection) and
    /// call the related methods in these classes.
    /// </summary>
    public interface IAppEvents
    {
        /// <summary>
        /// Method called after the application starts to perform any required startup actions by the implemetor.
        /// </summary>
        /// <remarks>This method must not used multi-threading and must perform it's task as fast as possible.</remarks>
        void ApplicationBegin();

        /// <summary>
        /// Method called before the application stops to perform any required shutdown actions by the implemetor.
        /// </summary>
        /// <remarks>This method must not used multi-threading and must perform it's task as fast as possible.</remarks>
        void ApplicationEnd();
    }
}
