// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.Pages.Components
{
    using System.Collections.Generic;

    using Dnn.PersonaBar.Pages.Components.Dto;
    using Dnn.PersonaBar.Pages.Services.Dto;
    using DotNetNuke.Entities.Tabs;

    public interface ITemplateController
    {
        string SaveAsTemplate(PageTemplate template);

        IEnumerable<Template> GetTemplates();
        int GetDefaultTemplateId(IEnumerable<Template> templates);
        void CreatePageFromTemplate(int templateId, TabInfo tab, int portalId);
    }
}
