// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Entities.Tabs
{
    using System;

    public interface ITabPublishingController
    {
        /// <summary>
        /// Check if a page is published or not.
        /// </summary>
        /// <param name="tabID">Tha tab Id.</param>
        /// <param name="portalID">The portal ID where the tab is.</param>
        /// <returns></returns>
        bool IsTabPublished(int tabID, int portalID);

        /// <summary>
        /// Set a page as published or unpublished.
        /// </summary>
        /// <param name="tabID">The tab ID.</param>
        /// <param name="portalID">The portal ID where the tab is.</param>
        /// <param name="publish">A boolean value where True means the page is going to be published and otherwise unpublished.</param>
        void SetTabPublishing(int tabID, int portalID, bool publish);

        /// <summary>
        /// Check if Publish/Unpublish page actions can be performed.
        /// </summary>
        /// <param name="tabID">The tab ID.</param>
        /// <param name="portalID">The portal ID where the tab is.</param>
        /// <returns></returns>
        bool CanPublishingBePerformed(int tabID, int portalID);
    }
}
