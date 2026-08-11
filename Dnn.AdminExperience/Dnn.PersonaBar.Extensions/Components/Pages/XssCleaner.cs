// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.Pages.Components;

using Dnn.PersonaBar.Library.Dto.Tabs;
using Dnn.PersonaBar.Pages.Services.Dto;
using DotNetNuke.Security;

/// <summary>Cleans input values.</summary>
public static class XssCleaner
{
    /// <summary>Cleans <see cref="PageSettings"/> input.</summary>
    /// <param name="input">The input to clean.</param>
    public static void Clean(this PageSettings input)
    {
        input.Title = Clean(input.Title);
        input.Description = Clean(input.Description);
        input.Name = Clean(input.Name);
        input.Keywords = Clean(input.Keywords);
        input.Tags = Clean(input.Tags);
        input.Url = Clean(input.Url);
        input.PageType = Clean(input.PageType);
        input.Alias = Clean(input.Alias);
        input.LocalizedName = Clean(input.LocalizedName);
        input.PageStyleSheet = Clean(input.PageStyleSheet);
    }

    /// <summary>Cleans <see cref="BulkPage"/> input.</summary>
    /// <param name="input">The input to clean.</param>
    public static void Clean(this BulkPage input)
    {
        input.BulkPages = Clean(input.BulkPages);
        input.Keywords = Clean(input.Keywords);
        input.Tags = Clean(input.Tags);
    }

    /// <summary>Cleans <see cref="PageTemplate"/> input.</summary>
    /// <param name="input">The input to clean.</param>
    public static void Clean(this PageTemplate input)
    {
        input.Description = Clean(input.Description);
        input.Name = Clean(input.Name);
    }

    /// <summary>Cleans <see cref="DnnPagesRequest"/> input.</summary>
    /// <param name="input">The input to clean.</param>
    public static void Clean(this DnnPagesRequest input)
    {
        foreach (var locale in input.Locales)
        {
            locale.Clean();
        }

        foreach (var page in input.Pages)
        {
            page.Clean();
        }

        foreach (var module in input.Modules)
        {
            module.Clean();
        }
    }

    /// <summary>Cleans <see cref="LocaleInfoDto"/> input.</summary>
    /// <param name="input">The input to clean.</param>
    public static void Clean(this LocaleInfoDto input)
    {
        input.CultureCode = Clean(input.CultureCode);
    }

    /// <summary>Cleans <see cref="DnnPageDto"/> input.</summary>
    /// <param name="input">The input to clean.</param>
    public static void Clean(this DnnPageDto input)
    {
        input.Title = Clean(input.Title);
        input.Description = Clean(input.Description);
        input.TabName = Clean(input.TabName);
        input.LocalResourceFile = Clean(input.LocalResourceFile);
        input.CultureCode = Clean(input.CultureCode);
        input.PageUrl = Clean(input.PageUrl);
        input.Path = Clean(input.Path);
        input.Position = Clean(input.Position);
    }

    /// <summary>Cleans <see cref="DnnModulesRequest"/> input.</summary>
    /// <param name="input">The input to clean.</param>
    public static void Clean(this DnnModulesRequest input)
    {
        foreach (var module in input.Modules)
        {
            module.Clean();
        }
    }

    /// <summary>Cleans <see cref="DnnModuleDto"/> input.</summary>
    /// <param name="input">The input to clean.</param>
    public static void Clean(this DnnModuleDto input)
    {
        input.CultureCode = Clean(input.CultureCode);
        input.DefaultTabName = Clean(input.DefaultTabName);
        input.LocalResourceFile = Clean(input.LocalResourceFile);
        input.ModuleInfoHelp = Clean(input.ModuleInfoHelp);
        input.ModuleTitle = Clean(input.ModuleTitle);
    }

    /// <summary>Cleans <see cref="string"/> input.</summary>
    /// <param name="input">The input to clean.</param>
    /// <param name="filterFlag">The filter to clean with <paramref name="input"/> with.</param>
    /// <returns>The clean input.</returns>
    public static string Clean(
        string input,
#pragma warning disable CS0618 // Type or member is obsolete
        PortalSecurity.FilterFlag filterFlag = PortalSecurity.FilterFlag.NoMarkup)
#pragma warning restore CS0618 // Type or member is obsolete
    {
        return PortalSecurity.Instance.InputFilter(input, filterFlag);
    }
}
