// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Search
{
    using System.Diagnostics.CodeAnalysis;

    using DotNetNuke.Entities.Modules;

    /// <summary>
    /// The SearchContentModuleInfo class represents an extension (by containment)
    /// of ModuleInfo to add a parameter that determines whether a module is Searchable.
    /// </summary>
    public class SearchContentModuleInfo
    {
        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1306:FieldNamesMustBeginWithLowerCaseLetter", Justification = "Breaking Change")]
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        protected ModuleSearchBase SearchBaseControllerType;

        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1306:FieldNamesMustBeginWithLowerCaseLetter", Justification = "Breaking Change")]
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        protected ModuleInfo MModInfo;

        public ModuleSearchBase ModSearchBaseControllerType
        {
            get
            {
                return this.SearchBaseControllerType;
            }

            set
            {
                this.SearchBaseControllerType = value;
            }
        }

        public ModuleInfo ModInfo
        {
            get
            {
                return this.MModInfo;
            }

            set
            {
                this.MModInfo = value;
            }
        }
    }
}
