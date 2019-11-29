/// <reference name="MicrosoftAjax.js" />
/// <reference name="dnn.js" assembly="DotNetNuke.WebUtility" />
/// <reference name="dnn.xmlhttp.js" assembly="DotNetNuke.WebUtility" />

dnn.xmlhttp.parserName = 'JS';  //force JS version

dnn.xmlhttp.JsXmlHttpRequest = function ()
{
	dnn.xmlhttp.contextId += 1;
	this.contextId = dnn.xmlhttp.contextId;
	this.method = null;
	this.url = null;
	this.async = true;
	this.doc = null;
	
	this.iframe = document.createElement('IFRAME');
	this.iframe.name = 'dnniframe' + this.contextId;
	this.iframe.id = 'dnniframe' + this.contextId;
	this.iframe.src = '';	//TODO:  FIX FOR SSL!!!
	this.iframe.height = 0;
	this.iframe.width = 0;
	this.iframe.style.visibility = 'hidden';
	document.body.appendChild(this.iframe);	
}

dnn.xmlhttp.JsXmlHttpRequest.prototype = 
{
    open: function(sMethod, sURL, bAsync)
    {
	    this.method = sMethod;
	    this.url = sURL;
	    this.async = bAsync;
    },

    send: function(postData)
    {
	    this.assignIFrameDoc();

	    if (this.doc == null)	//opera does not allow access to iframe right away
	    {
		    window.setTimeout(dnn.dom.getObjMethRef(this, 'send'), 1000);
		    return;
	    }
	    this.doc.open();
	    this.doc.write('<html><body>');
	    this.doc.write('<form name="TheForm" method="post" target="" ');
	    var sSep = '?';
	    if (this.url.indexOf('?') > -1)
		    sSep = '&';
    		
	    this.doc.write(' action="' + this.url + sSep + '__U=' + this.getUnique() + '">');
	    this.doc.write('<input type="hidden" name="ctx" value="' + this.contextId + '">');
	    if (postData && postData.length > 0)
	    {
		    var aryData = postData.split('&');
		    for (var i=0; i<aryData.length; i++)
			    this.doc.write('<input type="hidden" name="' + aryData[i].split('=')[0] + '" value="' + aryData[i].split('=')[1] + '">');
	    }
	    this.doc.write('</form></body></html>');
	    this.doc.close();

	    this.assignIFrameDoc();	//opera needs this reassigned after we wrote to it
    	
	    this.doc.forms[0].submit();
    	
    },

    assignIFrameDoc: function()
    {
	    if (this.iframe.contentDocument) 
		    this.doc = this.iframe.contentDocument; 
	    else if (this.iframe.contentWindow) 
		    this.doc = this.iframe.contentWindow.document; 
	    else if (window.frames[this.iframe.name]) 
		    this.doc = window.frames[this.iframe.name].document; 
    },

    getResponseHeader: function(sKey)
    {
	    this.assignIFrameDoc();
	    var oCtl = dnn.dom.getById(sKey, this.doc);
	    if (oCtl != null)
		    return oCtl.value;
	    else
		    return 'WARNING:  response header not found';
    },

    getUnique: function ()
    {
	    return new Date().getTime();
    }
}

dnn.xmlhttp.JsXmlHttpRequest.registerClass('dnn.xmlhttp.JsXmlHttpRequest');