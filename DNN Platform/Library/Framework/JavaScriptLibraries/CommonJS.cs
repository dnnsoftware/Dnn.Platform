// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Framework.JavaScriptLibraries;

using System.Diagnostics.CodeAnalysis;

/// <summary>
///     this class contains a number of constants that map to <see cref="JavaScriptLibrary.LibraryName"/>s
///     done as a series of constants as enums do not allow hyphens or periods.
/// </summary>
public static class CommonJs
{
    /// <summary>jQuery library name.</summary>
    [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1303:ConstFieldNamesMustBeginWithUpperCaseLetter", Justification = "Breaking Change")]

    // ReSharper disable once InconsistentNaming
    public const string jQuery = "jQuery";

    /// <summary>jQuery Migrate library name.</summary>
    [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1303:ConstFieldNamesMustBeginWithUpperCaseLetter", Justification = "Breaking Change")]

    // ReSharper disable once InconsistentNaming
    public const string jQueryMigrate = "jQuery-Migrate";

    /// <summary>jQuery UI library name.</summary>
    [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1303:ConstFieldNamesMustBeginWithUpperCaseLetter", Justification = "Breaking Change")]

    // ReSharper disable once InconsistentNaming
    public const string jQueryUI = "jQuery-UI";

    /// <summary>Knockout library name.</summary>
    public const string Knockout = "Knockout";

    /// <summary>Knockout Mapping library name.</summary>
    public const string KnockoutMapping = "Knockout.Mapping";

    /// <summary>jQuery Fileupload library name.</summary>
    [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1303:ConstFieldNamesMustBeginWithUpperCaseLetter", Justification = "Breaking Change")]

    // ReSharper disable once InconsistentNaming
    public const string jQueryFileUpload = "jQuery.Fileupload";

    /// <summary>DNN jQuery plugins library name.</summary>
    public const string DnnPlugins = "DnnPlugins";

    /// <summary>HoverIntent library name.</summary>
    public const string HoverIntent = "HoverIntent";
}
