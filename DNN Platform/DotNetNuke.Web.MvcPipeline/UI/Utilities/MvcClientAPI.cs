// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.MvcPipeline.UI.Utilities
{
    using System;
    using System.Collections.Generic;
    using System.Web;

    /// <summary>
    /// Helper methods for collecting client-side variables and startup scripts for MVC views.
    /// </summary>
    public class MvcClientAPI
    {
        /// <summary>
        /// Gets the dictionary of client variables for the current HTTP request,
        /// creating it when it does not yet exist.
        /// </summary>
        /// <returns>
        /// A dictionary containing client variables for the current request.
        /// </returns>
        public static Dictionary<string, string> GetClientVariableList()
        {
            var dic = HttpContext.Current.Items["CAPIVariableList"] as Dictionary<string, string>;
            if (dic == null)
            {
                dic = new Dictionary<string, string>();
                HttpContext.Current.Items["CAPIVariableList"] = dic;
            }

            return dic;
        }

        /// <summary>
        /// Gets the dictionary of client startup scripts for the current HTTP request,
        /// creating it when it does not yet exist.
        /// </summary>
        /// <returns>
        /// A dictionary containing client startup scripts for the current request.
        /// </returns>
        public static Dictionary<string, string> GetClientStartupScriptList()
        {
            var dic = HttpContext.Current.Items["CAPIStartupScriptList"] as Dictionary<string, string>;
            if (dic == null)
            {
                dic = new Dictionary<string, string>();
                HttpContext.Current.Items["CAPIStartupScriptList"] = dic;
            }

            return dic;
        }

        /// <summary>
        /// Registers a client variable for the current request.
        /// </summary>
        /// <param name="key">The variable key.</param>
        /// <param name="value">The variable value.</param>
        /// <param name="overwrite">
        /// If set to <c>true</c>, overwrites an existing value for the same key; otherwise the existing value is preserved.
        /// </param>
        public static void RegisterClientVariable(string key, string value, bool overwrite)
        {
            var variables = GetClientVariableList();
            if (!overwrite && variables.ContainsKey(key))
            {
                return;
            }

            variables[key] = value;
        }

        /// <summary>
        /// Registers a client variable for an embedded resource.
        /// </summary>
        /// <param name="fileName">The resource file name.</param>
        /// <param name="assemblyType">A type from the assembly containing the embedded resource.</param>
        /// <exception cref="NotImplementedException">Always thrown; this method is not yet implemented.</exception>
        public static void RegisterEmbeddedResource(string fileName, Type assemblyType)
        {
            // RegisterClientVariable(FileName + ".resx", ThePage.ClientScript.GetWebResourceUrl(AssemblyType, FileName), true);
            throw new NotImplementedException();
        }

        /// <summary>
        /// Registers a startup script for the current request under the specified key.
        /// </summary>
        /// <param name="key">The script key.</param>
        /// <param name="value">The script content.</param>
        public static void RegisterStartupScript(string key, string value)
        {
            var scripts = GetClientStartupScriptList();
            if (!scripts.ContainsKey(key))
            {
                scripts.Add(key, value);
            }
        }

        /// <summary>
        /// Registers a script for the current request under the specified key.
        /// </summary>
        /// <param name="key">The script key.</param>
        /// <param name="value">The script content.</param>
        public static void RegisterScript(string key, string value)
        {
            var scripts = GetClientStartupScriptList();
            if (!scripts.ContainsKey(key))
            {
                scripts.Add(key, value);
            }
        }
    }
}
