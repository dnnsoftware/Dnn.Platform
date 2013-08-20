/// <reference name="MicrosoftAjax.js" />
/// <reference name="dnn.js" assembly="DotNetNuke.WebUtility" />
/// <reference name="dnn.xml.js" assembly="DotNetNuke.WebUtility" />

dnn.xml.parserName = 'JS';  //force JS version
//dnn.xml.JsDocument object ---------------------------------------------------------------------------------------------------------
dnn.xml.JsDocument = function()
{
	this.root = new dnn.xml.JsXmlNode(this, '__root');
	this.childNodes = this.root.childNodes;
	this.currentHashCode=0;
}

dnn.xml.JsDocument.prototype = 
{
    hasChildNodes: function ()
    {
	    return this.childNodes.length > 0;
    },

    loadXml: function (sXml)
    {
	    var oParser = new dnn.xml.JsParser();
	    oParser.parse(sXml, this.root);
	    return true;
    },

    getXml: function()
    {
	    return this.root.getXml();
    },

    findNode: function(oParent, sNodeName, sAttr, sValue)
    {
	    //primitive for now...  
	    for (var i=0; i < oParent.childNodes.length; i++)
	    {
		    oNode = oParent.childNodes[i];

		    //if (oNode.nodeType != 3)  //exclude nodeType of Text (Netscape/Mozilla) issue!
		    if (oNode.nodeName == sNodeName)
		    {
			    if (sAttr == null)
				    return oNode;
			    else
			    {
				    if (oNode.getAttribute(sAttr) == sValue)
					    return oNode;
			    }
		    }
		    if (oNode.childNodes.length > 0)
		    {
			    var o = this.findNode(oNode, sNodeName, sAttr, sValue);
			    if (o != null)
				    return o;
		    }
	    }
    },

    getNextHashCode: function()
    {
	    this.currentHashCode++;
	    return this.currentHashCode;
    }
}
dnn.xml.JsDocument.registerClass('dnn.xml.JsDocument');


//dnn.xml.JsXmlNode Object
dnn.xml.JsXmlNode = function(ownerDocument, name)
{
    this.ownerDocument = ownerDocument;
    this.nodeName = name;
    this.text = '';
    this.childNodes = new Array();
    this.attributes = new Array();
    this.parentNode = null;
    this.hashCode = this.ownerDocument.getNextHashCode();
    this.nodeType = 0;
    //this.xml = this.getXml;
}

dnn.xml.JsXmlNode.prototype = 
{
    appendChild: function(oNode)
    {
	    this.childNodes[this.childNodes.length] = oNode;
	    oNode.parentNode = this;
    },

    removeChild: function(oNode)
    {
	    var oParent = this;
	    var iHash = oNode.hashCode;
	    var bFound = false;
	    for (var i=0; i<oParent.childNodes.length; i++)
	    {
		    if (bFound == false)
		    {
			    if (oParent.childNodes[i].hashCode == iHash)
				    bFound = true;
		    }
		    if (bFound)
			    oParent.childNodes[i] = oParent.childNodes[i+1];
	    }
	    if (bFound)
		    oParent.childNodes.length = oParent.childNodes.length - 1; //remove last node
	    return oNode;
    },

    hasChildNodes: function()
    {
	    return this.childNodes.length > 0;
    },

    getXml: function(oNode)
    {
	    if (oNode == null)
		    oNode = this;

	    var sXml = '';
    	
	    if (oNode.nodeName != '__root')
		    sXml =  '<' + oNode.nodeName + this.getAttributes(oNode) + '>';
    	
	    for (var i=0; i<oNode.childNodes.length; i++)
	    {
		    sXml += this.getXml(oNode.childNodes[i]) + oNode.childNodes[i].text;				
	    }	
	    if (oNode.nodeName != '__root')
		    sXml = sXml + '</' + oNode.nodeName + '>';
	    return sXml;
    },

    getAttributes: function(oNode)
    {
	    var sRet = '';
	    for (var sAttr in oNode.attributes)
		    sRet += ' ' + sAttr + '="' + dnn.encodeHTML(oNode.attributes[sAttr]) + '"';					
	    return sRet;
    },

    getAttribute: function(sAttr)
    {
	    return this.attributes[sAttr];
    },

    setAttribute: function(sAttr, sVal)
    {
	    this.attributes[sAttr] = sVal;
    },

    removeAttribute: function(sAttr)
    {
	    delete this.attributes[sAttr];
    }
}
dnn.xml.JsXmlNode.registerClass('dnn.xml.JsXmlNode');    

