/*
  DotNetNuke - http://www.dotnetnuke.com
  Copyright (c) 2002-2007
  by DotNetNuke Corporation
 
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
	''' This is a script that renders a Rotator widget that is ideal
	''' for site home or section home pages. It displays links for 
	''' the user to click on to display content. While waiting for
	''' input, it rotates the display of other content.
	''' </summary>
	''' <remarks>
	''' </remarks>
	''' <history>
	'''     Version 1.0.0: Oct. 28, 2007, Nik Kalyani, nik.kalyani@dotnetnuke.com 
	''' </history>
	''' -----------------------------------------------------------------------------
*/

////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// BEGIN: RotatorWidget class                                                                              //
////////////////////////////////////////////////////////////////////////////////////////////////////////////////
DotNetNuke.UI.WebControls.Widgets.RotatorWidget = function(widget)
{
    DotNetNuke.UI.WebControls.Widgets.RotatorWidget.initializeBase(this, [widget]);
    DotNetNuke.UI.WebControls.Widgets.RotatorWidget.callBaseMethod(this, "setDependency", ["DotNetNuke.UI.WebControls.ContentRotator", "ContentRotator/scripts/ContentRotator.js", true]);
    this._displayHeight = 200;
    this._displayWidth = 400;
    this._displayInterval = -1;
    this._displayDirection = "";
    this._displayTransition = "";
    this._displayFeedUrl = "";
    this._displayFeedAttribute = "description";
    this._displayImageUrl = "";
    this._displayImageTemplate = "photo{INDEX}.jpg";
    this._displayImageCount = 0;
    this._displayImageScale = "";
    this._content = [];
    this._displayElementId = "";
    this._theme = "";
    this._scrollerObject = this._widget.id + "_Scroller";
    this._scrollerDisplayElementId = this._widget.id + "_ScrollerDisplay";
    this._overlayDisplayElementId = this._widget.id + "_OverlayDisplay";    
    this._navElementId = this._widget.id + "_Nav";
    
    this._widgetHtml = "";    
}

