/// <reference name="MicrosoftAjax.js" />
/// <reference name="dnn.js" assembly="DotNetNuke.WebUtility" />

/*add browser detect for chrome*/
var dnnJscriptVersion = "6.0.0";

if (typeof (Sys.Browser.Chrome) == "undefined") {
    Sys.Browser.Chrome = {};
    if (navigator.userAgent.indexOf(" Chrome/") > -1) {
        Sys.Browser.agent = Sys.Browser.Chrome;
        Sys.Browser.version = parseFloat(navigator.userAgent.match(/Chrome\/(\d+\.\d+)/)[1]);
        Sys.Browser.name = "Chrome";
        Sys.Browser.hasDebuggerStatement = true;
    }
}
else if (Sys.Browser.agent === Sys.Browser.InternetExplorer && Sys.Browser.version > 10) {
    // when browse in IE11, we need add attachEvent/detachEvent handler to make it works with MS AJAX library.
    HTMLAnchorElement.prototype.attachEvent = function (eventName, handler) {
        if (eventName.substr(0, 2) == "on") eventName = eventName.substr(2);
        this.addEventListener(eventName, handler, false);
    }
    HTMLAnchorElement.prototype.detachEvent = function (eventName, handler) {
        if (eventName.substr(0, 2) == "on") eventName = eventName.substr(2);
        this.removeEventListener(eventName, handler, false);
    }
}

//This is temp fix for jQuery UI issue: http://bugs.jqueryui.com/ticket/9315
//this code can be safe removed after jQuery UI library upgrade to 1.11.
if ($ && $.ui && $.ui.dialog) {
    $.extend($.ui.dialog.prototype.options, {
        open: function () {
            var htmlElement = $(document).find('html');
            htmlElement.css('overflow', 'hidden');
            var cacheScrollTop = htmlElement.find('body').scrollTop();
            if (cacheScrollTop > 0) {
                htmlElement.scrollTop(0);
                var target = $(this);
                target.data('cacheScrollTop', cacheScrollTop);
            }

            var uiDialog = $(this).closest('.ui-dialog');
            if (!$('html').hasClass('mobileView')) {
                var maxHeight = $(window).height();
                var dialogHeight = uiDialog.outerHeight();
                if (maxHeight - 20 >= dialogHeight) {
                    uiDialog.css({
                        position: 'fixed',
                        left: '50%',
                        top: '50%',
                        marginLeft: '-' + (uiDialog.outerWidth() / 2) + 'px',
                        marginTop: '-' + (uiDialog.outerHeight() / 2) + 'px',
                        maxHeight: 'inherit',
                        overflow: 'initial'
                    });
                } else {
                    uiDialog.css({
                        position: 'fixed',
                        left: '50%',
                        top: '0',
                        marginLeft: '-' + (uiDialog.outerWidth() / 2) + 'px',
                        marginTop: '0',
                        maxHeight: (maxHeight - 20) + 'px',
                        overflow: 'auto'
                    });
                }
            }

            uiDialog.find('.ui-dialog-titlebar-close').attr('aria-label', 'Close');
        },

        beforeClose: function () {
            var htmlElement = $(document).find('html');
            htmlElement.css('overflow', '');
            var cacheScrollTop = $(this).data('cacheScrollTop');
            if (cacheScrollTop) {
                htmlElement.find('body').scrollTop(cacheScrollTop);
                $(this).data('cacheScrollTop', null);
            }
			var uiDialog = $(this).closest('.ui-dialog');
            uiDialog.css({ overflow: 'initial' });
        }
    });
}

var DNN_HIGHLIGHT_COLOR = '#9999FF';
var COL_DELIMITER = String.fromCharCode(18);
var ROW_DELIMITER = String.fromCharCode(17);
var QUOTE_REPLACEMENT = String.fromCharCode(19);
var KEY_LEFT_ARROW = 37;
var KEY_UP_ARROW = 38;
var KEY_RIGHT_ARROW = 39;
var KEY_DOWN_ARROW = 40;
var KEY_RETURN = 13;
var KEY_ESCAPE = 27;

Type.registerNamespace('dnn');
dnn.extend = function (dest, src) {
    for (s in src)
        dest[s] = src[s];
    return dest;
}

