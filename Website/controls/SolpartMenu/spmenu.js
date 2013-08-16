//------------------------------------------------------//
// Solution Partner's ASP.NET Hierarchical Menu Control //
// Copyright (c) 2002-2005                              //
// Jon Henning - Solution Partner's Inc                 //  
// jhenning@solpart.com   -   http://www.solpart.com    //
// Compatible Menu Version:  <Min: 1400>             //
//                           <Max: 1.7.2.0>             //
// <Script Version: 1720>                               //
//------------------------------------------------------//
var m_oSolpartMenu;
if (m_oSolpartMenu == null)
	m_oSolpartMenu = new Array(); //stores all menu objects (SolpartMenu) in array 
var m_spm_sBrowser;
var m_spm_sVersion;
function spm_initMyMenu(oXML, oCtl)   //Creates SolpartMenu object and calls generate method
{

  m_oSolpartMenu[oCtl.id] = new SolpartMenu(oCtl);
  m_oSolpartMenu[oCtl.id].GenerateMenuHTML(oXML);
 
}
  
//------- Constructor -------//
function SolpartMenu(o)
{
__db(o.id + ' - constructor');
//  var me = this;  //allow attached events to reference this
  //--- Data Properties ---//
  this.systemImagesPath=spm_getAttr(o, 'SysImgPath', '');  
  this.iconImagesPath=spm_getAttr(o, 'IconImgPath', this.systemImagesPath);
  
  this.xml = spm_getAttr(o, 'XML', '');
  this.xmlFileName = spm_getAttr(o, 'XMLFileName', '');

  //--- Appearance Properties ---//
  this.fontStyle=spm_getAttr(o, 'FontStyle', 'font-family: arial;');
  this.backColor=spm_getAttr(o, 'BackColor');  
  this.foreColor=spm_getAttr(o, 'ForeColor');
  this.iconBackColor=spm_getAttr(o, 'IconBackColor');
  this.hlColor=spm_getAttr(o, 'HlColor', '');
  this.shColor=spm_getAttr(o, 'ShColor', ''); 
  this.selColor=spm_getAttr(o, 'SelColor');
  this.selForeColor=spm_getAttr(o, 'SelForeColor');
  this.selBorderColor=spm_getAttr(o, 'SelBorderColor');
  this.menuAlignment = spm_getAttr(o, 'MenuAlignment', 'Left');
  this.display=spm_getAttr(o, 'Display', 'horizontal');
  this.MBLeftHTML=spm_getAttr(o, 'MBLHTML', '');
  this.MBRightHTML=spm_getAttr(o, 'MBRHTML', '');

  this.rootArrow = spm_getAttr(o, 'RootArrow', '0');
  this.rootArrowImage = spm_getAttr(o, 'RootArrowImage', '');
  this.arrowImage = spm_getAttr(o, 'ArrowImage', '');
  this.backImage=spm_getAttr(o, 'BackImage', '');

	this.supportsTransitions = spm_getAttr(o, 'SupportsTrans', '0');

  //--- Transition Properteis ---//
  //this.menuEffectsStyle=spm_getAttr(o, 'MenuEffectsStyle', '');
  this.menuTransitionLength=spm_getAttr(o, 'MenuTransitionLength', .3);
  this.menuTransition=spm_getAttr(o, 'MenuTransition', 'None');
  this.menuTransitionStyle=spm_getAttr(o, 'MenuTransitionStyle', '');
  this.SolpartMenuTransitionObject = new SolpartMenuTransitionObject();
  
  //--- Behavior Properteis ---//
  this.moveable = spm_getAttr(o, 'Moveable', '0');
  this.moDisplay=spm_getAttr(o, 'MODisplay', 'HighLight');
  this.moExpand=spm_getAttr(o, 'MOExpand', "-1");
  this.moutDelay=spm_getAttr(o, 'MOutDelay', "0");
  this.minDelay=spm_getAttr(o, 'MInDelay', "0");
  this.minDelayType=null;
	this.minDelayTimer=null;
	this.minDelayObj=null;
	  
  if (spm_browserType() == 'safari')	//safari has issues with mouseoutdelay...
		this.moutDelay = 5000;
		
  this.target=spm_getAttr(o, 'target', "");
  this.moScroll=spm_getAttr(o, 'MOScroll', "-1");

  //--- Sizing Properties ---//
  this.menuBarHeight=spm_fixUnit(spm_getAttr(o, 'MenuBarHeight', '0'));
  this.menuItemHeight=spm_fixUnit(spm_getAttr(o, 'MenuItemHeight', '0'));
  this.iconWidth=spm_fixUnit(spm_getAttr(o, 'IconWidth', '0'));
  this.borderWidth=spm_getAttr(o, 'BorderWidth', '1');

  //--- CSS Properties ---//
  this.cssMenuContainer=spm_getAttr(o, 'CSSMenuContainer', '');
  this.cssMenuBar=spm_getAttr(o, 'CSSMenuBar', '');
  this.cssMenuItem=spm_getAttr(o, 'CSSMenuItem', '');
  this.cssMenuIcon=spm_getAttr(o, 'CSSMenuIcon', '');
  this.cssSubMenu=spm_getAttr(o, 'CSSSubMenu', '');
  this.cssMenuBreak=spm_getAttr(o, 'CSSMenuBreak', '');
  this.cssMenuItemSel=spm_getAttr(o, 'CSSMenuItemSel', '');
  this.cssMenuArrow=spm_getAttr(o, 'CSSMenuArrow', '');
  this.cssMenuRootArrow=spm_getAttr(o, 'CSSRootMenuArw', '');
  this.cssMenuScrollItem=spm_getAttr(o, 'CSSScrollItem', '');

	//for right to left (rtl) menus
	this.direction = spm_getCurrentStyle(document.body, 'direction');

	this.useIFrames=(spm_getAttr(o, 'useIFrames', '1') != '0' && spm_supportsIFrameTrick());	
	
	this.delaySubmenuLoad=(spm_getAttr(o, 'delaySubmenuLoad', '0') != '0' && spm_needsSubMenuDelay());	
	
  
  //---- methods ---//
  //this.GenerateMenuHTML=__GenerateMenuHTML;

  //----- private ----//
  this._m_sNSpace = o.id;               //stores namespace for menu
  this._m_sOuterTables = '';            //stores HTML for sub menus
  this._m_oDOM;                         //stores XML DOM object
	this._m_oMenu = o;                    //stores container
  this._m_oMenuMove;                    //stores control that is used for moving menu
  
  this._m_oTblMenuBar;                  //stores menu container
	this._m_aOpenMenuID = new Array();	  //stores list of menus that are currently displayed
	this._m_bMoving=false;                //flag to determine menu is being dragged
  this._m_dHideTimer = null;            //used to time when mouse out occured to auto hide menu based on mouseoutdelay
  this._m_oScrollingMenu = null;				//used in scrolling menu on mouse over

__db(this._m_oMenu.id + ' - constructor end');

}

//--- Destroys interrnal object references ---//
SolpartMenu.prototype.destroy = function ()
{
  this.systemImagesPath = null;  
  this.iconImagesPath = null;
  this.xml = null;
  this.xmlFileName = null;

  //--- Appearance Properties ---//
  this.fontStyle = null;
  this.backColor = null;  
  this.foreColor = null;
  this.iconBackColor = null;
  this.hlColor = null;
  this.shColor = null; 
  this.selColor = null;
  this.selForeColor = null;
  this.selBorderColor = null;
  this.menuAlignment = null;
  this.display = null;

  this.rootArrow = null;
  this.rootArrowImage = null;
  this.arrowImage = null;
  this.backImage = null;

  //--- Transition Properteis ---//
  //this.menuEffectsStyle = null;
  this.menuTransitionLength = null;
  this.menuTransition = null;
  this.SolpartMenuTransitionObject = null;
  
  //--- Behavior Properteis ---//
  this.moveable = null;
  this.moDisplay = null;
  this.moExpand = null;
  this.moutDelay = null;

  //--- Sizing Properties ---//
  this.menuBarHeight = null;
  this.menuItemHeight = null;
  this.iconWidth = null;
  this.borderWidth = null;

  //--- CSS Properties ---//
  this.cssMenuContainer = null;
  this.cssMenuBar = null;
  this.cssMenuItem = null;
  this.cssMenuIcon = null;
  this.cssSubMenu = null;
  this.cssMenuBreak = null;
  this.cssMenuItemSel = null;
  this.cssMenuArrow = null;
  this.cssMenuRootArrow = null;
  
  //---- methods ---//

  //----- private ----//
  m_oSolpartMenu[this._m_sNSpace] = null;

  this._m_sNSpace = null;                 //stores namespace for menu
  this._m_sOuterTables = null;            //stores HTML for sub menus
  this._m_oDOM = null;                    //stores XML DOM object
	this._m_oMenu = null;                   //stores container
  this._m_oMenuMove = null;               //stores control that is used for moving menu
  
  this._m_oTblMenuBar = null;             //stores menu container
	this._m_aOpenMenuID = null;	            //stores list of menus that are currently displayed
	this._m_bMoving = null;                 //flag to determine menu is being dragged
  this._m_dHideTimer = null;              //used to time when mouse out occured to auto hide menu based on mouseoutdelay
  this._m_oScrollingMenu = null;					//used in scrolling menu on mouse over
  
}

//--- xml document loaded (non-dataisland) ---//
SolpartMenu.prototype.onXMLLoad = function ()
{
  this.GenerateMenuHTML(this._m_oDOM);
}

