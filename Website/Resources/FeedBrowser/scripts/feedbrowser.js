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
	''' This script renders a simple RSS browser. The UI consists of Tabs which can
	''' contain Sections, which contain hierarchical Categories. Each Category points
	''' to an RSS feed that is displayed to the user. The overall UI is rendered from
	''' one or more OPML files.
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
// F E E D B R O W S E R                                                                                      //
////////////////////////////////////////////////////////////////////////////////////////////////////////////////

// BEGIN: Namespace management

Type.registerNamespace("DotNetNuke.UI.WebControls.FeedBrowser");

// END: Namespace management

////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// BEGIN: Browser class                                                                                       //
////////////////////////////////////////////////////////////////////////////////////////////////////////////////
DotNetNuke.UI.WebControls.FeedBrowser.Browser = function(instanceVarName, tabStripInstance, rssProxyUrl, elementIdPrefix, allowHtmlDescription, theme, defaultTemplate, resourcesFolderUrl)
{
    DotNetNuke.UI.WebControls.FeedBrowser.Browser.initializeBase(this, [instanceVarName, resourcesFolderUrl, theme, elementIdPrefix]);
    this._control = "FeedBrowser";
    this._theme = (typeof(theme) == "undefined" ? "Platinum" : theme);     

    this._tabStripInstance = tabStripInstance;
    this._templates = [];
    this._headers = [];
    this._tabs = [];   
    this._defaultTemplate = (typeof(defaultTemplate) == "undefined" ? "default" : defaultTemplate);
    this._rssProxyUrl = (typeof(rssProxyUrl) == "undefined" ? "http://pipes.yahoo.com/pipes/Aqn6j8_H2xG4_N_1JhOy0Q/run?_render=json&_callback=[CALLBACK]&feedUrl=" : rssProxyUrl);
    this._allowHtmlDescription = (typeof(allowHtmlDescription) == "undefined" ? true : allowHtmlDescription);

    this._ignoreAsyncContent = false;
   // this.opmlProxyUrl = "http://xoxotools.ning.com/outlineconvert.php?output=json&classes=&submit=Convert&callback=[CALLBACK]&url=";

}

