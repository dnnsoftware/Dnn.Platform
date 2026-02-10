// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.Client.ClientResourceManagement
{
    using System.Web;
    using System.Web.UI;
    using System.Web.UI.WebControls;

    using DotNetNuke.Abstractions.ClientResources;

    /// <summary>Renders client resources.</summary>
    public class DnnResources : Literal
    {
        private readonly IClientResourceController clientResourceController;

        /// <summary>Initializes a new instance of the <see cref="DnnResources"/> class.</summary>
        /// <param name="clientResourceController">The client resource controller.</param>
        public DnnResources(IClientResourceController clientResourceController)
        {
            this.clientResourceController = clientResourceController;
        }

        /// <summary>Gets or sets the application path.</summary>
        public string ApplicationPath { get; set; }

        /// <summary>Gets or sets the provider.</summary>
        public string Provider { get; set; }

        private static bool AsyncPostBackHandlerEnabled =>
            HttpContext.Current != null && HttpContext.Current.Items.Contains("AsyncPostBackHandlerEnabled");

        /// <inheritdoc />
        protected override void Render(HtmlTextWriter writer)
        {
            base.Render(writer);
            writer.Write(this.clientResourceController.RenderDependencies(ResourceType.All, this.Provider, this.ApplicationPath));

            var scriptManager = ScriptManager.GetCurrent(this.Page);
            if (this.Provider == ClientResourceProviders.DnnBodyProvider && scriptManager != null && scriptManager.IsInAsyncPostBack)
            {
                var holderControl = this.Page.FindControl("BodySCRIPTS");
                holderControl.ID = "$crm_BodySCRIPTS";

                // Get scripts to be loaded during async postback
                var js = this.clientResourceController.RenderDependencies(ResourceType.Script, ClientResourceProviders.DnnBodyProvider, this.ApplicationPath);
                var css = this.clientResourceController.RenderDependencies(ResourceType.Stylesheet, ClientResourceProviders.DefaultCssProvider, this.ApplicationPath);
                scriptManager.RegisterDataItem(holderControl, $"{js}{css}");
            }
        }

        /// <inheritdoc />
        protected override void OnInit(System.EventArgs e)
        {
            if (AsyncPostBackHandlerEnabled && ScriptManager.GetCurrent(this.Page) == null)
            {
                throw new ScriptManagerRequiredException("The ClientResourceLoader control requires a ScriptManager on the page when AsyncPostBackHandlerEnabled is true.");
            }

            base.OnInit(e);
        }

        /// <inheritdoc/>
        protected override void OnPreRender(System.EventArgs e)
        {
            if (AsyncPostBackHandlerEnabled)
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
