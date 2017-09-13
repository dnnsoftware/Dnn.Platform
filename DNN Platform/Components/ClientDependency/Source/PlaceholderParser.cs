using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;

namespace ClientDependency.Core
{
    internal class PlaceholderParser
    {
        internal static event EventHandler<PlaceholderReplacementEventArgs> PlaceholderReplaced;
        private static void OnPlaceholderReplaced(PlaceholderReplacementEventArgs e)
        {
            var handler = PlaceholderReplaced;
            if (handler != null) handler(null, e);
        }
        internal static event EventHandler<PlaceholdersReplacedEventArgs> AllPlaceholdersReplaced;
        private static void OnAllPlaceholdersReplaced(PlaceholdersReplacedEventArgs e)
        {
            var handler = AllPlaceholdersReplaced;
            if (handler != null) handler(null, e);
        }

        /// <summary>
        /// This replaces the HTML placeholders that we're rendered into the html
        /// markup before the module calls this method to update the placeholders with 
        /// the real dependencies.
        /// </summary>
        /// <param name="currentContext"></param>
        /// <param name="html"></param>
        /// <param name="jsMarkupRegex"></param>
        /// <param name="cssMarkupRegex"></param>
        /// <param name="output"></param>
        /// <returns></returns>
        public static string ParseHtmlPlaceholders(HttpContextBase currentContext, string html, string jsMarkupRegex, string cssMarkupRegex, RendererOutput[] output)
        {
            html = Regex.Replace(html, jsMarkupRegex,
                (m) =>
                {
                    var grp = m.Groups["renderer"];
                    if (grp != null)
                    {
                        if (output.Any())
                        {
                            var rendererOutput = output.SingleOrDefault(x => x.Name == grp.ToString());

                            var args = new PlaceholderReplacementEventArgs(currentContext, ClientDependencyType.Javascript,
                                rendererOutput != null ? rendererOutput.OutputJs : "",
                                m);
                            OnPlaceholderReplaced(args);

                            return args.ReplacedText;
                        }
                        else
                        {
                            //output a message saying that there were no refs
                            return "<!-- CDF: No JS dependencies were declared //-->";   
                        }                        
                    }
                    
                    return m.ToString();

                //*** DNN related change *** begin
                });
                //*** DNN related change *** end

            html = Regex.Replace(html, cssMarkupRegex,
                (m) =>
                {
                    var grp = m.Groups["renderer"];
                    if (grp != null)
                    {
                        if (output.Any())
                        {
                            var rendererOutput = output.SingleOrDefault(x => x.Name == grp.ToString());

                            var args = new PlaceholderReplacementEventArgs(currentContext, ClientDependencyType.Css,
                                rendererOutput != null ? rendererOutput.OutputCss : "",
                                m);
                            OnPlaceholderReplaced(args);
                            return args.ReplacedText;
                        }
                        else
                        {
                            //output a message saying that there were no refs
                            return "<!-- CDF: No CSS dependencies were declared //-->";
                        }                        
                    }

                    return m.ToString();

                //*** DNN related change *** begin
                });
                //*** DNN related change *** end


            var replacedArgs = new PlaceholdersReplacedEventArgs(currentContext, html);
            OnAllPlaceholdersReplaced(replacedArgs);
            return replacedArgs.ReplacedText;
        }

    }
}
