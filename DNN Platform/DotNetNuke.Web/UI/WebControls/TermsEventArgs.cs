// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
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
