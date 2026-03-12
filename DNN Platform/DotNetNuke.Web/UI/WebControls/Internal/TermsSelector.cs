// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Web.UI.WebControls.Internal
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Web;
    using System.Web.UI.WebControls;

    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Abstractions.ClientResources;
    using DotNetNuke.Abstractions.Logging;
    using DotNetNuke.Entities.Content.Taxonomy;

    using Microsoft.Extensions.DependencyInjection;

    using Globals = DotNetNuke.Common.Globals;

    /// <summary>This control is only for internal use, please don't reference it in any other place as it may be removed in the future.</summary>
    /// <param name="appStatus">The application status.</param>
    /// <param name="eventLogger">The event logger.</param>
    /// <param name="clientResourceController">The client resource controller.</param>
    /// <param name="termController">The term controller.</param>
    public class TermsSelector(IApplicationStatusInfo appStatus, IEventLogger eventLogger, IClientResourceController clientResourceController, ITermController termController)
        : DnnComboBox(
            appStatus ?? Globals.GetCurrentServiceProvider().GetRequiredService<IApplicationStatusInfo>(),
            eventLogger ?? Globals.GetCurrentServiceProvider().GetRequiredService<IEventLogger>(),
            clientResourceController ?? Globals.GetCurrentServiceProvider().GetRequiredService<IClientResourceController>())
    {
        private static readonly char[] TermIdSeparator = [',',];
        private readonly ITermController termController = termController ?? Globals.GetCurrentServiceProvider().GetRequiredService<ITermController>();

        /// <summary>Initializes a new instance of the <see cref="TermsSelector"/> class.</summary>
        [Obsolete("Deprecated in DotNetNuke 10.2.2. Please use overload with IApplicationStatusInfo. Scheduled removal in v12.0.0.")]
        public TermsSelector()
            : this(null, null, null, null)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="TermsSelector"/> class.</summary>
        /// <param name="appStatus">The application status.</param>
        /// <param name="eventLogger">The event logger.</param>
        /// <param name="clientResourceController">The client resource controller.</param>
        [Obsolete("Deprecated in DotNetNuke 10.2.4. Please use overload with IContentController. Scheduled removal in v12.0.0.")]
        public TermsSelector(IApplicationStatusInfo appStatus, IEventLogger eventLogger, IClientResourceController clientResourceController)
            : this(appStatus, eventLogger, clientResourceController, null)
        {
        }

        /// <summary>Gets or sets the portal ID.</summary>
        public int PortalId { get; set; }

        /// <summary>Gets or sets a value indicating whether to include terms from system vocabularies.</summary>
        public bool IncludeSystemVocabularies { get; set; }

        /// <summary>Gets or sets a value indicating whether to include terms from the tags vocabulary.</summary>
        public bool IncludeTags { get; set; } = true;

        /// <summary>Gets or sets the terms.</summary>
        public List<Term> Terms
        {
            get
            {
                var terms = new List<Term>();
                if (!string.IsNullOrEmpty(this.Value))
                {
                    var termIds = this.Value.Split(TermIdSeparator, StringSplitOptions.RemoveEmptyEntries);
                    foreach (var i in termIds)
                    {
                        if (!string.IsNullOrEmpty(i.Trim()))
                        {
                            var termId = Convert.ToInt32(i.Trim(), CultureInfo.InvariantCulture);
                            var term = this.termController.GetTerm(termId);
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
                this.Value = string.Join(",", value.Select(t => t.TermId.ToString(CultureInfo.InvariantCulture)));

                this.Items.Clear();
                value.Select(t => new ListItem(t.Name, t.TermId.ToString(CultureInfo.InvariantCulture)) { Selected = true }).ToList().ForEach(this.Items.Add);
            }
        }

        /// <inheritdoc />
        public override bool MultipleSelect { get; set; } = true;

        /// <inheritdoc />
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            if (!string.IsNullOrEmpty(this.CssClass))
            {
                this.CssClass = $"{this.CssClass} TermsSelector";
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

            this.Options.Load = $$"""
                                  function(query, callback) {
                                      $.ajax({
                                          url: '{{HttpUtility.JavaScriptStringEncode(apiPath)}}' + encodeURIComponent(query),
                                          type: 'GET',
                                          error: function() {
                                              callback();
                                          },
                                          success: function(data) {
                                              callback(data);
                                          }
                                      });
                                  }
                                  """;
        }
    }
}
