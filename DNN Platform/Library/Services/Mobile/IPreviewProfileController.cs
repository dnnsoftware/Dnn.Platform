// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Mobile
{
    using System;
    using System.Collections.Generic;

    public interface IPreviewProfileController
    {
        void Save(IPreviewProfile profile);

        void Delete(int portalId, int id);

        IList<IPreviewProfile> GetProfilesByPortal(int portalId);

        IPreviewProfile GetProfileById(int portalId, int id);
    }
}
