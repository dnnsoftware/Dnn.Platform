/*
  DotNetNuke - http://www.dotnetnuke.com
  Copyright (c) 2002-2007
  by DotNetNuke Corporation
 
  Attribution for getElementsByClassName():
	v1.03 Copyright (c) 2006 Stuart Colville
	http://muffinresearch.co.uk/archives/2006/04/29/getelementsbyclassname-deluxe-edition/

  Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated 
  documentation files (the "Software"), to deal in the Software without restriction, including without limitation 
  the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and 
  to permit persons to whom the Software is furnished to do so, subject to the following conditions:
 
  The above copyright notice and this permission notice shall be included in all copies or substantial portions 
  of the Software.
 
  THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED 
  TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL 
  THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF 
  CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
  DEALINGS IN THE SOFTWARE.

	''' -----------------------------------------------------------------------------
	''' <summary>
	''' This script renders all StyleScrubber widgets defined on the page.
	''' When rendered, the widget will find all elements matching "className"
	''' and strip the element of its "style" attribute by default if no
	''' "removeAttribute" parameter is specified, otherwise it will remove
	''' the attribute named in each removeAttribute parameter specified.
	''' This script is designed to be called by StyleSheetWidget.js.
	''' Calling it directly will produce an error.
	''' </summary>
	''' <remarks>
	''' </remarks>
	''' <history>
	'''     Version 1.0.0: Apr. 1, 2008, Nik Kalyani, nik.kalyani@dotnetnuke.com 
	''' </history>
	''' -----------------------------------------------------------------------------
*/

////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// BEGIN: StyleScrubberWidget class                                                                           //
////////////////////////////////////////////////////////////////////////////////////////////////////////////////
DotNetNuke.UI.WebControls.Widgets.StyleScrubberWidget = function(widget)
{
    DotNetNuke.UI.WebControls.Widgets.StyleScrubberWidget.initializeBase(this, [widget]);
}

DotNetNuke.UI.WebControls.Widgets.StyleScrubberWidget.prototype = 
{        
        // BEGIN: render
        render : 
        function()
        {
	        var classNames = [];
	        var tag = "*";
            var recursive = false;
	        var removeAttributes = [];
            var params = this._widget.childNodes;
            for(var p=0;p<params.length;p++)
            {
                try
                {
                    var paramName = params[p].name.toLowerCase();
                    switch(paramName)
                    {
                        case "classnames" : classNames = params[p].value.split(";"); break;
                        case "tag" : tag = params[p].value.toLowerCase(); break;
			            case "removeattribute" : removeAttributes[removeAttributes.length] = params[p].value.toLowerCase(); break;
			            case "recursive" : recursive = (params[p].value.toLowerCase() == "true" ? true : false);
                    }
                }
                catch(e)
                {                
                }
            }

	        if (removeAttributes.length == 0)
		        removeAttributes[0] = "style";

            if (classNames.length > 0)           
            {
                for(var cn=0;cn<classNames.length;cn++)
                {
		            var elements = this.getElementsByClassName(classNames[cn], tag);
		            for(var e=0;e<elements.length;e++)
		            {
			            var obj = elements[e];
			            this.stripAttributes(obj, removeAttributes, recursive);
		            }
		        }
            }            
	        var span = document.createElement("span");
            DotNetNuke.UI.WebControls.Widgets.StyleScrubberWidget.callBaseMethod(this, "render", [span]);
            
        },                
        // END: render

        // BEGIN: stripAttributes
        stripAttributes:
        function(element, attrs, recurse)
        {
	        for(var r=0;r<attrs.length;r++)
	        {
	            try 
	            {
	                element.removeAttribute(attrs[r]);            
	            }
	            catch(e)
	            {
	            }
	        }
            if (recurse)
            {
                try
                {
                    if (element.childNodes.length > 0)
                    {
                        for(var c=0;c<element.childNodes.length;c++)
                            this.stripAttributes(element.childNodes[c], attrs, recurse);
                    }
                }
                catch(e)
                {
                }
            }
        },
        
        // END: stripAttributes
	    // BEGIN: getElementsByClassName
	    getElementsByClassName:
	    function(strClass, strTag, objContElm) 
	    {
  		    strTag = strTag || "*";
  		    objContElm = objContElm || document;    
  		    var objColl = objContElm.getElementsByTagName(strTag);
  		    if (!objColl.length &&  strTag == "*" &&  objContElm.all) objColl = objContElm.all;
  		    var arr = new Array();                              
  		    var delim = strClass.indexOf('|') != -1  ? '|' : ' ';   
  		    var arrClass = strClass.split(delim);    
  		    for (var i = 0, j = objColl.length; i < j; i++) 
		    {                         
    			    var arrObjClass = objColl[i].className.split(' ');   
    			    if (delim == ' ' && arrClass.length > arrObjClass.length) continue;
    			    var c = 0;
    			    comparisonLoop:
    			    for (var k = 0, l = arrObjClass.length; k < l; k++) 
			    {
     				    for (var m = 0, n = arrClass.length; m < n; m++) 
				    {
        				    if (arrClass[m] == arrObjClass[k]) c++;
        				    if ((delim == '|' && c == 1) || (delim == ' ' && c == arrClass.length)) 
					    {
          					    arr.push(objColl[i]); 
          					    break comparisonLoop;
        				    }
      				    }
    			    }
  		    }
  		    return arr; 
	    }
	    // END: getElementsByClassName	
	
}

  

// To cover IE 5 Mac lack of the push method
Array.prototype.push = function(value) {this[this.length] = value; };

DotNetNuke.UI.WebControls.Widgets.StyleScrubberWidget.inheritsFrom(DotNetNuke.UI.WebControls.Widgets.BaseWidget);
DotNetNuke.UI.WebControls.Widgets.StyleScrubberWidget.registerClass("DotNetNuke.UI.WebControls.Widgets.StyleScrubberWidget", DotNetNuke.UI.WebControls.Widgets.BaseWidget);
DotNetNuke.UI.WebControls.Widgets.renderWidgetType("DotNetNuke.UI.WebControls.Widgets.StyleScrubberWidget");
// END: StyleScrubberWidget class
