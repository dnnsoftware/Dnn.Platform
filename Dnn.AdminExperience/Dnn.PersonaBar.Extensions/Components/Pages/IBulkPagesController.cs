// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.Pages.Components
{
    using Dnn.PersonaBar.Pages.Services.Dto;

    public interface IBulkPagesController
    {
        BulkPageResponse AddBulkPages(BulkPage page, bool validateOnly);
    }
}