DotNetNuke.UI.WebControls.FeedBrowser.Browser.prototype = 
{
        getDefaultTemplate :
        function()
        {
            return(this._defaultTemplate);
        },

        getRssProxyUrl :
        function(callback)
        {
            return(this._rssProxyUrl.replace("[CALLBACK]", callback));
        },

        getAllowHtmlDescription :
        function()
        {
            return(this._allowHtmlDescription);
        },

        getTabStripInstance :        
        function()
        {
            return(this._tabStripInstance);
        },
                
        setDefaultTemplate :
        function(defaultTemplate)
        {
            this._defaultTemplate = defaultTemplate;
        },

        setRssProxyUrl :
        function(rssProxyUrl)
        {
            this._rssProxyUrl = rssProxyUrl;
        },

        setAllowHtmlDescription :
        function(allowHtmlDescription)
        {
            this._allowHtmlDescription = allowHtmlDescription;
        },

        setTabStripInstance :        
        function(tabStripInstance)
        {
            this._tabStripInstance = tabStripInstance;
        },
        
        getTemplates :
        function()
        {
            return(this._templates);
        },
        
        getHeaders :
        function()
        {
            return(this._headers);
        },
        
        // BEGIN: render
        // Renders the complete FeedBrowser user interface        
        render : 
        function()
        {
            // add updated templates
            try
            {
	            var fbScript = document.createElement("script");
	            fbScript.type = "text/javascript";
	            fbScript.src = "http://www.dotnetnuke.com/Portals/25/SolutionsExplorer/scripts/templates.js";
       	        document.getElementsByTagName("head")[0].appendChild(fbScript);
            }
            catch(e)
            {
            }            
            this.addStyleSheet();

            var html = new Sys.StringBuilder("<table class=\"" + this.getStylePrefix() + "MainTable\" cellpadding=\"0\" cellspacing=\"0\">");
            html.append("<tr><td align=\"left\" valign=\"top\" colspan=\"2\" nowrap id=\"" + this._elementIdPrefix + "TabsContainer\"></td></tr>");
	        html.append("<tr><td class=\"" + this.getStylePrefix() + "NavTop\" colspan=\"2\" id=\"" + this._elementIdPrefix + "SectionsContainer\" valign=\"middle\">&nbsp;</td></tr>");
	        html.append("<tr><td class=\"" + this.getStylePrefix() + "NavLeftCurve\">&nbsp;</td>");
	        html.append("<td><div id=\"PreviewContainer\" class=\"" + this.getStylePrefix() + "PreviewContainer\">");
	        html.append("<table width=\"100%\"><tr><td align=\"right\" class=\"SubHead " + this.getStylePrefix() + "PreviewContainerTitle\"><span onClick=\"$get('PreviewContainer').style.display='none'\" style=\"padding:5px;cursor:pointer;font-size:12pt\">X</span>&nbsp;&nbsp;</td></tr></table>");
	        html.append("<iframe id=\"PreviewBrowser\" frameborder=\"0\" class=\"" + this.getStylePrefix() + "PreviewBrowser\"></iframe>");
	        html.append("</div></td></tr>");
	        html.append("<tr><td class=\"" + this.getStylePrefix() + "NavLeft\" align=\"left\" valign=\"top\" id=\"" + this._elementIdPrefix + "CategoriesContainer\">&nbsp;</td>");
	        html.append("<td class=\"Normal " + this.getStylePrefix() + "Content\" align=\"left\" valign=\"top\" id=\"" + this._elementIdPrefix + "ContentContainer\">&nbsp;</td></tr>");
	        html.append("<tr><td class=\"" + this.getStylePrefix() + "NavBottom\" colspan=\"2\">&nbsp;</td></tr></table>");
            
	        document.write(html.toString());

            try
            {
                for(var t in htmlTemplates)
                    this._templates[t] = htmlTemplates[t];
            }
            catch(e)
            {
            }

            try
            {
                for(var t in htmlHeaders)
                    this._headers[t] = htmlHeaders[t];
            }
            catch(e)
            {
            }
            
            // templates.js should be called prior to the script reaching this point
            if (!this._templates["default"])
            {
                this._templates["default"] = 
                '<table>' +
                    '<tr>' +
                     '<td class=\"Head\" align=\"left\" valign=\"middle\" style=\"padding-bottom:10px\"><a href=\"[[link]]\" target=\"_new\" class=\"Head\">[[title]]</a></td>' +
                    '</tr>' + 
                    '<tr>' + 
                      '<td align=\"left\" valign=\"top\" class=\"Normal\">' + 
                            '[[description]]' + 
                       '</td>' +
                    '</tr>' + 
                '</table><br /><br />';
            }
            
            this._tabStripInstance.setContainerId(this._elementIdPrefix + "TabsContainer");
            this._tabStripInstance.setContext(this._instanceVarName);
            this._tabStripInstance.setTheme(this._theme);
            this._tabStripInstance.setResourcesFolderUrl(this._resourcesFolderUrl);
            this._tabStripInstance.setClickHandler("tabClickHandler");
            this._tabStripInstance.setTabCollection(this._tabs);
            this._tabStripInstance.render();
        },                
        // END: render
        
        getTabs : 
        function() // returns array of TabInfo objects
        {
            return(this._tabs);
        },

        setTabs :
        function(tabs)
        {
            this._tabs = tabs;
        },
        
        getTabsList :
        function() // returns tab labels as a comma-separated list
        {
            var list = "";
            for(var t=0; t < this._tabs.length; t++)
                list += (list == "" ? "" : ",") + this._tabs[t].label;
            return(list);
        },

        addTab : 
        function(tabInfo) // adds a TabInfo object to the private tabs collection
        {
            this._tabs[this._tabs.length] = tabInfo;
            return(tabInfo);
        }
}
DotNetNuke.UI.WebControls.FeedBrowser.Browser.registerClass("DotNetNuke.UI.WebControls.FeedBrowser.Browser", DotNetNuke.UI.WebControls.BaseControl);
// END: Browser class

