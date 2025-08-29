// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Abstractions.ClientResources
{
    /// <summary>Defines the CORS settings to use when including a script or stylesheet from a different origin.</summary>
    public enum CrossOrigin
    {
        /// <summary>No CORS setting is specified.</summary>
        None,

        /// <summary>Anonymous CORS request.</summary>
        Anonymous,

        /// <summary>Credentials CORS request.</summary>
        UseCredentials,
    }
}
