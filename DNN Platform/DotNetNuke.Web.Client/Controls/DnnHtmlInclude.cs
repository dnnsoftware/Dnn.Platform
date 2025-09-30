// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.Client.ClientResourceManagement
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.RegularExpressions;
    using System.Web.UI.WebControls;

    using DotNetNuke.Abstractions.ClientResources;
    using DotNetNuke.Web.Client.Cdf;
    using DotNetNuke.Web.Client.ResourceManager;

    /// <summary>Allows for registration of CSS and JavaScript resources.</summary>
    public class DnnHtmlInclude : Literal
    {
        public const string TagPattern = @"<{0}((\s+\w+(\s*=\s*(?:"".*?""|'.*?'|[^'"">\s]+))?)+\s*|\s*)/?>";
        public const string AttributePattern = @"{0}(\s*=\s*(?:""(?<val>.*?)""|'(?<val>.*?)'|(?<val>[^'"">\s]+)))";

        private const string MatchAllAttributes = "(\\S+)=[\"']?((?:.(?![\"']?\\s+(?:\\S+)=|[>\"']))+.)[\"']?";

        private static readonly Regex LinkTagRegex = new Regex(string.Format(TagPattern, "link"), RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);
        private static readonly Regex ScriptTagRegex = new Regex(string.Format(TagPattern, "script"), RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);

        private readonly IClientResourceController clientResourceController;

        public DnnHtmlInclude(IClientResourceController clientResourceController)
        {
            this.clientResourceController = clientResourceController;
        }

        /// <summary>
        /// Gets or sets the provider to force for this resource include.
        /// </summary>
        public string ForceProvider { get; set; }

        /// <summary>
        /// Gets or sets the priority for this resource include.
        /// </summary>
        public int Priority { get; set; } = 100;

        /// <summary>
        /// Gets or sets the group for this resource include.
        /// </summary>
        public int Group { get; set; } = 100;

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            this.RegisterIncludes(this.Text);
            this.Text = string.Empty;
        }

        private void RegisterIncludes(string innerHtml)
        {
            this.RegisterIncludes(this.GetIncludes(innerHtml, ClientDependencyType.Css), ClientDependencyType.Css);
            this.RegisterIncludes(this.GetIncludes(innerHtml, ClientDependencyType.Javascript), ClientDependencyType.Javascript);
        }

        private void RegisterIncludes(IEnumerable<BasicFile> files, ClientDependencyType dependencyType)
        {
            foreach (var file in files)
            {
                switch (dependencyType)
                {
                    case ClientDependencyType.Css:
                        var styleSheet = this.clientResourceController.CreateStylesheet()
                                                        .FromSrc(file.FilePath)
                                                        .SetProvider(file.ForceProvider)
                                                        .SetPriority(file.Priority);
                        foreach (var a in file.HtmlAttributes)
                        {
                            styleSheet.AddAttribute(a.Key, a.Value);
                        }

                        styleSheet.Register();
                        break;
                    case ClientDependencyType.Javascript:
                        var script = this.clientResourceController.CreateScript()
                                                        .FromSrc(file.FilePath)
                                                        .SetProvider(file.ForceProvider)
                                                        .SetPriority(file.Priority);
                        foreach (var a in file.HtmlAttributes)
                        {
                            script.AddAttribute(a.Key, a.Value);
                        }

                        script.Register();
                        break;
                }
            }
        }

        private IEnumerable<BasicFile> GetIncludes(string innerHtml, ClientDependencyType dependencyType)
        {
            Regex tagRegex;
            string sourceAttribute, mime;
            if (dependencyType == ClientDependencyType.Css)
            {
                tagRegex = LinkTagRegex;
                sourceAttribute = "href";
                mime = "text/css";
            }
            else
            {
                tagRegex = ScriptTagRegex;
                sourceAttribute = "src";
                mime = null;
            }

            var files = new List<BasicFile>();
            foreach (Match match in tagRegex.Matches(innerHtml))
            {
                var allAttributes = Regex.Matches(match.Value, MatchAllAttributes, RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)
                                         .Cast<Match>()
                                         .ToArray();

                var type = allAttributes.FirstOrDefault(x =>
                {
                    if (x.Groups.Count < 3)
                    {
                        return false;
                    }

                    return x.Groups[1].Value == "type";
                });

                var href = allAttributes.FirstOrDefault(x =>
                {
                    if (x.Groups.Count < 3)
                    {
                        return false;
                    }

                    return x.Groups[1].Value == sourceAttribute;
                });

                if (href == null || (type == null && mime != null) || (type != null && mime != null && type.Groups[2].Value != mime))
                {
                    continue;
                }

                var attributes = allAttributes.Where(x =>
                {
                    if (x.Groups.Count < 3)
                    {
                        return false;
                    }

                    return x.Groups[1].Value != sourceAttribute && x.Groups[1].Value != "type";
                }).ToDictionary(x => x.Groups[1].Value, x => x.Groups[2].Value);

                var file = new BasicFile(dependencyType)
                {
                    FilePath = href.Groups[2].Value,
                    Group = this.Group,
                    Priority = this.Priority,
                    ForceProvider = this.ForceProvider,
                };

                foreach (var a in attributes)
                {
                    file.HtmlAttributes.Add(a.Key, a.Value);
                }

                files.Add(file);
            }

            return files;
        }
    }
}