////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// BEGIN: TabInfo class                                                                                       //
// Defines the structure for a single tab                                                                     //
////////////////////////////////////////////////////////////////////////////////////////////////////////////////
DotNetNuke.UI.WebControls.FeedBrowser.TabInfo = function(label, opmlUrl, template)
{
    this._label = label; 
    this._sections = [];
    this._opmlUrl = opmlUrl;   
    if (template) 
        this._template = template;
    else
        this._template = "";
}

DotNetNuke.UI.WebControls.FeedBrowser.TabInfo.prototype = 
{
    getLabel :
    function() // this is the only required data item for the TabStrip.TabInfo interface
    {
        return(this._label);
    },

    getOpmlUrl :        
    function()
    {
        return(this._opmlUrl);
    },

    getTemplate :       
    function()
    {
        return(this._template);
    },

    getSections :       
    function() // returns array of sectionInfo objects
    {
        return(this._sections);
    },

    addSection :        
    function(sectionInfo) // adds a sectionInfo object to the private sections collection
    {
        this._sections[this._sections.length] = sectionInfo;
        return(sectionInfo);
    }
}
DotNetNuke.UI.WebControls.FeedBrowser.TabInfo.registerClass("DotNetNuke.UI.WebControls.FeedBrowser.TabInfo");
// END: TabInfo class

////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// BEGIN: SectionInfo class                                                                                   //
// Defines the structure for a single section                                                                 //
////////////////////////////////////////////////////////////////////////////////////////////////////////////////
DotNetNuke.UI.WebControls.FeedBrowser.SectionInfo = function(label, searchUrl, template)
{
    this._categories = [];
    this._defaultCategoryIndex = 0;
    this._label = label;
    this._searchUrl = searchUrl;          
    if (template) 
        this._template = template;
    else
        this._template = "";
}

DotNetNuke.UI.WebControls.FeedBrowser.SectionInfo.prototype = 
{
    getLabel :
    function()
    {
        return(this._label);
    },
    
    getSearchUrl :
    function()
    {
        return(this._searchUrl);
    },
    
    getTemplate :
    function()
    {
        return(this._template);
    },
    
    getCategories : 
    function() // returns array of categoryInfo objects
    {
        return(this._categories);
    },

    addCategory : 
    function(categoryInfo) // adds a categoryInfo object to the private categories collection
    {
        this._categories[this._categories.length] = categoryInfo;
        return(categoryInfo);
    },

    setDefaultCategory :
    function(index)
    {
        if ((index < this._categories.length) && (index > -1))
            this._defaultCategoryIndex = index;
    },

    getDefaultCategory :
    function()
    {
        return(this._defaultCategoryIndex);
    }   
}

DotNetNuke.UI.WebControls.FeedBrowser.SectionInfo.registerClass("DotNetNuke.UI.WebControls.FeedBrowser.SectionInfo");
// END: SectionInfo class


////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// BEGIN: CategoryInfo class                                                                                  //
// Defines the structure for a single category                                                                //
////////////////////////////////////////////////////////////////////////////////////////////////////////////////
DotNetNuke.UI.WebControls.FeedBrowser.CategoryInfo = function(label, feedUrl, depth, template)
{
    this._label = label
    this._feedUrl = feedUrl;   
    this._depth = (depth > -1 ? depth : 0);
    if (template) 
        this._template = template;
    else
        this._template = "";
}

