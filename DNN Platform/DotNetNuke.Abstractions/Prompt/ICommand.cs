// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Abstractions.Prompt
{
    using System;

    /// <summary>
    /// This is used to retrieve and keep a list of all commands found in the installation.
    /// </summary>
    [Obsolete("Deprecated in v9.9.1. Use IDnnCommand instead. Will be removed in v11.", false)]
    public interface ICommand : IDnnCommand
    {
    }
}
