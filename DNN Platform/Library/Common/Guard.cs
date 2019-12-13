// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
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