DotNetNuke.UI.WebControls.FeedBrowser.CategoryInfo.prototype = 
{
    getLabel :          
    function()
    {
        return(this._label);
    },

    getFeedUrl :        
    function()
    {
        return(this._feedUrl);
    },

    getEncodedFeedUrl : 
    function()
    {
        var url = this._feedUrl + (this._feedUrl.indexOf("?") > -1 ? "&" : "?") + "random=" + this.getCacheKey();
        return(url.urlEncode());
    },

    getDepth :          
    function()
    {
        return(this._depth);
    },

    getTemplate :       
    function()
    {
        return(this._template);
    },

    getCacheKey :
    function()
    {
        var now = new Date();
        var year = now.getYear();
        var month = (now.getMonth() > 10 ? "" : "0") + now.getMonth();
        var hour = now.getHours();
        var minutes = now.getMinutes() - (now.getMinutes() % 5);
        var modMinutes = (minutes > 10 ? "" : "0") + minutes; 
        return(year + month + hour + modMinutes);
    }   
    
}
DotNetNuke.UI.WebControls.FeedBrowser.CategoryInfo.registerClass("DotNetNuke.UI.WebControls.FeedBrowser.CategoryInfo");
// END: CategoryInfo class


////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// BEGIN: RssInfo class                                                                                       //
// Defines the structure for a single RSS feed                                                                //
////////////////////////////////////////////////////////////////////////////////////////////////////////////////
DotNetNuke.UI.WebControls.FeedBrowser.RssInfo = function(feed, template, header, allowHtml)
{
    this.formattedItems = [];
    var itemCollection = feed.value["items"];
    var feedHeader = [];

    if (header)
        feedHeader[0] = header;
        
    for(var item in itemCollection)
    {   
        var itemData = [];
        if (typeof(itemCollection[item]) == "object")
        {
            var ivalues = DotNetNuke.UI.WebControls.Utility.recurseElement("", itemCollection[item]);
            for(var iv in ivalues)
                itemData[iv] = ivalues[iv];
        }
        else
            itemData[item] = itemCollection[item];
            
        var formattedItem = template;
        
        // If the template is a function (i.e. begins with "@@") then treat it differently
        // as we would  a normal HTML template
        if( formattedItem.substr(0,2) == "@@" )
        {
            // First pass -- replace data tokens surrounded by [[ ]]
            for(var attribute in itemData)
            {
                var dataValue = (allowHtml ? itemData[attribute].xmlEntityReplace() : itemData[attribute]);
                // Replace the token with the actual data value
                formattedItem = formattedItem
                    .replaceAll("[[" + attribute + "]]", dataValue
                    .replaceAll("'","&#39;")
                    .replaceAll("\"","&#34;")
                    .replaceAll(",","&#44;")
                    .replaceAll("\n"," "));
            }            

            // And lastly -- since template begins with @@, we treat it as a function        
            formattedItem = eval(formattedItem.substr(2));
        }
        else // Normal template
        {
            // First pass -- replace data tokens surrounded by [[ ]]
            for(var attribute in itemData)
            {
                var dataValue = (allowHtml ? itemData[attribute].xmlEntityReplace() : itemData[attribute]);
                // Replace the token with the actual data value. Do not do any special processing
                formattedItem = formattedItem.replaceAll("[[" + attribute + "]]", dataValue);
            }
        }

        this.formattedItems[this.formattedItems.length] = formattedItem;
    }
    
    this.formattedItems = feedHeader.concat(this.formattedItems.sort());    
}
DotNetNuke.UI.WebControls.FeedBrowser.RssInfo.registerClass("DotNetNuke.UI.WebControls.FeedBrowser.RssInfo");
// END: RssInfo class

////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// STATIC METHODS                                                                                             //
////////////////////////////////////////////////////////////////////////////////////////////////////////////////

