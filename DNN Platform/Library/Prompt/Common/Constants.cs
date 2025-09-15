// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Prompt
{
    /// <summary>Constant values used by Prompt.</summary>
    public static class Constants
    {
        /// <summary>The virtual path of the default resource file.</summary>
        public const string DefaultPromptResourceFile = "~/App_GlobalResources/Prompt.resx";

        /// <summary>The known Prompt categories.</summary>
        public class CommandCategoryKeys
        {
            /// <summary>The modules category resource key.</summary>
            public const string Modules = "Prompt_ModulesCategory";

            /// <summary>The general category resource key.</summary>
            internal const string General = "Prompt_GeneralCategory";

            /// <summary>The host category resource key.</summary>
            internal const string Host = "Prompt_HostCategory";

            /// <summary>The portal category resource key.</summary>
            internal const string Portal = "Prompt_PortalCategory";
        }
    }
}
