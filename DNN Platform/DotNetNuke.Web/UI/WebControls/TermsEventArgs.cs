// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Web.UI.WebControls;

using System;

using DotNetNuke.Entities.Content.Taxonomy;

public class TermsEventArgs : EventArgs
{
    private readonly Term selectedTerm;

    /// <summary>Initializes a new instance of the <see cref="TermsEventArgs"/> class.</summary>
    /// <param name="selectedTerm"></param>
    public TermsEventArgs(Term selectedTerm)
    {
        this.selectedTerm = selectedTerm;
    }

    public Term SelectedTerm
    {
        get
        {
            return this.selectedTerm;
        }
    }
}
