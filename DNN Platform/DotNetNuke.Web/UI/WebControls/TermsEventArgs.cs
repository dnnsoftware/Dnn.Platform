// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
#region Usings

using System;

using DotNetNuke.Entities.Content.Taxonomy;

#endregion

namespace DotNetNuke.Web.UI.WebControls
{
    public class TermsEventArgs : EventArgs
    {
        private readonly Term _SelectedTerm;

        #region "Constructors"

        public TermsEventArgs(Term selectedTerm)
        {
            _SelectedTerm = selectedTerm;
        }

        #endregion

        #region "Public Properties"

        public Term SelectedTerm
        {
            get
            {
                return _SelectedTerm;
            }
        }

        #endregion
    }
}