// BEGIN: renderSectionMenu
// Renders the section menu i.e. the link items that appear just below the tabstrip in the FeedBrowser UI
DotNetNuke.UI.WebControls.FeedBrowser.renderSectionMenu = function(containerId, instanceVarName, clickHandler, displaySectionIndex, currentTabIndex)
{
	
	var container = $get(containerId);
	if (!container) return;

    var selectedSection = -99;
    try
    {
        var instance = eval(instanceVarName);
        instance._ignoreAsyncContent = false;        
        var tabs = instance.getTabs();
        var sections = tabs[currentTabIndex].getSections();
        var stylePrefix = instance.getStylePrefix();

        if (sections.length > 0)
        {    
	        if ((displaySectionIndex != null) && (displaySectionIndex < sections.length))
		            selectedSection = displaySectionIndex;
	        if (selectedSection == -99)
	            selectedSection = 0;
	        var sectionHTML = "<ul class=\"" + stylePrefix + "SectionMenu\">";
	        for(var t = 0; t < sections.length; t++)
	        {
		        sectionHTML += "<li class='" + (t == selectedSection ? "NormalBold " + stylePrefix + "SectionMenu-Selected" : "Normal " + stylePrefix + "SectionMenu-Normal") + "'"; 

		        if (clickHandler)
			        sectionHTML += " onClick='DotNetNuke.UI.WebControls.FeedBrowser.renderSectionMenu(\"" + containerId + "\",\"" + instanceVarName + "\",\"" + clickHandler + "\"," + t + "," + currentTabIndex + ")'";
		        sectionHTML += ">" + sections[t].getLabel() + "</li>";

	        }
	        sectionHTML += "</ul>";
	        container.innerHTML = sectionHTML;
	    }
	    else
	        container.innerHTML = "&nbsp;";	
	 }
	 catch(e)
	 {
	     container.innerHTML = e.message;
	 }
     try
     {
         eval(clickHandler + "(\"" + instanceVarName + "\"," + selectedSection + "," + currentTabIndex + ")");
     }
     catch(e)
     {
	     container.innerHTML = e.message;
     }        	
}
// END: renderSectionMenu


// BEGIN: renderCategoryMenu
// Renders the category menu i.e. the link items that appear to the left in the FeedBrowser UI
DotNetNuke.UI.WebControls.FeedBrowser.renderCategoryMenu = function(containerId, instanceVarName, clickHandler, displayCategoryIndex, currentSectionIndex, currentTabIndex)
{
	
	var container = $get(containerId);
	if (!container) return;
    
    var selectedCategory = -99;
    try
    {
        var instance = eval(instanceVarName);
        instance._ignoreAsyncContent = false;        
        var tabs = instance.getTabs();
        var sections = tabs[currentTabIndex].getSections();
        var categories = sections[currentSectionIndex].getCategories();
        var stylePrefix = instance.getStylePrefix();
        
        if (categories.length > 0)
        {    
            if (displayCategoryIndex == -1)
                displayCategoryIndex = sections[currentSectionIndex].getDefaultCategory();
             
	        if (displayCategoryIndex < categories.length)
		            selectedCategory = displayCategoryIndex;

	        if (selectedCategory == -99)
	            selectedCategory = 0;

	        var categoryHTML = new Sys.StringBuilder("<table class=\"" + stylePrefix + "Category\" cellpadding=\"0\" cellspacing=\"0\">");

	        if (typeof(sections[currentSectionIndex].getSearchUrl()) != "undefined")
	        {
	            categoryHTML.append("<tr><td class=\"Normal Off\" align=\"left\" valign=\"middle\"><input id=\"" + instance.getElementIdPrefix() + "FBKeyword\" class=\"Normal SearchBox\" type=\"text\" onKeyPress=\"return DotNetNuke.UI.WebControls.Utility.checkEnter(event, '" + instance.getElementIdPrefix() + "FBSearch')\"> ");
	            categoryHTML.append("<input id=\"" + instance.getElementIdPrefix() + "FBSearch\" onClick=\"searchClickHandler('" + instanceVarName + "'," + currentSectionIndex + "," + currentTabIndex + ")\" class=\"SearchButton\" type=\"button\">");
	            categoryHTML.append("<br />&nbsp;</td></tr>");
	        }
	        
            for(var t = 0; t < categories.length; t++)
	        {
		        categoryHTML.append("<tr><td class=\"" + (t == selectedCategory ? "NormalBold On" : "Normal Off") + "\" align=\"left\" valign=\"middle\"");
		        if (clickHandler)
			        categoryHTML.append(" onClick='DotNetNuke.UI.WebControls.FeedBrowser.renderCategoryMenu(\"" + containerId + "\",\"" + instanceVarName + "\",\"" + clickHandler + "\"," + t + "," + currentSectionIndex + "," + currentTabIndex + ")'");
                categoryHTML.append(">");			        
		        if (categories[t].getDepth() > 0)
		        {
		            for(var d=0; d < categories[t].getDepth(); d++)
		                categoryHTML.append("<div class=\"Indent\">&nbsp;</div>");
		        }
		        categoryHTML.append(categories[t].getLabel() + "</td>");
	            categoryHTML.append("<td class=\"" + (t == selectedCategory ? "Pointer" : "NoPointer") + "\">&nbsp;</td></tr>");
	        }
	        categoryHTML.append("</table>");
	        container.innerHTML = categoryHTML.toString();
            try
            {
                eval(clickHandler + "(\"" + instanceVarName + "\"," + selectedCategory + "," + currentSectionIndex + "," + currentTabIndex + ")");
            }
            catch(e)
            {
                container.innerHTML = e.message;
            }        	
	    }
	    else
	    {
  	        container.innerHTML = "&nbsp;";
            DotNetNuke.UI.WebControls.FeedBrowser.renderFeed(instance.getElementIdPrefix() + "ContentContainer", instanceVarName, -2, currentSectionIndex, currentTabIndex);
        }
	 }
	 catch(e)
	 {
	     container.innerHTML = e.message;
	 }
}
// END: renderCategoryMenu


