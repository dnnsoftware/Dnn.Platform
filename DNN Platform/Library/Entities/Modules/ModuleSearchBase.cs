// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Entities.Modules
{
    using System;
    using System.Collections.Generic;

    using DotNetNuke.Services.Search.Entities;

    /// <summary>
    /// Modules participating in Search should inherit from this class. A scheduled job will call the methods from this class.
    /// </summary>
    /// <remarks>Since the methods will be called through a Scheduled job, there will be no Portal Context available by the module to take advantage of.</remarks>
    public abstract class ModuleSearchBase
    {
        /// <summary>
        /// Return a list of Modified Search Documents based on date. The documents will be stored in Search Index.
        /// </summary>
        /// <param name="moduleInfo">Module Info.</param>
        /// <param name="beginDateUtc">Provide modified content from this time in Utc.</param>
        /// <returns>Collection of SearchDocument.</returns>
        /// <remarks>Module must return New, Updated and Deleted Search Documents.
        /// It is important to include all the relevant Properties for Updated content (sames as supplied for New document), as partial SearchDocument cannot be Updated in Search Index.
        /// This is different from standard SQL Update where selective columns can updated. In this case, entire Document must be supplied during Update or else information will be lost.
        /// For Deleted content, set IsActive = false property.
        /// When IsActive = true, an attempt is made to delete any existing document with same UniqueKey, PortalId, SearchTypeId=Module, ModuleDefitionId and ModuleId(if specified).
        /// System calls the module based on Scheduler Frequency. This call is performed for modules that have indicated supportedFeature type="Searchable" in manifest.
        /// Call is performed for every Module Definition defined by the Module. If a module has more than one Module Defition, module must return data for the main Module Defition,
        /// or else duplicate content may get stored.
        /// Module must include ModuleDefition Id in the SearchDocument. In addition ModuleId and / or TabId can also be specified if module has TabId / ModuleId specific content.</remarks>
        public abstract IList<SearchDocument> GetModifiedSearchDocuments(ModuleInfo moduleInfo, DateTime beginDateUtc);
    }
}
