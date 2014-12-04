/*
 * CKEditor Html Editor Provider for DotNetNuke
 * ========
 * http://dnnckeditor.codeplex.com/
 * Copyright (C) Ingo Herbote
 *
 * The software, this file and its contents are subject to the CKEditor Provider
 * License. Please read the license.txt file before using, installing, copying,
 * modifying or distribute this file or part of its contents. The contents of
 * this file is part of the Source Code of the CKEditor Provider.
 */

namespace WatchersNET.CKEditor.Extensions
{
    using System;
    using System.Reflection;

    /// <summary>
    /// Custom Attribute Extensions
    /// </summary>
    public static class CustomAttributeExtensions
    {
        /// <summary>
        /// Gets the custom attribute of <see cref="T" />
        /// </summary>
        /// <typeparam name="T">The Type</typeparam>
        /// <param name="propertyInfo">The property info.</param>
        /// <param name="inherit">The inherit.</param>
        /// <returns>
        /// Returns the Custom Attribute
        /// </returns>
        public static T GetCustomAttribute<T>(this PropertyInfo propertyInfo, bool inherit) where T : Attribute
        {
            object[] attributes = propertyInfo.GetCustomAttributes(typeof(T), inherit);

            return attributes.Length == 0 ? null : attributes[0] as T;
        }
    }
}