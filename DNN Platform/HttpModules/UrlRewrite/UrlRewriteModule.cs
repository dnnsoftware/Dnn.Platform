// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

#region Usings

using System.Web;

using DotNetNuke.Entities.Urls;
using DotNetNuke.Framework.Providers;
using DotNetNuke.HttpModules.UrlRewrite;

#endregion

namespace DotNetNuke.HttpModules
{
    public class UrlRewriteModule : IHttpModule
    {
        private string _providerToUse;
        private UrlRewriterBase _urlRewriter;

        public string ModuleName
        {
            get { return "UrlRewriteModule"; }
        }

        #region IHttpModule Members

        public void Init(HttpApplication application)
        {
            _providerToUse = DotNetNuke.Common.Utilities.Config.GetFriendlyUrlProvider();

            //bind events depending on currently configured friendly url provider
            //note that the current configured friendly url provider determines what type 
            //of url rewriting is required.

            switch (_providerToUse)
            {
                case "advanced":
                    var advancedRewriter = new AdvancedUrlRewriter();
                    _urlRewriter = advancedRewriter;
                    //bind the rewrite event to the begin request event
                    application.BeginRequest += _urlRewriter.RewriteUrl;
                    break;
                default:
                    var basicRewriter = new BasicUrlRewriter();
                    _urlRewriter = basicRewriter;
                    application.BeginRequest += _urlRewriter.RewriteUrl;
                    break;
            }
        }

        public void Dispose()
        {
        }

        #endregion
    }
}
