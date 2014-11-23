/*
  DotNetNuke - http://www.dotnetnuke.com
  Copyright (c) 2002-2014
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
	''' This script renders all PngTransparency widgets defined on the page.
	''' </summary>
	''' <remarks>
	''' </remarks>
	''' <history>
	'''     Version 1.0.0: May. 1, 2008, Nik Kalyani, nik.kalyani@dotnetnuke.com 
	''' </history>
	''' -----------------------------------------------------------------------------
*/

////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// BEGIN: PngTransparencyWidget class                                                                           //
////////////////////////////////////////////////////////////////////////////////////////////////////////////////
DotNetNuke.UI.WebControls.Widgets.PngTransparencyWidget = function(widget)
{
    DotNetNuke.UI.WebControls.Widgets.PngTransparencyWidget.initializeBase(this, [widget]);
}

DotNetNuke.UI.WebControls.Widgets.PngTransparencyWidget.prototype = 
{        
        // BEGIN: render
        render : 
        function()
        {
            if(Sys.Browser.agent == Sys.Browser.InternetExplorer) 
            {
                if (Sys.Browser.version < 7)
                {
                    var divs = document.getElementsByTagName("div");
                    for(var d in divs)
                    {   
                        var div = divs[d];
                        if (div.currentStyle == null)
                            continue;

                        if (div.style && div.style.backgroundImage)
                        {
                            var src = div.style.backgroundImage.replace("url(","");
                            src = src.replace(")", "");
                            if (src.indexOf(".png") < 0) continue;
                            div.style.filter="progid:DXImageTransform.Microsoft.AlphaImageLoader(src='" + src + "', sizingMethod='scale')";
                            div.style.backgroundImage = "";
                        }
                    }
                    var spans = document.getElementsByTagName("span");
                    for(var s in spans)
                    {
                        var span = spans[s];
                        if (span.style && span.style.backgroundImage)
                        {
                            var src = span.style.backgroundImage.replace("url(","");
                            src = src.replace(")", "");
                            if (src.indexOf(".png") < 0) continue;
                            span.style.filter="progid:DXImageTransform.Microsoft.AlphaImageLoader(src='" + src + "', sizingMethod='scale')";
                            span.style.backgroundImage = "";
                        }
                    }
                };
            }
	        var span2 = document.createElement("span");
            DotNetNuke.UI.WebControls.Widgets.PngTransparencyWidget.callBaseMethod(this, "render", [span2]);
        }                
        // END: render	
}
 
DotNetNuke.UI.WebControls.Widgets.PngTransparencyWidget.inheritsFrom(DotNetNuke.UI.WebControls.Widgets.BaseWidget);
DotNetNuke.UI.WebControls.Widgets.PngTransparencyWidget.registerClass("DotNetNuke.UI.WebControls.Widgets.PngTransparencyWidget", DotNetNuke.UI.WebControls.Widgets.BaseWidget);
DotNetNuke.UI.WebControls.Widgets.renderWidgetType("DotNetNuke.UI.WebControls.Widgets.PngTransparencyWidget");
// END: PngTransparencyWidget class
