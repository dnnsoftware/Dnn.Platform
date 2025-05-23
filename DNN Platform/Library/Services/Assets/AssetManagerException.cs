﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Services.Assets
{
    using System;

    public class AssetManagerException : Exception
    {
        /// <summary>Initializes a new instance of the <see cref="AssetManagerException"/> class.</summary>
        /// <param name="message">The message that describes the error.</param>
        public AssetManagerException(string message)
            : base(message)
        {
        }
    }
}