//primitive Js Xml Parser 
//sure a regular expression guru could make better
dnn.xml.JsParser = function()
{
    this.pos = null;
    this.xmlArray = null;
    this.root = null;
}

dnn.xml.JsParser.prototype = 
{
    parse: function(sXml, oRoot)
    {
	    this.pos = 0;
	    this.xmlArray = sXml.split('>');
	    this.processXml(oRoot);
    },

    getProcessString: function ()
    {
	    var s = this.xmlArray[this.pos];
	    if (s == null)
		    s = '';
	    return s.replace(/^\s*/, "").replace(/\s*$/, ""); //trim off spaces on both sides
    },

    processXml: function(oParent)
    {
	    var oNewParent = oParent;
	    var bClose = this.isCloseTag();
	    var bOpen = this.isOpenTag();
	    while ((bClose == false || (bClose && bOpen)) && this.getProcessString().length > 0)
	    {
		    if (bClose)
		    {
			    this.processOpenTag(oParent);
			    this.pos +=1;
		    }
		    else
		    {
			    oNewParent = this.processOpenTag(oParent);
			    this.pos +=1;
			    this.processXml(oNewParent);
		    }					

		    bClose = this.isCloseTag();
		    bOpen = this.isOpenTag();
	    }
	    var s = this.getProcessString();		
	    if (bClose && s.substr(0,1) != '<')
		    oParent.text = s.substr(0,s.indexOf('<'));
	    this.pos +=1;
    },

    isCloseTag: function()
    {
	    var s = this.getProcessString();
	    if (s.substr(0, 1) == '/' || s.indexOf('</') > -1 || s.substr(s.length-1) == '/')
		    return true;
	    else
		    return false;
    },

    isOpenTag: function()
    {
	    var s = this.getProcessString();
	    if (s.substr(0, 1) == '<' && s.substr(0, 2) != '</' && s.substr(0,2) != '<?')
		    return true;
	    else
		    return false;
    },

    processOpenTag: function(oParent)
    {		
	    if (this.isOpenTag(this.getProcessString()))
	    {
		    var sArr = this.getProcessString().split(' ');
		    var oNode = new dnn.xml.JsXmlNode(oParent.ownerDocument);
    		
		    oNode.nodeName = sArr[0].substr(1).replace('/', '');
		    oNode.parentNode = oParent;
		    this.processAttributes(oNode);
		    oParent.appendChild(oNode);
		    oParent = oNode;
	    }
	    return oParent
    },

    processAttributes: function(oNode)
    {
	    var s = this.getProcessString();
	    if (s.indexOf(' ') > -1)
		    s = s.substr(s.indexOf(' ') + 1);
    	
	    if (s.indexOf('=') > -1)
	    {
		    var bValue=false;
		    var sName='';
		    var sValue='';
		    var sChar;
		    for (var i=0; i<s.length; i++)
		    {
			    sChar = s.substr(i, 1);
			    if (sChar == '"')
			    {
				    if (bValue)
				    {
					    //need to escape out special characters
					    oNode.attributes[sName] = dnn.decodeHTML(sValue);
					    sName = '';
					    sValue = '';
					    i++; //skip space
				    }						
				    bValue = !bValue;
			    }
			    else if (sChar != '=' || bValue==true)	//if inside value then allow =
			    {
				    if (bValue)
					    sValue += sChar;
				    else
					    sName += sChar;
			    }
		    }
	    }
    }
}
dnn.xml.JsParser.registerClass('dnn.xml.JsParser');    
