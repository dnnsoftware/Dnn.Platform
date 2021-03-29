// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

using System;

namespace DotNetNuke.Abstractions.Prompt
{
    using System.Collections.Generic;

    /// <summary>
    /// The repository handles retrieving of commands from the entire DNN installation.
    /// </summary>
    [Obsolete("Deprecated in favor of IDnnCommandRepository. Will be removed in a future release.", false)]
    public interface ICommandRepository : IDnnCommandRepository
    {
    }
}
