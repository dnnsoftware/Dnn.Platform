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

namespace DotNetNuke.Services.Tokens
{
    /// <summary>
    /// Scope informs the property access classes about the planned usage of the token
    /// </summary>
    /// <remarks>
    /// The result of a token replace operation depends on the current context, privacy settings
    /// and the current scope. The scope should be the lowest scope needed for the current purpose.
    /// The property access classes should evaluate and use the scope before returning a value.
    /// </remarks>
    public enum Scope
    {
        /// <summary>
        /// Only access to Date and Time
        /// </summary>
        NoSettings = 0,
        /// <summary>
        /// Tokens for Host, Portal, Tab (, Module), user name
        /// </summary>
        Configuration = 1,
        /// <summary>
        /// Configuration, Current User data and user data allowed for registered members
        /// </summary>
        DefaultSettings = 2,
        /// <summary>
        /// System notifications to users and adminstrators
        /// </summary>
        SystemMessages = 3,
        /// <summary>
        /// internal debugging, error messages, logs
        /// </summary>
        Debug = 4
    }
}