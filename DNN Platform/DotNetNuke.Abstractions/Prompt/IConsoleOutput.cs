// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Abstractions.Prompt;

/// <summary>A field to be output which could contain plain text or HTML.</summary>
public interface IConsoleOutput
{
    /// <summary>Gets a value indicating whether <see cref="Output"/> is HTML or plain text.</summary>
    public bool IsHtml { get; }

    /// <summary>Gets the output.</summary>
    public string Output { get; }
}
