// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Search;

using System.Diagnostics.CodeAnalysis;

using DotNetNuke.Entities.Modules;

/// Namespace:  DotNetNuke.Services.Search
/// Project:    DotNetNuke.Search.Index
/// Class:      SearchContentModuleInfo
/// <summary>
/// The SearchContentModuleInfo class represents an extension (by containment)
/// of ModuleInfo to add a parameter that determines whether a module is Searchable.
/// </summary>
public class SearchContentModuleInfo
{
#pragma warning disable 0618

    // ReSharper disable InconsistentNaming
    [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1306:FieldNamesMustBeginWithLowerCaseLetter", Justification = "Breaking Change")]
    [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]

    protected ISearchable MModControllerType;
#pragma warning restore 0618

    [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1306:FieldNamesMustBeginWithLowerCaseLetter", Justification = "Breaking Change")]
    [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
    protected ModuleSearchBase SearchBaseControllerType;

    [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1306:FieldNamesMustBeginWithLowerCaseLetter", Justification = "Breaking Change")]
    [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
    protected ModuleInfo MModInfo;

    // ReSharper restore InconsistentNaming
#pragma warning disable 0618
    public ISearchable ModControllerType
    {
        get
        {
            return this.MModControllerType;
        }

        set
        {
            this.MModControllerType = value;
        }
    }
#pragma warning restore 0618

    public ModuleSearchBase ModSearchBaseControllerType
    {
        get
        {
            return this.SearchBaseControllerType;
        }

        set
        {
            this.SearchBaseControllerType = value;
        }
    }

    public ModuleInfo ModInfo
    {
        get
        {
            return this.MModInfo;
        }

        set
        {
            this.MModInfo = value;
        }
    }
}