//--- Generates menu HTML through passed in XML DOM ---//
SolpartMenu.prototype.GenerateMenuHTML = function (oXML) 
{
__db(this._m_oMenu.id + ' - GenerateMenuHTML');
    //'Generates the main menu bar
  var sHTML = '';
  this._m_sOuterTables = '';

	if (oXML == null)
	{
	  if (this._m_oDOM == null)
	  {
	    oXML = spm_createDOMDoc();
	    this._m_oDOM = oXML;
        	  
	    if (this.xml.length)
	      oXML.loadXML(this.xml);
  	  
	    if (this.xmlFileName.length)
	    {
	      oXML.onload = eval('onxmlload' + this._m_sNSpace); 
	      oXML.load(this.xmlFileName);
	      return; //async load
	    }
    }
	}
	else
	  this._m_oDOM = oXML;

  if (this.display == "vertical")
  {
      sHTML += '<table ID="tbl' + this._m_sNSpace + 'MenuBar" CELLPADDING=\'0\' CELLSPACING=\'0\' BORDER="0" CLASS="' + spm_fixCSSForMac(this.getIntCSSName('spmbctr') + this.cssMenuContainer) + '" HEIGHT="100%" STYLE="vertical-align: middle;">\n';	//removed position: relative;  for IE and display: block; for Opera
      sHTML += MyIIf(this.MBLeftHTML.length, '<tr>\n       <td>' + this.MBLeftHTML + '</td>\n</tr>\n', '');
      sHTML += MyIIf(Number(this.moveable), '<tr>\n       <td ID="td' + this._m_sNSpace + 'MenuMove" height=\'3px\' style=\'cursor: move; ' + spm_getMenuBorderStyle(this) + '\'>' + spm_getSpacer(this) + '</td>\n</tr>\n', '');
      sHTML +=         this.GetMenuItems(this._m_oDOM.documentElement);
      sHTML += '       <tr><td HEIGHT="100%">' + spm_getSpacer(this) + '</td>\n' ;
      sHTML += '   </tr>\n';
      sHTML += MyIIf(this.MBRightHTML.length, '<tr>\n       <td>' + this.MBRightHTML + '</td>\n</tr>\n', '');
      sHTML += '</table>\n';
  }
  else
  {
      sHTML += '<table ID="tbl' + this._m_sNSpace + 'MenuBar" CELLPADDING=\'0\' CELLSPACING=\'0\' BORDER="0" CLASS="' + spm_fixCSSForMac(this.getIntCSSName('spmbctr') + this.cssMenuContainer) + '" WIDTH="100%" STYLE="vertical-align: middle; ">\n';	//removed position: relative;  for IE and display: block; for Opera
      sHTML += '	<tr>\n';
      sHTML += MyIIf(this.MBLeftHTML.length, '<td>' + this.MBLeftHTML + '</td>\n', '');
      sHTML += MyIIf(Number(this.moveable), '       <td ID="td' + this._m_sNSpace + 'MenuMove" width=\'3px\' style=\'cursor: move; ' + spm_getMenuBorderStyle(this) + '\'>' + spm_getSpacer(this) + '</td>\n', '');
      sHTML += spm_getMenuSpacingImage('left', this);
      sHTML +=         this.GetMenuItems(this._m_oDOM.documentElement);
      sHTML += spm_getMenuSpacingImage('right', this);
      sHTML += MyIIf(this.MBRightHTML.length, '<td>' + this.MBRightHTML + '</td>\n', '');
      sHTML += '   </tr>\n';
      sHTML += '</table>\n';
  }
	
	this._m_oMenu.innerHTML = sHTML;

	this.GenerateSubMenus();

	
  this._m_oMenuMove = spm_getById('td' + this._m_sNSpace + 'MenuMove');

  spm_getTags("BODY")[0].onclick = spm_appendFunction(spm_getTags("BODY")[0].onclick, 'm_oSolpartMenu["' + this._m_sNSpace + '"].bodyclick();'); //document.body.onclick = this.bodyclick;

  this._m_oTblMenuBar = spm_getById('tbl' + this._m_sNSpace + 'MenuBar'); 
  
  this.fireEvent('onMenuComplete');

__db(this._m_oMenu.id + ' - GenerateMenuHTML end');    
}

SolpartMenu.prototype.GenerateSubMenus = function (oXML) 
{
	if (this._m_sOuterTables.length > 0)
	{
			var oDiv = spm_getById(this._m_sNSpace + '_divOuterTables');
			if (oDiv == null)
			{
				alert('It appears that your menu dll is out of sync with your script file.');
				return;
			}
			
			if (this.delaySubmenuLoad != '0' && document.readyState != 'complete')
				return;
							
			oDiv.innerHTML = this._m_sOuterTables;
			
	}
	this._m_sOuterTables = '';
}

function spm_getMenuBarEvents(sCtl)
{
  return 'onmouseover="m_oSolpartMenu[\'' + sCtl + '\'].onMBMO(this);" onmouseout="m_oSolpartMenu[\'' + sCtl + '\'].onMBMOUT(this);" onclick="m_oSolpartMenu[\'' + sCtl + '\'].onMBC(this, event);" onmousedown="m_oSolpartMenu[\'' + sCtl + '\'].onMBMD(this);" onmouseup="m_oSolpartMenu[\'' + sCtl + '\'].onMBMU(this);"';
}

function spm_getMenuItemEvents(sCtl)
{
  return 'onmouseover="m_oSolpartMenu[\'' + sCtl + '\'].onMBIMO(this);" onmouseout="m_oSolpartMenu[\'' + sCtl + '\'].onMBIMOUT(this);" onclick="m_oSolpartMenu[\'' + sCtl + '\'].onMBIC(this, event);"';
}

