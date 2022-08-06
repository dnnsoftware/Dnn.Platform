// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Entities.Users
{
    using System;

    [Serializable]
    public class InvalidPasswordException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidPasswordException"/> class.
        /// </summary>
        public InvalidPasswordException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidPasswordException"/> class.
        /// </summary>
        /// <param name="message"></param>
        public InvalidPasswordException(string message)
            : base(message)
        {
        }
    }
}
