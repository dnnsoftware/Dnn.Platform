#region Copyright

// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2013
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
using System.Globalization;

#endregion

namespace DotNetNuke.Common
{
    /// <summary>Specifies that a certain condition is an error</summary>
    public static class Guard
    {
        /// <summary>
        ///     Indicates that the given <paramref name="condition" /> must not be <c>true</c>, throwing an
        ///     <see cref="InvalidOperationException" /> if it is.
        /// </summary>
        /// <param name="condition">if set to <c>true</c>, throws an <see cref="InvalidOperationException" />.</param>
        /// <param name="message">
        ///     A message that describes the error condition, as a composite format string (i.e. with <c>{0}</c>
        ///     placeholders, like <see cref="string.Format(string,object[])" />).
        /// </param>
        /// <param name="args">An array of objects to fill in the placeholders in <paramref name="message" />.</param>
        /// <exception cref="InvalidOperationException">When <paramref name="condition" /> is <c>true</c></exception>
        public static void Against(bool condition, string message, params object[] args)
        {
            Against(condition, string.Format(CultureInfo.CurrentUICulture, message, args));
        }

        /// <summary>
        ///     Indicates that the given <paramref name="condition" /> must not be <c>true</c>, throwing an
        ///     <see cref="InvalidOperationException" /> if it is.
        /// </summary>
        /// <param name="condition">if set to <c>true</c>, throws an <see cref="InvalidOperationException" />.</param>
        /// <param name="message">A message that describes the error condition.</param>
        /// <exception cref="InvalidOperationException">When <paramref name="condition" /> is <c>true</c></exception>
        public static void Against(bool condition, string message)
        {
            if ((condition))
            {
                throw new InvalidOperationException(message);
            }
        }
    }
}