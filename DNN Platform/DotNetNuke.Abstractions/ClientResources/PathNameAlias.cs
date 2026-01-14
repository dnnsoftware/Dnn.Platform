// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Abstractions.ClientResources
{
    /// <summary>Contains constants for the supported PathNameAlias values within the framework's registration system.</summary>
    public class PathNameAlias
    {
        /// <summary>
        /// Root path of the DNN installation.
        /// </summary>
        public const string Default = "";

        /// <summary>
        /// The path to the current skin.
        /// </summary>
        public const string SkinPath = "SkinPath";

        /// <summary>
        /// Path relative to ~/Resources/Shared/Scripts/.
        /// </summary>
        public const string SharedScripts = "SharedScripts";
    }
}
