using System;
using System.Reflection;

namespace DotNetNuke.DependencyInjection.Extensions
{
    /// <summary>
    /// <see cref="Type"/> specific extensions to be used
    /// in Dependency Injection invocations.
    /// </summary>
    public static class TypeExtensions
    {
        // There is no logging in this file by design as
        // it would create a dependency on the Logging library
        // and this library can't have any dependencies on other
        // DNN Libraries.

        /// <summary>
        /// Safely Get all Types from the assembly. If there
        /// is an error while retrieving the types it will 
        /// return an empty array of <see cref="Type"/>
        /// </summary>
        /// <param name="assembly">
        /// The assembly to retrieve all types from.
        /// </param>
        /// <returns>
        /// An array of all <see cref="Type"/> in the given <see cref="Assembly"/>
        /// </returns>
        public static Type[] SafeGetTypes(this Assembly assembly)
        {
            Type[] types = null;
            try
            {
                types = assembly.GetTypes();
            }
            catch (Exception)
            {               
                types = new Type[0];
            }

            return types;
        }
    }
}
