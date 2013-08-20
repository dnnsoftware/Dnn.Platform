//General
//for example: instead of each module writing out script found in moduleMaxMin_OnClick have the functionality cached
//

var DNN_COL_DELIMITER = String.fromCharCode(16);
var DNN_ROW_DELIMITER = String.fromCharCode(15);
var __dnn_m_bPageLoaded = false;

if (window.addEventListener)
    window.addEventListener("load", __dnn_Page_OnLoad, false);
else
    window.attachEvent("onload", __dnn_Page_OnLoad);

function __dnn_ClientAPIEnabled()
{
	return typeof(dnn) != 'undefined';
}

function __dnn_Page_OnLoad()
{
	if (__dnn_ClientAPIEnabled())
	{	
		dnn.dom.attachEvent(window, 'onscroll', __dnn_bodyscroll);
	}
	__dnn_m_bPageLoaded = true;
}

function __dnn_KeyDown(iKeyCode, sFunc, e)
{
	if (e == null)
		e = window.event;

	if (e.keyCode == iKeyCode)
	{
		eval(unescape(sFunc));
		return false;
	}
}

function __dnn_bodyscroll() 
{
	var oF=document.forms[0];	
	if (__dnn_ClientAPIEnabled() && __dnn_m_bPageLoaded && typeof(oF.ScrollTop) != 'undefined')
		oF.ScrollTop.value=document.documentElement.scrollTop ? document.documentElement.scrollTop : dnn.dom.getByTagName("body")[0].scrollTop;
}

function __dnn_setScrollTop(iTop)
{
	if (__dnn_ClientAPIEnabled())
	{
		if (iTop == null)
			iTop = document.forms[0].ScrollTop.value;
	
		var sID = dnn.getVar('ScrollToControl');
		if (sID != null && sID.length > 0)
		{
			var oCtl = dnn.dom.getById(sID);
			if (oCtl != null)
			{
				iTop = dnn.dom.positioning.elementTop(oCtl);
				dnn.setVar('ScrollToControl', '');
			}
		}

		if (document.getElementsByTagName("html")[0].style["overflow"] != "hidden") {
			window.scrollTo(0, iTop);
		}
	}
}

//Focus logic
function __dnn_SetInitialFocus(sID)
{
	var oCtl = dnn.dom.getById(sID);	
	if (oCtl != null && __dnn_CanReceiveFocus(oCtl))
		oCtl.focus();
}	

function __dnn_CanReceiveFocus(e)
{
	//probably should call getComputedStyle for classes that cause item to be hidden
	if (e.style.display != 'none' && e.tabIndex > -1 && e.disabled == false && e.style.visible != 'hidden')
	{
		var eParent = e.parentElement;
		while (eParent != null && eParent.tagName != 'BODY')
		{
			if (eParent.style.display == 'none' || eParent.disabled || eParent.style.visible == 'hidden')
				return false;
			eParent = eParent.parentElement;
		}
		return true;
	}
	else
		return false;
}

//Max/Min Script
function __dnn_ContainerMaxMin_OnClick(oLnk, sContentID)
{
	var oContent = dnn.dom.getById(sContentID);
	if (oContent != null)
	{
		var oBtn = oLnk.childNodes[0];
		var sContainerID = dnn.getVar('containerid_' + sContentID); //oLnk.getAttribute('containerid');
		var sCookieID = dnn.getVar('cookieid_' + sContentID); //oLnk.getAttribute('cookieid');
		var sCurrentFile = oBtn.src.toLowerCase().substr(oBtn.src.lastIndexOf('/'));
		var sMaxFile;
		var sMaxIcon;
		var sMinIcon;

		if (dnn.getVar('min_icon_' + sContainerID))
			sMinIcon = dnn.getVar('min_icon_' + sContainerID);
		else
			sMinIcon = dnn.getVar('min_icon');

		if (dnn.getVar('max_icon_' + sContainerID))
			sMaxIcon = dnn.getVar('max_icon_' + sContainerID);
		else
			sMaxIcon = dnn.getVar('max_icon');

		sMaxFile = sMaxIcon.toLowerCase().substr(sMaxIcon.lastIndexOf('/'));

		var iNum = 5;

		var animf = dnn.getVar('animf_' + sContentID);
		if (animf != null)
			iNum = new Number(animf);
			
		if (sCurrentFile == sMaxFile)
		{
			oBtn.src = sMinIcon;				
			//oContent.style.display = '';
			dnn.dom.expandElement(oContent, iNum);
			oBtn.title = dnn.getVar('min_text');
			if (sCookieID != null)
			{
				if (dnn.getVar('__dnn_' + sContainerID + ':defminimized') == 'true')
					dnn.dom.setCookie(sCookieID, 'true', 365);
				else
					dnn.dom.deleteCookie(sCookieID);
			}
			else
				dnn.setVar('__dnn_' + sContainerID + '_Visible', 'true');
		}
		else
		{
			oBtn.src = sMaxIcon;				
			//oContent.style.display = 'none';
			dnn.dom.collapseElement(oContent, iNum);
			oBtn.title = dnn.getVar('max_text');
			if (sCookieID != null)
			{
				if (dnn.getVar('__dnn_' + sContainerID + ':defminimized') == 'true')
					dnn.dom.deleteCookie(sCookieID);
				else
					dnn.dom.setCookie(sCookieID, 'false', 365);				
			}
			else
				dnn.setVar('__dnn_' + sContainerID + '_Visible', 'false');			
		}
		
		return true;	//cancel postback
	}
	return false;	//failed so do postback
}

