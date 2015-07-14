/*Special Thanks to Ruben Verborgh for his work on this and in the Umbraco core!*/

if (typeof ClientDependency == 'undefined') var ClientDependency = {};
if (!ClientDependency.Sys) ClientDependency.Sys = {};


ClientDependency.Sys.LazyLoader = function() {
    this.addedDependencies = [];
    this.jsQueue = new Array();
    this.jsQueueWaiting = false;
}

// ********************* Dependency Loader Methods *********************
ClientDependency.Sys.LazyLoader.prototype.AddJs = function(filePath, callbackMethod) {
    if (this.addedDependencies[filePath] == undefined) {
        //Sys.Debug.trace("LazyLoader: Queueing '" + filePath + "'.");

        // add script to queue
        var script = new Array();
        script.filePath = filePath;
        script.callbackMethod = callbackMethod;
        script.loaded = false;
        this.jsQueue[this.jsQueue.length] = script;
        this.addedDependencies[filePath] = true;

        //Sys.Debug.trace("LazyLoader: Queued '" + filePath + "', queue length " + this.jsQueue.length + ".");

        if (!this.jsQueueWaiting)
            this.LoadNextJs();
    }
    else {
        //Sys.Debug.trace("LazyLoader: Javascript file has already been added '" + filePath + "'.");
    }

    return this;
}

ClientDependency.Sys.LazyLoader.prototype.RegisterCallbackMethod = function(callbackMethod) {
    /// <summary>
    /// This registers a method to be called. The system will check if the method is available in the DOM 
    /// to be called, if it is not, it will wait until it is.
    /// </summary>

    if (callbackMethod == "") {
        return this;
    }
    var script = { loaded: false, callbackMethod: callbackMethod };
    ClientDependency.Sys.onScriptAvailable(script);

    return this;
}

function logthis(txt) {

}

ClientDependency.Sys.LazyLoader.prototype.AddCss = function(filePath) {
    if (this.addedDependencies[filePath] == undefined) {

        var tempCss = document.createElement("link")
        tempCss.setAttribute("href", filePath);
        tempCss.setAttribute("rel", "stylesheet");
        tempCss.setAttribute("type", "text/css");
        document.getElementsByTagName("head")[0].appendChild(tempCss);

        this.addedDependencies[filePath] = "loaded";

    }
    else {
        //Sys.Debug.trace("LazyLoader: Css file has already been added: '" + filePath + "'.");
    }
    return this;
}

ClientDependency.Sys.LazyLoader.prototype.LoadNextJs = function() {
    if (this.jsQueue.length > 0) {
        this.jsQueueWaiting = true;

        var script = this.jsQueue[0];
        this.jsQueue.splice(0, 1);

        //Sys.Debug.trace("LazyLoader: Loading '" + script.filePath + "'. (" + this.jsQueue.length + " scripts left)");
        var tempJs = document.createElement('script');
        tempJs.setAttribute("src", script.filePath);
        tempJs.setAttribute("type", "text/javascript");

        tempJs.onload = function() { ClientDependency.Sys.onScriptAvailable(script); };
        tempJs.onreadystatechange = function() {
            if (this.readyState == 'loaded' || this.readyState == 'complete') {
                ClientDependency.Sys.onScriptAvailable(script);
            }
        };
        document.getElementsByTagName("head")[0].appendChild(tempJs);
    }
    else {
        //Sys.Debug.trace('LazyLoader: No scripts left.');
        this.jsQueueWaiting = false;
    }
}

// Initialize
var CDLazyLoader = new ClientDependency.Sys.LazyLoader();

ClientDependency.Sys.onScriptAvailable = function(script) {
    /// <summary>
    /// This method checks if the string representation of a method (sMethod) is registered as a function yet,
    /// and if it is it'll call the function (oCallback), if not it'll try again after 50 ms.
    /// </summary>
    if (!script.loaded) {
        //Sys.Debug.trace("LazyLoader: Loaded '" + script.filePath + "'.");
        script.loaded = true;
    }
    if (script.callbackMethod == '') {
        CDLazyLoader.LoadNextJs();
    }
    else {
        
        try {
            eval(script.callbackMethod);
        }
        catch (err) {
            //the object definitely doesn't exist.
            setTimeout(function() {
                ClientDependency.Sys.onScriptAvailable(script);
            }, 50);
            return;
        }

        if (typeof (eval(script.callbackMethod)) == 'function') {
            CDLazyLoader.LoadNextJs();
            //Sys.Debug.trace("LazyLoader: Executing '" + script.filePath + "' callback '" + script.callbackMethod + "'.");
            var func = eval(script.callbackMethod);
            func.call(this);
        }
        else {
            setTimeout(function() {
                ClientDependency.Sys.onScriptAvailable(script);
            }, 50);
        }
    }
}