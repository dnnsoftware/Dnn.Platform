// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
#region Usings

using System;

using DotNetNuke.Entities.Modules;

#endregion

namespace DotNetNuke.Services.Search
{
    /// -----------------------------------------------------------------------------
    /// Namespace:  DotNetNuke.Services.Search
    /// Project:    DotNetNuke.Search.Index
    /// Class:      SearchContentModuleInfo
    /// -----------------------------------------------------------------------------
    /// <summary>
    /// The SearchContentModuleInfo class represents an extendension (by containment)
    /// of ModuleInfo to add a parametere that determines whether a module is Searchable
    /// </summary>
    /// <remarks>
    /// </remarks>
    /// -----------------------------------------------------------------------------
    public class SearchContentModuleInfo
    {
        protected ModuleSearchBase SearchBaseControllerType;
        protected ModuleInfo MModInfo;

        public ModuleSearchBase ModSearchBaseControllerType
        {
            get
            {
                return SearchBaseControllerType;
            }
            set
            {
                SearchBaseControllerType = value;
            }
        }

        public ModuleInfo ModInfo
        {
            get
            {
                return MModInfo;
            }
            set
            {
                MModInfo = value;
            }
        }
    }
}
