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

namespace DotNetNuke.Web.UI.Internal.WebControls
{
    public class TermsSelector : TextBox
    {
        private string _initOptions;
        public TermsSelector()
        {
            IncludeSystemVocabularies = false;
            IncludeTags = true;
            EnableViewState = false;
        }

        #region Public Properties

        public int PortalId { get; set; }

        public bool IncludeSystemVocabularies { get; set; }

        public bool IncludeTags { get; set; }

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
                _initOptions = $"[{string.Join(",", value.Select(t => $"{{value: {t.TermId}, text: \"{t.Name}\"}}"))}]";
            }
        }

        #endregion

        #region Protected Methods

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            JavaScript.RequestRegistration(CommonJs.jQueryMigrate);

            if (!string.IsNullOrEmpty(CssClass))
            {
                CssClass = string.Format("{0} TermsSelector", CssClass);
            }
            else
            {
                CssClass = "TermsSelector";
            }
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
        }

        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);

            RegisterRequestResources();
        }

        #endregion

        #region Private Methods

        private void RegisterRequestResources()
        {
            JavaScript.RequestRegistration(CommonJs.DnnPlugins);

            var package = JavaScriptLibraryController.Instance.GetLibrary(l => l.LibraryName == "Selectize");
            if (package != null)
            {
                JavaScript.RequestRegistration("Selectize");

                var libraryPath =
                    $"~/Resources/Libraries/{package.LibraryName}/{DotNetNuke.Common.Globals.FormatVersion(package.Version, "00", 3, "_")}/";
                ClientResourceManager.RegisterStyleSheet(Page, $"{libraryPath}selectize.css");
                ClientResourceManager.RegisterStyleSheet(Page, $"{libraryPath}selectize.default.css");

                var includeSystem = IncludeSystemVocabularies.ToString().ToLowerInvariant();
                var includeTags = IncludeTags.ToString().ToLowerInvariant();
                var apiPath = Globals.ResolveUrl($"~/API/InternalServices/ItemListService/GetTerms?includeSystem={includeSystem}&includeTags={includeTags}&q=");

                var initScripts = $@"
$('#{ClientID}').selectize({{
    valueField: 'value',
    labelField: 'text',
    searchField: 'text',
    create: false,
    preload: 'focus',
    highlight: false,
    plugins: ['remove_button'],
    render: {{
        option: function(item, escape) {{
            return '<div>' + item.text + '</div>';
        }}
    }},
    load: function(query, callback) {{
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
    }},
    options: {_initOptions}
}});";

                Page.ClientScript.RegisterStartupScript(Page.GetType(), $"{ClientID}Sctipts", initScripts, true);
            }
        }


        #endregion
    }
}