function __dnn_Help_OnClick(sHelpID)
{
	var oHelp = dnn.dom.getById(sHelpID);
	if (oHelp != null)
	{
		if (oHelp.style.display == 'none')
			oHelp.style.display = '';
		else
			oHelp.style.display = 'none';

		return true;	//cancel postback
	}
	return false;	//failed so do postback
}

function __dnn_SectionMaxMin(oBtn, sContentID)
{
	var oContent = dnn.dom.getById(sContentID);
	if (oContent != null)
	{
		var sMaxIcon = oBtn.getAttribute('max_icon');
		var sMinIcon = oBtn.getAttribute('min_icon');
		var bCallback = oBtn.getAttribute('userctr') != null;
		var sVal;
		if (oContent.style.display == 'none')
		{
			oBtn.src = sMinIcon;				
			oContent.style.display = '';
			if (bCallback)
			    sVal = 'True';
			else
			    dnn.setVar(oBtn.id + ':exp', 1);
		}
		else
		{
			oBtn.src = sMaxIcon;				
			oContent.style.display = 'none';
			if (bCallback)
			    sVal = 'False';
			else
                dnn.setVar(oBtn.id + ':exp', 0);
		}
		if (bCallback)
		    dnncore.setUserProp(oBtn.getAttribute('userctr'), oBtn.getAttribute('userkey'), sVal, null);
		return true;	//cancel postback
	}
	return false;	//failed so do postback
}

//Drag N Drop
function __dnn_enableDragDrop()
{
	var aryConts = dnn.getVar('__dnn_dragDrop').split(";");	
	var aryTitles;

	for (var i=0; i < aryConts.length; i++)
	{
		aryTitles = aryConts[i].split(" ");
		if (aryTitles[0].length > 0)
		{			
			var oCtr = dnn.dom.getById(aryTitles[0]);
			var oTitle = dnn.dom.getById(aryTitles[1]);
			if (oCtr != null && oTitle != null)
			{
				oCtr.setAttribute('moduleid', aryTitles[2]);
				dnn.dom.positioning.enableDragAndDrop(oCtr, oTitle, '__dnn_dragComplete()', '__dnn_dragOver()');
			}	
		}
	}
}

