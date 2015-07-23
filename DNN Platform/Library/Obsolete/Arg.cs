using System;

// ReSharper disable once CheckNamespace
namespace DotNetNuke.Common
{
    public static class Arg
    {
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

        [Obsolete("Deprecated in DNN 5.4.0. Replaced by Requires.PropertyNotNull()")]
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
    }
}