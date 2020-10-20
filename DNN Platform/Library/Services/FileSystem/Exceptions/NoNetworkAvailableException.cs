﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Services.FileSystem
{
    using System;
    using System.Runtime.Serialization;

    [Serializable]
    public class NoNetworkAvailableException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NoNetworkAvailableException"/> class.
        /// </summary>
        public NoNetworkAvailableException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NoNetworkAvailableException"/> class.
        /// </summary>
        /// <param name="message"></param>
        public NoNetworkAvailableException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NoNetworkAvailableException"/> class.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="inner"></param>
        public NoNetworkAvailableException(string message, Exception inner)
            : base(message, inner)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NoNetworkAvailableException"/> class.
        /// </summary>
        /// <param name="info"></param>
        /// <param name="context"></param>
        public NoNetworkAvailableException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
