// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Common
{
    using System;

    using DotNetNuke.Services.Localization;

    /// <summary>
    /// Assert Class.
    /// </summary>
    public static class Requires
    {
        /// <summary>
        /// Determines whether argValue is type of T.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="argName">Name of the arg.</param>
        /// <param name="argValue">The arg value.</param>
        /// <exception cref="ArgumentException"></exception>
        public static void IsTypeOf<T>(string argName, object argValue)
        {
            if (!(argValue is T))
            {
                throw new ArgumentException(Localization.GetExceptionMessage("ValueMustBeOfType", "The argument '{0}' must be of type '{1}'.", argName, typeof(T).FullName));
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
        /// Determines whether the argument is null.
        /// </summary>
        /// <param name="item">The object to test.</param>
        /// <exception cref="ArgumentNullException"></exception>
        public static void NotNull<T>(T item)
        {
            if (item == null)
            {
                throw new ArgumentNullException(typeof(T).Name);
            }
        }

        /// <summary>
        /// Determines whether the argValue is null.
        /// </summary>
        /// <param name="argName">Name of the arg.</param>
        /// <param name="argValue">The arg value.</param>
        /// <exception cref="ArgumentNullException"></exception>
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
        /// Determines whether the argValue is null, empty, or has whitespace only.
        /// </summary>
        /// <param name="argName">Name of the arg.</param>
        /// <param name="argValue">The arg value.</param>
        /// <exception cref="ArgumentException"></exception>
        public static void NotNullOrHasNoWhiteSpace(string argName, string argValue)
        {
            if (string.IsNullOrWhiteSpace(argValue))
            {
                throw new ArgumentException(Localization.GetExceptionMessage("ArgumentCannotBeNullOrEmpty", "The argument '{0}' cannot be null or empty.", argName), argName);
            }
        }

        /// <summary>
        /// Determines whether a property is negative.
        /// </summary>
        /// <param name="item">The object to test.</param>
        /// <param name="propertyName">Name of the property.</param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public static void PropertyNotNegative<T>(T item, string propertyName)
        {
            // Check first if the item is null
            NotNull(item);

            var type = typeof(T);
            var property = type.GetProperty(propertyName);
            var propertyValue = property.GetValue(item);

            var intValue = (int)propertyValue;

            if (intValue < 0)
            {
                throw new ArgumentOutOfRangeException(
                    propertyName,
                    Localization.GetExceptionMessage("PropertyCannotBeNegative", "The property '{1}' in object '{0}' cannot be negative.", typeof(T).Name, propertyName));
            }
        }

        /// <summary>
        /// Determines whether propertyValue is less than zero.
        /// </summary>
        /// <param name="argName">Name of the arg.</param>
        /// <param name="argProperty">The arg property.</param>
        /// <param name="propertyValue">The property value.</param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public static void PropertyNotNegative(string argName, string argProperty, int propertyValue)
        {
            if (propertyValue < 0)
            {
                throw new ArgumentOutOfRangeException(
                    argName,
                    Localization.GetExceptionMessage("PropertyCannotBeNegative", "The property '{1}' in object '{0}' cannot be negative.", argName, argProperty));
            }
        }

        /// <summary>
        /// Determines whether the argValue is null.
        /// </summary>
        /// <param name="argName">Name of the arg.</param>
        /// <param name="argValue">The arg value.</param>
        /// <exception cref="ArgumentNullException"></exception>
        public static void PropertyNotNull(string argName, string argValue)
        {
            NotNull(argName, argValue);
        }

        /// <summary>
        /// Determines whether a property is null.
        /// </summary>
        /// <param name="item">The object to test.</param>
        /// <param name="propertyName">Name of the property.</param>
        /// <exception cref="ArgumentNullException"></exception>
        public static void PropertyNotNull<T>(T item, string propertyName)
            where T : class
        {
            // Check first if the item is null
            NotNull(item);

            var type = typeof(T);
            var property = type.GetProperty(propertyName);
            var propertyValue = property.GetValue(item);

            if (propertyValue == null)
            {
                throw new ArgumentNullException(propertyName);
            }
        }

        /// <summary>
        /// Determines whether a property is null or empty.
        /// </summary>
        /// <param name="item">The object to test.</param>
        /// <param name="propertyName">Name of the property.</param>
        /// <exception cref="ArgumentException"></exception>
        public static void PropertyNotNullOrEmpty<T>(T item, string propertyName)
        {
            // Check first if the item is null
            NotNull(item);

            var type = typeof(T);
            var property = type.GetProperty(propertyName);
            var propertyValue = property.GetValue(item);
            var stringValue = propertyValue as string;

            if (string.IsNullOrEmpty(stringValue))
            {
                throw new ArgumentException(propertyName, Localization.GetExceptionMessage("PropertyCannotBeNullOrEmpty", "The property '{1}' in object '{0}' cannot be null or empty.", typeof(T).Name, propertyName));
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
                throw new ArgumentException(
                    argName,
                    Localization.GetExceptionMessage("PropertyCannotBeNullOrEmpty", "The property '{1}' in object '{0}' cannot be null or empty.", argName, argProperty));
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
        public static void PropertyNotEqualTo<TValue>(string argName, string argProperty, TValue propertyValue, TValue testValue)
            where TValue : IEquatable<TValue>
        {
            if (propertyValue.Equals(testValue))
            {
                throw new ArgumentException(argName, Localization.GetExceptionMessage("PropertyNotEqualTo", "The property '{1}' in object '{0}' is invalid.", argName, argProperty));
            }
        }
    }
}
