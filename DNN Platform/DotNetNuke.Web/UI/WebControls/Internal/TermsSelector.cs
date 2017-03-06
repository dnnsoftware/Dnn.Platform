#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2017
// by DotNetNuke Corporation
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated 
// documentation files (the "Software"), to deal in the Software without restriction, including without limitation 
// the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and 
// to permit persons to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or substantial portions 
// of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED 
// TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL 
// THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF 
// CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
// DEALINGS IN THE SOFTWARE.
#endregion
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
    /// This control is added only for internal use, please don't reference it in any other place as it may be removed in future.
    /// </remarks>
    public class TermsSelector : DnnComboBox
    {
        private IList<ListItem> _initOptions = new List<ListItem>();

        #region Public Properties

        public int PortalId { get; set; }

        public bool IncludeSystemVocabularies { get; set; } = false;

        public bool IncludeTags { get; set; } = true;

        public List<Term> Terms
        {
            get
            {
                var terms = new List<Term>();
                if (!string.IsNullOrEmpty(Text))
                {
                    var termRep = Util.GetTermController();

                    var termIds = Text.Split(new char[] {','}, StringSplitOptions.RemoveEmptyEntries);
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
                Text = string.Join(",", value.Select(t => t.TermId.ToString()));
                _initOptions = value.Select(t => new ListItem(t.Name, t.TermId.ToString())).ToList();
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
            Options.Items = _initOptions;
        }

        #endregion

        #region Private Methods


        #endregion
    }
}