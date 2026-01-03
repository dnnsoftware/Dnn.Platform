// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace Dnn.PersonaBar.SiteSettings.Services.Dto
{
    using Newtonsoft.Json;

    public class UpdateOtherSettingsRequest
    {
        public int? PortalId { get; set; }

        public string CultureCode { get; set; }

        public string AllowedExtensionsWhitelist { get; set; }

        public bool EnablePopups { get; set; }

        public bool InjectModuleHyperLink { get; set; }

        public bool InlineEditorEnabled { get; set; }

        public bool AllowJsInModuleHeaders { get; set; }

        public bool AllowJsInModuleFooters { get; set; }

        public bool ShowQuickModuleAddMenu { get; set; }

        public bool? EnabledVersioning { get; set; }

        public int? MaxNumberOfVersions { get; set; }

        public bool? WorkflowEnabled { get; set; }

        public int? DefaultTabWorkflowId { get; set; }

        public string PagePipeline { get; set; }
    }
}
