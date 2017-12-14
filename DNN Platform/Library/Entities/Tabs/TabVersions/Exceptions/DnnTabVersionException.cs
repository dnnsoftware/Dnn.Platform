#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2017
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
