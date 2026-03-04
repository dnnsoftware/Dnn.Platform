/// <reference name="MicrosoftAjax.js" />
/// <reference name="dnn.controls.js" assembly="DotNetNuke.WebControls" />
/// <reference name="dnn.js" assembly="DotNetNuke.WebUtility" />
/// <reference name="dnn.dom.positioning.js" assembly="DotNetNuke.WebUtility" />
/// <reference name="dnn.xmlhttp.js" assembly="DotNetNuke.WebUtility" />

Type.registerNamespace('dnn.controls');

dnn.controls.DNNToolBar = function (ns, behaviorID)
{
	this.ctr = document.createElement('div');
	this.ctr.id = ns + '_tb';
	dnn.controls.DNNToolBar.initializeBase(this, [this.ctr]);	
	this.initialize(behaviorID);
	//this.ns = ns;
	
	this.css = null;
	this.cssButton = null;
	this.cssButtonHover = null;
	this.moutDelay = null;

	this.buttons = [];
	this.relatedCtl = null;
	this.ctr.style.position = 'relative';	//allow relative positioning without taking up space
	this.ctl = document.createElement('div');
	this.ctr.appendChild(this.ctl);
	this.ctl.style.display = 'none';
	this.moutDelay = 1000;
	this._hideDelegate = dnn.createDelegate(this, this.hide);
	this.actionHandler = null;
}

dnn.controls.DNNToolBar.prototype = 
{

loadDefinition: function(toolbarID, nsPrefix, relatedCtl, parent, insertBeforeCtl, actionHandler)
{
	var def = dnn.controls.toolbars[nsPrefix + ':' + toolbarID]; 
	if (def == null)	//try and load with xhtmlconformance off
        def = dnn.controls.toolbars[nsPrefix + '$' + toolbarID];
	if (def == null)	//try and load global toolbar
		def = dnn.controls.toolbars[toolbarID];

	if (def)
	{
		this.relatedCtl = relatedCtl;
		this.css = def.css;
		this.cssButton = def.cssb;
		this.cssButtonHover = def.cssbh;
		this.actionHandler = actionHandler;
		for (var i=0; i<def.btns.length; i++)
		{
			var btn = def.btns[i];
			this.addButton(btn.key, btn.ca, btn.css, btn.cssh, btn.img, btn.txt, btn.alt, btn.js, btn.url, true);
		}
			
		if (def.mod)
			this.moutDelay = def.mod;
		
		if (insertBeforeCtl)
		{
		    //mini-hack to work around issue with &nbsp; just before ctl which causes "bumping"
		    var sibling = dnn.dom.getSibling(insertBeforeCtl, -1);
		    if (sibling != null && sibling.nodeType == 3)
		        insertBeforeCtl = sibling;
			parent.insertBefore(this.ctr, insertBeforeCtl);
		}
		else
			parent.appendChild(this.ctr);	
	}

},

addButton: function (key, clickAct, css, hoverCss, img, text, toolTip, js, url, visible)
{
	if (key == null)
		key = this.buttons.length;
	if (this.cssButton)
		css = this.cssButton + ' ' + css;
	if (this.cssButtonHover)
		hoverCss = this.cssButtonHover + ' ' + hoverCss;
		
	this.buttons[key] = new dnn.controls.DNNToolBarButton(key, clickAct, css, hoverCss, img, text, toolTip, js, url, visible, this);
},

refresh: function()
{
	this.ctl.className = this.css;
	for (var key in this.buttons)
	{
		var btn = this.buttons[key];
		if (typeof btn == 'function')	//safeguard against other js frameworks
			continue;

		if (btn.ctl == null)
		{
			btn.render();
			this.ctl.appendChild(btn.ctl);
		}
		btn.ctl.style.display = (btn.visible ? '' : 'none');
	}	
},

show: function(hide)
{
	dnn.cancelDelay(this.ns + 'mout');
	if (this.ctl.style.display != '')
	{
		this.refresh();
		this.ctl.style.display = '';
	}
	if (hide)
		this.beginHide();
},

beginHide: function()
{
	if (this.moutDelay > 0)
		dnn.doDelay(this.ns + 'mout', this.moutDelay, this._hideDelegate);

}, 

hide: function()
{
	this.ctl.style.display = 'none';
},

buttonClick: function (evt) 
{
    var tb = this.tb;
    var arg = new dnn.controls.DNNToolBarEventArgs(this);
    tb.invoke_handler('click', arg);
    if (arg.get_cancel())
        return;
    this.click();
},

buttonMouseOver: function (evt) 
{
	this.tb.show(false);
	this.hover();
},

buttonMouseOut: function (evt) 
{
	this.tb.beginHide();
	this.unhover();
},

dispose: function()
{
    this._hideDelegate = null;
    this.actionHandler = null;
    dnn.controls.DNNToolBar.callBaseMethod(this, 'dispose');
}


}
dnn.controls.DNNToolBar.registerClass('dnn.controls.DNNToolBar', dnn.controls.control);


