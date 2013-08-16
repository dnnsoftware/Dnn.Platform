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
	''' </summary>
	''' <remarks>
	''' </remarks>
	''' <history>
	'''     Version 1.0.0: June 1, 2008, Nik Kalyani, nik.kalyani@dotnetnuke.com 
	''' </history>
	''' -----------------------------------------------------------------------------
*/

////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// BEGIN: PaneCollapseWidget class                                                                           //
////////////////////////////////////////////////////////////////////////////////////////////////////////////////
DotNetNuke.UI.WebControls.Widgets.PaneCollapseWidget = function(widget)
{
    DotNetNuke.UI.WebControls.Widgets.PaneCollapseWidget.initializeBase(this, [widget]);
}

DotNetNuke.UI.WebControls.Widgets.PaneCollapseWidget.prototype = 
{        
        // BEGIN: render
        render : 
        function()
        {
	        var processed = [];
            var params = this._widget.childNodes;
            for(var p=0;p<params.length;p++)
            {
                try
                {
                    var paramName = params[p].name.toLowerCase();
                    if (paramName.indexOf("rule") == 0)
                    {
                          var ruleItems = params[p].value.split(":"); 
                          if (ruleItems.length == 3)
                          {
                              var target = ruleItems[0];
                              if (processed[target] == null)
                              {
                                  var panes = ruleItems[1].split("+");
                                  var newClassName = ruleItems[2];
                                  if (newClassName != "")
                                  {
                                      var isEmpty = true;
                                      for(var q=0;q<panes.length;q++)
                                      {
                                          var pane = $get(panes[q]);
                                          try
                                          {
                                              if (pane.innerHTML.length > 0)
                                              {
                                                  isEmpty = false;
                                                  break;
                                              }
                                          }
                                          catch(e)
                                          {
                                          }
                                      }
                                      
                                      if (isEmpty)
                                      {
                                          processed[target] = true;
                                          $get(target).className = newClassName;
                                      }
                                  }
                              }
                          }
                    }
                }
                catch(e)
                {                
                }
            }

	        var span = document.createElement("span");
            DotNetNuke.UI.WebControls.Widgets.PaneCollapseWidget.callBaseMethod(this, "render", [span]);
        }                
        // END: render
	
}


DotNetNuke.UI.WebControls.Widgets.PaneCollapseWidget.inheritsFrom(DotNetNuke.UI.WebControls.Widgets.BaseWidget);
DotNetNuke.UI.WebControls.Widgets.PaneCollapseWidget.registerClass("DotNetNuke.UI.WebControls.Widgets.PaneCollapseWidget", DotNetNuke.UI.WebControls.Widgets.BaseWidget);
DotNetNuke.UI.WebControls.Widgets.renderWidgetType("DotNetNuke.UI.WebControls.Widgets.PaneCollapseWidget");
// END: PaneCollapseWidget class
