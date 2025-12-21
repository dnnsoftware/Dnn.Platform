// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Journal.Internal
{
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;

    using DotNetNuke.Common.Utilities;

    [SuppressMessage("Microsoft.Design", "CA1711:IdentifiersShouldNotHaveIncorrectSuffix", Justification = "Breaking change")]
    public class InternalJournalControllerImpl : IInternalJournalController
    {
        private readonly IJournalDataService dataService;

        /// <summary>Initializes a new instance of the <see cref="InternalJournalControllerImpl"/> class.</summary>
        public InternalJournalControllerImpl()
        {
            this.dataService = JournalDataService.Instance;
        }

        /// <inheritdoc/>
        [SuppressMessage("Microsoft.Naming", "CA1725:ParameterNamesShouldMatchBaseDeclaration", Justification = "Breaking change")]
        public IList<JournalItem> GetJournalItemsByProfile(int portalId, int moduleId, int currentUserId, int profileId, int rowIndex, int maxRows)
        {
            return
                CBO.FillCollection<JournalItem>(
                    this.dataService.Journal_ListForProfile(
                        portalId,
                        moduleId,
                        currentUserId,
                        profileId,
                        rowIndex,
                        maxRows));
        }

        /// <inheritdoc/>
        [SuppressMessage("Microsoft.Naming", "CA1725:ParameterNamesShouldMatchBaseDeclaration", Justification = "Breaking change")]
        public IList<JournalItem> GetJournalItemsByGroup(int portalId, int moduleId, int currentUserId, int groupId, int rowIndex, int maxRows)
        {
            return
                CBO.FillCollection<JournalItem>(
                    this.dataService.Journal_ListForGroup(
                        portalId,
                        moduleId,
                        currentUserId,
                        groupId,
                        rowIndex,
                        maxRows));
        }

        /// <inheritdoc/>
        [SuppressMessage("Microsoft.Naming", "CA1725:ParameterNamesShouldMatchBaseDeclaration", Justification = "Breaking change")]
        public IList<JournalItem> GetJournalItems(int portalId, int moduleId, int currentUserId, int rowIndex, int maxRows)
        {
            return
                CBO.FillCollection<JournalItem>(
                    this.dataService.Journal_ListForSummary(
                        portalId,
                        moduleId,
                        currentUserId,
                        rowIndex,
                        maxRows));
        }

        /// <inheritdoc/>
        public void DeleteFilters(int portalId, int moduleId)
        {
            this.dataService.Journal_TypeFilters_Delete(portalId, moduleId);
        }

        /// <inheritdoc/>
        [SuppressMessage("Microsoft.Naming", "CA1725:ParameterNamesShouldMatchBaseDeclaration", Justification = "Breaking change")]
        public void SaveFilters(int portalId, int moduleId, int journalTypeId)
        {
            this.dataService.Journal_TypeFilters_Save(portalId, moduleId, journalTypeId);
        }
    }
}
