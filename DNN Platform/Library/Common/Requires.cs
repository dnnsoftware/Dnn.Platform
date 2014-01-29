#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2014
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

using DotNetNuke.Services.Localization;

#endregion

namespace DotNetNuke.Common
{
	/// <summary>
	/// Assert Class.
	/// </summary>
    public static class Requires
    {
        #region "Public Methods"

		/// <summary>
		/// Determines whether argValue is type of T.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="argName">Name of the arg.</param>
		/// <param name="argValue">The arg value.</param>
		/// <exception cref="ArgumentException"></exception>
        public static void IsTypeOf<T>(string argName, object argValue)
        {
            if (!((argValue) is T))
            {
                throw new ArgumentException(Localization.GetExceptionMessage("ValueMustBeOfType", "The argument '{0}' must be of type '{1}'.", argName, typeof (T).FullName));
            }
        }

		/// <summary>
		/// Determines whether argValue is less than zero.
		/// </summary>
		/// <param name="argName">Name of the arg.</param>
		/// <param name="argValue">The arg value.</param>
		/// <exception cref="ArgumentException"></exception>
        public static void NotNegative(string argName, int argValue)
        {
            if (argValue < 0)
            {
                throw new ArgumentOutOfRangeException(argName, Localization.GetExceptionMessage("ValueCannotBeNegative", "The argument '{0}' cannot be negative.", argName));
            }
        }

		/// <summary>
		/// Determines whether the argValue is null.
		/// </summary>
		/// <param name="argName">Name of the arg.</param>
		/// <param name="argValue">The arg value.</param>
		/// <exception cref="ArgumentException"></exception>
        public static void NotNull(string argName, object argValue)
        {
            if (argValue == null)
            {
                throw new ArgumentNullException(argName);
            }
        }

		/// <summary>
		/// Determines whether the argValue is null or empty.
		/// </summary>
		/// <param name="argName">Name of the arg.</param>
		/// <param name="argValue">The arg value.</param>
		/// <exception cref="ArgumentException"></exception>
        public static void NotNullOrEmpty(string argName, string argValue)
        {
            if (string.IsNullOrEmpty(argValue))
            {
                throw new ArgumentException(Localization.GetExceptionMessage("ArgumentCannotBeNullOrEmpty", "The argument '{0}' cannot be null or empty.", argName), argName);
            }
        }

		/// <summary>
		/// Determins whether propertyValye is not null or empty.
		/// </summary>
		/// <param name="argName">Name of the arg.</param>
		/// <param name="argProperty">The arg property.</param>
		/// <param name="propertyValue">The property value.</param>
		/// <exception cref="ArgumentException"></exception>
        public static void PropertyNotNullOrEmpty(string argName, string argProperty, string propertyValue)
        {
            if (string.IsNullOrEmpty(propertyValue))
            {
                throw new ArgumentException(argName,
                                            Localization.GetExceptionMessage("PropertyCannotBeNullOrEmpty", "The property '{1}' in object '{0}' cannot be null or empty.", argName, argProperty));
            }
        }

		/// <summary>
		/// Determines whether propertyValue is less than zero.
		/// </summary>
		/// <param name="argName">Name of the arg.</param>
		/// <param name="argProperty">The arg property.</param>
		/// <param name="propertyValue">The property value.</param>
		/// <exception cref="ArgumentException"></exception>
        public static void PropertyNotNegative(string argName, string argProperty, int propertyValue)
        {
            if (propertyValue < 0)
            {
                throw new ArgumentOutOfRangeException(argName,
                                                      Localization.GetExceptionMessage("PropertyCannotBeNegative", "The property '{1}' in object '{0}' cannot be negative.", argName, argProperty));
            }
        }

		/// <summary>
		/// Determines whether propertyValue equal to testValue.
		/// </summary>
		/// <typeparam name="TValue">The type of the value.</typeparam>
		/// <param name="argName">Name of the arg.</param>
		/// <param name="argProperty">The arg property.</param>
		/// <param name="propertyValue">The property value.</param>
		/// <param name="testValue">The test value.</param>
		/// <exception cref="ArgumentException"></exception>
        public static void PropertyNotEqualTo<TValue>(string argName, string argProperty, TValue propertyValue, TValue testValue) where TValue : IEquatable<TValue>
        {
            if (propertyValue.Equals(testValue))
            {
                throw new ArgumentException(argName, Localization.GetExceptionMessage("PropertyNotEqualTo", "The property '{1}' in object '{0}' is invalid.", argName, argProperty));
            }
        }

        #endregion
    }

    public static class Arg
    {
        #region "Public Methods"

        [Obsolete("Deprecated in DNN 5.4.0. Replaced by Requires.IsTypeOf()")]
        public static void IsTypeOf<T>(string argName, object argValue)
        {
            Requires.IsTypeOf<T>(argName, argValue);
        }

        [Obsolete("Deprecated in DNN 5.4.0. Replaced by Requires.NotNegative()")]
        public static void NotNegative(string argName, int argValue)
        {
            Requires.NotNegative(argName, argValue);
        }

        [Obsolete("Deprecated in DNN 5.4.0. Replaced by Requires.NotNull()")]
        public static void NotNull(string argName, object argValue)
        {
            Requires.NotNull(argName, argValue);
        }

        [Obsolete("Deprecated in DNN 5.4.0. Replaced by Requires.NotNullOrEmpty()")]
        public static void NotNullOrEmpty(string argName, string argValue)
        {
            Requires.NotNullOrEmpty(argName, argValue);
        }

        [Obsolete("Deprecated in DNN 5.4.0. Replaced by Requires.PropertyNotNullOrEmpty()")]
        public static void PropertyNotNullOrEmpty(string argName, string argProperty, string propertyValue)
        {
            Requires.PropertyNotNullOrEmpty(argName, argProperty, propertyValue);
        }

        [Obsolete("Deprecated in DNN 5.4.0. Replaced by Requires.PropertyNotNegative()")]
        public static void PropertyNotNegative(string argName, string argProperty, int propertyValue)
        {
            Requires.PropertyNotNegative(argName, argProperty, propertyValue);
        }

        [Obsolete("Deprecated in DNN 5.4.0. Replaced by Requires.PropertyNotEqualTo()")]
        public static void PropertyNotEqualTo<TValue>(string argName, string argProperty, TValue propertyValue, TValue testValue) where TValue : IEquatable<TValue>
        {
            Requires.PropertyNotEqualTo(argName, argProperty, propertyValue, testValue);
        }

        #endregion
    }
}
