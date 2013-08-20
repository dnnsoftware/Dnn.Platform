/// <reference name="MicrosoftAjax.js" />
/// <reference name="dnn.controls.js" assembly="DotNetNuke.WebControls" />
/// <reference name="dnn.js" assembly="DotNetNuke.WebUtility" />
/// <reference name="dnn.dom.positioning.js" assembly="DotNetNuke.WebUtility" />
/// <reference name="dnn.xmlhttp.js" assembly="DotNetNuke.WebUtility" />

Type.registerNamespace('dnn.controls');

dnn.extend(dnn.controls, {
    initTree: function(ctl)
    {
        if (ctl)
        {
            var tree = new dnn.controls.DNNTree(ctl);
            tree.initialize();
            return tree;
        }
    }
});

//------- Constructor -------//
dnn.controls.DNNTree = function(o)
{
    dnn.controls.DNNTree.initializeBase(this, [o]);

    //--- Data Properties ---//  
    this.rootNode = null;
    this.nodes = [];
    this._loadNodes();
    this.hoverTreeNode = null;
    this.selTreeNode = null;

    //--- Appearance Properties ---//
    this.css = this.getProp('css', '');
    this.cssChild = this.getProp('csschild', '');
    this.cssHover = this.getProp('csshover', '');
    this.cssSel = this.getProp('csssel', '');
    this.cssIcon = this.getProp('cssicon', '');

    this.sysImgPath = this.getProp('sysimgpath', 'images/');
    this.imageList = this.getProp('imagelist', '').split(',');
    this.expandImg = this.getProp('expimg', '');
    this.workImg = this.getProp('workimg', 'dnnanim.gif');
    this.animf = new Number(this.getProp('animf', '5'));

    this.collapseImg = this.getProp('colimg', '');

    this.indentWidth = new Number(this.getProp('indentw', '10'));
    if (this.indentWidth == 0)
        this.indentWidth = 10;
    this.checkBoxes = this.getProp('checkboxes', '0') == '1';
    this.checkBoxMode = new Number(this.getProp('cbm', '0'));
    this.target = this.getProp('target', '');
    this.defaultJS = this.getProp('js', '');

    this.postBack = this.getProp('postback', '');
    this.callBack = this.getProp('callback', '');
    this.callBackStatFunc = this.getProp('callbackSF', '');
    if (this.callBackStatFunc.length > 0)
        this.add_handler('callBackStatus', eval(this.callBackStatFunc));

    //obtain width of expand image
    this.expImgWidth = new Number(this.getProp('expcolimgw', '12'));

    kbCtl = this.container;
    if (this.container.tabIndex <= 0)
        this.container.tabIndex = 0;
    else
    {
        kbCtl = document.createElement('input');
        kbCtl.type = 'text';
        kbCtl.style.width = 0;
        kbCtl.style.height = 0;
        kbCtl.style.background = 'transparent';
        kbCtl.style.border = 0;
        kbCtl.style.positioning = 'absolute';
        kbCtl.style.left = '-999em';
        this.container.parentNode.appendChild(kbCtl);
    }
    this.addHandlers(kbCtl, { "keydown": this.keydownHandler,
        "focus": this.focusHandler
    }, this);

    this._onsubmitDelegate = Function.createDelegate(this, this._onsubmit);
    dnn.controls.submitComp.add_handler(this._onsubmitDelegate);

}

