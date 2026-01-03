// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.MvcPipeline.Commons
{
    using System.Web.Routing;

    /// <summary>
    /// Helper methods for working with anonymous or POCO types in the MVC pipeline.
    /// </summary>
    public static class TypeHelper
    {
        /// <summary>
        /// Given an object of anonymous type, add each property as a key and associated with its value to a dictionary.
        ///
        /// This helper will cache accessors and types, and is intended when the anonymous object is accessed multiple
        /// times throughout the lifetime of the web application.
        /// </summary>
        /// <param name="value">The source object whose public properties will be projected.</param>
        /// <returns>A route value dictionary containing the object's property names and values.</returns>
        public static RouteValueDictionary ObjectToDictionary(object value)
        {
            var dictionary = new RouteValueDictionary();

            if (value != null)
            {
                foreach (var helper in PropertyHelper.GetProperties(value))
                {
                    dictionary.Add(helper.Name, helper.GetValue(value));
                }
            }

            return dictionary;
        }
    }
}
