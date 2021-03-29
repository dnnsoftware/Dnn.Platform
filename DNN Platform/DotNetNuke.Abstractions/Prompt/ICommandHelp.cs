// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Abstractions.Prompt
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// This is used to send the result back to the client when a user asks help for a command.
    /// </summary>
    [Obsolete("Deprecated in favor of IDnnCommandHelp. Will be removed in a future release.", false)]
    public interface ICommandHelp : IDnnCommandHelp
    {
    }
}
