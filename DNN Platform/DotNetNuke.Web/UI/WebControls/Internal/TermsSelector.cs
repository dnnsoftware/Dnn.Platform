#region Usings

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI.WebControls;
using DotNetNuke.Entities.Content.Common;
using DotNetNuke.Entities.Content.Taxonomy;
using DotNetNuke.Framework.JavaScriptLibraries;
using DotNetNuke.Web.Client.ClientResourceManagement;
using Globals = DotNetNuke.Common.Globals;

#endregion

namespace DotNetNuke.Web.UI.WebControls.Internal
{
    ///<remarks>
    /// This control is only for internal use, please don't reference it in any other place as it may be removed in future.
    /// </remarks>
    public class TermsSelector : DnnComboBox
    {
        #region Public Properties

        public int PortalId { get; set; }

        public bool IncludeSystemVocabularies { get; set; } = false;

        public bool IncludeTags { get; set; } = true;

        public List<Term> Terms
        {
            get
            {
                var terms = new List<Term>();
                if (!string.IsNullOrEmpty(Value))
                {
                    var termRep = Util.GetTermController();

                    var termIds = Value.Split(new char[] {','}, StringSplitOptions.RemoveEmptyEntries);
                    foreach (var i in termIds)
                    {
                        if (!string.IsNullOrEmpty(i.Trim()))
                        {
                            var termId = Convert.ToInt32(i.Trim());
                            var term = termRep.GetTerm(termId);
                            if (term != null)
                            {
                                terms.Add(term);
                            }
                        }
                    }
                }

                return terms;
            }
            set
            {
                Value = string.Join(",", value.Select(t => t.TermId.ToString()));

                Items.Clear();
                value.Select(t => new ListItem(t.Name, t.TermId.ToString()) {Selected = true}).ToList().ForEach(Items.Add);
            }
        }

        public override bool MultipleSelect { get; set; } = true;

        #endregion

        #region Protected Methods

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            if (!string.IsNullOrEmpty(CssClass))
            {
                CssClass = string.Format("{0} TermsSelector", CssClass);
            }
            else
            {
                CssClass = "TermsSelector";
            }

            var includeSystem = IncludeSystemVocabularies.ToString().ToLowerInvariant();
            var includeTags = IncludeTags.ToString().ToLowerInvariant();
            var apiPath = Globals.ResolveUrl($"~/API/InternalServices/ItemListService/GetTerms?includeSystem={includeSystem}&includeTags={includeTags}&q=");

            Options.Preload = "focus";
            Options.Plugins.Add("remove_button");
            Options.Render = new RenderOption
                {
                    Option = "function(item, escape) {return '<div>' + item.text + '</div>';}"
                };

            Options.Load = $@"function(query, callback) {{
                                $.ajax({{
                                        url: '{apiPath}' + encodeURIComponent(query),
                                    type: 'GET',
                                    error: function() {{
                                        callback();
                                    }},
                                    success: function(data) {{
                                        callback(data);
                                    }}
                                }});
                            }}
";
        }

        #endregion

        #region Private Methods


        #endregion
    }
}
