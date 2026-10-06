// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.Extensions.Components.BulkInstall.Exceptions
{
    using System;
    using System.Runtime.Serialization;

    using Dnn.PersonaBar.Extensions.Components.BulkInstall.DataAccess.Models;

    /// <summary>An exception indicating that a <see cref="Setting"/> could not be found.</summary>
    [Serializable]
    public class SettingNotFoundException : Exception
    {
        /// <summary>Initializes a new instance of the <see cref="SettingNotFoundException"/> class.</summary>
        public SettingNotFoundException()
        {
        }

        /// <summary>Initializes a new instance of the <see cref="SettingNotFoundException"/> class with a specified error message.</summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        public SettingNotFoundException(string message)
            : base(message)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="SettingNotFoundException"/> class with a specified error message and a reference to the inner exception that is the cause of this exception.</summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="innerException">The exception that is the cause of the current exception, or <see langword="null"/> if no inner exception is specified.</param>
        public SettingNotFoundException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="SettingNotFoundException"/> class with serialized data.</summary>
        /// <param name="info">The <see cref="SerializationInfo"/> that holds the serialized object data about the exception being thrown.</param>
        /// <param name="context">The <see cref="StreamingContext"/> that contains contextual information about the source or destination.</param>
        public SettingNotFoundException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        /// <summary>Creates a new <see cref="SettingNotFoundException"/> instance.</summary>
        /// <param name="group">The setting group.</param>
        /// <param name="key">The setting key.</param>
        /// <returns>A new <see cref="SettingNotFoundException"/> instance.</returns>
        public static SettingNotFoundException Create(string group, string key)
        {
            return new SettingNotFoundException($"Setting in group '{group}' with key '{key}' was not found.");
        }
    }
}
