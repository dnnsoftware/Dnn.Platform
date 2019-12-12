// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
namespace Dnn.PersonaBar.Pages.Components.Dto
{
    public class PageUrlResult
    {
        public bool Success { get; set; }
        public string ErrorMessage { get; set; }
        public string SuggestedUrlPath { get; set; }
        public int? Id { get; set; }
    }
}