//--- Returns HTML for menu items (recursive function) ---//
SolpartMenu.prototype.GetMenuItems = function (oParent)
{
  var oNode;
  var sHTML = '';
  var sID;
  var sParentID;
  var sClickAction;
  
	for (var i = 0; i < oParent.childNodes.length; i++)
	{
		oNode = oParent.childNodes[i];

		if (oNode.nodeType != 3 && oNode.nodeType != 8)  //exclude nodeType of Text (Netscape/Mozilla) issue!
		{
		  //'determine if root level item and set parent id accordingly
		  if (oNode.parentNode.nodeName != "menuitem")
			  sParentID = "-1";
		  else
			  sParentID = oNode.parentNode.getAttribute("id");

		  if (oNode.nodeName == "menuitem")
			  sID = oNode.getAttribute("id");
		  else
			  sID = "";


  __db(sID + ' getmenuitems');
			sClickAction = spm_getMenuClickAction(oNode, this);


		  if (sParentID == "-1")	//'if top level menu item
		  {
		
			  if (this.display == "vertical")
				  sHTML += "<tr>\n"; //'if vertical display then add rows for each top menuitem
  			
  			if (oNode.nodeName == 'menubreak')
  			{
					if (this.display == "vertical")
						sHTML += "<tr>\n"; //'if vertical display then add rows for each top menuitem

 					var sBreakHTML = spm_getAttr(oNode, 'lefthtml', '') + spm_getAttr(oNode, 'righthtml', '');
 					if (sBreakHTML.length > 0)
 						sHTML += '   <td class="' + spm_fixCSSForMac(this.getIntCSSName('spmbrk') + this.cssMenuBreak) + '">' + sBreakHTML + '</td>\n';
 					else
 						sHTML += '   <td class="' + spm_fixCSSForMac(this.getIntCSSName('spmbrk') + this.cssMenuBreak) + '">' + spm_getMenuImage('spacer.gif', this, true, ' ') + '</td>\n';

					if (this.display == "vertical")
						sHTML += "</tr>\n";
  			}
  			else
  			{
					sHTML += '<td>\n<table width="100%" CELLPADDING="0" CELLSPACING="0" border="0">\n<tr id="td' + this._m_sNSpace + sID + '" ' + spm_getMenuBarEvents(this._m_sNSpace) + '  class="' + spm_fixCSSForMac(this.getIntCSSName('spmbar spmitm') + this.cssMenuBar + ' ' + this.cssMenuItem + ' ' + spm_getMenuItemCSS(oNode)) + '" savecss="' + spm_getMenuItemCSS(oNode) + '" saveselcss="' + spm_getMenuItemSelCSS(oNode) + '" menuclick="' + sClickAction + '" style="' + spm_getMenuItemStyle('item', oNode) + '">\n';
					var sAlign = this.display=='vertical' ? 'align="' + this.menuAlignment + '"' : '';
					sHTML += '<td unselectable="on" NOWRAP="NOWRAP" ' + sAlign + ' TITLE="' + spm_getAttr(oNode, 'tooltip', '') + '">' + spm_getImage(oNode, this) + spm_getItemHTML(oNode, 'left', '&nbsp;') + spm_getAttr(oNode, 'title', '') + spm_getItemHTML(oNode, 'right') + MyIIf(Number(this.rootArrow) && spm_nodeHasChildren(oNode), '</td>\n<td align="right" class="' + spm_fixCSSForMac(this.getIntCSSName('spmrarw') + this.cssMenuRootArrow) + '">' + spm_getArrow(this.rootArrowImage, this) + "", '&nbsp;') + '\n</td>\n</tr>\n</table>\n</td>\n';
				}
				  	    
			  if (this.display == "vertical")
				  sHTML += "</tr>\n";
		  
		 
		  }
		  else                        //'submenu - not top level menu item
		  {
			  switch(oNode.nodeName)
			  {
				  case "menuitem":
				  {
					  sHTML +=		'   <tr ID="tr' + this._m_sNSpace + sID + '" ' + spm_getMenuItemEvents(this._m_sNSpace) + ' parentID="' + sParentID + '" class="' + spm_fixCSSForMac(this.getIntCSSName('spmitm') + this.cssMenuItem + ' ' + spm_getMenuItemCSS(oNode)) + '" savecss="' + spm_getMenuItemCSS(oNode) + '" saveselcss="' + spm_getMenuItemSelCSS(oNode) + '" menuclick="' + sClickAction + '" style="' + spm_getMenuItemStyle('item', oNode) + '">\n';
					  sHTML +=		'       <td unselectable="on" id="icon' + this._m_sNSpace + sID + '" class="' + spm_fixCSSForMac(this.getIntCSSName('spmicn') + this.cssMenuIcon) + '" style="' + spm_getMenuItemStyle('image', oNode) + '; ' + spm_getMenuItemStyle('item', oNode) + '">' + spm_getImage(oNode, this) + '</td>\n';
					  sHTML +=		'       <td unselectable="on" id="td' + this._m_sNSpace + sID + '" class="' + spm_fixCSSForMac(this.getIntCSSName('spmitm') + this.cssMenuItem + ' ' + spm_getMenuItemCSS(oNode)) + '" savecss="' + spm_getMenuItemCSS(oNode) + '" NOWRAP="NOWRAP" TITLE="' + spm_getAttr(oNode, 'tooltip', '') + '" style="' + spm_getMenuItemStyle('item', oNode) + '">' + spm_getItemHTML(oNode, 'left', '') + spm_getAttr(oNode, 'title', '') + spm_getItemHTML(oNode, 'right', '') + '</td>\n';
					  sHTML +=		'       <td unselectable="on" id="arrow' + this._m_sNSpace + sID + '" width="15px" CLASS="' + spm_fixCSSForMac(this.getIntCSSName('spmarw') + this.cssMenuArrow) + '" style="' + spm_getMenuItemStyle('item', oNode) + '">' + MyIIf(spm_nodeHasChildren(oNode), spm_getArrow(this.arrowImage, this), spm_getSpacer(this)) + '</td>\n';
					  sHTML +=		'   </tr>\n';

					  break;
				  }
				  case "menubreak":
				  {
 						var sBreakHTML = spm_getAttr(oNode, 'lefthtml', '') + spm_getAttr(oNode, 'righthtml', '');
 						if (sBreakHTML.length > 0)
							sHTML += '   <tr><td colspan="3" class="' + spm_fixCSSForMac(this.getIntCSSName('spmbrk') + this.cssMenuBreak) + '">' + sBreakHTML + '</td>\n</tr>\n';
 						else
							sHTML += '   <tr>\n<td style="height: 1px" class="' + spm_fixCSSForMac(this.getIntCSSName('spmicn') + this.cssMenuIcon) + '">' + spm_getMenuImage('spacer.gif', this, true, ' ') + '</td>\n<td colspan="2" class="' + spm_fixCSSForMac(this.getIntCSSName('spmbrk') + this.cssMenuBreak) + '">' + spm_getMenuImage('spacer.gif', this, true, ' ') + '</td>\n</tr>\n';

					  break;
				  }
			  }
		  }

		  //'Generate sub menu - note: we are recursively calling ourself
		  //'netscape renders tables with display: block as having cellpadding!!! therefore using div outside table - LAME!
		  if (oNode.childNodes.length > 0)
		  {
				var sTag = 'DIV';
				var sStyle = '';

				if (spm_isMac('ie'))
				{
					sTag = 'P';
					sStyle = 'margin-top:0px; margin-left:0px;'
				}
			  this._m_sOuterTables = '\n<' + sTag + ' ID="tbl' + this._m_sNSpace + sID + '" CLASS="' + spm_fixCSSForMac(this.getIntCSSName('spmsub') + this.cssSubMenu) + '" STYLE="display:none; position: absolute;' + sStyle + this.menuTransitionStyle + '">\n<table CELLPADDING="0" CELLSPACING="0" BORDER="0">\n' + this.GetMenuItems(oNode) + '\n</table>\n</' + sTag + '>\n' + this._m_sOuterTables;
			}

    }
	}
	return sHTML;
}

	//--------------- Event Functions ---------------//
  //--- menubar click event ---//
	SolpartMenu.prototype.onMBC = function (e, evt)
	{
		this.GenerateSubMenus();

		var oCell = e; //event.srcElement;
		var sID = oCell.id.substr(2);

		var oMenu = spm_getById("tbl" + sID);
		
		if (oMenu != null)
		{
			this.hideAllMenus();		//mindelay mod
			if (oMenu.style.display == '')
			{
				this.hideAllMenus();		
				if (this.useIFrames)
					spm_iFrameIndex(oMenu, false, this.systemImagesPath);
				else
					spm_showElement("SELECT|OBJECT");
			}
			else
			{
				spm_positionMenu(this, oMenu, oCell);
				
				this.doTransition(oMenu);
				oMenu.style.display = "";
				this._m_aOpenMenuID[0] = sID;
				if (this.useIFrames)
					spm_iFrameIndex(oMenu, true, this.systemImagesPath);
				else
					spm_hideElement("SELECT|OBJECT",oMenu);
			}
		}
		
    this.fireEvent('onMenuBarClick', oCell);
    
    oMenu = spm_getById("td" + sID);
    if (spm_getAttr(oMenu, "menuclick", '').length)
    {
      eval(spm_getAttr(oMenu, "menuclick", ''));
      this.hideAllMenus();
    }
		spm_stopEventBubbling(evt);
	}
	
  //--- menubar mousedown event ---//
	SolpartMenu.prototype.onMBMD = function (e)
	{
		var oCell = e; //event.srcElement;
		this.applyBorder(oCell, 1, this.shColor, this.hlColor);
	}
  
  //--- menubar mouseup event ---//
	SolpartMenu.prototype.onMBMU = function (e)
	{
		var oCell = e; //event.srcElement;
		this.applyBorder(oCell, 1, this.hlColor, this.shColor);
	}
  
  //--- menubar mouseover event ---//
	SolpartMenu.prototype.onMBMO = function (e, bBypassDelay)
	{
		this.GenerateSubMenus();
		var oCell = e; //event.srcElement;
		
		if (oCell.id.length == 0) //cancelBubble
		  return;
		var sID = oCell.id.substr(2);
		var oMenu = spm_getById("tbl" + sID);

		if (this._m_aOpenMenuID.length || this.moExpand != '0')
		{
			if (this.minDelay != 0 && bBypassDelay != true)
			{
				if (this.minDelayTimer != null)
					window.clearTimeout(this.minDelayTimer);
				this.minDelayType = 'root';
				this.minDelayObj = e;
				this.minDelayTimer = setTimeout('m_oSolpartMenu["' + this._m_sNSpace + '"].mouseInDelayHandler()', this.minDelay);
			}
			else
			{
				//--- if menu is shown then mouseover triggers the showing of all menus ---//
				this.hideAllMenus();

				if (oMenu != null)
				{
					spm_positionMenu(this, oMenu, oCell);
					this.doTransition(oMenu);
					oMenu.style.display = "";
					this._m_aOpenMenuID[0] = sID;
					if (this.useIFrames)
						spm_iFrameIndex(oMenu, true, this.systemImagesPath);
					else
						spm_hideElement("SELECT|OBJECT",oMenu);
				}
			}
			this.applyBorder(oCell, 1, this.shColor, this.hlColor);
		}
		else
		{
			this.applyBorder(oCell, 1, this.hlColor, this.shColor);
		}

		oCell.className =  spm_fixCSSForMac(this.getIntCSSName('spmitmsel spmbar') + this.cssMenuBar + ' ' + this.cssMenuItemSel + ' ' + spm_getAttr(oCell, 'saveselcss', '') + ' ' + spm_getAttr(oCell, 'savecss', ''));
		
		this._m_dHideTimer = null;
		
		this.fireEvent('onMenuBarMouseOver', oCell);
		
	}
  //--- menubar mouseout event ---//
	SolpartMenu.prototype.onMBMOUT = function (e)
	{
		var oCell = e; //event.srcElement;
		var sID = oCell.id.substr(2);
		this.applyBorder(oCell, 1, spm_getCellBackColor(oCell), spm_getCellBackColor(oCell), "none");	
		this._m_dHideTimer = new Date();

		if (this.moutDelay != 0)
		  setTimeout('m_oSolpartMenu["' + this._m_sNSpace + '"].hideMenuTime()', this.moutDelay);
		  
    oCell.className = spm_fixCSSForMac(this.getIntCSSName('spmbar spmitm') + this.cssMenuBar + ' ' + this.cssMenuItem + ' ' + spm_getAttr(e, 'savecss', ''));
    this.stopTransition();
    this.minDelayType = null;
    this.fireEvent('onMenuBarMouseOut', oCell);
	}
	
  //--- menuitem click ---//
	SolpartMenu.prototype.onMBIC = function (e, evt)
	{
		var oRow = spm_getSourceTR(e, this._m_sNSpace);  //event.srcElement
		var sID = oRow.id.substr(2);
		if (spm_itemHasChildren(sID, this._m_sNSpace) == false)
			this.hideAllMenus();

		this.fireEvent('onMenuItemClick', oRow);

    if (spm_getAttr(oRow, "menuclick", '').length)
    {
      eval(spm_getAttr(oRow, "menuclick", ''));
      this.hideAllMenus();
		}
		spm_stopEventBubbling(evt);
		
		this.handlembi_mo(oRow, true);
	}

  //--- menuitem mouseover event ---//
	SolpartMenu.prototype.onMBIMO = function (e)
	{		
		this.handlembi_mo(spm_getSourceTR(e, this._m_sNSpace)); //event.srcElement

		this._m_dHideTimer = null;
	}
  //--- menuitem mouseout event ---//
	SolpartMenu.prototype.onMBIMOUT = function (e)
	{	
		this.handlembi_mout(spm_getSourceTR(e, this._m_sNSpace));  //event.srcElement
		this._m_dHideTimer = new Date;
		//setTimeout(this.hideMenuTime, this.moutDelay);
		if (this.moutDelay != 0)
		  setTimeout('m_oSolpartMenu["' + this._m_sNSpace + '"].hideMenuTime()', this.moutDelay);
		 
		this.minDelayType = null;
	}

	SolpartMenu.prototype.bodyclick = function()
	{
		this.hideAllMenus();
	}

  //--- handles display of newly opened menu ---//
	SolpartMenu.prototype.handleNewItemSelect = function (sID)
	{
		var i;
		var iNewLength=-1;
		var bDeleteRest=false; 
		for (i=0; i<this._m_aOpenMenuID.length; i++)
		{		
			if (bDeleteRest)
			{
				spm_getById("tbl" + this._m_aOpenMenuID[i]).style.display = "none";
				if (this.useIFrames)
					spm_iFrameIndex(spm_getById("tbl" + this._m_aOpenMenuID[i]), false, this.systemImagesPath);
			}
			if (this._m_aOpenMenuID[i] == this._m_sNSpace + sID)
			{
				bDeleteRest=true;
				iNewLength = i;
			}				
		}
		if (iNewLength != -1)
			this._m_aOpenMenuID.length = iNewLength+1;
	}
	
  //--- hides all menus that are currently displayed ---//
	SolpartMenu.prototype.hideAllMenus = function ()
	{
		var i;
		var oMenu;
		for (i=0; i<this._m_aOpenMenuID.length; i++)
		{		
			oMenu = spm_getById("tbl" + this._m_aOpenMenuID[i]);
			oMenu.style.display = "none";

			if (this.useIFrames)
				spm_iFrameIndex(oMenu, false, this.systemImagesPath);
		}
		if (this.useIFrames != true)
			spm_showElement("SELECT|OBJECT");

		this._m_aOpenMenuID.length = 0;
	}		
  
  
  function SolpartMenuTransitionObject()
  {
    this.id=null;
    this.stop = false;
  } 

  //--- stops menu transition effect ---//
  SolpartMenu.prototype.stopTransition = function ()
  {
    this.SolpartMenuTransitionObject.stop = true;
    this.doFilter();
    this.SolpartMenuTransitionObject = new SolpartMenuTransitionObject();
  }
  
  //--- starts menu transition effect ---//
  SolpartMenu.prototype.doTransition = function (oMenu)
  {
    if (this.menuTransition == 'None' || this.supportsTransitions == '0')
      return;

    var sID = this.SolpartMenuTransitionObject.id;
    
    switch (this.menuTransition)
    {
      case 'AlphaFade':
      {
        if (this.SolpartMenuTransitionObject.id != oMenu.id) 
        {
          this.SolpartMenuTransitionObject.id = oMenu.id;
          this.SolpartMenuTransitionObject.opacity = 0;
          this.doFilter();
        }
        break;
      }
      case 'Wave':
      {
        if (this.SolpartMenuTransitionObject.id != oMenu.id) 
        {        
          this.SolpartMenuTransitionObject.id = oMenu.id;
          this.SolpartMenuTransitionObject.phase = 0;
          this.doFilter();
        }
        break;
      }
      case 'ConstantWave':
      {
        if (sID != oMenu.id) 
        {        
          this.SolpartMenuTransitionObject.id = oMenu.id;
          this.SolpartMenuTransitionObject.phase = 0;
          this.SolpartMenuTransitionObject.constant=true;
          this.doFilter();
        }
        break;
      }
      case 'Inset': case 'RadialWipe': case 'Slide': case 'Spiral': case 'Stretch': case 'Strips': case 'Wheel': case 'GradientWipe': case 'Zigzag': case 'Barn': case 'Blinds': case 'Checkerboard': case 'Fade': case 'Iris': case 'RandomBars':
      {
        oMenu.filters('DXImageTransform.Microsoft.' + this.menuTransition).apply();
        oMenu.filters('DXImageTransform.Microsoft.' + this.menuTransition).duration = this.menuTransitionLength;
        oMenu.filters('DXImageTransform.Microsoft.' + this.menuTransition).play();
        break;
      }
    }
  }

  //--- applys transition filter ---//
  SolpartMenu.prototype.doFilter = function (bStop) 
  {      
    if (this.SolpartMenuTransitionObject.id == null)
      return;
      
    var o = spm_getById(this.SolpartMenuTransitionObject.id);
    window.status = new Date();
    switch (this.menuTransition)
    {
      case 'AlphaFade':
      {
        if (this.SolpartMenuTransitionObject.stop)
        {
          o.filters('DXImageTransform.Microsoft.Alpha').opacity = 100;
        }
        else
        {
          o.filters('DXImageTransform.Microsoft.Alpha').opacity = this.SolpartMenuTransitionObject.opacity;
          if (this.SolpartMenuTransitionObject.opacity < 100)
          {
            setTimeout('m_oSolpartMenu["' + this._m_sNSpace + '"].doFilter()', 50);
            this.SolpartMenuTransitionObject.opacity += (100/20* this.menuTransitionLength);
          }
        }
        break;
      }
      case 'Wave': case 'ConstantWave':
      {
        if (this.SolpartMenuTransitionObject.stop)
        {
            o.filters("DXImageTransform.Microsoft.Wave").freq = 0;
            o.filters("DXImageTransform.Microsoft.Wave").lightstrength = 0;
            o.filters("DXImageTransform.Microsoft.Wave").strength = 0;
            o.filters("DXImageTransform.Microsoft.Wave").phase = 0;
        }
        else
        {
          o.filters("DXImageTransform.Microsoft.Wave").freq = 1;
          o.filters("DXImageTransform.Microsoft.Wave").lightstrength = 20;
          o.filters("DXImageTransform.Microsoft.Wave").strength = 5;
          o.filters("DXImageTransform.Microsoft.Wave").phase = this.SolpartMenuTransitionObject.phase;

          if (this.SolpartMenuTransitionObject.phase < 100 * this.menuTransitionLength || this.SolpartMenuTransitionObject.constant == true)
          {
            setTimeout('m_oSolpartMenu["' + this._m_sNSpace + '"].doFilter()', 50);
            this.SolpartMenuTransitionObject.phase += 5;
          }
          else
          {
            o.filters("DXImageTransform.Microsoft.Wave").freq = 0;
            o.filters("DXImageTransform.Microsoft.Wave").lightstrength = 0;
            o.filters("DXImageTransform.Microsoft.Wave").strength = 0;
            o.filters("DXImageTransform.Microsoft.Wave").phase = 0;
          }
        }
        break;
      }
    }
  }          

  //--- handles mouseover for menu item ---//
	SolpartMenu.prototype.handlembi_mo = function (oRow, bBypassDelay)
	{
		var sID = oRow.id.substr(2);

		spm_getById("icon" + sID).className = spm_fixCSSForMac(this.getIntCSSName('spmitmsel spmicn') + this.cssMenuIcon + ' ' + this.cssMenuItemSel + ' ' + spm_getAttr(oRow, 'saveselcss', ''));
		spm_getById("td" + sID).className = spm_fixCSSForMac(this.getIntCSSName('spmitmsel') + this.cssMenuItemSel + ' ' + spm_getAttr(oRow, 'saveselcss', ''));
		spm_getById("arrow" + sID).className = spm_fixCSSForMac(this.getIntCSSName('spmitmsel spmarw') + this.cssMenuItemSel + ' ' + this.cssMenuArrow + ' ' + spm_getAttr(oRow, 'saveselcss', ''));
		
		if (this.selBorderColor != '')
			spm_applyRowBorder(oRow, 1, this.selBorderColor, true);

		if (this.minDelay != 0 && bBypassDelay != true)
		{
			if (this.minDelayTimer != null)
				window.clearTimeout(this.minDelayTimer);
			this.minDelayType = 'sub';
			this.minDelayObj = oRow;
			this.minDelayTimer = setTimeout('m_oSolpartMenu["' + this._m_sNSpace + '"].mouseInDelayHandler()', this.minDelay);
			return;
		}
	
		if (this._m_aOpenMenuID[this._m_aOpenMenuID.length - 1] != oRow.id.replace('tr', ''))
		{
			this.handleNewItemSelect(spm_getAttr(oRow, "parentID", ""));
		
			if (spm_getById("tbl" + sID) != null)
			{
				var iWidth;
				oMenu = spm_getById("tbl" + sID);

				var oPDims = new spm_elementDims(oRow);
				var oMDims = new spm_elementDims(oMenu);
				        			
				oMenu.style.top = spm_getCoord(oPDims.t);
				
				spm_resetScroll(oMenu);

				this.doTransition(oMenu);

				oMDims = new spm_elementDims(oMenu);	//now that we moved need to reget dims
				oMenu.style.display = "";

			  if (oMDims.t - spm_getBodyScrollTop() + oMDims.h > spm_getViewPortHeight())
			  {
				  if (oMDims.h < spm_getViewPortHeight())
						oMenu.style.top = spm_getCoord(spm_getViewPortHeight() + spm_getBodyScrollTop() - oMDims.h);
					else
					{
						spm_handleScrollMenu(this, oMenu);
						
						oMDims = new spm_elementDims(oMenu);	//now that we moved need to reget dims
					}
			  }

				if (this.direction == 'rtl')
					oMenu.style.left = spm_getCoord(oPDims.l - oMDims.w - spm_getBodyScrollLeft());
				else
					oMenu.style.left = spm_getCoord(oPDims.l + oPDims.w - spm_getBodyScrollLeft());

				if (this.direction == 'rtl')
				{
					if (oMDims.l - spm_getBodyScrollLeft() < 0)
						oMenu.style.left = spm_getCoord(oPDims.l + oPDims.w - spm_getBodyScrollLeft());
				}
				else  
				{
					if (oPDims.l - spm_getBodyScrollLeft() + oPDims.w + oMDims.w > spm_getViewPortWidth())
						oMenu.style.left = spm_getCoord(oPDims.l - oMDims.w - spm_getBodyScrollLeft());
				}
					
				this._m_aOpenMenuID[this._m_aOpenMenuID.length] = sID;
				if (this.useIFrames)
					spm_iFrameIndex(oMenu, true, this.systemImagesPath);
				else
					spm_hideElement("SELECT|OBJECT",oMenu);

			}
		}
		this.fireEvent('onMenuItemMouseOver', oRow);
		
	}
	
  //--- handles mouseout for menu item ---//
	SolpartMenu.prototype.handlembi_mout = function (oRow)
	{
			var sID = oRow.id.substr(2);

			oRow.className = spm_fixCSSForMac(this.getIntCSSName('spmitm') + ' ' + this.cssMenuItem + ' ' + spm_getAttr(oRow, 'savecss', ''));
		  spm_getById("icon" + sID).className = spm_fixCSSForMac(this.getIntCSSName('spmicn') + this.cssMenuIcon);
		  spm_getById("td" + sID).className = spm_fixCSSForMac(this.getIntCSSName('spmitm') + ' ' + this.cssMenuItem + ' ' + spm_getAttr(oRow, 'savecss', ''));
		  spm_getById("arrow" + sID).className = spm_fixCSSForMac(this.getIntCSSName('spmarw') + this.cssMenuArrow);
			
			if (this.selBorderColor != '')
				spm_applyRowBorder(oRow, 1, "", false);

      this.stopTransition();
	}

  //used for raising events to client javascript
  SolpartMenu.prototype.fireEvent = function (sEvent, src) 
  {
		return; //disabled for now
    if (eval('this.' + sEvent + ' != null'))
		{
			var e = new Object();
			if (src != null)
				e.srcElement = src;
			else
				e.srcElement = this._m_oMenu;
				
				eval('this.' + sEvent + '(e)');
		}
  }

	//--- called by setTimeOut to check mouseout hide delay ---//
	SolpartMenu.prototype.hideMenuTime = function ()
  {
    if (this._m_dHideTimer != null && this.moutDelay > 0)
    {
      if (new Date() - this._m_dHideTimer >= this.moutDelay)
      {
        this.hideAllMenus();
        this._m_dHideTimer = null;
      }
      else
        setTimeout(this.hideMenuTime, this.moutDelay);
    }
  }

	SolpartMenu.prototype.mouseInDelayHandler = function ()
	{
		if (this.minDelayType == 'root')
			this.onMBMO(this.minDelayObj, true);
		else if (this.minDelayType == 'sub')
			this.handlembi_mo(this.minDelayObj, true);
		this.minDelayTimer = null;
		this.minDelayObj = null;
	}

	//--- called by setTimeOut to check mouseout hide delay ---//
	SolpartMenu.prototype.scrollMenu = function ()
  {
		if (this._m_oScrollingMenu != null)
		{
			if (spm_ScrollMenuClick(this._m_oScrollingMenu) == false)
				setTimeout('m_oSolpartMenu["' + this._m_sNSpace + '"].scrollMenu()', 500);
			else
				this._m_oScrollingMenu = null;
		}
  }

