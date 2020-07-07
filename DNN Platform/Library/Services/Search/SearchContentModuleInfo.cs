// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Search
{
    using System;

    using DotNetNuke.Entities.Modules;

    /// -----------------------------------------------------------------------------
    /// Namespace:  DotNetNuke.Services.Search
    /// Project:    DotNetNuke.Search.Index
    /// Class:      SearchContentModuleInfo
    /// -----------------------------------------------------------------------------
    /// <summary>
    /// The SearchContentModuleInfo class represents an extendension (by containment)
    /// of ModuleInfo to add a parametere that determines whether a module is Searchable.
    /// </summary>
    /// <remarks>
    /// </remarks>
    /// -----------------------------------------------------------------------------
    public class SearchContentModuleInfo
    {
#pragma warning disable 0618
        protected ISearchable MModControllerType;
#pragma warning restore 0618
        protected ModuleSearchBase SearchBaseControllerType;
        protected ModuleInfo MModInfo;

#pragma warning disable 0618
        public ISearchable ModControllerType
        {
            get
            {
                return this.MModControllerType;
            }

            set
            {
                this.MModControllerType = value;
            }
        }
#pragma warning restore 0618

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
