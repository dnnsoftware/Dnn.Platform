// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Mobile
{
    using System;

    public interface IMatchRule
    {
        /// <summary>
        /// Gets primary Id.
        /// </summary>
        int Id { get; }

        /// <summary>
        /// Gets or sets capbility name.
        /// </summary>
        string Capability { get; set; }

        /// <summary>
        /// Gets or sets reg expression to match the request.
        /// </summary>
        string Expression { get; set; }
    }
}