//global
	function spm_iFrameIndex(eMenu, bShow, sysImgPath)
	{
		if (spm_browserType() == 'op')
			return;	//not needed
		
		if (document.readyState != 'complete')
			return;	//avoid operation aborted
		
		if (bShow)
		{
			var oIFR=spm_getById('ifr' + eMenu.id);
			if (oIFR == null)
			{
				var oIFR = document.createElement('iframe');
				oIFR.id = 'ifr' + eMenu.id;
				//oIFR.src = 'javascript: void(0);';
				oIFR.src = sysImgPath + 'spacer.gif';
				oIFR.style.top = spm_getCoord(0);
				oIFR.style.left = spm_getCoord(0);
				oIFR.style.filter = "progid:DXImageTransform.Microsoft.Alpha(opacity=0)";
				oIFR.scrolling = 'no';
				oIFR.frameBorder = 'no';
				oIFR.style.display = 'none';
				oIFR.style.position = 'absolute';
				document.body.appendChild(oIFR);
			}
			var oMDims = new spm_elementDims(eMenu);
			
			oIFR.style.width=oMDims.w;
			oIFR.style.height=oMDims.h;
			oIFR.style.top=spm_getCoord(oMDims.t);
			oIFR.style.left=spm_getCoord(oMDims.l);
			
			var iIndex = spm_getCurrentStyle(eMenu, 'zIndex');	//eMenu.style.zIndex;
			if (iIndex == null || iIndex == 0)
				eMenu.style.zIndex = 1;
			oIFR.style.zIndex=iIndex-1;
			oIFR.style.display="block";
		}
		else if (spm_getById('ifr' + eMenu.id) != null)
		{
			spm_getById('ifr' + eMenu.id).style.display='none';
		}
	}

	function spm_showElement(elmID)
	{
		if (spm_browserType() == 'op')
			return;	//not needed

		// Display any element that was hidden
		var sTags = elmID.split('|');
		for (var x=0; x<sTags.length; x++)
		{
			elmID = sTags[x];
			for (var i = 0; i < spm_getTags(elmID).length; i++)
			{
				obj = spm_getTags(elmID)[i];
				if (! obj || ! obj.offsetParent)
					continue;
				obj.style.visibility = "";
			}
		}
	}

	function spm_hideElement(elmID, eMenu)
	{
		if (spm_browserType() == 'op')
			return;	//not needed

		var obj;
		// Hide any element that overlaps with the dropdown menu
		var sTags = elmID.split('|');
		
		var oMDims = new spm_elementDims(eMenu);
		
		for (var x=0; x<sTags.length; x++)
		{
			elmID = sTags[x];
			for (var i = 0; i < spm_getTags(elmID).length; i++)
			{
				obj = spm_getTags(elmID)[i];
				var oODims = new spm_elementDims(obj);
				
				if (oODims.t > oMDims.t + oMDims.h)
				{
					//if element is below bottom of menu then do nothing
				}
				else if (oODims.l > oMDims.l + oMDims.w)
				{
					//if element is to the right of menu then do nothing
				}
				else if (oODims.l + oODims.w < oMDims.l)
				{
					//if element is to the left of menu then do nothing
				}
				else if (oODims.t + oODims.h < oMDims.t)
				{
					//if element is to the top of menu then do nothing
				}
				else
				{
					obj.style.visibility = "hidden";
				}
			}
		}
	}

	function spm_positionMenu(me, oMenu, oCell)
	{

		spm_resetScroll(oMenu);

		var oPDims = new spm_elementDims(oCell, false, me);
		
		if (me.display == 'vertical')
		{
			oMenu.style.top = spm_getCoord(oPDims.t);
			var oMDims = new spm_elementDims(oMenu);

			if (oMDims.t - spm_getBodyScrollTop() + oMDims.h >= spm_getViewPortHeight())
			{
				if (oMDims.h < spm_getViewPortHeight())
					oMenu.style.top = spm_getCoord(spm_getViewPortHeight() - oMDims.h + spm_getBodyScrollTop());	
				else
					spm_handleScrollMenu(me, oMenu);
			}
			
			var oOrigMDims;
			
			if (spm_browserType() != 'ie') //since mozilla doesn't set width greater than window size we need to store it here
				 oOrigMDims = new spm_elementDims(oMenu);
			
      if (me.direction == 'rtl')                
      {
          var oMDims2 = new spm_elementDims(oMenu);
          oMenu.style.left = spm_getCoord((oPDims.l) - oMDims2.w - spm_getBodyScrollLeft());
      }
      else
          oMenu.style.left = spm_getCoord(oPDims.l + oPDims.w - spm_getBodyScrollLeft());
					
			oMDims = new spm_elementDims(oMenu);
			if (oOrigMDims == null)
				oOrigMDims = oMDims;
			
			if (oMDims.l - spm_getBodyScrollLeft(true) + oOrigMDims.w > spm_getViewPortWidth())
			{
			  if (spm_getViewPortWidth() - oOrigMDims.w > 0)  //only do this if it fits
				  oMenu.style.left = spm_getCoord(oPDims.l - oOrigMDims.w - spm_getBodyScrollLeft(true));
			}

			//oMenu.style.display = "";
		}
		else
		{
			if (me.direction == 'rtl')			
			{
				var oMDims2 = new spm_elementDims(oMenu);
				oMenu.style.left = spm_getCoord((oPDims.l + oPDims.w) - oMDims2.w - spm_getBodyScrollLeft());
			}
			else			
				oMenu.style.left = spm_getCoord(oPDims.l - spm_getBodyScrollLeft());
				
			oMenu.style.top = spm_getCoord(oPDims.t + oPDims.h);
			var oMDims = new spm_elementDims(oMenu);
			
			if (oMDims.l - spm_getBodyScrollLeft(true) + oMDims.w > spm_getViewPortWidth())
			{
			  if (spm_getViewPortWidth() - oMDims.w > 0)  //only do this if it fits
				  oMenu.style.left = spm_getCoord(spm_getViewPortWidth() - oMDims.w + spm_getBodyScrollLeft(true));
			}
			
			if (oMDims.t - spm_getBodyScrollTop() + oMDims.h > spm_getViewPortHeight())
			{
			  if (oPDims.t - oMDims.h - spm_getBodyScrollTop() > 0) //only do this if it fits
				  oMenu.style.top = spm_getCoord(oPDims.t - oMDims.h);	//place above menu bar
				else
					spm_handleScrollMenu(me, oMenu);
			}
			//oMenu.style.display = "none";
		}
	}

	//--------- Internal (private) Functions --------//
	//--- Applies border to cell ---//
	SolpartMenu.prototype.applyBorder = function (oCell, iSize, sTopLeftColor, sBottomRightColor, sStyle)
	{
		if (this.moDisplay == 'Outset')
		{
			if (sStyle == null)
				sStyle = "solid";

			if (sTopLeftColor.length > 0 && sBottomRightColor.length > 0)
			{
				if (oCell.tagName == 'TR')
					oCell = oCell.childNodes(0);
				
				oCell.style.borderTop = sStyle + " " + iSize + "px " + sTopLeftColor;
				oCell.style.borderLeft = sStyle + " " + iSize + "px " + sTopLeftColor;
				oCell.style.borderRight = sStyle + " " + iSize + "px " + sBottomRightColor;
				oCell.style.borderBottom = sStyle + " " + iSize + "px " + sBottomRightColor;	
				
			}
		}
		if (this.moDisplay == 'HighLight')
		{
			if (sTopLeftColor == this.backColor)
			{
        oCell.className = spm_fixCSSForMac(this.getIntCSSName('spmbar spmitm') + ' ' + this.cssMenuItem + ' ' + spm_getAttr(oCell, 'savecss', ''));
			}
			else
			{
        oCell.className = spm_fixCSSForMac(this.getIntCSSName('spmbar spmitmsel') + ' ' + this.cssMenuItemSel + ' ' + spm_getAttr(oCell, 'saveselcss', ''));
			}
		}		
	}

	function spm_applyRowBorder(oRow, iSize, sColor, bSelected, sStyle)
	{
		if (oRow.cells.length == 0) //(spm_browserType() == 'safari')
			return;	//safari has issues with accessing cell
		
		var sColor2=sColor;
		if (sStyle == null)
			sStyle = "solid";

		if (sColor == "")
		{
				sColor2 = spm_getCurrentStyle(oRow.cells[0], 'background-Color');
				if ((sColor2 == null || sColor2 == '') && spm_browserType() != 'ie')
					sColor2 = 'transparent';
		}

		spm_applyBorders(oRow.cells[0], sStyle, iSize, sColor2, true, true, false, true);

		if (sColor == "" && bSelected == false)
    {
      sColor2 = spm_getCellBackColor(oRow.cells[1]);
      if (sColor2 == null || sColor2 == '')
				sColor2 = 'transparent';
    }
   
    //if (sColor2 != 'transparent')
    //{
			spm_applyBorders(oRow.cells[1], sStyle, iSize, sColor2, true, false, false, true);
			spm_applyBorders(oRow.cells[2], sStyle, iSize, sColor2, true, false, true, true);
		//}
	}
	
	function spm_getCellBackColor(o)
	{
		var sColor = spm_getCurrentStyle(o, 'background-Color');  
    if (spm_browserType() == 'ie')
    {
      //--- fix IE transparent border issue ---//
      while (sColor == 'transparent')
      {
        sColor = spm_getCurrentStyle(o, 'background-Color');  
        o = o.parentElement;
        if (o.id.indexOf('divOuterTables') != -1)	//if we are outside the realm of the menu then use transparency
					break;
      }
    }
    return sColor;
	}
	
	function spm_applyBorders(o, sStyle, iSize, sColor, t, l, r, b)
	{

		if (t) o.style.borderTop = sStyle + " " + iSize + "px " + sColor;
		if (b) o.style.borderBottom = sStyle + " " + iSize + "px " + sColor;
		if (r) o.style.borderRight = sStyle + " " + iSize + "px " + sColor;
		if (l) o.style.borderLeft = sStyle + " " + iSize + "px " + sColor;

	}

	function spm_resetScroll(oMenu)
	{
	
		if (oMenu.scrollItems != null)
		{
			oMenu.scrollPos = 1;
			oMenu.scrollItems = 9999;
			spm_showScrolledItems(oMenu);
		}	
	}
	
	
	function spm_handleScrollMenu(me, oMenu)
	{
		var oTbl = spm_getTags('table', oMenu)[0]; //oMenu.childNodes[1];	
		oMenu.style.display = '';
		if (oMenu.scrollPos == null)
		{
			oMenu.scrollPos = 1;			
						
			var oRow = spm_insertTableRow(oTbl);
			var oCell = document.createElement('TD');		
			oCell.id = 'dn' + oMenu.id.substring(3);
			oCell.colSpan = 3;
			oCell.align = 'center';
			oCell.style.backgroundColor = 'gray';	//can be overridden by MenuScroll style
			oCell.innerHTML='<div id="dn' + oMenu.id.substr(3) + '" onclick="return spm_ScrollMenuClick(this, event);" onmouseover="spm_ScrollMenuMO(this, m_oSolpartMenu[\'' + me._m_sNSpace + '\']);" onmouseout="spm_ScrollMenuMOUT(m_oSolpartMenu[\'' + me._m_sNSpace + '\']);" class="' + spm_fixCSSForMac(me.getIntCSSName('spmitmscr')) + ' ' + me.cssMenuScrollItem + '" style="width: 100%; font-size: 6pt;">...</div>';
			oRow.appendChild(oCell);

			oRow = spm_insertTableRow(oTbl, 0);
			oCell = document.createElement('TD');		
			oCell.id = 'up' + oMenu.id.substring(3);
			oCell.colSpan = 3;
			oCell.align = 'center';
			oCell.style.backgroundColor = 'gray';	//can be overridden by MenuScroll style
			oCell.innerHTML='<div id="up' + oMenu.id.substr(3) + '" onclick="return spm_ScrollMenuClick(this, event);" onmouseover="spm_ScrollMenuMO(this, m_oSolpartMenu[\'' + me._m_sNSpace + '\']);" onmouseout="spm_ScrollMenuMOUT(m_oSolpartMenu[\'' + me._m_sNSpace + '\']);" class="' + spm_fixCSSForMac(me.getIntCSSName('spmitmscr')) + ' ' + me.cssMenuScrollItem + '" style="width: 100%; font-size: 6pt;">...</div>';
			oRow.style.display = 'none';
			oRow.appendChild(oCell);
		}	

		if (oMenu.ScrollRowHeight == null)
		{
			spm_getTags('tr', oTbl)[0].style.display = '';
			oMenu.ScrollItemHeight = (spm_getElementHeight(spm_getTags('tr', oTbl)[0]) * 2);
			spm_getTags('tr', oTbl)[0].style.display = 'none';

			oMenu.ScrollRowHeight = spm_getElementHeight(spm_getTags('tr', oTbl)[1]);
		}

		oMenu.scrollItems = parseInt((spm_getViewPortHeight() - spm_elementTop(oMenu) + spm_getBodyScrollTop() - oMenu.ScrollItemHeight) / (oMenu.ScrollRowHeight + 1));
		spm_showScrolledItems(oMenu);

	}
		
	function spm_ScrollMenuMO(e, me)
	{
		me._m_dHideTimer = null;
		me._m_oScrollingMenu = e;
		if (Number(me.moScroll))
			setTimeout('m_oSolpartMenu["' + me._m_sNSpace + '"].scrollMenu()', 500);

	}
	
	function spm_ScrollMenuMOUT(me)
	{
		me._m_oScrollingMenu = null;

		me._m_dHideTimer = new Date();
		if (me.moutDelay != 0)
		  setTimeout('m_oSolpartMenu["' + me._m_sNSpace + '"].hideMenuTime()', me.moutDelay);
		

	}
	
	function spm_ScrollMenuClick(e, evt)
	{		
		if (e != null)
		{	
			var oCell = e.parentNode;
			var oTbl = oCell.parentNode.parentNode.parentNode;
			var oMenu = oTbl.parentNode;

			if (oCell.id == 'up' + oMenu.id.substring(3))
			{
				if (oMenu.scrollPos > 1)
					oMenu.scrollPos--;					
				else
					return true;
			}
			else 
			{
				if (oMenu.scrollPos + oMenu.scrollItems < oTbl.rows.length - 1)
					oMenu.scrollPos++;
				else
					return true;
			}
				
			spm_showScrolledItems(oMenu);
			if (evt != null)
				spm_stopEventBubbling(evt);
		}
		return false;
	}

	function spm_showScrolledItems(oMenu)
	{
		var oTbl = spm_getTags('table', oMenu)[0];
		var oRows = spm_getTags('tr', oTbl);	//oTbl.rows.length
		
		for (var i=1; i < oRows.length; i++)	
		{
			//if row is not within display "window" then don't display it
			if (i < oMenu.scrollPos || i >= oMenu.scrollPos + oMenu.scrollItems)
				oRows[i].style.display = 'none';
			else
				oRows[i].style.display = '';			
		}
		
		// if we are scrolled down at least one then display up scroll item
		if (oMenu.scrollPos > 1)
			oRows[0].style.display = '';
		else
			oRows[0].style.display = 'none';
		
		
		// if there is at least one item not displayed then show down item
		if (oMenu.scrollPos + oMenu.scrollItems < oTbl.rows.length - 1)
			oRows[oRows.length-1].style.display = '';
		else
			oRows[oRows.length-1].style.display = 'none';
			
	}

	function spm_insertTableRow(tbl, iPos)
	{
		var oRow;
		var oTB;
		oRow = document.createElement('TR');
		if (tbl.getElementsByTagName('TBODY').length == 0)
		{
			oTB = document.createElement('TBODY');
			tbl.appendChild(oTB);
		}
		else
			oTB = tbl.getElementsByTagName('TBODY')[0];

		if (iPos == null)
			oTB.appendChild(oRow);
		else
			oTB.insertBefore(oRow, tbl.rows[iPos]);
		return oRow;
	
	}

	function spm_getElementHeight(o)
	{	
		if (o.offsetHeight == null || o.offsetHeight == 0)
		{
			if (o.offsetParent.offsetHeight == null || o.offsetParent.offsetHeight == 0)
			{
				if (o.offsetParent.offsetParent != null)
					return o.offsetParent.offsetParent.offsetHeight; //needed for Konqueror
				else
					return 0;
			}
			else
				return o.offsetParent.offsetHeight;
		}
		else
			return o.offsetHeight;
	}

	function spm_getElementWidth(o)
	{
		if (o.offsetWidth == null || o.offsetWidth == 0)
		{
			if (o.offsetParent.offsetWidth == null || o.offsetParent.offsetWidth == 0)
			{
				if (o.offsetParent.offsetParent != null)
					return o.offsetParent.offsetParent.offsetWidth; //needed for Konqueror
				else
					return 0;
			}
			else
				return o.offsetParent.offsetWidth

		}
		else
			return o.offsetWidth;
	}
	
	//viewport logic taken from http://dhtmlkitchen.com/js/measurements/index.jsp
	function spm_getViewPortWidth()
	{
		// supported in Mozilla, Opera, and Safari
    if(window.innerWidth)
			return window.innerWidth;
    // supported in standards mode of IE, but not in any other mode
    if(window.document.documentElement.clientWidth)
			return document.documentElement.clientWidth;
	
    // supported in quirks mode, older versions of IE, and mac IE (anything else).
    return window.document.body.clientWidth;
	}
	
  function spm_getBodyScrollTop()
  {
		if (window.pageYOffset)
			return window.pageYOffset;
		
		var oBody = (document.compatMode && document.compatMode != "BackCompat") ? document.documentElement : document.body;		
		return oBody.scrollTop;
  }

  function spm_getBodyScrollLeft(bOverride)
  { 
		if (window.pageXOffset)
			return window.pageXOffset;

		var oBody = (document.compatMode && document.compatMode != "BackCompat") ? document.documentElement : document.body;
		return oBody.scrollLeft;
  }
	
	function spm_getViewPortHeight()
	{
		// supported in Mozilla, Opera, and Safari
    if(window.innerHeight)
			return window.innerHeight;
    // supported in standards mode of IE, but not in any other mode
    if(window.document.documentElement.clientHeight)
			return document.documentElement.clientHeight;
	
    // supported in quirks mode, older versions of IE, and mac IE (anything else).
    return window.document.body.clientHeight;
	}
		
	function spm_elementTop(eSrc, includeBody)
	{
		
		var iTop = 0;
		var eParent;
		eParent = eSrc;

		while (eParent.tagName.toUpperCase() != "BODY")
		{

			//Safari incorrectly calculates the TR tag to be at the top of the table, so try and get child TD tag to use for measurement
			//if (spm_browserType() == 'safari' && eParent.tagName.toUpperCase() == 'TR' && spm_getTags('TD', eParent).length)
			//	eParent = spm_getTags('TD', eParent)[0];

			iTop += eParent.offsetTop;
			
			eParent = eParent.offsetParent;
			if (eParent == null)
				break;
		}
		if (includeBody != null && eParent != null && (spm_browserType() == 'safari' || spm_browserType() == 'kq')) 
			iTop += eParent.offsetTop;
		
		return iTop;
	}


	function spm_elementLeft(eSrc, includeBody)
	{	
		var iLeft = 0;
		var eParent;
		eParent = eSrc;
		while (eParent.tagName.toUpperCase() != "BODY")
		{

			iLeft += eParent.offsetLeft;
				
			eParent = eParent.offsetParent;
			if (eParent == null)
				break;
		}
		if (includeBody != null && eParent != null && (spm_browserType() == 'safari' || spm_browserType() == 'kq'))
			iLeft += eParent.offsetLeft;

		
		return iLeft;
	}
	
	function spm_getElement(e, sID) 
	{
		var o=e;
		var i=0;
		while (o.id != sID)
		{
			o=o.parentNode;
			i++;
		}
		return o;
	}

	function spm_getSourceTR(e, ns)
	{
		while (e.id == "")
		{
			e= e.parentElement;
		}
		if (e.id.indexOf("arrow") != -1)
		{
			var sID = e.id.substr(5);
			return spm_getById("tr" + sID);
		}
		else if (e.id.indexOf("td") != -1)
		{
			var sID = e.id.substr(2);
			return spm_getById("tr" + sID);
		}	
		else if (e.id.indexOf("icon") != -1)
		{
			var sID = e.id.substr(4);
			return spm_getById("tr" + sID);
		}	
		else if (e.id.indexOf("img") != -1)
		{
			var sID = e.id.substr(3);
			return spm_getById("tr" + sID);
		}	
		else
		{
			return e;
		}
	}

	function spm_itemHasChildren(sID, ns)
	{
		return spm_getById("tbl" + sID) != null;
	}

