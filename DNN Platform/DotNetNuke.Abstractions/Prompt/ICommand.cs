// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

using System;

namespace DotNetNuke.Abstractions.Prompt
{
    /// <summary>
    /// This is used to retrieve and keep a list of all commands found in the installation.
    /// </summary>
    [Obsolete("Deprecated in favor of IDnnCommand. Will be removed in a future release.", false)]
    public interface ICommand : IDnnCommand
    {
    }
}