dnn.extend(dnn, {
    apiversion: new Number('04.02'),
    pns: '',
    ns: 'dnn',
    diagnostics: null,
    vars: null,
    dependencies: new Array(),
    isLoaded: false,
    delay: [],
    _delayedSet: null,  //used to delay setting variable until page is loaded - perf

    getVars: function () {
        /// <summary>
        /// Gets array of name value pairs set on the server side by the RegisterClientVariable method.
        /// </summary>
        /// <value type="String" />
        if (this.vars == null) {
            //this.vars = new Array();
            var ctl = dnn.dom.getById('__dnnVariable');

            if (ctl != null) {
                if (ctl.value.indexOf('`') == 0)
                    ctl.value = ctl.value.substring(1).replace(/`/g, '"');

                if (ctl.value.indexOf('__scdoff') != -1) //back compat
                {
                    COL_DELIMITER = '~|~';
                    ROW_DELIMITER = '~`~';
                    QUOTE_REPLACEMENT = '~!~';
                }
            }

            if (ctl != null && ctl.value.length > 0)
                this.vars = Sys.Serialization.JavaScriptSerializer.deserialize(ctl.value);
            else
                this.vars = [];
        }
        return this.vars;
    },

    getVar: function (key, def) {
        /// <summary>
        /// Gets value for passed in variable name set on the server side by the RegisterClientVariable method.
        /// </summary>
        /// <param name="key" type="String">
        /// Name of parameter to retrieve value for
        /// </param>
        /// <param name="def" type="String">
        /// Default value if key not present
        /// </param>
        /// <returns type="String" />
        if (this.getVars()[key] != null) {
            var re = eval('/' + QUOTE_REPLACEMENT + '/g');
            return this.getVars()[key].replace(re, '"');
        }
        return def;
    },

    setVar: function (key, val) {
        /// <summary>
        /// Sets value for variable to be sent to the server
        /// </summary>
        /// <param name="key" type="String">
        /// Name of parameter to set value for
        /// </param>
        /// <param name="val" type="String">
        /// value
        /// </param>
        /// <returns type="Boolean" />
        if (this.vars == null)
            this.getVars();
        this.vars[key] = val;
        var ctl = dnn.dom.getById('__dnnVariable');
        if (ctl == null) {
            ctl = dnn.dom.createElement('INPUT');
            ctl.type = 'hidden';
            ctl.id = '__dnnVariable';
            dnn.dom.appendChild(dnn.dom.getByTagName("body")[0], ctl);
        }
        if (dnn.isLoaded)
            ctl.value = Sys.Serialization.JavaScriptSerializer.serialize(this.vars);
        else
            dnn._delayedSet = { key: key, val: val }; //doesn't matter how many times this gets overwritten, we just want one value to set after load so serialize is called
        return true;
    },

    callPostBack: function (action) {
        /// <summary>
        /// Initiates a postback call for the passed in action. In order to work the action will need to be registered on the server side.
        /// </summary>
        /// <param name="action" type="String">
        /// Action name to be raised
        /// </param>
        /// <param name="N Params" type="Array">
        /// Pass in any number of parameters the postback requires. Parameters should be in the form of 'paramname=paramvalue', 'paramname=paramvalue', 'paramname=paramvalue'
        /// </param>
        /// <returns type="Boolean" />
        var postBack = dnn.getVar('__dnn_postBack');
        var data = '';
        if (postBack.length > 0) {
            data += action;
            for (var i = 1; i < arguments.length; i++) {
                var aryParam = arguments[i].split('=');
                data += COL_DELIMITER + aryParam[0] + COL_DELIMITER + aryParam[1];
            }
            eval(postBack.replace('[DATA]', data));
            return true;
        }
        return false;
    },

    //atlas
    createDelegate: function (oThis, ptr) {
        /// <summary>
        /// Creates delegate (closure)
        /// </summary>
        /// <param name="oThis" type="Object">
        /// Object to create delegate on
        /// </param>
        /// <param name="ptr" type="Function">
        /// Function to invoke
        /// </param>
        /// <returns type="Function" />
        return Function.createDelegate(oThis, ptr);
    },

    doDelay: function (key, time, ptr, ctx) {
        /// <summary>
        /// Allows for a setTimeout to occur that will also pass a context object.
        /// </summary>
        /// <param name="key" type="String">
        /// Key to identify the particular delay. If you wish to cancel this delay you need to call cancelDelay passing this key.
        /// </param>
        /// <param name="time" type="Number">
        /// Number of milliseconds to wait before firing timer. This value is simply passed into the second parameter in setTimeout.
        /// </param>
        /// <param name="ptr" type="Function">
        /// Pointer to the function to invoke after time has elapsed
        /// </param>
        /// <param name="ctx" type="Object">
        /// Context to be passed to the function
        /// </param>
        /// <returns />
        if (this.delay[key] == null) {
            this.delay[key] = new dnn.delayObject(ptr, ctx, key);
            this.delay[key].num = window.setTimeout(dnn.createDelegate(this.delay[key], this.delay[key].complete), time);
        }
    },

    cancelDelay: function (key) {
        /// <summary>
        /// Allows for delay to be canceled.
        /// </summary>
        /// <param name="key" type="String">
        /// Key to identify the particular delay. 
        /// </param>
        /// <returns />
        if (this.delay[key] != null) {
            window.clearTimeout(this.delay[key].num);
            this.delay[key] = null;
        }
    },

    decodeHTML: function (html) {
        /// <summary>
        /// Unencodes html string
        /// </summary>
        /// <param name="html" type="String">
        /// encoded html
        /// </param>
        /// <returns type="String" />
        return html.toString().replace(/&amp;/g, "&").replace(/&lt;/g, "<").replace(/&gt;/g, ">").replace(/&quot;/g, '"');
    },

    encode: function (arg, doubleEncode) {
        /// <summary>
        /// Encodes string using either encodeURIComponent or escape
        /// </summary>
        /// <param name="arg" type="String">
        /// string to encode
        /// </param>
        /// <returns type="String" />
        var ret = arg;
        if (encodeURIComponent)
            ret = encodeURIComponent(ret);
        else
            ret = escape(ret);

        if (doubleEncode == false)
            return ret;
        //handle double encoding for encoded value "+" encode-> "%2B" replace-> "%252B"
        return ret.replace(/%/g, "%25");
    },

    encodeHTML: function (html) {
        /// <summary>
        /// Encodes html string
        /// </summary>
        /// <param name="html" type="String">
        /// html to encode
        /// </param>
        /// <returns type="String" />
        return html.toString().replace(/&/g, "&amp;").replace(/</g, "&lt;").replace(/>/g, "&gt;").replace(/'/g, "&apos;").replace(/\"/g, "&quot;");
    },

    encodeJSON: function (json) {
        /// <summary>
        /// Encodes json string
        /// </summary>
        /// <param name="json" type="String">
        /// json to encode
        /// </param>
        /// <returns type="String" />

        //todo: does Atlas provide method for this?
        return json.toString().replace(/&/g, "&amp;").replace(/</g, "&lt;").replace(/>/g, "&gt;").replace(/'/g, "\u0027").replace(/\"/g, "&quot;").replace(/\\/g, "\\\\");
    },

    //atlas
    evalJSON: function (data) {
        /// <summary>
        /// dencodes data
        /// </summary>
        /// <param name="data" type="String">
        /// json to dencode
        /// </param>
        /// <returns type="Object" />
        return Sys.Serialization.JavaScriptSerializer.deserialize(data);
    },

    escapeForEval: function (s)	//needs work...
    {
        /// <summary>
        /// Allows a string to be evaluated successfully without worry of inappropriate characters. For example ' will be replaced with \' so when evaluated it is equal to 
        /// </summary>
        /// <param name="s" type="String">
        /// string to escape
        /// </param>
        /// <returns type="String" />
        return s.replace(/\\/g, '\\\\').replace(/\'/g, "\\'").replace(/\r/g, '').replace(/\n/g, '\\n').replace(/\./, '\\.');
    },

    getEnumByValue: function (enumType, val) {
        /// <summary>
        /// Obtains enum from value passed in
        /// </summary>
        /// <param name="enumType" type="Object">
        /// Enumeration type
        /// </param>
        /// <param name="val" type="Number">
        /// Value of enumerator
        /// </param>
        /// <returns type="Object" />
        for (var prop in enumType) {
            if (typeof (enumType[prop]) == 'number' && enumType[prop] == val)
                return prop;
        }
    },

    _onload: function () {
        dnn.isLoaded = true;
        if (dnn._delayedSet)
            dnn.setVar(dnn._delayedSet.key, dnn._delayedSet.val);
    },

    addIframeMask: function (ele) { //add an iframe behind the element, so that element will not mask by some special objects.
        if (dnn.dom.browser.isType('ie') && (ele.previousSibling == null || ele.previousSibling.nodeName.toLowerCase() != "iframe")) {
            var mask = document.createElement("iframe"); //"$("<iframe src=\"about:blank\" frameborder=\"0\"></iframe>");
            ele.parentNode.insertBefore(mask, ele);
            var rect = ele.getBoundingClientRect();
            mask.style.position = 'absolute';
            mask.style.left = ele.offsetLeft + "px";
            mask.style.top = ele.offsetTop + "px";
            mask.style.width = (rect.right - rect.left) + "px";
            mask.style.height = (rect.bottom - rect.top) + "px";
            mask.style.opacity = '0';
            mask.style.filter = "progid:DXImageTransform.Microsoft.Alpha(opacity=0)";
            mask.style.zIndex = "-1";

            return mask;
        }

        return null;
    },
    removeIframeMask: function (ele) {
        if (dnn.dom.browser.isType('ie') && (ele.previousSibling != null && ele.previousSibling.nodeName.toLowerCase() == "iframe")) {
            ele.parentNode.removeChild(ele.previousSibling);
        }
    }
});

//delayObject
dnn.delayObject = function (ptr, ctx, type) {
    /// <summary>
    /// Object used to hold context for the doDelay functionality
    /// </summary>
    this.num = null;
    this.pfunc = ptr;
    this.context = ctx;
    this.type = type;
}

dnn.delayObject.prototype =
{
    complete: function () {
        /// <summary>
        /// This function is invoked internally by the setTimout of the doDelay. It in turn will invoke the function referenced by the pfunc property, passing the context
        /// </summary>
        /// <returns />
        dnn.delay[this.type] = null;
        this.pfunc(this.context);
    }
}
dnn.delayObject.registerClass('dnn.delayObject');

dnn.ScriptRequest = function (src, text, fCallBack) {
    /// <summary>
    /// The ScriptRequest object allows the loading of external script files from script
    /// </summary>

    this.ctl = null;
    this.xmlhttp = null;
    this.src = null;
    this.text = null;
    if (src != null && src.length > 0) {
        var file = dnn.dom.scriptFile(src);
        var embedSrc = dnn.getVar(file + '.resx', '');
        if (embedSrc.length > 0)
            this.src = embedSrc;
        else
            this.src = src;
    }
    if (text != null && text.length > 0)
        this.text = text;
    this.callBack = fCallBack;
    this.status = 'init';
    this.timeOut = 5000;
    this._xmlhttpStatusChangeDelegate = dnn.createDelegate(this, this.xmlhttpStatusChange);
    this._statusChangeDelegate = dnn.createDelegate(this, this.statusChange);
    this._completeDelegate = dnn.createDelegate(this, this.complete);
    this._reloadDelegate = dnn.createDelegate(this, this.reload);
    //this.alreadyLoaded = false;
}
dnn.ScriptRequest.prototype =
{
    load: function () {
        /// <summary>
        /// Loads script
        /// </summary>
        this.status = 'loading';
        this.ctl = document.createElement('script');
        this.ctl.type = 'text/javascript';

        if (this.src != null) {
            if (dnn.dom.browser.isType(dnn.dom.browser.Safari)) {
                this.xmlhttp = new XMLHttpRequest();
                this.xmlhttp.open('GET', this.src, true);
                this.xmlhttp.onreadystatechange = this._xmlhttpStatusChangeDelegate;
                this.xmlhttp.send(null);
                return;
            }
            else {
                if (dnn.dom.browser.isType(dnn.dom.browser.InternetExplorer))
                    this.ctl.onreadystatechange = this._statusChangeDelegate;
                else if (dnn.dom.browser.isType(dnn.dom.browser.Opera) == false)	//opera loads synchronously
                    this.ctl.onload = this._completeDelegate;

                this.ctl.src = this.src;
            }
            dnn.dom.scriptElements[this.src] = this.ctl; //JON VERIFY THIS!!!			
        }
        else {
            if (dnn.dom.browser.isType(dnn.dom.browser.Safari))
                this.ctl.innerHTML = dnn.encodeHTML(this.text);
            else
                this.ctl.text = this.text;
        }

        var oHeads = dnn.dom.getByTagName('HEAD');
        if (oHeads) {
            //opera will load script twice if inline and appended to page 
            if (dnn.dom.browser.isType(dnn.dom.browser.Opera) == false || this.src != null)
                oHeads[0].appendChild(this.ctl);
        }
        else
            alert('Cannot load dynamic script, no HEAD tag present.');

        if (this.src == null || dnn.dom.browser.isType(dnn.dom.browser.Opera))	//opera loads script synchronously
            this.complete();
        else if (this.timeOut)
            dnn.doDelay('loadScript_' + this.src, this.timeOut, this._reloadDelegate, null);
    },

    xmlhttpStatusChange: function () {
        /// <summary>
        /// Event fires when script request status changes
        /// </summary>
        if (this.xmlhttp.readyState != 4)
            return;

        this.src = null;
        this.text = this.xmlhttp.responseText;
        this.load(); //load as inline script
    },

    statusChange: function () {
        /// <summary>
        /// Event fires when script request status changes
        /// </summary>
        if ((this.ctl.readyState == 'loaded' || this.ctl.readyState == 'complete') && this.status != 'complete')
            this.complete();
    },

    reload: function () {
        /// <summary>
        /// Reloads a script reference
        /// </summary>
        if (dnn.dom.scriptStatus(this.src) == 'complete') {
            this.complete();
        }
        else {
            this.load();
        }
    },

    complete: function () {
        /// <summary>
        /// Event fires when script request loaded
        /// </summary>
        dnn.cancelDelay('loadScript_' + this.src);
        this.status = 'complete';
        if (typeof (this.callBack) != 'undefined')
            this.callBack(this);
        this.dispose();
    },

    dispose: function () {
        /// <summary>
        /// Cleans up memory
        /// </summary>
        this.callBack = null;
        if (this.ctl) {
            if (this.ctl.onreadystatechange)
                this.ctl.onreadystatechange = new function () { }; //stop IE memory leak.  Not sure why can't set to null;
            else if (this.ctl.onload)
                this.ctl.onload = null;
            this.ctl = null;
        }
        this.xmlhttp = null;
        this._xmlhttpStatusChangeDelegate = null;
        this._statusChangeDelegate = null;
        this._completeDelegate = null;
        this._reloadDelegate = null;
    }
}
dnn.ScriptRequest.registerClass('dnn.ScriptRequest');

//--- dnn.dom
Type.registerNamespace('dnn.dom');

dnn.extend(dnn.dom, {

    pns: 'dnn',
    ns: 'dom',
    browser: null,
    __leakEvts: [],
    scripts: [],
    scriptElements: [],
    tweens: [],

    attachEvent: function (ctl, type, fHandler) {
        /// <summary>
        /// Attatches an event to an element. - you are encouraged to use the $addHandler method instead - kept only for backwards compatibility
        /// </summary>
        /// <param name="ctl" type="Object">
        /// Control
        /// </param>
        /// <param name="type" type="String">
        /// Event name to attach
        /// </param>
        /// <param name="fHandler" type="Function">
        /// Reference to the function that will react to event
        /// </param>
        /// <returns type="Boolean" />		
        if (ctl.addEventListener) {
            var name = type.substring(2);
            ctl.addEventListener(name, function (evt) { dnn.dom.event = new dnn.dom.eventObject(evt, evt.target); return fHandler(); }, false);
        }
        else
            ctl.attachEvent(type, function () { dnn.dom.event = new dnn.dom.eventObject(window.event, window.event.srcElement); return fHandler(); });
        return true;
    },

    cursorPos: function (ctl) {
        /// <summary>
        /// Obtains the current cursor position within a textbox
        /// </summary>
        /// <param name="ctl" type="Object">
        /// Control
        /// </param>
        /// <returns type="Number" />		

        // empty control means the cursor is at 0
        if (ctl.value.length == 0)
            return 0;

        // -1 for unknown
        var pos = -1;

        if (ctl.selectionStart)	// Moz - Opera
            pos = ctl.selectionStart;
        else if (ctl.createTextRange)// IE
        {
            var sel = window.document.selection.createRange();
            var range = ctl.createTextRange();

            // if the current selection is within the edit control			
            if (range == null || sel == null || ((sel.text != "") && range.inRange(sel) == false))
                return -1;

            if (sel.text == "") {
                if (range.boundingLeft == sel.boundingLeft)
                    pos = 0;
                else {
                    var tagName = ctl.tagName.toLowerCase();
                    // Handle inputs.
                    if (tagName == "input") {
                        var text = range.text;
                        var i = 1;
                        while (i < text.length) {
                            range.findText(text.substring(i));
                            if (range.boundingLeft == sel.boundingLeft)
                                break;

                            i++;
                        }
                    }
                        // Handle text areas.
                    else if (tagName == "textarea") {
                        var i = ctl.value.length + 1;
                        var oCaret = document.selection.createRange().duplicate();
                        while (oCaret.parentElement() == ctl && oCaret.move("character", 1) == 1)
                            --i;

                        if (i == ctl.value.length + 1)
                            i = -1;
                    }
                    pos = i;
                }
            }
            else
                pos = range.text.indexOf(sel.text);
        }
        return pos;
    },

    cancelCollapseElement: function (ctl) {
        /// <summary>
        /// Allows animation for the collapsing of an element to be canceled
        /// </summary>
        /// <param name="ctl" type="Object">
        /// Control
        /// </param>
        dnn.cancelDelay(ctl.id + 'col');
        ctl.style.display = 'none';
    },

    collapseElement: function (ctl, num, pCallBack) {
        /// <summary>
        /// Animates the collapsing of an element
        /// </summary>
        /// <param name="ctl" type="Object">
        /// Control
        /// </param>
        /// <param name="num" type="Number">
        /// Number of animations to perform the collapse. The more you specify, the longer it will take
        /// </param>
        /// <param name="pCallback" type="Function">
        /// Function to call when complete
        /// </param>
        if (num == null)
            num = 10;
        ctl.style.overflow = 'hidden';
        var ctx = new Object();
        ctx.num = num;
        ctx.ctl = ctl;
        ctx.pfunc = pCallBack;
        ctl.origHeight = ctl.offsetHeight;
        dnn.dom.__collapseElement(ctx);
    },

    __collapseElement: function (ctx) {
        var num = ctx.num;
        var ctl = ctx.ctl;

        var step = ctl.origHeight / num;
        if (ctl.offsetHeight - (step * 2) > 0) {
            ctl.style.height = (ctl.offsetHeight - step).toString() + 'px';
            dnn.doDelay(ctl.id + 'col', 10, dnn.dom.__collapseElement, ctx);
        }
        else {
            ctl.style.display = 'none';
            if (ctx.pfunc != null)
                ctx.pfunc();
        }
    },

    cancelExpandElement: function (ctl) {
        /// <summary>
        /// Allows animation for the expanding of an element to be canceled
        /// </summary>
        /// <param name="ctl" type="Object">
        /// Control
        /// </param>
        dnn.cancelDelay(ctl.id + 'exp');
        ctl.style.overflow = '';
        ctl.style.height = '';
    },

    disableTextSelect: function (ctl) {
        if (typeof ctl.onselectstart != "undefined") //ie
            ctl.onselectstart = function () { return false }
        else if (typeof ctl.style.MozUserSelect != "undefined") //ff
            ctl.style.MozUserSelect = "none"
        else //others
            ctl.onmousedown = function () { return false }
    },

    expandElement: function (ctl, num, pCallBack) {
        /// <summary>
        /// Animates the expanding of an element
        /// </summary>
        /// <param name="ctl" type="Object">
        /// Control
        /// </param>
        /// <param name="num" type="Number">
        /// Number of animations to perform the collapse. The more you specify, the longer it will take
        /// </param>
        /// <param name="pCallback" type="Function">
        /// Function to call when complete
        /// </param>
        if (num == null)
            num = 10;

        if (ctl.style.display == 'none' && ctl.origHeight == null) {
            ctl.style.display = '';
            ctl.style.overflow = '';
            ctl.origHeight = ctl.offsetHeight;
            ctl.style.overflow = 'hidden';
            ctl.style.height = '1px';
        }
        ctl.style.display = '';

        var ctx = new Object();
        ctx.num = num;
        ctx.ctl = ctl;
        ctx.pfunc = pCallBack;
        dnn.dom.__expandElement(ctx);
    },

    __expandElement: function (ctx) {
        var num = ctx.num;
        var ctl = ctx.ctl;
        var step = ctl.origHeight / num;
        if (ctl.offsetHeight + step < ctl.origHeight) {
            ctl.style.height = (ctl.offsetHeight + step).toString() + 'px';
            dnn.doDelay(ctl.id + 'exp', 10, dnn.dom.__expandElement, ctx);
        }
        else {
            ctl.style.overflow = '';
            ctl.style.height = '';
            if (ctx.pfunc != null)
                ctx.pfunc();
        }
    },

    deleteCookie: function (name, path, domain) {
        /// <summary>
        /// Deletes a cookie
        /// </summary>
        /// <param name="name" type="String">
        /// Name of the desired cookie to delete
        /// </param>
        /// <param name="path" type="String">
        /// Path for which the cookie is valid
        /// </param>
        /// <param name="domain" type="String">
        /// Domain for which the cookie is valid
        /// </param>
        /// <returns type="Boolean" />		
        if (this.getCookie(name)) {
            this.setCookie(name, '', -1, path, domain);
            return true;
        }
        return false;
    },

    getAttr: function (node, attr, def) {
        /// <summary>
        /// Utility funcion used to retrieve the attribute value of an object. Allows for a default value to be returned if null.
        /// </summary>
        /// <param name="node" type="Object">
        /// Object to obtain attribute from 
        /// </param>
        /// <param name="attr" type="String">
        /// Name of attribute to retrieve
        /// </param>
        /// <param name="def" type="String">
        /// Default value to retrieve if attribute is null or zero-length
        /// </param>
        /// <returns type="String" />		
        if (node.getAttribute == null)
            return def;
        var val = node.getAttribute(attr);

        if (val == null || val == '')
            return def;
        else
            return val;
    },

    //Atlas
    getById: function (id, ctl) {
        /// <summary>
        /// Retrieves element on page based off of passed in id. - use $get instead - backwards compat only
        /// </summary>
        /// <param name="id" type="String">
        /// Control's id to retrieve
        /// </param>
        /// <param name="ctl" type="Object" optional="true">
        /// If you wish to narrow down the search, pass in the control whose children you wish to search.
        /// </param>
        /// <returns type="Object" />		
        return $get(id, ctl);
    },

    getByTagName: function (tag, ctl) {
        /// <summary>
        /// Retrieves element on page based off of passed in id. - use $get instead - backwards compat only
        /// </summary>
        /// <param name="tag" type="String">
        /// TagName to retrieve
        /// </param>
        /// <param name="ctl" type="Object" optional="true">
        /// If you wish to narrow down the search, pass in the control whose children you wish to search.
        /// </param>
        /// <returns type="Array" />		
        if (ctl == null)
            ctl = document;
        if (ctl.getElementsByTagName)
            return ctl.getElementsByTagName(tag);
        else if (ctl.all && ctl.all.tags)
            return ctl.all.tags(tag);
        else
            return null;
    },

    getParentByTagName: function (ctl, tag) {
        /// <summary>
        /// Retrieves parent element of a particular tag. This function walks up the control's parent references until it locates control of particular tag or parent no longer exists.
        /// </summary>
        /// <param name="ctl" type="Object">
        /// Control you wish to start the lookup at
        /// </param>
        /// <param name="tag" type="String">
        /// TagName to of parent control retrieve
        /// </param>
        /// <returns type="Object" />		
        var parent = ctl.parentNode;
        tag = tag.toLowerCase();
        while (parent != null) {
            if (parent.tagName && parent.tagName.toLowerCase() == tag)
                return parent;
            parent = parent.parentNode;
        }
        return null;
    },

    getCookie: function (name) {
        /// <summary>
        /// Retrieves a cookie
        /// </summary>
        /// <param name="name" type="String">
        /// Name of the desired cookie 
        /// </param>
        /// <returns type="String" />		
        var cookie = " " + document.cookie;
        var search = " " + name + "=";
        var ret = null;
        var offset = 0;
        var end = 0;
        if (cookie.length > 0) {
            offset = cookie.indexOf(search);
            if (offset != -1) {
                offset += search.length;
                end = cookie.indexOf(";", offset)
                if (end == -1)
                    end = cookie.length;
                ret = unescape(cookie.substring(offset, end));
            }
        }
        return (ret);
    },

    getNonTextNode: function (node) {
        /// <summary>
        /// Retrieves first non-text node.  If passed in node is textnode, it looks at each sibling
        /// </summary>
        /// <param name="node" type="Object">
        /// Node to start looking at
        /// </param>
        /// <returns type="Object" />		
        if (this.isNonTextNode(node))
            return node;

        while (node != null && this.isNonTextNode(node)) {
            node = this.getSibling(node, 1);
        }
        return node;
    },

    addSafeHandler: function (ctl, evt, obj, method) {
        /// <summary>
        /// Creates memory safe event handler (closure) over element - use createDelegate instead with dispose - kept for backwards compatibility
        /// </summary>
        /// <param name="ctl" type="Object">
        /// Control to attach event on
        /// </param>
        /// <param name="evt" type="String">
        /// Event to attach
        /// </param>
        /// <param name="obj" type="Object">
        /// Instance of object to invoke method on
        /// </param>
        /// <param name="method" type="String">
        /// Method to invoke on object for event 
        /// </param>
        ctl[evt] = this.getObjMethRef(obj, method);

        if (dnn.dom.browser.isType(dnn.dom.browser.InternetExplorer))	//handle IE memory leaks with closures
        {
            if (this.__leakEvts.length == 0)
                dnn.dom.attachEvent(window, 'onunload', dnn.dom.destroyHandlers);

            this.__leakEvts[this.__leakEvts.length] = new dnn.dom.leakEvt(evt, ctl, ctl[evt]);
        }
    },

    destroyHandlers: function () {
        /// <summary>
        /// Automatically called (internal) - handles IE memory leaks with closures
        /// </summary>
        var iCount = dnn.dom.__leakEvts.length - 1;
        for (var i = iCount; i >= 0; i--) {
            var oEvt = dnn.dom.__leakEvts[i];
            oEvt.ctl.detachEvent(oEvt.name, oEvt.ptr);
            oEvt.ctl[oEvt.name] = null;
            dnn.dom.__leakEvts.length = dnn.dom.__leakEvts.length - 1;
        }
    },

    getObjMethRef: function (obj, methodName) {
        /// <summary>
        /// Creates event delegate (closure) - use createDelegate instead with dispose - kept for backwards compatibility
        /// adapted from http://jibbering.com/faq/faq_notes/closures.html (associateObjWithEvent)
        /// </summary>
        /// <param name="obj" type="Object">
        /// Instance of object to invoke method on
        /// </param>
        /// <param name="methodName" type="String">
        /// Method to invoke on object for event 
        /// </param>
        return (function (e) { e = e || window.event; return obj[methodName](e, this); });
    },

    getSibling: function (ctl, offset) {
        /// <summary>
        /// Starts at the passed in control and retrieves the sibling that is at the desired offset
        /// </summary>
        /// <param name="ctl" type="Object">
        /// Control in which to find the sibling related to it
        /// </param>
        /// <param name="offset" type="Number">
        /// How many positions removed from the passed in control to look for the sibling. For example if you wanted your immediate sibling below you would pass in 1
        /// </param>
        /// <returns type="Object" />		
        if (ctl != null && ctl.parentNode != null) {
            for (var i = 0; i < ctl.parentNode.childNodes.length; i++) {
                if (ctl.parentNode.childNodes[i].id == ctl.id) {
                    if (ctl.parentNode.childNodes[i + offset] != null)
                        return ctl.parentNode.childNodes[i + offset];
                }
            }
        }
        return null;
    },

    isNonTextNode: function (node) {
        /// <summary>
        /// Determines if passed in control is a text node (i.e. nodeType = 3 or 8)
        /// </summary>
        /// <param name="node" type="Object">
        /// Node object to verify
        /// </param>
        /// <returns type="Boolean" />		
        return (node.nodeType != 3 && node.nodeType != 8); //exclude nodeType of Text (Netscape/Mozilla) issue!
    },

    getScript: function (src) {
        if (this.scriptElements[src]) //perf
            return this.scriptElements[src];

        var oScripts = dnn.dom.getByTagName('SCRIPT'); //safari has document.scripts
        for (var s = 0; s < oScripts.length; s++) //safari
        {
            if (oScripts[s].src != null && oScripts[s].src.indexOf(src) > -1) {
                this.scriptElements[src] = oScripts[s]; //cache for perf
                return oScripts[s];
            }
        }
    },

    getScriptSrc: function (src) {
        var resx = dnn.getVar(src + '.resx', '');
        if (resx.length > 0)
            return resx;
        return src;
    },

    getScriptPath: function () {
        var oThisScript = dnn.dom.getScript('dnn.js');
        if (oThisScript) {
            var path = oThisScript.src;
            if (path.indexOf('?') > -1) {
                path = path.substr(0, path.indexOf('?'));
            }
            return path.replace('dnn.js', '');
        }
        var sSP = dnn.getVar('__sp');   //try and get from var
        if (sSP)
            return sSP;
        return '';

    },

    scriptFile: function (src)	//trims off path
    {
        var ary = src.split('/');
        return ary[ary.length - 1];
    },

    loadScript: function (src, text, callBack) {
        var sFile;
        if (src != null && src.length > 0) {
            sFile = this.scriptFile(src);
            if (this.scripts[sFile] != null)	//already loaded
                return;
        }
        var oSR = new dnn.ScriptRequest(src, text, callBack);
        if (sFile)
            this.scripts[sFile] = oSR;
        oSR.load();
        return oSR;
    },

    loadScripts: function (aSrc, aText, callBack) {
        if (dnn.scripts == null) {
            var oRef = function (aSrc, aText, callBack) //closure to invoke self with same params when done
            { return (function () { dnn.dom.loadScripts(aSrc, aText, callBack); }); };
            dnn.dom.loadScript(dnn.dom.getScriptPath() + 'dnn.scripts.js', null, oRef(aSrc, aText, callBack));
            //dnn.dom.loadScript(dnn.dom.getScriptPath() + 'dnn.scripts.js', null);
            return;
        }
        var oBatch = new dnn.scripts.ScriptBatchRequest(aSrc, aText, callBack);
        oBatch.load();
    },

    scriptStatus: function (src) {
        var sFile = this.scriptFile(src);
        if (this.scripts[sFile])
            return this.scripts[sFile].status; //dynamic load

        var oScript = this.getScript(src);
        if (oScript != null)	//not a dynamic load, must be complete if found
            return 'complete';
        else
            return '';
    },

    setScriptLoaded: function (src)	//called by pages js that is dynamically loaded.  Needed since Safari doesn't support onload for script elements
    {
        var sFile = this.scriptFile(src);
        if (this.scripts[sFile] && dnn.dom.scripts[sFile].status != 'complete')
            dnn.dom.scripts[sFile].complete();
    },

    navigate: function (sURL, sTarget) {
        if (sTarget != null && sTarget.length > 0) {
            if (sTarget == '_blank' || sTarget == '_new')	//todo: handle more
                window.open(sURL);
            else
                document.frames[sTarget].location.href = sURL;
        }
        else {
            if (Sys.Browser.agent === Sys.Browser.InternetExplorer)
                window.navigate(sURL);  //include referrer (WCT-8821)
            else
                window.location.href = sURL;
        }
        return false;
    },

    setCookie: function (name, val, days, path, domain, isSecure, milliseconds) {
        /// <summary>
        /// Sets a cookie
        /// </summary>
        /// <param name="name" type="String">
        /// Name of the desired cookie to delete
        /// </param>
        /// <param name="val" type="String">
        /// value
        /// </param>
        /// <param name="days" type="Number">
        /// days cookie is valid for
        /// </param>
        /// <param name="path" type="String">
        /// Path for which the cookie is valid
        /// </param>
        /// <param name="domain" type="String">
        /// Domain for which the cookie is valid
        /// </param>
        /// <param name="isSecure" type="Boolean">
        /// determines if cookie is secure
        /// </param>
        /// <returns type="Boolean" />		
        var sExpires;
        if (days) {
            sExpires = new Date();
            sExpires.setTime(sExpires.getTime() + (days * 24 * 60 * 60 * 1000));
        }

        if (milliseconds) {
            sExpires = new Date();
            sExpires.setTime(sExpires.getTime() + (milliseconds));

        }
        document.cookie = name + "=" + escape(val) + ((sExpires) ? "; expires=" + sExpires.toGMTString() : "") +
				((path) ? "; path=" + path : "") + ((domain) ? "; domain=" + domain : "") + ((isSecure) ? "; secure" : "");

        if (document.cookie.length > 0)
            return true;
    },

    //Atlas
    getCurrentStyle: function (node, prop) {
        var style = Sys.UI.DomElement._getCurrentStyle(node);
        if (style)
            return style[prop];
        return '';
    },

    getFormPostString: function (ctl) {
        var sRet = '';
        if (ctl != null) {
            if (ctl.tagName && ctl.tagName.toLowerCase() == 'form')	//if form, faster to loop elements collection
            {
                for (var i = 0; i < ctl.elements.length; i++)
                    sRet += this.getElementPostString(ctl.elements[i]);
            }
            else {
                sRet = this.getElementPostString(ctl);
                for (var i = 0; i < ctl.childNodes.length; i++)
                    sRet += this.getFormPostString(ctl.childNodes[i]); //1.3 fix (calling self recursive insead of elementpoststring)
            }
        }
        return sRet;
    },

    getElementPostString: function (ctl) {
        var tagName;
        if (ctl.tagName)
            tagName = ctl.tagName.toLowerCase();

        if (tagName == 'input') {
            var type = ctl.type.toLowerCase();
            if (type == 'text' || type == 'password' || type == 'hidden' || ((type == 'checkbox' || type == 'radio') && ctl.checked))
                return ctl.name + '=' + dnn.encode(ctl.value, false) + '&';
        }
        else if (tagName == 'select') {
            for (var i = 0; i < ctl.options.length; i++) {
                if (ctl.options[i].selected)
                    return ctl.name + '=' + dnn.encode(ctl.options[i].value, false) + '&';
            }
        }
        else if (tagName == 'textarea')
            return ctl.name + '=' + dnn.encode(ctl.value, false) + '&';
        return '';
    },

    //OBSOLETE METHODS
    //devreplace
    //this method is obsolete, call nodeElement.appendChild directly
    appendChild: function (oParent, oChild) {
        return oParent.appendChild(oChild);
    },

    //this method is obsolete, call nodeElement.parentNode.removeChild directly
    removeChild: function (oChild) {
        return oChild.parentNode.removeChild(oChild);
    },

    //devreplace
    //this method is obsolete, call document.createElement directly
    createElement: function (tagName) {
        return document.createElement(tagName.toLowerCase());
    }

});   //dnn.dom end

dnn.dom.leakEvt = function (name, ctl, oPtr) {
    this.name = name;
    this.ctl = ctl;
    this.ptr = oPtr;
}
dnn.dom.leakEvt.registerClass('dnn.dom.leakEvt');


dnn.dom.eventObject = function (e, srcElement) {
    this.object = e;
    this.srcElement = srcElement;
}
dnn.dom.eventObject.registerClass('dnn.dom.eventObject');

//--- dnn.dom.browser
//Kept as is, Atlas detects smaller number of browsers
dnn.dom.browserObject = function () {
    this.InternetExplorer = 'ie';
    this.Netscape = 'ns';
    this.Mozilla = 'mo';
    this.Opera = 'op';
    this.Safari = 'safari';
    this.Konqueror = 'kq';
    this.MacIE = 'macie';


    var type;
    var agt = navigator.userAgent.toLowerCase();

    if (agt.indexOf('konqueror') != -1)
        type = this.Konqueror;
    else if (agt.indexOf('msie') != -1 && agt.indexOf('mac') != -1)
        type = this.MacIE;
    else if (Sys.Browser.agent === Sys.Browser.InternetExplorer)
        type = this.InternetExplorer;
    else if (Sys.Browser.agent === Sys.Browser.FireFox)
        type = this.Mozilla; //this.FireFox;
    else if (Sys.Browser.agent === Sys.Browser.Safari)
        type = this.Safari;
    else if (Sys.Browser.agent === Sys.Browser.Opera)
        type = this.Opera;
    else
        type = this.Mozilla;

    this.type = type;
    this.version = Sys.Browser.version;
    var sAgent = navigator.userAgent.toLowerCase();
    if (this.type == this.InternetExplorer) {
        var temp = navigator.appVersion.split("MSIE");
        this.version = parseFloat(temp[1]);
    }
    if (this.type == this.Netscape) {
        var temp = sAgent.split("netscape");
        this.version = parseFloat(temp[1].split("/")[1]);
    }
}

dnn.dom.browserObject.prototype =
{
    toString: function () {
        return this.type + ' ' + this.version;
    },

    isType: function () {
        for (var i = 0; i < arguments.length; i++) {
            if (dnn.dom.browser.type == arguments[i])
                return true;
        }
        return false;
    }
}
dnn.dom.browserObject.registerClass('dnn.dom.browserObject');
dnn.dom.browser = new dnn.dom.browserObject();


//-- shorthand functions.  Only define if not already present
if (typeof ($) == 'undefined') {
    eval("function $() {var ary = new Array(); for (var i=0; i<arguments.length; i++) {var arg = arguments[i]; var ctl; if (typeof arg == 'string') ctl = dnn.dom.getById(arg); else ctl = arg; if (ctl != null && typeof(Element) != 'undefined' && typeof(Element.extend) != 'undefined') Element.extend(ctl); if (arguments.length == 1) return ctl; ary[ary.length] = ctl;} return ary;}");
}

//image flickering
try { document.execCommand("BackgroundImageCache", false, true); } catch (err) { }

Sys.Application.add_load(dnn._onload);