function spm_getMenuItemStyle(sType, oNode)
{
  return spm_getAttr(oNode, sType + "style", '');
}

function spm_getMenuItemCSS(oNode)
{
  return spm_getAttr(oNode, "css", '');
}

function spm_getMenuItemSelCSS(oNode)
{
  return spm_getAttr(oNode, "selcss", '');
}

SolpartMenu.prototype.getIntCSSName =  function(sClass)
{
  var ary = sClass.split(' ');
  var s='';
  for (var i=0; i<ary.length; i++)
    s += this._m_sNSpace.toLowerCase() + '_' + ary[i] + ' ';
  
  return s;
}

function spm_fixCSSForMac(s)
{
	var ary = s.split(' ');
	var sRet='';
	for (var i=0; i<ary.length; i++)
	{
		if (ary[i].rtrim().length > 0)
		{
			if (sRet.length)
				sRet += ' ' + ary[i];
			else
				sRet = ary[i];
		}
	}
	return sRet;
}

function spm_getMenuClickAction(oNode, me)
{
  //'function to determine if menu item has action associated (URL)
  var sName = spm_getAttr(me._m_oMenu, 'name', me._m_oMenu.name);

  if (sName == null || sName.length == 0)	//opera fix for getting name
		sName = spm_getAttr(me._m_oMenu, 'pbname', me._m_oMenu.pbname);
	
  if (spm_getAttr(oNode, "runat", '').length)
    return "__doPostBack('" + sName + "', '" + spm_getAttr(oNode, "id", "") + "');";
  if (spm_getAttr(oNode, "server", '').length)
    return "__doPostBack('" + sName + "', '" + spm_getAttr(oNode, "id", "") + "');";
  var sURL = spm_getAttr(oNode, "url", "");
  if (sURL.length)
	{
		if (sURL.toLowerCase().substr(0, "javascript:".length) == "javascript:")
			return sURL.substr("javascript:".length) + ";";
		else
		{
			if (me.target.length > 0 && document.frames[me.target] != null)
				return "document.frames['" + me.target + "'].location.href='" + sURL + "';";
			else
				return "document.location.href='" + sURL + "';";
		}
	}
	return '';
	
}

