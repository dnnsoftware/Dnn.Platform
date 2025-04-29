// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Installer.Log;

using System;

/// <summary>The LogEntry class provides a single entry for the Installer Log.</summary>
[Serializable]
public class LogEntry
{
    private readonly string description;

    /// <summary>
    /// Initializes a new instance of the <see cref="LogEntry"/> class.
    /// This Constructor builds a LogEntry from its type and description.
    /// </summary>
    /// <param name="description">The description (detail) of the entry.</param>
    /// <param name="type">The type of LogEntry.</param>
    public LogEntry(LogType type, string description)
    {
        this.Type = type;
        this.description = description;
    }

    /// <summary>Gets the description of LogEntry.</summary>
    /// <value>A String.</value>
    public string Description
    {
        get
        {
            if (this.description == null)
            {
                return "...";
            }

            return this.description;
        }
    }

    /// <summary>Gets the type of LogEntry.</summary>
    /// <value>A LogType.</value>
    public LogType Type { get; private set; }

    /// <inheritdoc/>
    public override string ToString()
    {
        return string.Format("{0}:  {1}", this.Type, this.Description);
    }
}