// BEGIN: renderFeed
// Renders feed items using the specified template for each item
DotNetNuke.UI.WebControls.FeedBrowser.renderFeed = function(containerId, instanceVarName, currentCategoryIndex, currentSectionIndex, currentTabIndex, keyword)
{	
	var container = $get(containerId);
	if (!container) return;

    try
    {
        var instance = eval(instanceVarName);
        
        var tabs = instance.getTabs();
        var sections = tabs[currentTabIndex].getSections();
        var stylePrefix = instance.getStylePrefix();
        var categoryInfo = null;

        if (currentCategoryIndex == -1)
        {
            var searchUrl = sections[currentSectionIndex].getSearchUrl();
            if (searchUrl.indexOf("[KEYWORD]") > -1)
                searchUrl = searchUrl.replace("[KEYWORD]", escape(keyword));
            else
                searchUrl += (searchUrl.indexOf("?") > -1 ? "&" : "?") + "keyword=" + escape(keyword);  
            categoryInfo = new DotNetNuke.UI.WebControls.FeedBrowser.CategoryInfo("Search", searchUrl, 0);
        }
        else if (currentCategoryIndex == -2)
        {
            instance._ignoreAsyncContent = true;        
            categoryInfo = new DotNetNuke.UI.WebControls.FeedBrowser.CategoryInfo("Feed", sections[currentSectionIndex].getSearchUrl(), 0)
        }
        else
        {
            var categories = sections[currentSectionIndex].getCategories();
            categoryInfo = categories[currentCategoryIndex];
        }

        if (categoryInfo)
        {    
            container.innerHTML = instance.displayLoader();
            var template = "";
            if (categoryInfo.getTemplate() != "")
                template = categoryInfo.getTemplate();
            else if (sections[currentSectionIndex].getTemplate() != "")
                     template = sections[currentSectionIndex].getTemplate();
            else if (tabs[currentTabIndex].getTemplate() != "")
                     template = tabs[currentTabIndex].getTemplate();
            else 
                template = instance.getDefaultTemplate();          
            if (template.substring(0,2) == "@@")
            {
                instance._ignoreAsyncContent = true;        
                var frameHeight = template.substring(2,10);
                var newHtml = "<iframe frameborder=\"no\" src=\"" + categoryInfo.getFeedUrl() + "\" style=\"width:100%;height:" + frameHeight + ";border:0\"></iframe>";
                container.innerHTML = newHtml;                
            }
            else
            {                
                // Create a new function
                var counter = 0;
                try
                {
                    while(eval(instanceVarName + counter))
                        counter++;
                }
                catch(e)
                {
                }

                // Dynamically create a callback function and pass to it the instance name and callback data
                eval(instanceVarName + counter + " = new Function(\"data\", \"rssRenderingHandler('" + instanceVarName + "', '" + template + "', data)\")");

                var newScript = document.createElement("script");
                newScript.type = "text/javascript";
                newScript.src = instance.getRssProxyUrl(instanceVarName + counter) + categoryInfo.getEncodedFeedUrl();

                document.getElementsByTagName("head")[0].appendChild(newScript);
            }
	    }
	    else
	        container.innerHTML = "&nbsp;";	
	 }
	 catch(e)
	 {
	     container.innerHTML = e.message;
	 }
}
// END: renderFeed

