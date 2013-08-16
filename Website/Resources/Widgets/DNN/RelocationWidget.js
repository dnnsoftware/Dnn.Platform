/*
  DotNetNuke - http://www.dotnetnuke.com
  Copyright (c) 2002-2007
  by DotNetNuke Corporation
 
 
	''' -----------------------------------------------------------------------------
	''' <summary>
	''' This script renders all Relocation widgets defined on the page.
	''' When rendered, the widget will move the content of the sourceId element
	''' and put it in the targetId element.
	''' This script is designed to be called by StyleSheetWidget.js.
	''' Calling it directly will produce an error.
	''' </summary>
	''' <remarks>
	''' </remarks>
	''' <history>
	'''     Version 1.0.0: May. 25, 2008, Nik Kalyani, nik.kalyani@dotnetnuke.com 
	''' </history>
	''' -----------------------------------------------------------------------------
*/

////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// BEGIN: RelocationWidget class                                                                           //
////////////////////////////////////////////////////////////////////////////////////////////////////////////////
DotNetNuke.UI.WebControls.Widgets.RelocationWidget = function(widget)
{
    DotNetNuke.UI.WebControls.Widgets.RelocationWidget.initializeBase(this, [widget]);
}

DotNetNuke.UI.WebControls.Widgets.RelocationWidget.prototype = 
{        
        // BEGIN: render
        render : 
        function()
        {
            var params = this.getParams();
            var sourceId = "";
            var targetId = "";
            for(var p=0;p<params.length;p++)
            {
                try
                {
                    var paramName = params[p].name.toLowerCase();
                    switch(paramName)
                    {
                        case "sourceid" : sourceId = params[p].value; break;
                        case "targetid" : targetId = params[p].value; break;
                    }
                }
                catch(e)
                {                
                }
            }

            var source = $get(sourceId);
            var target = $get(targetId);
            
            try
            {
                target.appendChild(source);
            }
            catch(e)
            {
            }
            
	        var span = document.createElement("span");
            DotNetNuke.UI.WebControls.Widgets.RelocationWidget.callBaseMethod(this, "render", [span]);
        }                
        // END: render	
}
  
DotNetNuke.UI.WebControls.Widgets.RelocationWidget.inheritsFrom(DotNetNuke.UI.WebControls.Widgets.BaseWidget);
DotNetNuke.UI.WebControls.Widgets.RelocationWidget.registerClass("DotNetNuke.UI.WebControls.Widgets.RelocationWidget", DotNetNuke.UI.WebControls.Widgets.BaseWidget);
DotNetNuke.UI.WebControls.Widgets.renderWidgetType("DotNetNuke.UI.WebControls.Widgets.RelocationWidget");
// END: RelocationWidget class
