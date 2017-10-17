using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Text.RegularExpressions;
 
namespace ClientDependency.Core.Controls
{
    [ToolboxData("<{0}:HtmlInclude runat=\"server\"></{0}:HtmlInclude>")]
    public class HtmlInclude : Literal
    {
        public const string TagPattern = @"<{0}((\s+\w+(\s*=\s*(?:"".*?""|'.*?'|[^'"">\s]+))?)+\s*|\s*)/?>";
        public const string AttributePattern = @"{0}(\s*=\s*(?:""(?<val>.*?)""|'(?<val>.*?)'|(?<val>[^'"">\s]+)))";

        private const string MatchAllAttributes = "(\\S+)=[\"']?((?:.(?![\"']?\\s+(?:\\S+)=|[>\"']))+.)[\"']?";

        //*** DNN related change *** begin
        private static readonly Regex LinkTagRegex = new Regex(string.Format(TagPattern, "link"), RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);
        private static readonly Regex ScriptTagRegex = new Regex(string.Format(TagPattern, "script"), RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);
        //*** DNN related change *** end

        public string ForceProvider { get; set; }
        public int Priority { get; set; }
        public int Group { get; set; }

        public HtmlInclude()
        {
            Priority = Constants.DefaultPriority;
            Group = Constants.DefaultGroup;
            ForceProvider = null;
        }

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            var isNew = false;
            var loader = ClientDependencyLoader.TryCreate(Page, new HttpContextWrapper(Context), out isNew);

            RegisterIncludes(Text, loader);

            Text = string.Empty;
        }

        private void RegisterIncludes(string innerHtml, ClientDependencyLoader loader)
        {
            RegisterIncludes(GetIncludes(innerHtml, ClientDependencyType.Css), loader, ClientDependencyType.Css);
            RegisterIncludes(GetIncludes(innerHtml, ClientDependencyType.Javascript), loader, ClientDependencyType.Javascript);
        }

        private void RegisterIncludes(IEnumerable<BasicFile> files, ClientDependencyLoader loader, ClientDependencyType dependencyType)
        {
            foreach (var file in files)
            {
                loader.RegisterDependency(file.Group, file.Priority, file.FilePath, "", dependencyType, file.HtmlAttributes, file.ForceProvider);
            }
        }

        internal IEnumerable<BasicFile> GetIncludes(string innerHtml, ClientDependencyType dependencyType)
        {
            //*** DNN related change *** begin
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
                mime = "text/javascript";
            }

            var files = new List<BasicFile>();
            foreach (Match match in tagRegex.Matches(innerHtml))
            {
                var allAttributes = Regex.Matches(match.Value, MatchAllAttributes,
                                                  RegexOptions.IgnoreCase |
                                                  RegexOptions.CultureInvariant)
                                         .Cast<Match>()
                                         .ToArray();
            //*** DNN related change *** end

                var type = allAttributes.FirstOrDefault(x =>
                {
                    if (x.Groups.Count < 3) return false;
                    return x.Groups[1].Value == "type";
                });

                var href = allAttributes.FirstOrDefault(x =>
                {
                    if (x.Groups.Count < 3) return false;
                    return x.Groups[1].Value == sourceAttribute;
                });

                if (type == null || href == null || type.Groups[2].Value != mime) continue;

                var attributes = allAttributes.Where(x =>
                {
                    if (x.Groups.Count < 3) return false;
                    return x.Groups[1].Value != sourceAttribute && x.Groups[1].Value != "type";
                }).ToDictionary(x => x.Groups[1].Value, x => x.Groups[2].Value);

                var file = new BasicFile(dependencyType)
                    {
                        FilePath = href.Groups[2].Value,
                        Group = Group,
                        Priority = Priority,
                        ForceProvider = ForceProvider
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
