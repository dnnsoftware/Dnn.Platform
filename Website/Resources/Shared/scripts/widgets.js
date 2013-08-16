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
''' This script renders all  widgets defined on the page.
''' This script requires that init.js be called prior to calling it.
''' </summary>
''' <remarks>
''' </remarks>
''' <history>
'''     Version 1.0.0: Oct. 16, 2007, Nik Kalyani, nik.kalyani@dotnetnuke.com 
''' </history>
''' -----------------------------------------------------------------------------
*/

////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// W I D G E T S                                                                                              //
////////////////////////////////////////////////////////////////////////////////////////////////////////////////

// BEGIN: Namespace management
Type.registerNamespace("DotNetNuke.UI.WebControls.Widgets");
// END: Namespace management



////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// B A S E  W I D G E T                                                                                //
////////////////////////////////////////////////////////////////////////////////////////////////////////////////

////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// BEGIN: BaseWidget class                                                                                    //
////////////////////////////////////////////////////////////////////////////////////////////////////////////////
DotNetNuke.UI.WebControls.Widgets.BaseWidget = function(widget)
{
    if (!widget)
        return (null);
    this._widget = widget;
    this._dependencies = [];
    this._readyCounter = 0;
}

DotNetNuke.UI.WebControls.Widgets.BaseWidget.prototype =
{
    getWidget:
        function()
        {
            return (this._widget);
        },

    getParams:
        function()
        {
            return (this._widget.childNodes);
        },

    setDependency:
        function(className, scriptPath, isResource)
        {
            // Only add the dependency if the class is not already available
            if (typeof (eval(className)) === "undefined")
            {
                if (isResource)
                    $.getScript($dnn.baseResourcesUrl + scriptPath);
                else
                    $.getScript(scriptPath);
                this._dependencies.push(className);
            }
        },

    onReady:
        function(handler, errorHandler, attempts, interval)
        {
            this._handler = handler;
            if (errorHandler)
                this._errorHandler = errorHandler;
            else
                this._errorHandler = "alert('Error loading dependent classes:[ERRORCLASSES]')";
            if ((attempts) && (attempts > 0))
                this._attempts = attempts;
            else
                this._attempts = 100;

            if ((interval) && (interval > 10))
                this._interval = interval;
            else
                this._interval = 10;

            this._checkIfReady(this);
        },

    elementHTML:
        function(element)
        {
            if (element == null)
                return ("");

            var _emptyTags =
            {
                "IMG": true,
                "BR": true,
                "INPUT": true,
                "META": true,
                "LINK": true,
                "PARAM": true,
                "HR": true
            };

            var attrs = element.attributes;
            var str = "<" + element.tagName;
            for (var i = 0; i < attrs.length; i++)
                str += " " + attrs[i].name + "=\"" + attrs[i].value + "\"";

            if (_emptyTags[element.tagName])
                return (str + "/>");

            return (str + ">" + element.innerHTML + "</" + element.tagName + ">");
        },

    _checkIfReady:
        function(self)
        {
            // Using "self" ensures that window.setTimeout can obtain the right context
            self._readyCounter++;
            if (self._readyCounter > self._attempts)
            {
                var errorClasses = "";
                for (var d = 0; d < self._dependencies.length; d++)
                {
                    if (self._dependencies[d] != "")
                        errorClasses += " " + self._dependencies[d];
                }
                eval(self._errorHandler.replace("[ERRORCLASSES]", errorClasses));
                return;
            }
            var ready = true;
            for (var d = 0; d < self._dependencies.length; d++)
            {
                if (self._dependencies[d] != "")
                {
                    if (typeof (eval(self._dependencies[d])) === "undefined")
                    {
                        ready = false;
                        window.setTimeout(function() { self._checkIfReady(self); }, self._interval);
                        break;
                    }
                    else
                        self._dependencies[d] = "";
                }
            }
            if (ready)
                eval(self._handler);
        },

    render:
        function(element)
        {
            element.id = this._widget.id;
            this._widget.parentNode.replaceChild(element, this._widget);
        }

}

DotNetNuke.UI.WebControls.Widgets.BaseWidget.inheritsFrom(Sys.Component);
DotNetNuke.UI.WebControls.Widgets.BaseWidget.registerClass("DotNetNuke.UI.WebControls.Widgets.BaseWidget", Sys.Component);
// END: BaseWidget class


////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// STATIC METHODS                                                                                             //
////////////////////////////////////////////////////////////////////////////////////////////////////////////////

// BEGIN: renderWidgets
// Detects all the widgets found on the page and loads scripts for each widget class

var $widgets = new Object();
$widgets.rendered = false;
$widgets.pageWidgets = new Array();
$widgets.pageWidgetDetectionAttempts = 0;
$widgets.pageWidgetRenderAttempts = new Array();

DotNetNuke.UI.WebControls.Widgets.renderWidgets = function()
{
    if ($widgets.rendered) return;
    $widgets.rendered = true;
    var objects = document.getElementsByTagName("object");
    if ((objects == null) || (objects.length == 0))
    {
        // Some browsers (like Opera) don't return the correct value for the number of objects
        // Give the browser some time and try again a few times
        if ($widgets.pageWidgetDetectionAttempts < 1000)
        {
            window.setTimeout(DotNetNuke.UI.WebControls.Widgets.renderWidgets, 5);
            $widgets.pageWidgetDetectionAttempts++;
        }
        return;
    }
    
    var pageWidgetClasses = new Array();
    // Get all the widget object IDs
    // Need to store the IDs and then process because replacing
    // objects changes the DOM making it impossible to loop
    // through the objects.
    for (var o = 0; o < objects.length; o++)
    {
        try
        {
            if (
                    (objects[o].codeType.toLowerCase() == "dotnetnuke/client") &&
                    (objects[o].id != "")
                )
            {
                $widgets.pageWidgets.push(objects[o]);
                var widgetType = objects[o].codeBase;

                // Prevent loading widgets from anyplace other than user widgets folder
                if (widgetType.indexOf("/") > 0)
                    widgetType = widgetType.substr(widgetType.lastIndexOf("/") + 1);

                objects[o].codeBase = (widgetType.indexOf(".") < 0 ? "DotNetNuke.UI.WebControls.Widgets." + widgetType : widgetType);

                if (typeof (pageWidgetClasses[widgetType]) === "undefined")
                {
                    pageWidgetClasses[widgetType] = widgetType;

                    if (widgetType.indexOf(".") > -1)
                    {
                        var widgetFolder = widgetType.substr(0, widgetType.indexOf("."));
                        $.getScript($dnn.baseResourcesUrl + "Widgets/User/" + widgetFolder + "/" + widgetType + ".js");
                    }
                    else
                        $.getScript($dnn.baseResourcesUrl + "Widgets/DNN/" + widgetType + ".js");
                }
            }
        }
        catch (e)
        {
        }
    }
    delete pageWidgetClasses;
}
// END: renderWidgets

// BEGIN: renderWidgetType
DotNetNuke.UI.WebControls.Widgets.renderWidgetType = function(widgetType)
{
    try
    {
        if (typeof (eval(widgetType)) === "function")
        {
            // Getting here means that the browser has finished loading and processing
            // the script for className. Let's find all the widgets of this class and
            // render them.
            for (var w = 0; w < $widgets.pageWidgets.length; w++)
            {
                try
                {
                    var currentWidgetInstance = $widgets.pageWidgets[w].codeBase;
                    if (currentWidgetInstance.indexOf("/") > 0)
                        currentWidgetInstance = currentWidgetInstance.substr(currentWidgetInstance.lastIndexOf("/") + 1);

                    if (currentWidgetInstance != widgetType)
                        continue;

                    var widget = eval("new " + widgetType + "($widgets.pageWidgets[w])");
                    widget.render();
                }
                catch (r)
                {
                }
            }
        }
    }
    catch (e)
    {
    }
}
// END: renderWidgets

$addHandler(window, "load", DotNetNuke.UI.WebControls.Widgets.renderWidgets);
$renderDNNWidgets();

// Backup method to render widgets in case our load handler was removed by something else
function $renderDNNWidgets()
{
    if ($widgets.rendered) return;
    window.setTimeout(DotNetNuke.UI.WebControls.Widgets.renderWidgets, 20);
}
