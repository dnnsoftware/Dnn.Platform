// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Tests.Integration.Executers.Builders
{
    using System;
    using System.Collections.Generic;

    using DotNetNuke.Tests.Integration.Executers.Dto;
    using NTestDataBuilder;

    public class PageSettingsBuilder : TestDataBuilder<PageSettings, PageSettingsBuilder>
    {
        public PageSettingsBuilder()
        {
            this.WithTabId(0);
            this.WithName("RB" + Guid.NewGuid().ToString().Replace("-", string.Empty));
            this.WithPageType(string.Empty);
            this.WithUrl(string.Empty);
            this.WithPermission(new TabPermissions());
        }

        public PageSettingsBuilder WithTabId(int tabId)
        {
            this.Set(x => x.tabId, tabId);
            return this;
        }

        public PageSettingsBuilder WithPageType(string pageType)
        {
            this.Set(x => x.pageType, pageType);
            return this;
        }

        public PageSettingsBuilder WithName(string name)
        {
            this.Set(x => x.Name, name);
            return this;
        }

        public PageSettingsBuilder WithStartDate(DateTime startDate)
        {
            this.Set(x => x.startDate, startDate);
            return this;
        }

        public PageSettingsBuilder WithEndDate(DateTime endDate)
        {
            this.Set(x => x.endDate, endDate);
            return this;
        }

        public PageSettingsBuilder WithKeyWords(string keyWords)
        {
            this.Set(x => x.keywords, keyWords);
            return this;
        }

        public PageSettingsBuilder WithDescription(string description)
        {
            this.Set(x => x.Description, description);
            return this;
        }

        public PageSettingsBuilder WithUrl(string url)
        {
            this.Set(x => x.Url, url);
            return this;
        }

        public PageSettingsBuilder WithPermission(TabPermissions permissions)
        {
            this.Set(x => x.permissions, permissions);
            return this;
        }

        public PageSettingsBuilder WithTemplateTabId(int templateTabId)
        {
            this.Set(x => x.templateTabId, templateTabId);
            return this;
        }

        public PageSettingsBuilder WithCopyModules(IList<CopyModuleItem> modules)
        {
            this.Set(x => x.modules, modules);

            return this;
        }

        public PageSettingsBuilder WithSecure(bool secure)
        {
            this.Set(x => x.isSecure, secure);
            return this;
        }

        protected override PageSettings BuildObject()
        {
            var name = this.GetOrDefault(p => p.Name);
            var pageSettings = new PageSettings
            {
                ApplyWorkflowToChildren = false,
                created = string.Empty,
                customUrlEnabled = true,
                hasChild = false,
                Hierarchy = string.Empty,
                IncludeInMenu = true,
                isCopy = false,
                isWorkflowCompleted = true,
                isWorkflowPropagationAvailable = false,
                keywords = this.GetOrDefault(p => p.keywords),
                localizedName = string.Empty,
                Name = name,
                pageType = this.GetOrDefault(p => p.pageType),
                tabId = this.GetOrDefault(p => p.tabId),
                tags = string.Empty,
                thumbnail = string.Empty,
                title = string.Empty,
                trackLinks = false,
                type = 0,
                workflowId = this.GetOrDefault(p => p.workflowId),
                Url = this.GetOrDefault(p => p.Url),
                Description = this.GetOrDefault(p => p.Description),
                startDate = this.GetOrDefault(p => p.startDate),
                endDate = this.GetOrDefault(p => p.endDate),
                permissions = this.GetOrDefault(p => p.permissions),
                templateTabId = this.GetOrDefault(p => p.templateTabId),
                modules = this.GetOrDefault(p => p.modules),
                isSecure = this.GetOrDefault(p => p.isSecure),
            };
            return pageSettings;
        }
    }
}
