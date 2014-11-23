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
	''' <remarks>
	''' </remarks>
	''' <history>
	'''     Version 1.0.0: Nov. 25, 2008, Nik Kalyani, nik.kalyani@dotnetnuke.com 
	''' </history>
	''' -----------------------------------------------------------------------------
*/

////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// BEGIN: EmbedWidget class                                                                              //
////////////////////////////////////////////////////////////////////////////////////////////////////////////////
DotNetNuke.UI.WebControls.Widgets.EmbedWidget = function(widget)
{
    DotNetNuke.UI.WebControls.Widgets.EmbedWidget.initializeBase(this, [widget]);
    this._type = "Unknown";
    this._multiValueDelimiter = ";";
    this._publisher = "";
    this._attributes = [];
}

DotNetNuke.UI.WebControls.Widgets.EmbedWidget.prototype =
{
    // BEGIN: render
    render:
        function()
        {
            var params = this._widget.childNodes;
            this._attributes["widgetid"] = this._widget.id;
            for (var p = 0; p < params.length; p++)
            {
                try
                {
                    var paramName = params[p].name.toLowerCase();
                    switch (paramName)
                    {
                        case "type": this._type = params[p].value; break;
                        case "multivaluedelimiter": this._multiValueDelimiter = params[p].value; break;
                        case "publisher": this._publisher = params[p].value; break;
                        default: this._attributes[paramName] = params[p].value; break;
                    }
                }
                catch (e)
                {
                }
            }
            var self = this;
            var folder = this._type.indexOf(".") > -1 ? this._type.split(".")[0] : this._type;
            var path = $dnn.baseResourcesUrl + "Widgets/DNN/EmbedWidgetResources/" + folder + "/" + this._type.toLowerCase() + ".snippet.htm";
            if (this._publisher != "")
                path = $dnn.baseResourcesUrl + "Widgets/User/" + this._publisher + "/EmbedWidgetResources/" + folder + "/" + this._type.toLowerCase() + ".snippet.htm"
            $.get(path, function(data) { self._internalRender(self._type, self._multiValueDelimiter, data); })

        },
    // END: render

    _internalRender:
        function(type, delim, data)
        {
            var re = /{\s*(\S+?)\s*:(.*?):(.*?)}/gi;
            var matches = data.match(re);
            if (matches != null)
            {
                for (var m = 0; m < matches.length; m++)
                {
                    var match = matches[m].replace(re, "$1 | $2 | $3").split("|");
                    var attr = match[0].trim();

                    var valuePresent = match[1].trim();
                    if (this._attributes[attr] != null)
                    {
                        var attrValues = this._attributes[attr].split(delim);
                        for (var v = 0; v < attrValues.length; v++)
                            valuePresent = valuePresent.replaceAll("{" + v + "}", attrValues[v]);
                    }

                    var valueMissing = match[2].trim();

                    data = data.replace(matches[m], this._attributes[attr] != null ? valuePresent : valueMissing);
                }

            }

            // Script added using innerHTML is not executed by most browsers
            // Need to extract all script blocks and append them as child elements in page
            re = /<script.*?>((\n|\r|.)*?)<\/script>/gi;
            var scripts = [];
            var scriptMatches = data.match(re);
            if (scriptMatches != null)
            {
                for (var m = 0; m < scriptMatches.length; m++)
                {
                    scripts[scripts.length] = scriptMatches[m].replace(re, "$1");
                    data = data.replace(scriptMatches[m], "");
                }
            }

            if ((this._attributes["elementId"] != null) && (this._attributes["elementId"] != ""))
            {
                $get(this._attributes["elementId"]).innerHTML = data;
                var span = document.createElement("span");
                DotNetNuke.UI.WebControls.Widgets.EmbedWidget.callBaseMethod(this, "render", [span]);
                DotNetNuke.UI.WebControls.Widgets.appendDynamicScripts(this._attributes["elementId"], scripts);
            }
            else
            {
                var div = document.createElement("div");
                DotNetNuke.UI.WebControls.Widgets.EmbedWidget.callBaseMethod(this, "render", [div]);
                div.innerHTML = data;
                DotNetNuke.UI.WebControls.Widgets.appendDynamicScripts(this._widget.id, scripts);
            }
        }
}

DotNetNuke.UI.WebControls.Widgets.EmbedWidget.inheritsFrom(DotNetNuke.UI.WebControls.Widgets.BaseWidget);
DotNetNuke.UI.WebControls.Widgets.EmbedWidget.registerClass("DotNetNuke.UI.WebControls.Widgets.EmbedWidget", DotNetNuke.UI.WebControls.Widgets.BaseWidget);
DotNetNuke.UI.WebControls.Widgets.renderWidgetType("DotNetNuke.UI.WebControls.Widgets.EmbedWidget");
// END: EmbedWidget class
