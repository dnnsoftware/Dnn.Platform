// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.Client.ClientResourceManagement
{
    using System;
    using System.Web;
    using System.Web.UI;

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
            foreach (var path in this.Paths)
            {
                path.Name = path.Name.ToLowerInvariant();
            }

            if (this.AsyncPostBackHandlerEnabled)
            {
                const string handlerScript = @"
var loadScriptInSingleMode = function(){
    var s = document.createElement( 'script' );
    s.type = 'text/javascript';
    s.src = window.dnnLoadScriptsInAjaxMode[0];
    s.onload = window.dnnLoadScriptsInAjaxModeComplete;
    s.onerror = window.dnnLoadScriptsInAjaxModeComplete;
    document.body.appendChild(s);
};

var loadScriptInMultipleMode = function(){
    for(var i = 0; i < window.dnnLoadScriptsInAjaxMode.length; i++){
        var s = document.createElement( 'script' );
        s.type = 'text/javascript';
        s.src = window.dnnLoadScriptsInAjaxMode[i];
        s.onload = window.dnnLoadScriptsInAjaxModeComplete;
        s.onerror = window.dnnLoadScriptsInAjaxModeComplete;
        document.body.appendChild(s);
    }
};

var isFirefox = navigator.userAgent.toLowerCase().indexOf('firefox') > -1;

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
    
    if(window.dnnLoadScriptsInAjaxMode.length > 0){
        window.loadingScriptsInAsyncRequest = true;
        if(isFirefox || window['forceLoadScriptsInSingleMode'] === true){
            loadScriptInSingleMode();
        } else {
            loadScriptInMultipleMode();
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
                window.loadingScriptsInAsyncRequest = false;
                $(window).trigger('dnnScriptLoadComplete');
            }else if(isFirefox || window['forceLoadScriptsInSingleMode'] === true){
                loadScriptInSingleMode();
            }
        }
    }
}

var originalScriptLoad = Sys._ScriptLoader.getInstance().loadScripts;
Sys._ScriptLoader.getInstance().loadScripts = function(){
    var self = this, args = arguments;
    if(window.loadingScriptsInAsyncRequest === true){
        $(window).one('dnnScriptLoadComplete', function(){
            originalScriptLoad.apply(self, args);
        });
    } else {
        originalScriptLoad.apply(self, args);
    }
};
}(jQuery));
";
                this.Page.ClientScript.RegisterStartupScript(this.GetType(), "CRMHandler", handlerScript, true);
            }

            base.OnPreRender(e);
        }
    }
}
