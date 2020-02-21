// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
using Dnn.PersonaBar.Pages.Services.Dto;

namespace Dnn.PersonaBar.Pages.Components
{
    public interface IBulkPagesController
    {
        BulkPageResponse AddBulkPages(BulkPage page, bool validateOnly);
    }
}
