﻿// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
using System;

namespace DotNetNuke.Entities.Tabs
{
    public interface ITabPublishingController
    {
        /// <summary>
        /// Check if a page is published or not.
        /// </summary>
        /// <param name="tabID">Tha tab Id</param>
        /// <param name="portalID">The portal ID where the tab is</param>        
        bool IsTabPublished(int tabID, int portalID);

        /// <summary>
        /// Set a page as published or unpublished
        /// </summary>
        /// <param name="tabID">The tab ID</param>
        /// <param name="portalID">The portal ID where the tab is</param>
        /// <param name="publish">A boolean value where True means the page is going to be published and otherwise unpublished</param>
        void SetTabPublishing(int tabID, int portalID, bool publish);

        /// <summary>
        /// Check if Publish/Unpublish page actions can be performed
        /// </summary>
        /// <param name="tabID">The tab ID</param>
        /// <param name="portalID">The portal ID where the tab is</param>
        bool CanPublishingBePerformed(int tabID, int portalID);
    }
}