function spm_getMenuSpacingImage(sPos, me)
{
  var sAlign = me.menuAlignment.toLowerCase();

  if ((sPos == 'left' && sAlign == 'right') || (sPos == 'right' && sAlign == 'left'))
		return "       <td width=\"100%\">" + spm_getSpacer(me) + "</td>";

  if ((sPos == 'right' && sAlign == 'left') || (sPos == 'left' && sAlign == 'right'))
		return "       <td width=\"3px\">" + spm_getSpacer(me) + "</td>";

	if (sAlign == 'Center')
		return "       <td width=\"33%\">" + spm_getSpacer(me) + "</td>";
	
	return '';   
}

function spm_getSpacer(me) 
{
  return spm_getMenuImage('spacer.gif', me, false, ' ');
    //return '&nbsp;'; //"<IMG SRC=\"" + me.systemImagesPath + "spacer.gif\">";
}

function spm_getImage(oAttr, me)
{
  //'retrieves an image for a passed in XMLAttribute
  var sImage = spm_getAttr(oAttr, 'image', '');

  if (sImage.length)
  {
    return spm_getHTMLImage(sImage, spm_getAttr(oAttr, 'imagepath', me.iconImagesPath), null, spm_getAttr(oAttr, 'title', ''));
  }
  else
    return spm_getMenuImage('spacer.gif', me, null, ' ');
}

