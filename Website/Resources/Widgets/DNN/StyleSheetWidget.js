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
	''' This script renders all StyleSheet widgets defined on the page.
	''' This script is designed to be called by StyleSheetWidget.js.
	''' Calling it directly will produce an error.
	''' </summary>
	''' <remarks>
	''' </remarks>
	''' <history>
	'''     Version 1.0.0: Oct. 28, 2007, Nik Kalyani, nik.kalyani@dotnetnuke.com 
	''' </history>
	''' -----------------------------------------------------------------------------
*/

////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// BEGIN: StyleSheetWidget class                                                                              //
////////////////////////////////////////////////////////////////////////////////////////////////////////////////
DotNetNuke.UI.WebControls.Widgets.StyleSheetWidget = function(widget)
{
    DotNetNuke.UI.WebControls.Widgets.StyleSheetWidget.initializeBase(this, [widget]);
}

DotNetNuke.UI.WebControls.Widgets.StyleSheetWidget.activateStyleSheet = function(object, widgetId, widgetStyleSheetCategory, widgetStyleSheetName, widgetStyleSheetId)
{
    // Handle the switching UI element
    var div = $get(widgetId);
    var elements = div.childNodes;
    for (var w = 0; w < elements.length; w++)
    {
        try
        {
            if (elements[w].id.indexOf(widgetStyleSheetCategory) < 0) continue;
            elements[w].className = "UnselectedWidget";
        }
        catch (e)
        {
        }
    }
    object.className = "SelectedWidget";

    var idPrefix = "StyleSheetWidget_" + widgetStyleSheetCategory;
    var cookieName = "StyleSheetWidget_" + widgetId;
    DotNetNuke.UI.WebControls.Utility.setCookie(cookieName, widgetStyleSheetName, 30, "/");

    var head = document.getElementsByTagName("head")[0];
    var links = document.getElementsByTagName("link");
    if (links.length > 0)
    {
        for (var l = 0; l < links.length; l++)
        {
            var link = links[l];
            if (link.getAttribute("title"))
            {
                var styleSheetTitle = link.getAttribute("title");
                if (styleSheetTitle.indexOf(widgetStyleSheetCategory) == 0)
                {
                    if (styleSheetTitle == widgetStyleSheetId)
                        link.disabled = false;
                    else
                        link.disabled = true;
                }
            }
        }
    }
}

DotNetNuke.UI.WebControls.Widgets.StyleSheetWidget.prototype =
{

    // BEGIN: render
    render:
        function()
        {
            var widgetHtml = "";
            var head = document.getElementsByTagName("head")[0];
            var params = this._widget.childNodes;
            var defaultValue = "";
            var firstValue = "";
            cookieName = "StyleSheetWidget_" + this._widget.id;
            var cookieValue = DotNetNuke.UI.WebControls.Utility.getCookie(cookieName);

            var category = this._widget.id;
            var template = "";
            var baseUrl = "";
            var selectedCssClass = "StyleSheetWidget-Selected";
            var cssClass = "StyleSheetWidget";
            var styleSheetList = "";
            var firstValue = "";
            var paramCounter = 0;

            //Detect config params
            for (var p = 0; p < params.length; p++)
            {
                try
                {
                    switch (params[p].name.toLowerCase())
                    {
                        case "template": template = params[p].value; break;
                        case "default": defaultValue = params[p].value; break;
                        case "baseurl": baseUrl = params[p].value; break;
                        case "cssclass": cssClass = params[p].value; break;
                        case "selectedcssclass": selectedCssClass = params[p].value; break;
                        default:
                            if (paramCounter == 0) firstValue = params[p].value;
                            styleSheetList += params[p].value + ";";
                            paramCounter++;
                    }
                }
                catch (e)
                {
                    continue;
                }
            }

            if (template == "")
                template = "<a href=\"javascript:void(0)\" {CLASS} {ID}>{TEXT}</a> ";

            if (baseUrl.substring(baseUrl.length - 1, 1) != "/")
                baseUrl += "/";

            if (cookieValue != "" && styleSheetList.indexOf(cookieValue + ";") > -1)
                defaultValue = cookieValue;
            else if (styleSheetList.indexOf(defaultValue + ";") < 0)
                defaultValue = firstValue;

            var clickEvent = "";
            for (var p = 0; p < params.length; p++)
            {
                param = params[p];
                tag = "";
                name = "";
                paramValue = "";
                try
                {
                    tag = param.tagName.toLowerCase();
                    name = param.name;
                    paramValue = param.value;
                }
                catch (e)
                {
                    continue;
                }

                var lcName = name.toLowerCase();
                var lcValue = paramValue.toLowerCase();
                if ((tag == "param") && (lcName != "default") && (lcName != "template") && (lcName != "baseurl") && (lcName != "cssclass") && (lcName != "selectedcssclass"))
                {
                    var cleanParam = lcValue.replace(/ /g, "-");
                    cleanParam = cleanParam.replace(/\//g, "_");

                    var styleSheetUrl = (baseUrl + lcValue + ".css").replace(/\/\//, "/");
                    var widgetClass = (defaultValue == lcValue ? "SelectedWidget" : "UnselectedWidget");
                    var widgetId = category + "_" + cleanParam;
                    var widgetEvent = "DotNetNuke.UI.WebControls.Widgets.StyleSheetWidget.activateStyleSheet(this, '" + this.getWidget().id + "', '" + category + "', '" + paramValue + "','" + widgetId + "')";
                    var html = template.replace(/\{CLASS\}/gi, " class=\"" + widgetClass + "\"");
                    html = html.replace(/\{ID\}/gi, " id=\"" + widgetId + "\" onclick=\"" + widgetEvent + "\"");
                    html = html.replace(/\{TEXT\}/gi, param.name);
                    widgetHtml += html;

                    var newStyleSheet = document.createElement("link");
                    newStyleSheet.rel = "alternate stylesheet";
                    newStyleSheet.type = "text/css";
                    newStyleSheet.title = widgetId;
                    newStyleSheet.href = styleSheetUrl;
                    newStyleSheet.disabled = true;
                    head.appendChild(newStyleSheet);

                    if (paramValue == defaultValue)
                        clickEvent = widgetEvent.replace("this", "$get('" + widgetId + "')");
                }
            }
            var div = document.createElement("div");

            div.innerHTML = widgetHtml;
            DotNetNuke.UI.WebControls.Widgets.StyleSheetWidget.callBaseMethod(this, "render", [div]);

            if (clickEvent != "")
                eval(clickEvent);
        }
    // END: render
}
DotNetNuke.UI.WebControls.Widgets.StyleSheetWidget.inheritsFrom(DotNetNuke.UI.WebControls.Widgets.BaseWidget);
DotNetNuke.UI.WebControls.Widgets.StyleSheetWidget.registerClass("DotNetNuke.UI.WebControls.Widgets.StyleSheetWidget", DotNetNuke.UI.WebControls.Widgets.BaseWidget);
DotNetNuke.UI.WebControls.Widgets.renderWidgetType("DotNetNuke.UI.WebControls.Widgets.StyleSheetWidget"); 
// END: StyleSheetWidget class