dnn.controls.DNNToolBarButton = function (key, clickAct, css, hoverCss, img, text, toolTip, js, url, visible, oToolbar)
{
	this.ctl = null;
	this.key = key;
	this.clickAction = clickAct;
	this.tb = oToolbar;
	this.css = css;
	this.hoverCss = hoverCss;
	this.img = img;
	this.tooltip = toolTip;
	this.txt = text;
	this.js = js;
	this.url = url;
	this.visible = visible;
}

dnn.controls.DNNToolBarButton.prototype = 
{

render: function ()
{
	if (!this.ctl)
	{
		//this.ctl = document.createElement('span');
		var bar = this.tb;
		this.ctl = bar.createChildControl('div', this.key, '');
		this.ctl.className = this.css;
		if (this.tooltip)
			this.ctl.title = this.tooltip;
		//this.ctl.key = this.key;
		if (this.img)
		{
			var img = document.createElement('img');
			img.src = this.img;
			this.ctl.appendChild(img);
		}
		if (this.txt)
		{
			var span = document.createElement('span');
			span.innerHTML = this.txt;
			this.ctl.appendChild(span);
		}
		
		
		/*
		dnn.dom.addSafeHandler(this.ctl, 'onmouseover', this, 'mouseOver');
		dnn.dom.addSafeHandler(this.ctl, 'onmouseout', this, 'mouseOut');
		if (dnn.dom.browser.isType(dnn.dom.browser.InternetExplorer))  //ie has issue if contenteditable looses focus to a span (img are ok).  this works for both
			dnn.dom.addSafeHandler(this.ctl, 'onmousedown', this, 'click');
		else
			dnn.dom.addSafeHandler(this.ctl, 'onclick', this, 'click');
	    */
	    var clickEvent = 'click';
	    if (dnn.dom.browser.isType(dnn.dom.browser.InternetExplorer))  //ie has issue if contenteditable looses focus to a span (img are ok).  this works for both
	        clickEvent = 'mousedown';
	    
	    bar.addHandlers(this.ctl, bar.getDynamicEventObject(clickEvent, bar.buttonClick), this);    
   	    bar.addHandlers(this.ctl, {'mouseover': bar.buttonMouseOver,
   	                               'mouseout': bar.buttonMouseOut}, this);
	}
},

hover: function()
{
    if (this.hoverCss)
	    this.ctl.className = this.css + ' ' + this.hoverCss;
},

unhover: function()
{
	if (this.hoverCss)
		this.ctl.className = this.css;
},

click: function () 
{
	if (this.clickAction == 'js')
		eval(this.js);
	else if (this.clickAction == 'navigate')
		dnn.dom.navigate(this.url);
	else if (this.tb.actionHandler != null)
		this.tb.actionHandler(this, this.tb.relatedCtl);

},
/*
mouseOver: function () 
{
	this.tb.show(false);
	if (this.hoverCss)
		this.ctl.className = this.css + ' ' + this.hoverCss;
},

mouseOut: function () 
{
	this.tb.beginHide();
	if (this.hoverCss)
		this.ctl.className = this.css;
},*/

getVal: function(sVal, sDef)
{
	return (sVal ? sVal : sDef);
}

}
dnn.controls.DNNToolBarButton.registerClass('dnn.controls.DNNToolBarButton');

