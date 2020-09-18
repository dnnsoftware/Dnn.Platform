// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Services.Journal.Internal
{
    using System;
    using System.Collections.Generic;

    public interface IInternalJournalController
    {
        IList<JournalItem> GetJournalItemsByProfile(int portalId, int moduleId, int userID, int profileId, int currentIndex, int rows);

        IList<JournalItem> GetJournalItemsByGroup(int portalId, int moduleId, int userID, int socialGroupId, int currentIndex, int rows);

        IList<JournalItem> GetJournalItems(int portalId, int moduleId, int userID, int currentIndex, int rows);

        void DeleteFilters(int portalId, int moduleId);

        void SaveFilters(int portalId, int moduleId, int toInt32);
    }
}
