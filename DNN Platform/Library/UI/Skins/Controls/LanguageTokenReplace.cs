// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.UI.Skins.Controls;

using System.Diagnostics.CodeAnalysis;

using DotNetNuke.Common;
using DotNetNuke.Services.Tokens;

public class LanguageTokenReplace : TokenReplace
{
    // see http://support.dotnetnuke.com/issue/ViewIssue.aspx?id=6505

    /// <summary>Initializes a new instance of the <see cref="LanguageTokenReplace"/> class.</summary>
    public LanguageTokenReplace()
        : base(Scope.NoSettings)
    {
        this.UseObjectLessExpression = true;
        this.AddPropertySource(ObjectLessToken, new LanguagePropertyAccess(this, Globals.GetPortalSettings()));
    }

    [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1300:ElementMustBeginWithUpperCaseLetter", Justification = "Breaking Change")]

    // ReSharper disable once InconsistentNaming
    public string resourceFile { get; set; }
}
