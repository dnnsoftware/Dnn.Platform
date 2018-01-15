#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2018
// by DotNetNuke Corporation
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated 
// documentation files (the "Software"), to deal in the Software without restriction, including without limitation 
// the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and 
// to permit persons to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or substantial portions 
// of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED 
// TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL 
// THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF 
// CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
// DEALINGS IN THE SOFTWARE.
#endregion
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
