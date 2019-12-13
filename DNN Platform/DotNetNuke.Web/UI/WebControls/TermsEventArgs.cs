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
