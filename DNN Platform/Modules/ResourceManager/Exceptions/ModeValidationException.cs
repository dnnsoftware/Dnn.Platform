﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace Dnn.Modules.ResourceManager.Exceptions
{
    using System;

    /// <summary>
    /// Thrown when the resource manager is not in the expected mode for the requested operation.
    /// </summary>
    public class ModeValidationException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ModeValidationException"/> class.
        /// </summary>
        /// <param name="message">The exception message.</param>
        public ModeValidationException(string message)
            : base(message)
        {
        }
    }
}
