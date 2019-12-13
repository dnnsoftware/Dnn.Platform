// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
#region Usings

using System;

#endregion

namespace DotNetNuke.Services.Installer.Log
{
    /// -----------------------------------------------------------------------------
    /// <summary>
    /// The LogEntry class provides a single entry for the Installer Log
    /// </summary>
    /// <remarks>
    /// </remarks>
    /// -----------------------------------------------------------------------------
    [Serializable]
    public class LogEntry
    {
        private readonly string _description;

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// This Constructor builds a LogEntry from its type and description
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <param name="description">The description (detail) of the entry</param>
        /// <param name="type">The type of LogEntry</param>
        /// -----------------------------------------------------------------------------
        public LogEntry(LogType type, string description)
        {
            Type = type;
            _description = description;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets the type of LogEntry
        /// </summary>
        /// <value>A LogType</value>
        /// -----------------------------------------------------------------------------
        public LogType Type { get; private set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets the description of LogEntry
        /// </summary>
        /// <value>A String</value>
        /// -----------------------------------------------------------------------------
        public string Description
        {
            get
            {
                if (_description == null)
                {
                    return "...";
                }
                
                return _description;
            }
        }

        public override string ToString()
        {
            return string.Format("{0}:  {1}", Type, Description);
        }
    }
}
