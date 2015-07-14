using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.IO;
using System.Web.UI;
using System.Text.RegularExpressions;
using ClientDependency.Core.CompositeFiles.Providers;
using ClientDependency.Core.Controls;
using ClientDependency.Core.FileRegistration.Providers;
using ClientDependency.Core.Config;
using ClientDependency.Core;
using System.Net;

namespace ClientDependency.Core.Module
{

    /// <summary>
    /// Used as an http response filter to modify the contents of the output html.
    /// This filter is used to intercept js and css rogue registrations on the html page.
    /// </summary>
    public class RogueFileFilter : IFilter
    {

        #region Private members

        private bool? m_Runnable = null;
        private string m_MatchScript = "<script(?:(?:.*(?<src>(?<=src=\")[^\"]*(?=\"))[^>]*)|[^>]*)>(?<content>(?:(?:\n|.)(?!(?:\n|.)<script))*)</script>";
        private string m_MatchLink = "<link\\s+[^>]*(href\\s*=\\s*(['\"])(?<href>.*?)\\2)";

        private RogueFileCompressionElement m_FoundPath = null;

        #endregion

        #region IFilter Members

        public void SetHttpContext(HttpContextBase ctx)
        {
            CurrentContext = ctx;
            m_FoundPath = GetSupportedPath();
        }

        /// <summary>
        /// This filter can only execute when it's a Page or MvcHandler
        /// </summary>
        /// <returns></returns>
        public virtual bool ValidateCurrentHandler()
        {
            //don't filter if we're in debug mode
            if (CurrentContext.IsDebuggingEnabled || !ClientDependencySettings.Instance.DefaultFileRegistrationProvider.EnableCompositeFiles)
                return false;

            return (CurrentContext.CurrentHandler is Page);
        }

        /// <summary>
        /// Returns true when this filter should be applied
        /// </summary>
        /// <returns></returns>
        public bool CanExecute()
        {
            if (!ValidateCurrentHandler())
            {
                return false;
            }

            if (!m_Runnable.HasValue)
            {
                m_Runnable = (m_FoundPath != null);
            }
            return m_Runnable.Value;

        }

        /// <summary>
        /// Replaces any rogue script tag's with calls to the compression handler instead 
        /// of just the script.
        /// </summary>
        public string UpdateOutputHtml(string html)
        {
            html = ReplaceScripts(html);
            html = ReplaceStyles(html);
            return html;
        }

        public HttpContextBase CurrentContext { get; private set; }

        #endregion

        #region Private methods

        private RogueFileCompressionElement GetSupportedPath()
        {
            var rogueFiles = ClientDependencySettings.Instance
                .ConfigSection
                .CompositeFileElement
                .RogueFileCompression;

            return (from m in rogueFiles.Cast<RogueFileCompressionElement>()
                    let reg = m.FilePath == "*" ? ".*" : m.FilePath
                    let matched = Regex.IsMatch(CurrentContext.Request.RawUrl, reg, RegexOptions.Compiled | RegexOptions.IgnoreCase)
                    where matched
                    let isGood = m.ExcludePaths.Cast<RogueFileCompressionExcludeElement>().Select(e => Regex.IsMatch(CurrentContext.Request.RawUrl, e.FilePath, RegexOptions.Compiled | RegexOptions.IgnoreCase)).All(excluded => !excluded)
                    where isGood
                    select m).FirstOrDefault();
        }

        /// <summary>
        /// Replaces all src attribute values for a script tag with their corresponding 
        /// URLs as a composite script.
        /// </summary>
        /// <param name="html"></param>
        /// <returns></returns>
        private string ReplaceScripts(string html)
        {
            //check if we should be processing!            
            if (CanExecute() && m_FoundPath.CompressJs)
            {
                return ReplaceContent(html, "src", m_FoundPath.JsRequestExtension.Split(','), ClientDependencyType.Javascript, m_MatchScript, CurrentContext);
            }
            return html;
        }

        /// <summary>
        /// Replaces all href attribute values for a link tag with their corresponding 
        /// URLs as a composite style.
        /// </summary>
        /// <param name="html"></param>
        /// <returns></returns>
        private string ReplaceStyles(string html)
        {
            //check if we should be processing!            
            if (CanExecute() && m_FoundPath.CompressCss)
            {
                return ReplaceContent(html, "href", m_FoundPath.CssRequestExtension.Split(','), ClientDependencyType.Css, m_MatchLink, CurrentContext);
            }
            return html;
        }

        /// <summary>
        /// Replaces the content with the new js/css paths
        /// </summary>
        /// <param name="html"></param>
        /// <param name="namedGroup"></param>
        /// <param name="extensions"></param>
        /// <param name="type"></param>
        /// <param name="regex"></param>
        /// <param name="http"></param>
        /// <returns></returns>
        /// <remarks>
        /// For some reason ampersands that aren't html escaped are not compliant to HTML standards when they exist in 'link' or 'script' tags in URLs,
        /// we need to replace the ampersands with &amp; . This is only required for this one w3c compliancy, the URL itself is a valid URL.
        /// </remarks>
        private static string ReplaceContent(string html, string namedGroup, string[] extensions,
            ClientDependencyType type, string regex, HttpContextBase http)
        {
            html = Regex.Replace(html, regex,
                (m) =>
                {
                    var grp = m.Groups[namedGroup];

                    //if there is no namedGroup group name or it doesn't end with a js/css extension or it's already using the composite handler,
                    //the return the existing string.
                    if (grp == null
                        || string.IsNullOrEmpty(grp.ToString())
                        || !grp.ToString().EndsWithOneOf(extensions)
                        || grp.ToString().StartsWith(ClientDependencySettings.Instance.CompositeFileHandlerPath))
                        return m.ToString();

                    
                    //make sure that it's an internal request, though we can deal with external 
                    //requests, we'll leave that up to the developer to register an external request
                    //explicitly if they want to include in the composite scripts.
                    try
                    {
                        var url = new Uri(grp.ToString(), UriKind.RelativeOrAbsolute);
                        if (!url.IsLocalUri(http))
                            return m.ToString(); //not a local uri        
                        else
                        {
                           
                            var dependency = new BasicFile(type) { FilePath = grp.ToString() };

                            var file = new[] { new BasicFile(type) { FilePath = dependency.ResolveFilePath(http) } };

                            var resolved = ClientDependencySettings.Instance.DefaultCompositeFileProcessingProvider.ProcessCompositeList(
                                file,
                                type,
                                http).Single();

                            return m.ToString().Replace(grp.ToString(), resolved.Replace("&", "&amp;"));
                        }
                    }
                    catch (UriFormatException)
                    {
                        //malformed url, let's exit
                        return m.ToString();
                    }

                },
                RegexOptions.Compiled);

            return html;
        }


        #endregion

    }
}
