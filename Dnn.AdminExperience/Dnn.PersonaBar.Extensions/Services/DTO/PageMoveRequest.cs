// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
namespace Dnn.PersonaBar.Pages.Services.Dto
{
    public class PageMoveRequest
    {
        public int PageId { get; set; }

        public int RelatedPageId { get; set; }

        public int ParentId { get; set; }

        public string Action { get; set; }
    }
}
