using System;

namespace DotNetNuke.Entities.Tabs.TabVersions.Exceptions
{
    /// <summary>
    /// Exception to notify error about managing tab versions
    /// </summary>
    public class DnnTabVersionException : ApplicationException
    {
        /// <summary>
        ///   Constructs an instance of <see cref = "ApplicationException" /> class with the specified message.
        /// </summary>
        /// <param name = "message">The message to associate with the exception</param>
        public DnnTabVersionException(string message) : base(message)
        {
        }

        /// <summary>
        ///   Constructs an instance of <see cref = "ApplicationException" /> class with the specified message and
        ///   inner exception.
        /// </summary>
        /// <param name = "message">The message to associate with the exception</param>
        /// <param name = "innerException">The exception which caused this error</param>
        public DnnTabVersionException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
