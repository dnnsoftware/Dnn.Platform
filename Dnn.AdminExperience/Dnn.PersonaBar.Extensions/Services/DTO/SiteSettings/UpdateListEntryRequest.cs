// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace Dnn.PersonaBar.SiteSettings.Services.Dto
{
    using Newtonsoft.Json;

    public class UpdateListEntryRequest
    {
        public int? PortalId { get; set; }

        public int? EntryId { get; set; }

        public bool EnableSortOrder { get; set; }

        public string ListName { get; set; }

        public string Text { get; set; }

        public string Value { get; set; }

        public int? SortOrder { get; set; }
    }
}
