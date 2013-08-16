////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// BEGIN: SampleWidget class                                                                              //
////////////////////////////////////////////////////////////////////////////////////////////////////////////////

// BEGIN: Namespace management
Type.registerNamespace("YourCompany.Widgets");
// END: Namespace management



YourCompany.Widgets.SampleWidget = function(widget)
{
    YourCompany.Widgets.SampleWidget.initializeBase(this, [widget]);
}

YourCompany.Widgets.SampleWidget.prototype =
{
    // BEGIN: render
    render:
        function()
        {
            var params = this._widget.childNodes;
            if (params != null)
            {
                var text = "Default Text";
                for (var p = 0; p < params.length; p++)
                {
                    try
                    {
                        var paramName = params[p].name.toLowerCase();
                        switch (paramName)
                        {
                            case "text": text = params[p].value; break;
                        }
                    }
                    catch (e)
                    {
                    }
                }
            }
            var div = document.createElement("div");
            div.setAttribute("style", "width:100px;height:100px;border:solid 4px red");
            div.innerHTML = text;
            $addHandler(div, "click", YourCompany.Widgets.SampleWidget.clickHandler);
            YourCompany.Widgets.SampleWidget.callBaseMethod(this, "render", [div]);
        }
    // END: render
}

YourCompany.Widgets.SampleWidget.clickHandler = function(sender)
{
    var clickedObject = sender.target;
    alert("The widget with ID=" + clickedObject.id + " contains text \"" + clickedObject.innerHTML + "\"");
}

YourCompany.Widgets.SampleWidget.inheritsFrom(DotNetNuke.UI.WebControls.Widgets.BaseWidget);
YourCompany.Widgets.SampleWidget.registerClass("YourCompany.Widgets.SampleWidget", DotNetNuke.UI.WebControls.Widgets.BaseWidget);
DotNetNuke.UI.WebControls.Widgets.renderWidgetType("YourCompany.Widgets.SampleWidget");
// END: SampleWidget class