function spm_getItemHTML(oNode, sSide, sDef)
{
  if (sDef == null) sDef = '';
  return spm_getAttr(oNode, sSide + "html", sDef);
}

function spm_getMenuImage(sImage, me, bForce, sAlt)
{
    //'generates html for image using the SystemImagesPath property
    return spm_getHTMLImage(sImage, me.systemImagesPath, bForce, sAlt);
}

function spm_getHTMLImage(sImage, sPath, bForce, sAlt)
{
    //'generates html for image using the SystemImagesPath property
    if (spm_browserNeedsSpacer() == false && sImage == 'spacer.gif' && bForce != true)
        return '&nbsp;'; 
    else
        return "<IMG SRC=\"" + sPath + sImage + "\" " + spm_getAlt(sAlt) + ">";
}

function spm_getAlt(sAlt)
{
	if (sAlt != null && sAlt.rtrim().length > 0)
		return ' ALT="' + sAlt + '" ';
	else
		return '';
}

function spm_browserNeedsSpacer()
{
	if (spm_browserType() == 'ie')
		return false;
	else
		return true;
}

function MyIIf(bFlag, sTrue, sFalse) 
{
    if (bFlag)
		return sTrue;
	else
		return sFalse;
}

function spm_getArrow(sImg, me) 
{
  //FIX
    if (sImg.length)
        return spm_getMenuImage(sImg, me, null, '>');
    else
    {
      if (me.direction == 'rtl')
				return "3"; 
      else
				return "4"; //'defaults to using wingdings font (4 = arrow)
    }
}

function spm_getMenuBorderStyle(me, shColor, hlColor, width)
{
  if (shColor == null) shColor = me.shColor;
  if (hlColor == null) hlColor = me.hlColor;
  if (width == null) width = me.borderWidth;
  
  //border-bottom: Gray 1px solid; border-left: White 1px solid; border-top: White 1px solid; border-right: Gray 1px solid;
  //return 'border-bottom: ' + shColor + ' ' + width + 'px solid; border-left: ' + hlColor + ' ' + width + 'px solid;  border-top: ' + hlColor + ' ' + width + 'px solid; border-right: ' + shColor + ' ' + width + 'px solid;';
  return getBorderStyle('border-bottom', shColor, width) + getBorderStyle('border-left', hlColor, width) + getBorderStyle('border-top', hlColor, width) + getBorderStyle('border-right', shColor, width);
}

function getBorderStyle(type, color, width)
{  
  return type + ': ' + color + ' ' + width + 'px solid; ';
}



//------------------------//
String.prototype.ltrim = function () { return this.replace(/^\s*/, "");}
String.prototype.rtrim = function () { return this.replace(/\s*$/, "");}
String.prototype.trim  = function () { return this.ltrim().rtrim(); }

if (spm_browserType() == 'safari')	//Safari Hack
	var Document = null;
	
if (spm_browserType() != 'ie' && spm_browserType() != 'op' && Document != null)
{
  Document.prototype.loadXML = function (s) 
    {
    
      // parse the string to a new doc
      var doc2 = (new DOMParser()).parseFromString(s, "text/xml");

      // remove all initial children
      while (this.hasChildNodes())
      this.removeChild(this.lastChild);

      // insert and import nodes
      for (var i = 0; i < doc2.childNodes.length; i++) 
      {
      this.appendChild(this.importNode(doc2.childNodes[i], true));
      }
    }

    function _Node_getXML() 
    {
      //create a new XMLSerializer
      var objXMLSerializer = new XMLSerializer;
      
      //get the XML string
      var strXML = objXMLSerializer.serializeToString(this);
      
      //return the XML string
      return strXML;
    }
    Node.prototype.__defineGetter__("xml", _Node_getXML);
}

function spm_createDOMDoc()
{
	if (spm_browserType() == 'ie')
	{
		var o = new ActiveXObject('MSXML.DOMDocument');
		o.async = false;
		return o;
	}
	else
		return document.implementation.createDocument("", "", null);
}

function spm_getById(sID)
{
  if (document.all == null)
    return document.getElementById(sID);
  else
    return document.all(sID);
}

function spm_getTags(sTag, oCtl)
{
	if (oCtl == null)
		oCtl = document;
	
	if (spm_browserType() == 'ie')
    return oCtl.all.tags(sTag);
  else
    return oCtl.getElementsByTagName(sTag);
}