DotNetNuke.UI.WebControls.Widgets.RotatorWidget.prototype = 
{        
        // BEGIN: render
        render : 
        function()
        {
            var params = this._widget.childNodes;
            for(var p=0;p<params.length;p++)
            {
                try
                {
                    var paramName = params[p].name.toLowerCase();
                    switch(paramName)
                    {
                        case "text" : this._text = params[p].value; break;
                        case "elementid"     : this._displayElementId = params[p].value; break;
                        case "height"        : if (params[p].value > 0) this._displayHeight = params[p].value; break;
                        case "width"         : if (params[p].value > 0) this._displayWidth = params[p].value; break;
                        case "interval"      : if (params[p].value > 0) this._displayInterval = params[p].value; break;
                        case "direction"     : 
                                                        var dir = params[p].value.toLowerCase();
                                                        if ((dir == "up") || (dir == "down") || (dir == "right") || (dir == "left") || (dir == "blend"))
                                                            this._displayDirection = dir;
                                                        break;    
                        case "transition"     : 
                                                        var tran = params[p].value.toLowerCase();
                                                        if ((tran == "slide") || (tran == "snap"))
                                                            this._displayTransition = tran;
                                                        break;    
                        case "feedurl"       : this._displayFeedUrl = params[p].value; break;
                        case "feedattribute" : this._displayFeedAttribute = params[p].value; break;
                        case "imageurl"      : this._displayImageUrl = params[p].value; break;
                        case "imagetemplate" : if (params[p].value.indexOf("{INDEX}") > -1) 
                                                            this._displayImageTemplate = params[p].value; 
                                                      break;
                        case "imagecount"    : if (params[p].value > 0) 
                                                            this._displayImageCount = params[p].value; 
                                                      break;
                        case "imagescale"    : if (params[p].value == "width")
                                                            this._displayImageScale = "width";
                                                      else if (params[p].value == "height")
                                                            this._displayImageScale = "height";
                                                      break;
                        case "contentelementid"     : if ($get(params[p].value) != null)
                                                      {
                                                            var contentElement = $get(params[p].value);
                                                            for(var c=0; c<contentElement.childNodes.length; c++)
                                                            { 
                                                                var node = contentElement.childNodes[c];
                                                                if (typeof(node.tagName) === "undefined") continue;
                                                                var content = DotNetNuke.UI.WebControls.Widgets.RotatorWidget.callBaseMethod(this, "elementHTML", [node]);

                                                                if (content != "")
                                                                    this._content.push(content);
                                                            }
                                                      }
                                                      break;          
                        case "navelementid"         : this._navElementId = params[p].value; break;
                        case "theme"                : this._theme = params[p].value; break;
                    }
                }
                catch(e)
                {                
                }
            }

            if ((this._displayImageUrl != "") && (this._displayImageCount > 0) && (this._displayImageTemplate != ""))
            {
                var scale = (this._displayImageScale == "width" ? " width:" + this._displayWidth + "px\""
                          : (this._displayImageScale == "height" ?  " height:" + this._displayHeight + "px\"" : "width:100%;height:100%"));
                for(var i=1; i<=this._displayImageCount; i++)
                {
                    var imgUrl = this._displayImageUrl + this._displayImageTemplate.replace("{INDEX}", i);
                    if (this._displayDirection == "blend")
                        this._content.push(imgUrl);
                    else
                    {
                        var newImg = "<div style=\"background-repeat:no-repeat;";
                        if((Sys.Browser.agent == Sys.Browser.InternetExplorer) && (Sys.Browser.version < 7))
                            newImg += "filter: progid:DXImageTransform.Microsoft.AlphaImageLoader(src='" + imgUrl + "', sizingMethod='scale');";
                        else
                            newImg += "background-image:url('" + imgUrl + "');";
                        newImg += scale + "\"></div>";                        

                        this._content.push(newImg);
                    }
                }
            }
            
            // Wait until all dependent classes are available, then render widget
            DotNetNuke.UI.WebControls.Widgets.RotatorWidget.callBaseMethod(this, "onReady", ["this._internalRender()"]);
        },
        // END: render

        _internalRender :
        function()
        {
            var div = document.createElement("div");
            var displayElement = null;
            
            if (this._displayElementId != "")
                displayElement = $get(this._displayElementId);
            
            var displayHtml = "<div id=\"" + this._scrollerDisplayElementId + "\"></div>" + 
                              "<div id=\"" + this._overlayDisplayElementId + "\"></div>" +
                              "<div id=\"" + this._navElementId + "\"></div>";
                              
            if (displayElement == null)
            {
                this._displayElementId = this._widget.id;
                div.innerHTML = displayHtml;
            }
            else
                displayElement.innerHTML = displayHtml;
                
            DotNetNuke.UI.WebControls.Widgets.RotatorWidget.callBaseMethod(this, "render", [div]);

            eval("window." + this._scrollerObject + " = new DotNetNuke.UI.WebControls.ContentRotator.Rotator(" + 
                                                            "\"window." + this._scrollerObject + "\"" + 
                                                            ",\"" + this._scrollerDisplayElementId + "\"" + 
                                                            (this._displayWidth > 0 ? ", " + this._displayWidth : "") +
                                                            (this._displayHeight > 0 ? ", " + this._displayHeight : "") +
                                                            (this._displayDirection != "" ? ", \"" + this._displayDirection + "\"" : "") +
                                                            (this._displayTransition != "" ? ", \"" + this._displayTransition + "\"" : "") +
                                                            (this._displayInterval > 0 ? ", " + this._displayInterval : "") +
                                                            (this._theme != "" ? ", " + this._theme : "") +
                                                           ");");
                                                                                                   

            var scroller = eval("window." + this._scrollerObject);
            for(var c=0; c<this._content.length; c++) 
            {          
                var item = this._content[c];                                                        
                if (item != "")
                    scroller.addContent(item);
            }

            if ((this._displayFeedUrl != "") && (this._displayFeedAttribute != ""))
                scroller.addFeedContent(this._displayFeedUrl,this._displayFeedAttribute);

            scroller.scroll();
            
        }                
}

DotNetNuke.UI.WebControls.Widgets.RotatorWidget.inheritsFrom(DotNetNuke.UI.WebControls.Widgets.BaseWidget);
DotNetNuke.UI.WebControls.Widgets.RotatorWidget.registerClass("DotNetNuke.UI.WebControls.Widgets.RotatorWidget", DotNetNuke.UI.WebControls.Widgets.BaseWidget);
DotNetNuke.UI.WebControls.Widgets.renderWidgetType("DotNetNuke.UI.WebControls.Widgets.RotatorWidget");
// END: RotatorWidget class