var __dnn_oPrevSelPane;
var __dnn_oPrevSelModule;
var __dnn_dragEventCount=0;
function __dnn_dragOver()
{
	__dnn_dragEventCount++;
	if (__dnn_dragEventCount % 75 != 0)	//only calculate position every 75 events
		return;
	
	var oCont = dnn.dom.getById(dnn.dom.positioning.dragCtr.contID);

	var oPane = __dnn_getMostSelectedPane(dnn.dom.positioning.dragCtr);
		
	if (__dnn_oPrevSelPane != null)	//reset previous pane's border
		__dnn_oPrevSelPane.pane.style.border = __dnn_oPrevSelPane.origBorder;

	if (oPane != null)
	{		
		__dnn_oPrevSelPane = oPane;
		oPane.pane.style.border = '4px double ' + DNN_HIGHLIGHT_COLOR;
		var iIndex = __dnn_getPaneControlIndex(oCont, oPane);

		var oPrevCtl;
		var oNextCtl;
		for (var i=0; i<oPane.controls.length; i++)
		{
			if (iIndex > i && oPane.controls[i].id != oCont.id)
				oPrevCtl = oPane.controls[i];
			if (iIndex <= i && oPane.controls[i].id != oCont.id)
			{
				oNextCtl = oPane.controls[i];
				break;
			}
		}			
		
		if (__dnn_oPrevSelModule != null)
			dnn.dom.getNonTextNode(__dnn_oPrevSelModule.control).style.border = __dnn_oPrevSelModule.origBorder;
			

		if (oNextCtl != null)
		{
			__dnn_oPrevSelModule = oNextCtl;
			dnn.dom.getNonTextNode(oNextCtl.control).style.borderTop = '5px groove ' + DNN_HIGHLIGHT_COLOR;
		}
		else if (oPrevCtl != null)
		{
			__dnn_oPrevSelModule = oPrevCtl;
			dnn.dom.getNonTextNode(oPrevCtl.control).style.borderBottom = '5px groove ' + DNN_HIGHLIGHT_COLOR;
		}
	}
}

function __dnn_dragComplete()
{
	var oCtl = dnn.dom.getById(dnn.dom.positioning.dragCtr.contID);
	var sModuleID = oCtl.getAttribute('moduleid');
	
	if (__dnn_oPrevSelPane != null)
		__dnn_oPrevSelPane.pane.style.border = __dnn_oPrevSelPane.origBorder;

	if (__dnn_oPrevSelModule != null)
		dnn.dom.getNonTextNode(__dnn_oPrevSelModule.control).style.border = __dnn_oPrevSelModule.origBorder;
		
	var oPane = __dnn_getMostSelectedPane(dnn.dom.positioning.dragCtr);
	var iIndex;
	if (oPane == null)
	{
		var oPanes = __dnn_Panes();
		for (var i=0; i<oPanes.length; i++)
		{
			if (oPanes[i].id == oCtl.parentNode.id)
				oPane = oPanes[i];
		}
	}	
	if (oPane != null)
	{
		iIndex = __dnn_getPaneControlIndex(oCtl, oPane);
		__dnn_MoveToPane(oPane, oCtl, iIndex);

		dnn.callPostBack('MoveToPane', 'moduleid=' + sModuleID, 'pane=' + oPane.paneName, 'order=' + iIndex * 2); 
	}
}

function __dnn_MoveToPane(oPane, oCtl, iIndex)
{

	if (oPane != null)
	{
		var aryCtls = new Array();
		for (var i=iIndex; i<oPane.controls.length; i++)
		{
			if (oPane.controls[i].control.id != oCtl.id)
				aryCtls[aryCtls.length] = oPane.controls[i].control;

			dnn.dom.removeChild(oPane.controls[i].control);
		}
		dnn.dom.appendChild(oPane.pane, oCtl);
		oCtl.style.top=0;
		oCtl.style.left=0;
		oCtl.style.position = 'relative';
		for (var i=0; i<aryCtls.length; i++)
		{
			dnn.dom.appendChild(oPane.pane, aryCtls[i]);
		}
		__dnn_RefreshPanes();
	}
	else
	{
		oCtl.style.top=0;
		oCtl.style.left=0;
		oCtl.style.position = 'relative';
	}
}

function __dnn_RefreshPanes()
{
	var aryPanes = dnn.getVar('__dnn_Panes').split(';');
	var aryPaneNames = dnn.getVar('__dnn_PaneNames').split(';');
	__dnn_m_aryPanes = new Array();
	for (var i=0; i<aryPanes.length; i++)
	{
		if (aryPanes[i].length > 0)
			__dnn_m_aryPanes[__dnn_m_aryPanes.length] = new __dnn_Pane(dnn.dom.getById(aryPanes[i]), aryPaneNames[i]);
	}
}

var __dnn_m_aryPanes;
var __dnn_m_aryModules;
function __dnn_Panes()
{
	if (__dnn_m_aryPanes == null)
	{
		__dnn_m_aryPanes = new Array();
		__dnn_RefreshPanes();
	}
	return __dnn_m_aryPanes;
}

function __dnn_Modules(sModuleID)
{
	if (__dnn_m_aryModules == null)
		__dnn_RefreshPanes();
	
	return __dnn_m_aryModules[sModuleID];
}

