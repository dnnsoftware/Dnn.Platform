﻿#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2014
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

using System;
using System.Web;
using System.Web.UI;

namespace DotNetNuke.Web.Client.ClientResourceManagement
{
    using ClientDependency.Core.Controls;

    /// <summary>
    /// The central control with which all client resources are registered.
    /// </summary>
    public class ClientResourceLoader : ClientDependencyLoader
    {
        private bool AsyncPostBackHandlerEnabled 
        {
            get
            {
                return HttpContext.Current != null
                       && HttpContext.Current.Items.Contains("AsyncPostBackHandlerEnabled");
            }
        }

        protected override void OnPreRender(System.EventArgs e)
        {
            foreach (var path in Paths)
            {
                path.Name = path.Name.ToLowerInvariant();
            }

            if (AsyncPostBackHandlerEnabled)
            {
                const string handlerScript = @"
(function($){
Sys.WebForms.PageRequestManager.getInstance().add_pageLoading(function (sender, args){
    var dataItems = args.get_dataItems();
    for(var item in dataItems){
        if(item.indexOf('$crm_') > -1){
            var content = dataItems[item];
            //check whether script already register to page.
            if(typeof window.dnnLoadScriptsInAjaxMode === 'undefined'){
                window.dnnLoadScriptsInAjaxMode = [];
            }
            var scripts = content.match(/<script.+?><\/script>/gi);
            if(scripts && scripts.length > 0){
                for(var i = 0; i < scripts.length; i++){
                    var src = scripts[i].match(/src=""(.+?)""/i)[1];
                    if($('script[src=""' + src + '""]').length == 0){
                        window.dnnLoadScriptsInAjaxMode.push(src);
                        var s = document.createElement( 'script' );
                        s.type = 'text/javascript';
                        s.src = src;
                        s.onload = window.dnnLoadScriptsInAjaxModeComplete;
                        document.body.appendChild(s);
                    }
                }
            }

            var styles = content.match(/<link[^>]+?>/gi);
            if(styles && styles.length > 0){
                for(var i = 0; i < styles.length; i++){
                    var src = styles[i].match(/href=""(.+?)""/i)[1];
                    if($('link[href=""' + src + '""]').length == 0){
                        $(document.body).append(styles[i]);
                    }
                }
            }
        }
    }
});

if(typeof window.dnnLoadScriptsInAjaxModeComplete == 'undefined'){
    window.dnnLoadScriptsInAjaxModeComplete = function(){
        var src = $(this).attr('src');
        if(typeof window.dnnLoadScriptsInAjaxMode !== 'undefined'){
            for(var i = 0; i < window.dnnLoadScriptsInAjaxMode.length; i++){
                if(window.dnnLoadScriptsInAjaxMode[i] == src){
                    window.dnnLoadScriptsInAjaxMode.splice(i, 1);
                    break;
                }
            }
            if(window.dnnLoadScriptsInAjaxMode.length == 0){
                $(window).trigger('dnnScriptLoadComplete');
            }
        }
    }
}
}(jQuery));
";
                Page.ClientScript.RegisterStartupScript(this.GetType(), "CRMHandler", handlerScript, true);
            }

            base.OnPreRender(e);
        }
    }
}