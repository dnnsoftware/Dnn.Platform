// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Web.UI.WebControls.Internal
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web.UI.WebControls;

    using DotNetNuke.Entities.Content.Common;
    using DotNetNuke.Entities.Content.Taxonomy;
    using DotNetNuke.Framework.JavaScriptLibraries;
    using DotNetNuke.Web.Client.ClientResourceManagement;

    using Globals = DotNetNuke.Common.Globals;

    /// <remarks>
    /// This control is only for internal use, please don't reference it in any other place as it may be removed in future.
    /// </remarks>
    public class TermsSelector : DnnComboBox
    {
        public int PortalId { get; set; }

        public bool IncludeSystemVocabularies { get; set; } = false;

        public bool IncludeTags { get; set; } = true;

        public List<Term> Terms
        {
            get
            {
                var terms = new List<Term>();
                if (!string.IsNullOrEmpty(this.Value))
                {
                    var termRep = Util.GetTermController();

                    var termIds = this.Value.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
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
                this.Value = string.Join(",", value.Select(t => t.TermId.ToString()));

                this.Items.Clear();
                value.Select(t => new ListItem(t.Name, t.TermId.ToString()) { Selected = true }).ToList().ForEach(this.Items.Add);
            }
        }

        public override bool MultipleSelect { get; set; } = true;

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            if (!string.IsNullOrEmpty(this.CssClass))
            {
                this.CssClass = string.Format("{0} TermsSelector", this.CssClass);
            }
            else
            {
                this.CssClass = "TermsSelector";
            }

            var includeSystem = this.IncludeSystemVocabularies.ToString().ToLowerInvariant();
            var includeTags = this.IncludeTags.ToString().ToLowerInvariant();
            var apiPath = Globals.ResolveUrl($"~/API/InternalServices/ItemListService/GetTerms?includeSystem={includeSystem}&includeTags={includeTags}&q=");

            this.Options.Preload = "focus";
            this.Options.Plugins.Add("remove_button");
            this.Options.Render = new RenderOption
            {
                Option = "function(item, escape) {return '<div>' + item.text + '</div>';}",
            };

            this.Options.Load = $@"function(query, callback) {{
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
    }
}