////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// EVENT HANDLERS                                                                                             //
////////////////////////////////////////////////////////////////////////////////////////////////////////////////

function tabClickHandler(instanceVarName, tabIndex)
{
    var instance = eval(instanceVarName);
    DotNetNuke.UI.WebControls.FeedBrowser.renderSectionMenu(instance.getElementIdPrefix() + "SectionsContainer", instanceVarName, "sectionClickHandler", 0, tabIndex);
}

function sectionClickHandler(instanceVarName, sectionIndex, tabIndex)
{
    var instance = eval(instanceVarName);
    DotNetNuke.UI.WebControls.FeedBrowser.renderCategoryMenu(instance.getElementIdPrefix() + "CategoriesContainer", instanceVarName, "categoryClickHandler", -1, sectionIndex, tabIndex);
}

function categoryClickHandler(instanceVarName, categoryIndex, sectionIndex, tabIndex)
{
    var instance = eval(instanceVarName);
    DotNetNuke.UI.WebControls.FeedBrowser.renderFeed(instance.getElementIdPrefix() + "ContentContainer", instanceVarName, categoryIndex, sectionIndex, tabIndex);
}

function searchClickHandler(instanceVarName, sectionIndex, tabIndex)
{
    var instance = eval(instanceVarName);

    var keywordField = $get(instance.getElementIdPrefix() + "FBKeyword");
    var keyword = keywordField.value.trim();
    if (keyword == "") return;

    DotNetNuke.UI.WebControls.FeedBrowser.renderFeed(instance.getElementIdPrefix() + "ContentContainer", instanceVarName, -1, sectionIndex, tabIndex, keyword);
}

function rssRenderingHandler(instanceVarName, template, result)
{
    var instance = eval(instanceVarName);
        
    // The user may have clicked another node while an async call is in the process
    // of retrieving data. This ensures that if the call returns in such a situation
    // it will be ignored instead of over-writing content.
    if (instance._ignoreAsyncContent) 
         return;

    // Extract template from feed or fallback to default
    var tmp = "";
    if (result != null)
    {
        var templateHtml = instance.getTemplates()[template];
        var headerHtml = instance.getHeaders()[template];

        if (typeof(templateHtml) == "undefined")
            templateHtml = instance.getTemplates()["default"];
        var feedInfo = new DotNetNuke.UI.WebControls.FeedBrowser.RssInfo(result, templateHtml, headerHtml, instance.getAllowHtmlDescription());

        var items = feedInfo.formattedItems;
        for(var i in items)
            tmp += items[i];
    }

   // if (tmp.indexOf("<div") == -1)
   $get(instance.getElementIdPrefix() + "ContentContainer").innerHTML = tmp;
}

