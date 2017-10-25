#region Copyright
//
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2017
// by DotNetNuke Corporation
//
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated
// documentation files (the "Software"), to deal in the Software without restriction, including without limitation
// the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and
// to permit persons to whom the Software is furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all copies or substantial portions
// of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED
// TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL
// THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF
// CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
// DEALINGS IN THE SOFTWARE.
#endregion

using System;
using System.Collections.Generic;
using DotNetNuke.Tests.Integration.Executers.Dto;
using NTestDataBuilder;

namespace DotNetNuke.Tests.Integration.Executers.Builders
{
    public class PageSettingsBuilder : TestDataBuilder<PageSettings, PageSettingsBuilder>
    {
        public PageSettingsBuilder()
        {
            WithTabId(0);
            WithName("RB" + Guid.NewGuid().ToString().Replace("-",""));
            WithPageType(string.Empty);
            WithUrl(string.Empty);
            WithPermission(new TabPermissions());
        }

        public PageSettingsBuilder WithTabId(int tabId)
        {
            Set(x => x.tabId, tabId);
            return this;
        }

        public PageSettingsBuilder WithPageType(string pageType)
        {
            Set(x => x.pageType, pageType);
            return this;
        }

        public PageSettingsBuilder WithName(string name)
        {
            Set(x => x.Name, name);
            return this;
        }

        public PageSettingsBuilder WithStartDate(DateTime startDate)
        {
            Set(x => x.startDate, startDate);
            return this;
        }

        public PageSettingsBuilder WithEndDate(DateTime endDate)
        {
            Set(x => x.endDate, endDate);
            return this;
        }

        public PageSettingsBuilder WithKeyWords(string keyWords)
        {
            Set(x => x.keywords, keyWords);
            return this;
        }

        public PageSettingsBuilder WithDescription(string description)
        {
            Set(x => x.Description, description);
            return this;
        }
        
        public PageSettingsBuilder WithUrl(string url)
        {
            Set(x => x.Url, url);
            return this;
        }

        public PageSettingsBuilder WithPermission(TabPermissions permissions)
        {
            Set(x => x.permissions, permissions);
            return this;
        }

        public PageSettingsBuilder WithTemplateTabId(int templateTabId)
        {
            Set(x => x.templateTabId, templateTabId);
            return this;
        }

        public PageSettingsBuilder WithCopyModules(IList<CopyModuleItem> modules)
        {
            Set(x => x.modules, modules);

            return this;
        }

        public PageSettingsBuilder WithSecure(bool secure)
        {
            Set(x => x.isSecure, secure);
            return this;
        }

        protected override PageSettings BuildObject()
        {
            var name = GetOrDefault(p => p.Name);
            var pageSettings = new PageSettings
            {
                ApplyWorkflowToChildren = false,
                created = "",
                customUrlEnabled = true,
                hasChild = false,
                Hierarchy = "",
                IncludeInMenu = true,
                isCopy = false,
                isWorkflowCompleted = true,
                isWorkflowPropagationAvailable = false,
                keywords = GetOrDefault(p => p.keywords),
                localizedName = "",
                Name = name,
                pageType = GetOrDefault(p => p.pageType),
                tabId = GetOrDefault(p => p.tabId),
                tags = "",
                thumbnail = "",
                title = "",
                trackLinks = false,
                type = 0,
                workflowId = GetOrDefault(p => p.workflowId),
                Url = GetOrDefault(p => p.Url),
                Description = GetOrDefault(p => p.Description),
                startDate = GetOrDefault(p => p.startDate),
                endDate = GetOrDefault(p => p.endDate),
                permissions = GetOrDefault(p => p.permissions),
                templateTabId = GetOrDefault(p => p.templateTabId),
                modules = GetOrDefault(p => p.modules),
                isSecure = GetOrDefault(p => p.isSecure)
            };
            return pageSettings;
        }
    }
}