function spm_browserType()
{
	if (m_spm_sBrowser == null)
	{
		var agt=navigator.userAgent.toLowerCase();

		if (agt.toLowerCase().indexOf('konqueror') != -1) 
			m_spm_sBrowser = 'kq';
		else if (agt.toLowerCase().indexOf('opera') != -1) 
			m_spm_sBrowser = 'op';
		else if (agt.toLowerCase().indexOf('netscape') != -1) 
			m_spm_sBrowser = 'ns';
		else if (agt.toLowerCase().indexOf('msie') != -1)
			m_spm_sBrowser = 'ie';
		else if (agt.toLowerCase().indexOf('safari') != -1)
			m_spm_sBrowser = 'safari';
	  
		if (m_spm_sBrowser == null)
			m_spm_sBrowser = 'mo';  
	}
	//window.status = m_spm_sBrowser;
	return m_spm_sBrowser;
}

function spm_browserVersion()
{
	//Please offer a better solution if you have one!
	var sType = spm_browserType();
	var iVersion = parseFloat(navigator.appVersion);
	var sAgent = navigator.userAgent.toLowerCase();
	if (sType == 'ie')
	{
		var temp=navigator.appVersion.split("MSIE");
		iVersion=parseFloat(temp[1]);
	}
	if (sType == 'ns')
	{
		var temp=sAgent.split("netscape");
		iVersion=parseFloat(temp[1].split("/")[1]);	
	}
	return iVersion;
}

function spm_needsSubMenuDelay()
{
	if (spm_browserType() == 'ie')
		return true;
	else
		return false;

}

function spm_supportsIFrameTrick()
{
	var sType = spm_browserType();
	var sVersion = spm_browserVersion();
	
	if ((sType == 'ie' && sVersion < 5.5) || (sType == 'ns' && sVersion < 7) || (spm_browserType() == 'safari') || spm_isMac('ie'))
	{
		return false;
	}
	return true;
}

function spm_isMac(sType)
{
//return true;
  var agt=navigator.userAgent.toLowerCase();
  if (agt.indexOf('mac') != -1) 
  {
		if (sType == null || spm_browserType() == sType)
			return true;
  }
  else
    return false;
  
}

//taken from http://groups.google.com/groups?hl=en&lr=&ie=UTF-8&oe=UTF-8&safe=off&threadm=b42qj3%24r8s1%40ripley.netscape.com&rnum=1&prev=/groups%3Fq%3Dmozilla%2B%2522currentstyle%2522%26hl%3Den%26lr%3D%26ie%3DUTF-8%26oe%3DUTF-8%26safe%3Doff%26scoring%3Dd 
function spm_getCurrentStyle(el, property) {
  if (document.defaultView) 
  {
   // Get computed style information:

    if (el.nodeType != el.ELEMENT_NODE) return null;
    return document.defaultView.getComputedStyle(el,'').getPropertyValue(property.split('-').join(''));
  }
  if (el.currentStyle) 
  {
    // Get el.currentStyle property value:
    return el.currentStyle[property.split('-').join('')];
    //return el.currentStyle.getAttribute(property.split('-').join(''));  //We need to get rid of slashes
  }
  if (el.style) 
  {
    // Get el.style property value:
    return el.style.getAttribute(property.split('-').join(''));  // We need to get rid of slashes
  } return  null;
}

function spm_getAttr(o, sAttr, sDef)
{
  if (sDef == null)
    sDef = '';
  var s = o.getAttribute(sAttr);
  if (s != null && s.length > 0)
    return o.getAttribute(sAttr);
  else
    return sDef;
}

function spm_setAttr(o, sAttr, sVal)
{
	if (sVal.length > 0)
		o.setAttribute(sAttr, sVal);
	else
		o.removeAttribute(sAttr);
}


function spm_fixUnit(s)
{
  if (s.length && isNaN(s) == false)
    return s + 'px';

}

function spm_nodeHasChildren(node)
{
  if (typeof(node.selectSingleNode) != 'undefined') //(node.selectSingleNode != null) //(spm_browserType() == 'ie')
    return node.selectSingleNode('./menuitem') != null;
  else
  {
    if (node.childNodes.length > 0)
    {
      //Netscape/Mozilla counts an empty <menuitem id></menuitem> as having a child...
      for (var i=0; i< node.childNodes.length; i++)
      {
        if (node.childNodes[i].nodeName == 'menuitem')
            return true;
      }
    }
  }
  return false;  
}

function spm_findNode(oParent, sID)
{
	for (var i = 0; i < oParent.childNodes.length; i++)
	{
		oNode = oParent.childNodes[i];

		if (oNode.nodeType != 3)  //exclude nodeType of Text (Netscape/Mozilla) issue!
		{

			if ((oNode.nodeName == "menuitem" || oNode.nodeName == "menubreak") && oNode.getAttribute("id") == sID)
				return oNode;

			if (oNode.childNodes.length > 0)
			{
				var o = spm_findNode(oNode, sID);
				if (o != null)
					return o;
			}
		}
	}
}

function spm_getSibling(oNode, iOffset)
{
	var sID = spm_getAttr(oNode, 'id');
	var o;
	for (var i=0; i<oNode.parentNode.childNodes.length; i++)
	{
		o = oNode.parentNode.childNodes[i];
		if (o.nodeType != 3)
		{
			if (spm_getAttr(o, 'id') == sID)
				return getOffsetNode(o.parentNode, i, iOffset);
		}
	}
}

function spm_stopEventBubbling(e)
{
    if (spm_browserType() == 'ie')
			window.event.cancelBubble = true;
		else
			e.stopPropagation();
}

//--- if you have a better solution send me an email - jhenning@solpart.com ---//
function spm_appendFunction(from_func, to_func)
{
  if (from_func == null)
    return new Function ( to_func ); 
  return new Function ( spm_parseFunctionContents(from_func) + '\n' + spm_parseFunctionContents(to_func) );
}
function spm_parseFunctionContents(fnc)
{
  var s =String(fnc).trim();
  if (s.indexOf('{') > -1)
		s = s.substring(s.indexOf('{') + 1, s.length - 1);
  return s;
}

//--- For JS DOM ---//
function SPJSXMLNode(sNodeName, sID, oParent, sTitle, sURL, sImage, sImagePath, sRightHTML, sLeftHTML, sRunAtServer, sItemStyle, sImageStyle, sToolTip, sItemCSS, sItemSelCSS) 
{ 
  this.nodeName = sNodeName;
  this.id=sID;
  this.childNodes = new Array();
  //this.nodeType = 3;
  
  
  this.parentNode = oParent;            
  if (oParent != null)
  {
    oParent.childNodes[oParent.childNodes.length] = this;
    
    if (oParent.documentElement == null)
      this.documentElement = oParent;
    else
      this.documentElement = oParent.documentElement;
  }
  else
    this.documentElement = this;
    
  this.title = sTitle;
  this.url = sURL;
  this.image = sImage;
  this.imagepath = sImagePath;
  this.righthtml = sRightHTML;
  this.lefthtml = sLeftHTML;
  this.server = sRunAtServer;
  this.itemstyle = sItemStyle;
  this.imagestyle = sImageStyle;
  this.tooltip = sToolTip;
  this.css = sItemCSS;
  this.selcss = sItemSelCSS;
}      

SPJSXMLNode.prototype.getAttribute = function(s)
{
  return this[s];
}


  var m_iSPTimer;
  var m_iSPTotalTimer=0;
  var m_sSPDebugText;
  var m_oSPDebugCtl;
  var m_bSPDebug = false;
  
  function __db(s)
  {
    if (spm_browserType() != 'ie' || m_bSPDebug == false)
      return;
     
    var sT = new Date() - m_iSPTimer;
    if (sT > 120000)
    {
      sT = '';
      m_oSPDebugCtl.value = '---reset---';
      m_iSPTotalTimer=0;
    }
    else if (sT > 100)
    {
      m_iSPTotalTimer+= sT;
      sT = ' *** [' + sT + '] *** ';
    }
    else if (sT > 0)
    {
      m_iSPTotalTimer+= sT;
      sT = ' [' + sT + ']';
    }
    else
      sT = '';
      
    if (document.forms.length > 0 && m_oSPDebugCtl == null)
    {      
      document.forms(0).insertAdjacentHTML('afterEnd', '<br><TEXTAREA ID="my__Debug" STYLE="WIDTH: 100%; HEIGHT: 100px"></TEXTAREA>');
      m_oSPDebugCtl = document.all('my__Debug');
    }

    if (m_oSPDebugCtl != null)
      m_oSPDebugCtl.value += '[' + m_iSPTotalTimer + '] ' + s + sT + '\n';
    else
      m_sSPDebugText += '[' + m_iSPTotalTimer + '] ' + s + sT + '\n'; 
      
    m_iSPTimer = new Date();
  }

	if (window.__smartNav != null)
		window.setTimeout(spm_fixSmartNav, 1000);
	function spm_fixSmartNav()
	{
		if (window.__smartNav != null)
		{
			if (document.readyState == 'complete')
			{
				var o = spm_getById('SolpartMenuDI');
				if (o != null)
				{
					if (o.length == null)
					{
							if (o.xml != null)
								spm_initMyMenu(o, o.parentElement);
					}
					else
					{
						for (var i=0; i<o.length; i++)
						{
							if (o[i].xml != null)
								spm_initMyMenu(o[i], o.parentElement);
						}
					}
				}
			}
			else
				window.setTimeout(spm_fixSmartNav, 1000);
		}
	}

	function spm_elementDims(o, bIncludeBody, me)
	{
		var bHidden = (o.style.display == 'none');
		
		if (bHidden)
			o.style.display = "";
		this.t = spm_elementTop(o, bIncludeBody);
		this.l = spm_elementLeft(o, bIncludeBody);
		if (!spm_isMac('ie'))
		{
			o.style.top = spm_getCoord(0);
			o.style.left = spm_getCoord(0);
		}
		this.w = spm_getElementWidth(o);
		this.h = spm_getElementHeight(o);
		if (!spm_isMac('ie'))
		{
			o.style.top = spm_getCoord(this.t);
			o.style.left = spm_getCoord(this.l);
		}
		if (bHidden)
			o.style.display = "none";
	}

function spm_getCoord(i)
{
	return i + 'px';
}
