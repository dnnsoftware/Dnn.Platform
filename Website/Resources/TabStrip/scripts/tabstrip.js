/*
  DotNetNuke® - http://www.dotnetnuke.com
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
	''' This script renders a simple but graphically rich tab strip.
	'''
    ''' Ensure that ~/Resources/Shared/scripts/init.js is called from the browser before calling this script
    ''' This script will fail if the required AJAX libraries loaded by init.js are not present.
	''' </summary>
	''' <remarks>
	''' </remarks>
	''' <history>
	'''     Version 1.0.0: Feb. 28, 2007, Nik Kalyani, nik.kalyani@dotnetnuke.com 
	''' </history>
	''' -----------------------------------------------------------------------------
*/

////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// T A B S T R I P                                                                                            //
////////////////////////////////////////////////////////////////////////////////////////////////////////////////

// BEGIN: Namespace management
Type.registerNamespace("DotNetNuke.UI.WebControls.TabStrip");
// END: Namespace management

////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// BEGIN: Strip class                                                                                         //
////////////////////////////////////////////////////////////////////////////////////////////////////////////////
DotNetNuke.UI.WebControls.TabStrip.Strip = function(instanceVarName, containerId, clickHandler, context, theme, resourcesFolderUrl)
{
    DotNetNuke.UI.WebControls.TabStrip.Strip.initializeBase(this, [instanceVarName, resourcesFolderUrl, theme]);
    this._control = "TabStrip";
    this._theme = (typeof(theme) == "undefined" ? "Platinum" : theme);     

    this._containerId = containerId;
    this._clickHandler = (typeof(clickHandler) == "undefined" ? "" : clickHandler);
    this._context = context; // This value is passed to the clickhandler as-is so the clickhandler has some context when the event is fired
    this._tabCollection = [];
    this._styleSheetLoaded = false;
}

DotNetNuke.UI.WebControls.TabStrip.Strip.prototype = 
{
        getContext :
        function()
        {
            return(this._context);
        },
        
        getContainerId :
        function()
        {
            return(this._containerId);
        },
        
        getClickHandler :
        function()
        {
            return(this._clickHandler);
        },
        
        setContext :
        function(context)
        {
            this._context = context;
        },
        
        setContainerId :
        function(containerId)
        {
            this._containerId = containerId;
        },

        setClickHandler :
        function(clickHandler)
        {
            this._clickHandler = clickHandler;
        },
        
        addTab :
        function(tabInfo)
        {
                if (typeof(tabInfo.getLabel) != "undefined")
                    this._tabCollection[this._tabCollection.length] = tabInfo;
        },

        removeTab :
        function(index)
        {
            if ((index < this._tabCollection.length) && (index > -1))
                Array.removeAt(this._tabCollection, index);
        },
                
        updateTab :
        function(index, tabInfo)
        {
            if ((index < this._tabCollection.length) && (index > -1) && (typeof(tabInfo.getLabel) != "undefined"))
                this._tabCollection[index] = tabInfo;
            
        },
        
        setTabCollection :
        function(tabCollection)
        {
            var tabs = [];
            for(var t=0; t < tabCollection.length; t++)
            {
                    // The only requirement for each item in the tabCollection is that
                    // it implement a getLabel() method.
                    if (typeof(tabCollection[t].getLabel) != "undefined")
                        tabs[tabs.length] = tabCollection[t];
            }
            this._tabCollection = tabs;
        },

        getTabCollection :      
        function()
        {
             return(this._tabCollection);
        },
        
        // BEGIN: renderTabs
        // Renders a list of labels with a tabbed UI and injects the rendered HTML into the [containerId] element
        render :
        function(displayTabIndex)
        {
	        var container = $get(this._containerId);
	        if (!container) return;

            if (!this._styleSheetLoaded)
            {
                this.addStyleSheet();
                this._styleSheetLoaded = true;
            }
            
            var selectedTabIndex = -99;
            try
            {
                var tabs = this._tabCollection;
                var stylePrefix = this.getStylePrefix();

                if (tabs.length > 0)
                {    
	                if ((displayTabIndex != null) && (displayTabIndex < tabs.length))
		                    selectedTabIndex = displayTabIndex;

	                if (selectedTabIndex == -99)
	                {
		                //optional: add code to check for last viewed tab cookie
	                }

	                if (selectedTabIndex == -99)
	                    selectedTabIndex = 0;
	                var tabHtml = new Sys.StringBuilder("<div class=\"" + stylePrefix + "TabsContainer\">");
	                tabHtml.append("<ul class=\"" + stylePrefix + "Tabs\">");
	                for(var t = 0; t < tabs.length; t++)
	                {
		                tabHtml.append("<li class='" + stylePrefix);
		                var isSelected = (t == selectedTabIndex);
		                if (t == 0)
			                tabHtml.append((isSelected ? "Tabs-LeftSelected" : "Tabs-LeftNormal")); 
		                else
		                {
			                if (selectedTabIndex == (t-1))
				                tabHtml.append("Tabs-RightSelectedNormal");
			                else
				                tabHtml.append((isSelected ? "Tabs-RightNormalSelected" : "Tabs-RightNormalNormal")); 
		                }
		                tabHtml.append("'></li>");
		                tabHtml.append("<li class='Normal " + stylePrefix + (isSelected ? "Tabs-MiddleSelected" : "Tabs-MiddleNormal") + "'");

		                if (typeof(this._clickHandler) != "undefined")
			                tabHtml.append(" onClick='" + this._instanceVarName + ".render(" + t + ")'");
		                tabHtml.append(">" + tabs[t].getLabel() + "</li>");

		                if (t == (tabs.length-1))
		                {
			                tabHtml.append("<li class='" + stylePrefix);
			                tabHtml.append((isSelected ? "Tabs-RightSelected" : "Tabs-RightNormal") + "'></li>");
		                }
	                }
	                tabHtml.append("</ul></div>");
	                container.innerHTML = tabHtml.toString();

	            }
	            else
	                container.innerHTML = "&nbsp;";
            	
	            // optional: add code to set last viewed tab cookie;
	         }
	         catch(e)
	         {
                container.innerHTML = e.message;
	         }
             try
             {
                 eval(this._clickHandler + "(\"" + this._context + "\"," + selectedTabIndex + ")");
             }
             catch(e)
             {
                container.innerHTML = e.message;
             }        	
        }
        // END: render class                                    
 }
DotNetNuke.UI.WebControls.TabStrip.Strip.registerClass("DotNetNuke.UI.WebControls.TabStrip.Strip", DotNetNuke.UI.WebControls.BaseControl);
// END: Strip class

////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// BEGIN: TabInfo class                                                                                       //
// Defines the structure for a single tab                                                                     //
////////////////////////////////////////////////////////////////////////////////////////////////////////////////
DotNetNuke.UI.WebControls.TabStrip.TabInfo = function(label)
{
    this._label = label; 
}

DotNetNuke.UI.WebControls.TabStrip.TabInfo.prototype = 
{
    getLabel :
    function() // this is the only required data item for the TabStrip.TabInfo interface
    {
        return(this._label);
    }
}
DotNetNuke.UI.WebControls.TabStrip.TabInfo.registerClass("DotNetNuke.UI.WebControls.TabStrip.TabInfo");
// END: TabInfo class