function __dnn_getMostSelectedPane(oContent)
{
	var oCDims = new dnn.dom.positioning.dims(oContent);
	var iTopScore=0;
	var iScore;
	var oTopPane;
	for (var i=0; i<__dnn_Panes().length; i++)
	{
		var oPane = __dnn_Panes()[i];
		var oPDims = new dnn.dom.positioning.dims(oPane.pane);
		iScore = dnn.dom.positioning.elementOverlapScore(oPDims, oCDims);
		
		if (iScore > iTopScore)
		{
			iTopScore = iScore;
			oTopPane = oPane;
		}
	}
	return oTopPane;
}

function __dnn_getPaneControlIndex(oContent, oPane)
{
	if (oPane == null)
		return;
	var oCDims = new dnn.dom.positioning.dims(oContent);
	var oCtl;
	if (oPane.controls.length == 0)
		return 0;
	for (var i=0; i<oPane.controls.length; i++)
	{
		oCtl = oPane.controls[i];
		var oIDims = new dnn.dom.positioning.dims(oCtl.control);
		if (oCDims.t < oIDims.t)
			return oCtl.index;
	}
	if (oCtl != null)
		return oCtl.index+1;
	else
		return 0;
}

//Objects
function __dnn_Pane(ctl, sPaneName)
{
	this.pane = ctl;
	this.id = ctl.id;
	this.controls = new Array();
	this.origBorder = ctl.style.border;
	this.paneName = sPaneName;
	
	var iIndex = 0;
	var strModuleOrder='';
	for (var i=0; i<ctl.childNodes.length; i++)
	{
		var oNode = ctl.childNodes[i];
		if (dnn.dom.isNonTextNode(oNode))	
		{
			if (__dnn_m_aryModules == null)
				__dnn_m_aryModules = new Array();

			//if (oNode.tagName == 'A' && oNode.childNodes.length > 0)
			//	oNode = oNode.childNodes[0];	//DNN now embeds anchor tag 
				
			var sModuleID = oNode.getAttribute('moduleid');
			if (sModuleID != null && sModuleID.length > 0)
			{
				strModuleOrder += sModuleID + '~';
				this.controls[this.controls.length] = new __dnn_PaneControl(oNode, iIndex);
				__dnn_m_aryModules[sModuleID] = oNode.id;
				iIndex+=1;
			}
		}
	}
	this.moduleOrder = strModuleOrder;

}

function __dnn_PaneControl(ctl, iIndex)
{
	this.control = ctl;
	this.id = ctl.id;
	this.index = iIndex;
	this.origBorder = ctl.style.border;

}

function __dnn_ShowModalPage(urlPage) {
    dnnModal.show(urlPage, /*showReturn*/true, 550, 950, true, '');    
}

//move towards dnncore ns.  right now only for personalization
function __dnncore()
{
    this.GetUserVal = 0;
    this.SetUserVal = 1;
}

__dnncore.prototype = {
getUserProp: function(sNameCtr, sKey, pFunc) {
    this._doUserCallBack(dnncore.GetUserVal, sNameCtr, sKey, null, new dnncore.UserPropArgs(sNameCtr, sKey, pFunc));
},

setUserProp: function(sNameCtr, sKey, sVal, pFunc) {
    this._doUserCallBack(dnncore.SetUserVal, sNameCtr, sKey, sVal, new dnncore.UserPropArgs(sNameCtr, sKey, pFunc));
},

_doUserCallBack: function(iType, sNameCtr, sKey, sVal, pFunc) {
    if (dnn && dnn.xmlhttp)
    {
        var sPack = iType + COL_DELIMITER + sNameCtr + COL_DELIMITER + sKey + COL_DELIMITER + sVal;
        dnn.xmlhttp.doCallBack('__Page',sPack,dnncore._callBackSuccess,pFunc,dnncore._callBackFail,null,true,null,0);
    }
    else
        alert('Client Personalization not enabled');
},

_callBackSuccess: function (result, ctx, req) {
    if (ctx.pFunc)
        ctx.pFunc(ctx.namingCtr, ctx.key, result);
},

_callBackFail: function (result, ctx) {
	window.status = result;
}
}

__dnncore.prototype.UserPropArgs = function(sNameCtr, sKey, pFunc)
{
    this.namingCtr = sNameCtr;
	this.key = sKey;
	this.pFunc = pFunc;
}

var dnncore = new __dnncore();