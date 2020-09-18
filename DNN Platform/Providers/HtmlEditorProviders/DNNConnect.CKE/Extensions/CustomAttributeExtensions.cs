using System;
using System.Reflection;

namespace DNNConnect.CKEditorProvider.Extensions
{

    /// <summary>
    /// Custom Attribute Extensions
    /// </summary>
    public static class CustomAttributeExtensions
    {
        /// <summary>
        /// Gets the custom attribute of <typeparamref name="T"/>
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