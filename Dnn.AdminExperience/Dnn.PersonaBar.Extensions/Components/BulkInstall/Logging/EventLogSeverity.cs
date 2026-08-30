// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.Extensions.Components.BulkInstall.Logging
{
    using Dnn.PersonaBar.Extensions.Components.BulkInstall.DataAccess.Models;

    /// <summary>The severity of an <see cref="EventLog"/>.</summary>
    public enum EventLogSeverity
    {
        /// <summary>Information.</summary>
        Info = 0,

        /// <summary>Warning.</summary>
        Warning = 1,

        /// <summary>Alert.</summary>
        Alert = 2,

        /// <summary>Critical.</summary>
        Critical = 3,
    }
}
