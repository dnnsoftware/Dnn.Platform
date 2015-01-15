/// <reference name="MicrosoftAjax.js" />
/// <reference name="dnn.js" assembly="DotNetNuke.WebUtility" />

//dnn.xml Namespace 
Type.registerNamespace('dnn.xml');

dnn.extend(dnn.xml, {
	pns: 'dnn',
	ns: 'xml',
	parserName: null,

    get_parserName: function()
    {
        if (this.parserName == null)
            this.parserName = this._getParser();
        return this.parserName;
    },

    createDocument: function()
    {
        if (dnn.xml.get_parserName() == 'MSXML')
        {
	        var o = new ActiveXObject('MSXML.DOMDocument');
	        o.async = false;
	        return new dnn.xml.documentObject(o); 
	    }
	    else if (dnn.xml.get_parserName() == 'DOMParser')
	    {
	        return new dnn.xml.documentObject(document.implementation.createDocument("", "", null)); 
	    }
	    else
	        return new dnn.xml.documentObject(new dnn.xml.JsDocument()); 
	},
	
    init: function()
    {
        if (this.get_parserName() == 'DOMParser')
	    {		        
		    function __dnn_getNodeXml() 
		    {
			    var oXmlSerializer = new XMLSerializer;  //create a new XMLSerializer    			    
			    var sXml = oXmlSerializer.serializeToString(this); //get the XML string
			    return sXml; //return the XML string
		    }   //todo: move to inline function
		    Node.prototype.__defineGetter__("xml", __dnn_getNodeXml);
	    }
    },
    
    _getParser: function()
    {
	    if (dnn.dom.browser.isType(dnn.dom.browser.InternetExplorer))
		    return 'MSXML';
	    else if (dnn.dom.browser.isType(dnn.dom.browser.Netscape,dnn.dom.browser.Mozilla))
		    return 'DOMParser';
	    else
		    return 'JS';
    }
    
});

//dnn.xml.documentObject Object
dnn.xml.documentObject = function(oDoc)
{
    this._doc = oDoc;
}

dnn.xml.documentObject.prototype = 
{
    getXml: function()
    {
        //todo: use switch here
        if (dnn.xml.get_parserName() == 'MSXML')
            return this._doc.xml;
	    else if (dnn.xml.get_parserName() == 'DOMParser')
	        return this._doc.xml;
	    else
	        return this._doc.getXml();	//wish other browsers supported getters/setters	
    
    },

    loadXml: function(sXml)
    {
        if (dnn.xml.get_parserName() == 'MSXML')
            return this._doc.loadXML(sXml);
	    else if (dnn.xml.get_parserName() == 'DOMParser')
	    {
	        // parse the string to a new doc
	        var oDoc = (new DOMParser()).parseFromString(sXml, "text/xml");
			    
	        // remove all initial children
	        while (this._doc.hasChildNodes())
		        this._doc.removeChild(this._doc.lastChild);

	        // insert and import nodes
	        for (var i = 0; i < oDoc.childNodes.length; i++) 
		        this._doc.appendChild(this._doc.importNode(oDoc.childNodes[i], true));
	    }
	    else
	        return this._doc.loadXml(sXml);
    },

    childNodes: function(iIndex)
    {
        if (this._doc.childNodes[iIndex] != null)
	        return new dnn.xml.XmlNode(this._doc.childNodes[iIndex]);
    },

    findNode: function(sNodeName, sAttr, sValue)
    {
        return this.childNodes(0).findNode(sNodeName, sAttr, sValue);
    },

    childNodeCount: function()
    {
        return this._doc.childNodes.length;
    },

    rootNode: function()
    {
        var oNode;
        for (var i=0; i<this.childNodeCount(); i++)
        {
	        if (this.childNodes(i).nodeType != 7)
	        {
		        oNode = this.childNodes(i);
		        break;
	        }
        }
        return oNode;
    }
}
dnn.xml.documentObject.registerClass('dnn.xml.documentObject');

//dnn.xml.XmlNode ---------------------------------------------------------------------------------------------------------
dnn.xml.XmlNode = function(oNode)
{
	this.node = oNode;
	this.ownerDocument = this.node.ownerDocument;
	this.nodeType = this.node.nodeType;
}
	
dnn.xml.XmlNode.prototype = 
{
    parentNode: function()
	{
		if (this.node.parentNode != null)
			return new dnn.xml.XmlNode(this.node.parentNode);
		else
			return null;
	},
	
	childNodes: function(iIndex)
	{
		if (this.node.childNodes[iIndex] != null)
			return new dnn.xml.XmlNode(this.node.childNodes[iIndex]);
	},

	childNodeCount: function()
	{
		return this.node.childNodes.length;
	},

	nodeName: function()
	{
		return this.node.nodeName;
	},
	
	getAttribute: function(sAttr, sDef)
	{
		var sRet = this.node.getAttribute(sAttr);
		if (sRet == null)
			sRet = sDef;
		return sRet;
	},

	setAttribute: function (sAttr, sVal)
	{
		if (sVal == null)
			return this.node.removeAttribute(sAttr);
		else
			return this.node.setAttribute(sAttr, sVal);
	},
	
	getXml: function()
	{
		if (this.node.xml != null)
			return this.node.xml;
		else
			return this.node.getXml();	
	},

	getDocumentXml: function()
	{
		if (this.node.ownerDocument.xml != null)
			return this.node.ownerDocument.xml;
		else
			return this.node.ownerDocument.getXml();
	},

	appendXml: function(sXml)
	{
		var oDoc = dnn.xml.createDocument();
		oDoc.loadXml('<___temp>' + sXml + '</___temp>');	//need to guarantee a single root
		var aNodes = new Array();

		for (var i=0; i<oDoc.childNodes(0).childNodeCount(); i++)	//appending child actually removes node from document, so get references then do append
			aNodes[aNodes.length] = oDoc.childNodes(0).childNodes(i).node;	//use real underlying node object
		for (var i=0; i<aNodes.length; i++)
			this.node.appendChild(aNodes[i]);	//surprised I don't need importNode
		
		return true;
	},

	getNodeIndex: function(sIDName)
	{
		var oParent = this.parentNode();
		var sID = this.getAttribute(sIDName);
		for (var i=0; i<oParent.childNodeCount(); i++)
		{
			if (oParent.childNodes(i).getAttribute(sIDName) == sID)
				return i;
		}
		return -1;
	},

	findNode: function(sNodeName, sAttr, sValue)
	{
		var sXPath = "//" + sNodeName + "[@" + sAttr + "='" + sValue + "']";
		var oNode;
		if (typeof(this.node.selectSingleNode) != 'undefined')
			oNode = this.node.selectSingleNode(sXPath);
		else if (typeof(this.node.ownerDocument.evaluate) != 'undefined')
		{
			var oNodeList = (this.node.ownerDocument.evaluate(sXPath, this.node, null, 0, null));
			if (oNodeList != null)
				oNode = oNodeList.iterateNext();
		}
		else
			oNode = this.node.ownerDocument.findNode(this.node, sNodeName, sAttr, sValue);
		
		if (oNode != null)
			return new dnn.xml.XmlNode(oNode);
	},

	removeChild: function(oNode)
	{
		return this.node.removeChild(oNode.node);
	}
}
dnn.xml.XmlNode.registerClass('dnn.xml.XmlNode');

dnn.xml.init();