dnn.controls.DNNTree.prototype =
{

	initialize: function () {
		dnn.controls.DNNTree.callBaseMethod(this, 'initialize');
		this.generateTreeHTML();
	},

	focusHandler: function (e) {
		var tNode = this.hoverTreeNode;
		if (tNode == null)
			tNode = new dnn.controls.DNNTreeNode(this.rootNode.childNodes(0));
		this.hoverNode(tNode);
		this.container.onfocus = null;
	},

	keydownHandler: function (e) {
		var dir = 0;
		var axis = '';

		if (e.keyCode == KEY_UP_ARROW) {
			dir = -1;
			axis = 'y';
		}
		if (e.keyCode == KEY_DOWN_ARROW) {
			dir = 1;
			axis = 'y';
		}
		if (e.keyCode == KEY_LEFT_ARROW) {
			dir = -1;
			axis = 'x';
		}
		if (e.keyCode == KEY_RIGHT_ARROW) {
			dir = 1;
			axis = 'x';
		}

		if (dir != 0) {
			var tNode = this.hoverTreeNode;
			var node;
			if (tNode == null)
				tNode = new dnn.controls.DNNTreeNode(this.rootNode.childNodes(0));

			if (axis == 'x') {
				if (dir == -1) {
					if (tNode.hasNodes && tNode.expanded)
						this.collapseNode(tNode);
					else
						node = tNode.node.parentNode();
				}

				if (dir == 1) {
					if (tNode.hasNodes || tNode.hasPendingNodes) {
						if (tNode.expanded != true)
							this.expandNode(tNode);
						else
							node = tNode.node.childNodes(0);
					}
				}
			}
			else if (axis == 'y') {
				var iNodeIndex = tNode.node.getNodeIndex('id');
				var parentNode = tNode.node.parentNode();
				if (tNode.hasNodes && tNode.expanded && dir > 0)	//if has expanded nodes and going down, select first child
					node = tNode.node.childNodes(0);
				else if (iNodeIndex + dir < 0)	//if first node was selected and going up, select parent
					node = parentNode;
				else if (iNodeIndex + dir < parentNode.childNodeCount())	//if navigated index less than number of nodes contained in parent
				{
					node = parentNode.childNodes(iNodeIndex + dir); //navigate there
					if (dir == -1)		//if going up... look for expanded sibling above (recursively)
					{
						var tNode2 = new dnn.controls.DNNTreeNode(node);
						while (tNode2.expanded)	//determine if parent node is expanded, if so find its last child node
						{
							if (tNode2.node.childNodeCount() == 0)
								break;
							node = tNode2.node.childNodes(tNode2.node.childNodeCount() - 1); //select last node in parent's collection
							tNode2 = new dnn.controls.DNNTreeNode(node); //needed to check expanded property
						}
					}
				}
				else if (parentNode.nodeName() != 'root')	//logic for last node in collection
				{
					var iNodeIndex = parentNode.getNodeIndex('id');
					var tempParent = parentNode;
					if (dir == 1)	//if going down... verify that parent node has sibling available to select, if not recursively look for one
					{
						while (tempParent.nodeName() != 'root' && iNodeIndex + dir >= tempParent.parentNode().childNodeCount())	//while index greater than node count
						{
							tempParent = tempParent.parentNode();
							iNodeIndex = tempParent.getNodeIndex('id');
						}
					}
					if (tempParent.nodeName() != 'root')
						node = tempParent.parentNode().childNodes(iNodeIndex + 1);
				}
			}
			if (node != null && node.nodeName() != 'root')
				this.hoverNode(new dnn.controls.DNNTreeNode(node));

			return false;
		}

		if (e.keyCode == KEY_RETURN && this.hoverTreeNode != null) {
			this.selectNode(this.hoverTreeNode);
			return false;
		}

	},

	hoverNode: function (tNode) {
		if (this.hoverTreeNode != null) {
			this.hoverTreeNode.hover = false;
			this.assignCss(this.hoverTreeNode);
		}
		tNode.hover = true;
		this.assignCss(tNode);
		this.hoverTreeNode = tNode;
	},

	getXml: function () {
		return this.rootNode.getXml();
	},

	expandNode: function (tNode) {
		var ctr = this.getChildControl(tNode.id, 'pctr');
		var expandCol = this.getChildControl(tNode.id, 'expcol');

		expandCol.src = this.expandImg;
		tNode.expanded = true;
		tNode.update();
		this.update();

		if (tNode.hasPendingNodes) {
			var sXml = tNode.node.getXml();
			tNode.tree = this; //need to give reference back to self

			if (this.workImg != null) {
				var icon = this.getChildControl(tNode.id, 'icn');
				if (icon)
					icon.src = this.sysImgPath + this.workImg;
			}
			if (this.callBack.indexOf('[NODEXML]') > -1)
				eval(this.callBack.replace('[NODEXML]', dnn.escapeForEval(sXml)));
			else
				eval(this.callBack.replace('[NODEID]', tNode.id));

			tNode.hasPendingNodes = false;
			tNode.hasNodes = true;
			this.hoverTreeNode = tNode;
		}
		else {
			dnn.dom.expandElement(ctr, this.animf);
		}

		return true;
	},

	collapseNode: function (tNode) {
		var ctr = this.getChildControl(tNode.id, 'pctr');
		var expandCol = this.getChildControl(tNode.id, 'expcol');
		//ctr.style.display = 'none';
		dnn.dom.collapseElement(ctr, this.animf);
		expandCol.src = this.collapseImg;
		tNode.expanded = null;
		tNode.update();
		this.update();
		return true;
	},

	selectNode: function (tNode) {
		var arg = new dnn.controls.DNNNodeEventArgs(tNode);
		this.invoke_handler('click', arg);
		if (arg.get_cancel())
			return;

		if (this.selTreeNode != null && this.checkBoxes == false) {
			this.selTreeNode.selected = null;
			this.assignCss(this.selTreeNode);
			this.selTreeNode.update('selected');
		}

		if (tNode.selected) {
			tNode.selected = null;
			this.assignCss(tNode);
		}
		else {
			tNode.selected = true;
			this.hoverTreeNode = tNode;
			this.assignCss(tNode);
		}
		tNode.update('selected');

		this.selTreeNode = tNode;
		this.update();

		var chk = this.getChildControl(tNode.id, 'chk');
		if (chk != null)
			chk.checked = tNode.selected;

		if (tNode.selected) {
			var js = '';
			if (this.defaultJS.length > 0)
				js = this.defaultJS;
			if (tNode.js.length > 0)
				js = tNode.js;

			if (js.length > 0) {
				if (eval(js) == false)
					return; //don't do postback if returns false
			}

			if (tNode.clickAction == null || tNode.clickAction == dnn.controls.action.postback)
				eval(this.postBack.replace('[NODEID]', tNode.id));
			else if (tNode.clickAction == dnn.controls.action.nav)
				dnn.dom.navigate(tNode.url, tNode.target.length > 0 ? tNode.target : this.target);
			else if (tNode.clickAction == dnn.controls.action.expand) {
				if (tNode.hasNodes || tNode.hasPendingNodes) {
					if (tNode.expanded)
						this.collapseNode(tNode);
					else
						this.expandNode(tNode);
				}
			}
		}

		return true;
	},

	selectAllChildren: function (tNode) {
		var childTNode;
		for (var i = 0; i < tNode.childNodeCount(); i++) {
			childTNode = tNode.childNodes(i);
			if (childTNode.selected != tNode.selected)
				this.selectNode(childTNode);

			this.selectAllChildren(childTNode);
		}
	},

	assignCss: function (tNode) {
		var oText = this.getChildControl(tNode.id, 't'); //, this.container);
		var sNodeCss = this.css;

		if (tNode.level > 0 && this.cssChild.length > 0)
			sNodeCss = this.cssChild;

		if (tNode.css.length > 0)
			sNodeCss = tNode.css;

		//tNode.hoverCss;

		if (tNode.hover)
			sNodeCss += ' ' + (tNode.cssHover.length > 0 ? tNode.cssHover : this.cssHover);
		if (tNode.selected)
			sNodeCss += ' ' + (tNode.cssSel.length > 0 ? tNode.cssSel : this.cssSel);

		oText.className = sNodeCss;
	},

	update: function (force) {
		if (force) {
			if (this.selTreeNode)
				dnn.setVar(this.ns + ':selected', this.selTreeNode.id); 	//BACKWARDS COMPAT ONLY!!!  
			dnn.setVar(this.ns + '_json', dnn.decodeHTML(this.rootNode.getJSON()));
		}
		return true;
	},

	//--- Event Handlers ---//
	_onsubmit: function () {
		this.update(true);
	},

	callBackStatus: function (result, ctx, req) {
		var tNode = ctx;
		var tree = tNode.tree;
		tree.invoke_compatHandler('callBackStatus', result, ctx, req);
	},

	callBackSuccess: function (result, ctx, req) {
		var tNode = ctx;
		var tree = tNode.tree;
		var parent = tNode.node; ;

		var json = dnn.evalJSON("{" + result + "}");
		parent.nodes = json.nodes;
		parent.setupJSONNodes(parent.rootNode(), parent, parent.nodes);

		if (tree.workImg != null) {
			var icon = tree.getChildControl(tNode.id, 'icn');
			if (tNode.image != '')
				icon.src = tNode.image;
			else if (tNode.imageIndex > -1)
				icon.src = tree.imageList[tNode.imageIndex];
		}

		//ctx.tree.generateTreeHTML();	
		//get container
		var ctr = tree.getChildControl(tNode.id, 'pctr');
		tree.renderNode(tNode.node, ctr, true);

		tree.update();

		//var ctr = tree.getChildControl(tNode.id, 'pctr');
		tree.expandNode(new dnn.controls.DNNTreeNode(tNode.node));

		tree.invoke_compatHandler('callBackStatus', result, ctx, req);
		tree.invoke_handler('callBackSuccess', new dnn.controls.DNNCallbackEventArgs(result, ctx, req));
	},

	callBackFail: function (result, ctx, req) {
		var tNode = ctx;
		var tree = tNode.tree;
		tree.invoke_handler('callBackFail', new dnn.controls.DNNCallbackEventArgs(result, ctx, req));
	},

	nodeExpColClick: function (evt) {
		var node = this._findEventNode(evt);
		if (node != null) {
			var tNode = new dnn.controls.DNNTreeNode(node);

			var ctr = this.getChildControl(tNode.id, 'pctr');
			if (tNode.expanded)
				this.collapseNode(tNode);
			else
				this.expandNode(tNode);
		}
	},

	nodeCheck: function (evt) {
		var node = this._findEventNode(evt);
		if (node != null) {
			var tNode = new dnn.controls.DNNTreeNode(node);
			if (this.checkBoxMode == 2 && this.selTreeNode != null)
				this.selectNode(this.selTreeNode);   //unselect node

			this.selectNode(tNode);

			if (this.checkBoxMode == 1)
				this.selectAllChildren(tNode);

		}
	},

	nodeTextClick: function (evt) {
		var node = this._findEventNode(evt);
		if (node != null) {
			this.selectNode(new dnn.controls.DNNTreeNode(node));
		}
	},

	nodeTextMOver: function (evt) {
		var node = this._findEventNode(evt);
		if (node != null)
			this.hoverNode(new dnn.controls.DNNTreeNode(node));
	},

	nodeTextMOut: function (evt) {
		var node = this._findEventNode(evt);
		if (node != null)
			this.assignCss(new dnn.controls.DNNTreeNode(node));
	},

	dispose: function () {
		this._onsubmitDelegate = null;
		dnn.controls.DNNTree.callBaseMethod(this, 'dispose');
	},

	//--- Generates tree HTML through passed in XML DOM ---//
	generateTreeHTML: function () {
		//this.debugWrite('generateTreeHTML', 'generateTreeHTML');
		//this.rootNode = this.DOM.rootNode();
		this.renderNode(null, this.container);
		//this.debugWrite('generateTreeHTML', 'generateTreeHTML [END]');	
	},

	renderNode: function (node, oCont, bExists) {
		var oChildCont = oCont;
		var tNode;

		if (bExists != true) {
			if (node != null) {
				//render node
				tNode = new dnn.controls.DNNTreeNode(node);
				var oNewContainer;
				oNewContainer = this.createChildControl('DIV', tNode.id, 'ctr'); //container for Node
				if (document.getElementById(oNewContainer.id) == null) {
					oNewContainer.appendChild(this.renderSpacer((this.indentWidth * tNode.level) + ((tNode.hasNodes || tNode.hasPendingNodes) ? 0 : this.expImgWidth))); //indent node
					if (tNode.hasNodes || tNode.hasPendingNodes)	//if node has children then render expand/collapse icon
						oNewContainer.appendChild(this.renderExpCol(tNode));

					if (this.checkBoxes)
						oNewContainer.appendChild(this.renderCheckbox(tNode));

					var oIconCont = this.renderIconCont(tNode);
					oNewContainer.appendChild(oIconCont);
					if (tNode.imageIndex > -1 || tNode.image != '')	//if node has image 
					{
						oIconCont.appendChild(this.renderIcon(tNode));
						//oNewContainer.appendChild(this.renderSpacer(10));
					}
					//else
					//    oIconCont.appendChild(this.renderSpacer(this.indentWidth));

					oNewContainer.appendChild(this.renderText(tNode)); //render text

					oCont.appendChild(oNewContainer);
					this.assignCss(tNode);
				}
			}
			else
				node = this.rootNode;

			if (tNode != null && (tNode.hasNodes || tNode.hasPendingNodes))	//if node has children render container and hide if necessary
			{
				oChildCont = this.createChildControl('DIV', tNode.id, 'pctr'); //Not using SPAN due to FireFox bug...
				if (tNode.expanded != true)
					oChildCont.style.display = 'none';
				oCont.appendChild(oChildCont);
			}
		}

		for (var i = 0; i < node.childNodeCount(); i++)	//recursively call child rendering
			this.renderNode(node.childNodes(i), oChildCont);


	},

	renderExpCol: function (tNode) {
		var img = this.createChildControl('IMG', tNode.id, 'expcol');
		if ((tNode.hasNodes || tNode.hasPendingNodes) && this.expandImg.length) {
			if (tNode.expanded)
				img.src = this.expandImg;
			else
				img.src = this.collapseImg;
		}
		//img.style.width = this.expImgWidth;	
		//img.style.cursor = 'hand';	//ie
		img.style.cursor = 'pointer';
		//dnn.dom.attachEvent(img, 'onclick', this.nodeExpColClick);
		//dnn.dom.attachEvent(img, 'onclick', dnn.dom.getObjMethRef(this, 'nodeExpColClick'));
		//img.onclick = dnn.dom.getObjMethRef(this, 'nodeExpColClick');
		this.addHandlers(img, { "click": this.nodeExpColClick }, this);

		return img;
	},

	renderIconCont: function (tNode) {
		var span = this.createChildControl('SPAN', tNode.id, 'icnc');
		if (tNode.cssIcon.length > 0)
			span.className = tNode.cssIcon;
		else if (this.cssIcon.length > 0)
			span.className = this.cssIcon;

		return span;
	},

	renderIcon: function (tNode) {
		var img = this.createChildControl('IMG', tNode.id, 'icn');
		if (tNode.image != '')
			img.src = tNode.image;
		else if (tNode.imageIndex > -1)
			img.src = this.imageList[tNode.imageIndex];
		//img.style.paddingRight = 10;	//doesn't work in IE???
		return img;
	},

	renderCheckbox: function (tNode) {
		var chk = this.createChildControl('INPUT', tNode.id, 'chk');
		chk.type = 'checkbox';
		chk.defaultChecked = tNode.selected;
		chk.checked = tNode.selected;

		this.addHandlers(chk, { "click": this.nodeCheck }, this);
		return chk;
	},

	renderSpacer: function (width) {
		var img = document.createElement('IMG');
		img.src = this.sysImgPath + 'spacer.gif';
		img.width = width;
		img.height = 1;
		img.style.width = width + 'px';
		img.style.height = '1px';
		return img;
	},

	renderText: function (tNode) {
		var span = this.createChildControl('SPAN', tNode.id, 't');
		span.innerHTML = tNode.text;
		span.style.cursor = 'pointer';

		if (tNode.toolTip.length > 0)
			span.title = tNode.toolTip;

		if (tNode.enabled || tNode.clickAction == dnn.controls.action.expand) {
			//span.onclick = dnn.dom.getObjMethRef(this, 'nodeTextClick');
			if (this.checkBoxes)
				this.addHandlers(span, { "click": this.nodeCheck }, this);
			else
				this.addHandlers(span, { "click": this.nodeTextClick }, this);

			if (this.cssHover.length > 0)	//only do this if necessary
			{
				//span.onmouseover = dnn.dom.getObjMethRef(this, 'nodeTextMOver');
				//span.onmouseout = dnn.dom.getObjMethRef(this, 'nodeTextMOut');
				this.addHandlers(span, { "mouseover": this.nodeTextMOver,
					"mouseout": this.nodeTextMOut
				}, this);
			}
		}

		if (tNode.selected) {
			this.selTreeNode = tNode;
			this.hoverTreeNode = tNode;
		}

		return span;
	},

	_findEventNode: function (evt) {
		return this.rootNode.findNode(this.getChildControlBaseId(evt.target));
	},

	_loadNodes: function () {
		var json = dnn.evalJSON(dnn.getVar(this.ns + '_json'));
		if (json) {
			this.nodes = json.nodes;
			this.rootNode = {};
			this.rootNode.nodes = this.nodes;
			this.rootNode.id = this.ns;
			this.rootNode = new dnn.controls.JSONNode(this.rootNode, 'root', 0);
		}
	}


	/*get_DebugMode: function() {return this._debugMode;},
	set_DebugMode: function(value) 
	{
	this._debugMode = value;
	if (this._debugMode && this._debugControl == null)
	{
	this._debugControl = document.createElement('textarea');
	this._debugControl.rows = 20;
	this._debugControl.cols = 150;
	document.forms[0].appendChild(this._debugControl);
	}
	else if (this._debugControl)
	{
	this._debugControl.style.display = 'none';
	}
	},

	_debugWrite: function()
	{
	this._debugControl.value += this._debugBuffer;
	this._debugBuffer = '';
	},

	debugWrite: function(category, text)
	{
	if (this._debugMode)
	{
	if (this._debugControl == null)
	{
	this.set_DebugMode(this._debugMode);
	}
    
	var s = text;
	var d = new Date();
	if (this._debugTimes[category] && text.indexOf('[END]') > -1)
	{
	s += ' (' + ((d - this._debugTimes[category])/ 1000) + ')'; 
	}
	this._debugTimes[category] = d;
	this._debugBuffer += s + '\n';
	if (this._prevDebugTimeout)
	{
	window.clearTimeout(this._prevDebugTimeout);
	}
	this._prevDebugTimeout = window.setTimeout(Function.createDelegate(this, this._debugWrite), 1000);
	}
	}*/

} //END DNNTree
    dnn.controls.DNNTree.registerClass('dnn.controls.DNNTree', dnn.controls.control);

    dnn.controls.DNNTreeNode = function(node)
    {
        dnn.controls.DNNTreeNode.initializeBase(this, [node]);

        //tree specific attributes
        this.hover = false;
        this.expanded = node.getAttribute('expanded', '0') == '1' ? true : null;
        this.selected = node.getAttribute('selected', '0') == '1' ? true : null;
        this.clickAction = node.getAttribute('ca', dnn.controls.action.postback);
        this.imageIndex = new Number(node.getAttribute('imgIdx', '-1')); //defaulting to 0 for backwards compat!

        //this.checkBox = node.getAttribute('checkBox', '0');	//IS THIS NECESSARY?
    }

    //DNNTreeNode specific methods
    dnn.controls.DNNTreeNode.prototype =
{
    childNodes: function(iIndex)
    {
        if (this.node.childNodes(iIndex) != null)
            return new dnn.controls.DNNTreeNode(this.node.childNodes(iIndex));
    }
}
    dnn.controls.DNNTreeNode.registerClass('dnn.controls.DNNTreeNode', dnn.controls.DNNNode);


    //BACKWARDS COMPAT ONLY!
    var DT_ACTION_POSTBACK = 0;
    var DT_ACTION_EXPAND = 1;
    var DT_ACTION_NONE = 2;
    var DT_ACTION_NAV = 3;

    function __dt_DNNTreeNode(ctl)
    {
        var node = dnn.controls.controls[ctl.ns].rootNode.findNode(ctl.nodeid);
        if (node != null)
        {
            var tNode = new dnn.controls.DNNTreeNode(node);

            this.ctl = ctl;
            this.id = ctl.id;
            this.key = tNode.key;
            this.nodeID = ctl.nodeid; //trim off t
            this.text = tNode.text;
            this.serverName = ctl.name;
        }
    }


