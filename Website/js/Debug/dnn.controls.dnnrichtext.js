/// <reference name="MicrosoftAjax.js" />
/// <reference name="dnn.controls.js" assembly="DotNetNuke.WebControls" />
/// <reference name="dnn.controls.dnnlabeledit.js" assembly="DotNetNuke.WebControls" />

//DNNRichText is a dynamically loaded script used by the DNNLabelEdit control
dnn.controls.DNNRichText = function (initFunc)
{
	this.supportsCE = (document.body.contentEditable != null);
	this.text = '';
	this.supportsMultiLine = true;
	this.document = null;
	this.control = null;
	this.initialized = false;
	this.isRichText = true;
	this.loaded = false;

	if (this.supportsCE)
	{
		this.document = document;
		this.container = document.createElement('div');
		this.container.contentEditable = true;	//ie doesn't need no stinkin' iframe
		this.control = this.container;
		this.initialized = true;
	}
	else
	{
		this.container = document.createElement('iframe');
		this.container.src = '';
		this.container.style.border = '0';
		this.initFunc = initFunc;	//pointer to function to call when iframe completely loads
		this._initDelegate = Function.createDelegate(this, this.initDocument);		
		dnn.doDelay(this.container.id + 'initEdit', 10, this._initDelegate);	//onreadystate and onload not completely reliable
	}
}

dnn.controls.DNNRichText.prototype = 
{
focus: function()
{
	if (this.supportsCE)
		this.control.focus();
	else
		this.container.contentWindow.focus();
},

execCommand: function(cmd, ui, vValue)
{
	this.document.execCommand(cmd, ui, vValue);	
},

getText: function()
{
		return this.control.innerHTML;
},

setText: function (s)
{
	if (this.initialized)
		this.control.innerHTML = s;		
	else
		this.text = s;
},

//method continually called until iframe is completely loaded
initDocument: function ()
{
	if (this.container.contentDocument != null)
	{
		if (this.document == null)	//iframe loaded, now write some HTML, thus causing it to not be loaded again
		{
			this.container.contentDocument.designMode = 'on';
			this.document = this.container.contentWindow.document;
			this.document.open();
			dnn.dom.addSafeHandler(this.container, 'onload', this, 'initDocument');
			this.document.write('<HEAD>' + this._getCSSLinks() + '</HEAD><BODY id="__dnn_body"></BODY>');
			this.document.close();
		}
		else if (this.control == null && this.document.getElementById('__dnn_body') != null)	//iframe loaded, now check if body is loaded
		{
			this.control = this.document.getElementById('__dnn_body');
			this.control.style.margin = 0;			
			this.control.tabIndex = 0;
			this.initialized = true;
			this.setText(this.text);
			this.initFunc();		
		}
	}
	if (this.initialized == false)	//iframe and body not loaded, call ourselves until it is
	    dnn.doDelay(this.container.id + 'initEdit', 10, this._initDelegate);
},

_getCSSLinks: function()	//probably a better way to handle this...
{
	var arr = dnn.dom.getByTagName('link');
	var s = '';
	for (var i=0; i< arr.length; i++)
	{
		s+= '<LINK href="' + arr[i].href + '" type=text/css rel=stylesheet>';
	}
	return s;
}

}

dnn.controls.DNNRichText.registerClass('dnn.controls.DNNRichText');

