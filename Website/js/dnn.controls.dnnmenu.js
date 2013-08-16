
Type.registerNamespace('dnn.controls');dnn.extend(dnn.controls,{initMenu:function(ctl)
{if(ctl)
{var menu=new dnn.controls.DNNMenu(ctl);menu.initialize();return menu;}}});dnn.controls.DNNMenu=function(o)
{dnn.controls.DNNMenu.initializeBase(this,[o]);this.rootNode=null;this.nodes=[];this._loadNodes();this.mbcss=this.getProp('mbcss','');this.mcss=this.getProp('mcss','');this.css=this.getProp('css','');this.cssChild=this.getProp('csschild','');this.cssHover=this.getProp('csshover','');this.cssSel=this.getProp('csssel','');this.cssIcon=this.getProp('cssicon','');this.sysImgPath=this.getProp('sysimgpath','images/');this.imagePaths=this.getProp('imagepaths','').split(',');this.imageList=this.getProp('imagelist','').split(',');for(var i=0;i<this.imageList.length;i++)
{var index=this.imageList[i].indexOf(']');if(index>-1)
this.imageList[i]=this.imagePaths[this.imageList[i].substring(1,index)]+this.imageList[i].substring(index+1);}
this.urlList=this.getProp('urllist','').split(',');this.workImg=this.getProp('workimg','dnnanim.gif');this.rootArrow=this.getProp('rarrowimg','');this.childArrow=this.getProp('carrowimg','');this.target=this.getProp('target','');this.defaultJS=this.getProp('js','');this.postBack=this.getProp('postback','');this.callBack=this.getProp('callback','');this.callBackStatFunc=this.getProp('callbacksf','');if(this.callBackStatFunc.length>0)
this.add_handler('callBackStatus',eval(this.callBackStatFunc));this.orient=new Number(this.getProp('orient',dnn.controls.orient.horizontal));this.suborient=new Number(this.getProp('suborient',dnn.controls.orient.vertical));this.openMenus=[];this.moutDelay=this.getProp('moutdelay',500);this.minDelay=new Number(this.getProp('mindelay',250));this.renderMode=new Number(this.getProp('rmode',0));this.useTables=(this.renderMode==dnn.controls.menuRenderMode.normal);this.enablePostbackState=(this.getProp('enablepbstate','0')=='1');this.podInProgress=false;this.keyboardAccess=(this.getProp('kbaccess','1')=='1');this.hoverMNode=null;this.selMNode=null;this.animation=new Number(this.getProp('anim','0'));this.easingType=new Number(this.getProp('easeType','3'));this.easingDir=new Number(this.getProp('easeDir','1'));;this.animationLen=new Number(this.getProp('animLen','1'));this.animationInterval=new Number(this.getProp('animInt','10'));this._attachedHandlers=[];this._onsubmitDelegate=null;this._hideMenusDelegate=null;this._expandNodeDelegate=null;}
dnn.controls.DNNMenu.prototype={initialize:function()
{dnn.controls.DNNMenu.callBaseMethod(this,'initialize');if(this.keyboardAccess)
{this._setupKeyHandling();}
this.generateMenuHTML();if(this.enablePostbackState)
{this._onsubmitDelegate=Function.createDelegate(this,this._onsubmit);dnn.controls.submitComp.add_handler(this._onsubmitDelegate);}
this._hideMenusDelegate=dnn.createDelegate(this,this.hideMenus);this._expandNodeDelegate=dnn.createDelegate(this,this.__expandNode);},generateMenuHTML:function()
{this.container.className=this.mbcss;dnn.dom.disableTextSelect(this.container);
if(this.rootNode!==null){for(var i=0;i<this.rootNode.childNodeCount();i++)this.renderNode(this.rootNode.childNodes(i),this.container);this.addHandlers(document.body,{"click":this._bodyClick},this);}dnn.setVar(this.ns + '_json', '');},renderNode:function(node,ctr)
{var mNode=new dnn.controls.DNNMenuNode(node);if(mNode.selected)
this.selMNode=mNode;var menuBuilder=this._getMenuBuilder(mNode,ctr);if(menuBuilder.alreadyRendered==false)
{if(this.isNodeVertical(mNode))
menuBuilder.newRow();menuBuilder.newCont();if(mNode.lhtml.length>0)
menuBuilder.appendChild(this.renderCustomHTML(mNode.lhtml));var icon=this.renderIcon(mNode);menuBuilder.appendChild(icon);if(this.useTables==false||mNode.level==0)
icon.className='icn '+(mNode.cssIcon.length>0?mNode.cssIcon:this.cssIcon);else
menuBuilder.subcont.className='icn '+(mNode.cssIcon.length>0?mNode.cssIcon:this.cssIcon);if(mNode.isBreak==false)
menuBuilder.appendChild(this.renderText(mNode),true);menuBuilder.newCell();this.renderArrow(mNode,menuBuilder.subcont);if(mNode.rhtml.length>0)
menuBuilder.appendChild(this.renderCustomHTML(mNode.rhtml));if(mNode.toolTip.length>0)
menuBuilder.row.title=mNode.toolTip;}
this.assignCss(mNode);if(mNode.enabled)
this.addHandlers(menuBuilder.row,{"click":this._nodeTextClick},this);if(this._attachedHandlers[menuBuilder.container.id]!='mouseover')
{this._attachedHandlers[menuBuilder.container.id]='mouseover';this.addHandlers(menuBuilder.container,{"mouseover":this._nodeMOver,"mouseout":this._nodeMOut},this);}
if(mNode.hasNodes||mNode.hasPendingNodes)
{var subMenu=this.renderSubMenu(mNode);this.container.appendChild(subMenu);dnn.dom.positioning.placeOnTop(subMenu,false,this.sysImgPath+'spacer.gif');for(var i=0;i<node.childNodeCount();i++)
this.renderNode(node.childNodes(i),subMenu);}},renderCustomHTML:function(sHTML)
{var ctl=dnn.dom.createElement('span');ctl.innerHTML=sHTML;return ctl;},renderIcon:function(mNode)
{var ctl=dnn.dom.createElement('span');if(mNode.imageIndex>-1||mNode.image!='')
{var img=this.createChildControl('img',mNode.id,'icn');img.src=(mNode.image.length>0?mNode.image:this.imageList[mNode.imageIndex]);ctl.appendChild(img);}
return ctl;},renderArrow:function(mNode,ctr)
{if(mNode.hasNodes||mNode.hasPendingNodes)
{var imgSrc=(mNode.level==0?this.rootArrow:this.childArrow);if(imgSrc.trim().length>0)
{if(this.useTables&&mNode.level>0)
{var img=dnn.dom.createElement('img');img.src=imgSrc;ctr.appendChild(img);}
else
{ctr.style.backgroundImage='url('+imgSrc+')';ctr.style.backgroundRepeat='no-repeat';ctr.style.backgroundPosition='right';}}}},renderText:function(mNode)
{var ctl=this.createChildControl('span',mNode.id,'t');ctl.className='txt';ctl.innerHTML=mNode.text;ctl.style.cursor='pointer';return ctl;},renderSubMenu:function(mNode)
{var menuBuilder=this._getMenuBuilder(mNode,null);var subMenu=menuBuilder.createSubMenu();subMenu.style.position='absolute';subMenu.style.display='none';var css=this.mcss;css+=' m m'+mNode.level;css+=' mid'+mNode.id;subMenu.className=css;return subMenu;},_getMenuBuilder:function(mNode,ctr)
{var menuBuilder;if(ctr)
{if(this.renderMode==dnn.controls.menuRenderMode.normal&&mNode.level>0&&this.isNodeVertical(mNode))
menuBuilder=new dnn.controls.DNNMenuTableBuilder(this,mNode,ctr);else if(this.renderMode==dnn.controls.menuRenderMode.unorderdlist)
menuBuilder=new dnn.controls.DNNMenuListBuilder(this,mNode,ctr);else
menuBuilder=new dnn.controls.DNNMenuBuilder(this,mNode,ctr);}
else
{if(this.renderMode==dnn.controls.menuRenderMode.normal&&this.suborient==dnn.controls.orient.vertical)
menuBuilder=new dnn.controls.DNNMenuTableBuilder(this,mNode);else if(this.renderMode==dnn.controls.menuRenderMode.unorderdlist)
menuBuilder=new dnn.controls.DNNMenuListBuilder(this,mNode);else
menuBuilder=new dnn.controls.DNNMenuBuilder(this,mNode);}
return menuBuilder;},hoverNode:function(mNode)
{if(this.hoverMNode!=null)
{this.hoverMNode.hover=false;this.assignCss(this.hoverMNode);}
if(mNode!=null)
{mNode.hover=true;this.assignCss(mNode);}
this.hoverMNode=mNode;},__expandNode:function(ctx)
{this.expandNode(ctx,true);},expandNode:function(mNode,force)
{dnn.cancelDelay(this.ns+'min');if(mNode.hasPendingNodes)
{if(this.podInProgress==false)
{this.podInProgress=true;this.showWorkImage(mNode,true);mNode.menu=this;if(this.callBack.indexOf('[NODEXML]')>-1)
eval(this.callBack.replace('[NODEXML]',dnn.escapeForEval(mNode.node.getXml())));else
eval(this.callBack.replace('[NODEID]',mNode.id));}}
else
{if(this.openMenus.length>0&&this.openMenus[this.openMenus.length-1].id==mNode.id)
return;if(this.minDelay==0||force)
{this.hideMenus(new dnn.controls.DNNMenuNode(mNode.node.parentNode()));var subMenu=this.getChildControl(mNode.id,'sub');if(subMenu!=null)
{this.positionMenu(mNode,subMenu);this.showSubMenu(subMenu,true,mNode);this.openMenus[this.openMenus.length]=mNode;mNode.expanded=true;mNode.update();}}
else
dnn.doDelay(this.ns+'min',this.minDelay,this._expandNodeDelegate,mNode);}
return true;},showSubMenu:function(subMenu,show,mNode)
{dnn.dom.positioning.placeOnTop(subMenu,show,this.sysImgPath+'spacer.gif');subMenu.style.clip='rect(auto,auto,auto,auto)';if(this.animation!=0)
{subMenu.style.display='';var dir;if(this.isNodeVertical(mNode))
dir=(show?dnn.motion.animationDir.Right:dnn.motion.animationDir.Left);else
dir=(show?dnn.motion.animationDir.Down:dnn.motion.animationDir.Up);dnn.dom.animate(subMenu,this.animation,dir,this.easingType,this.easingDir,this.animationLen,this.animationInterval);}
else
subMenu.style.display=(show?'':'none');},showWorkImage:function(mNode,show)
{if(this.workImg!=null)
{var icon=this.getChildControl(mNode.id,'icn');if(icon!=null)
{if(show)
icon.src=this.sysImgPath+this.workImg;else
icon.src=(mNode.image.length>0?mNode.image:this.imageList[mNode.imageIndex]);}}},isNodeVertical:function(mNode)
{return((mNode.level==0&&this.orient==dnn.controls.orient.vertical)||(mNode.level>0&&this.suborient==dnn.controls.orient.vertical));},hideMenus:function(mNode)
{for(var i=this.openMenus.length-1;i>=0;i--)
{if(mNode!=null&&this.openMenus[i].id==mNode.id)
break;this.collapseNode(this.openMenus[i]);this.openMenus.length=this.openMenus.length-1;}},collapseNode:function(mNode)
{var subMenu=this.getChildControl(mNode.id,'sub');if(subMenu!=null)
{this.positionMenu(mNode,subMenu);this.showSubMenu(subMenu,false,mNode);mNode.expanded=null;mNode.update();return true;}},positionMenu:function(mNode,menu)
{var oPCtl=this.getChildControl(mNode.id,'ctr');if(dnn.dom.browser.isType(dnn.dom.browser.Safari,dnn.dom.browser.Opera))
{if(oPCtl.tagName=='TR'&&oPCtl.childNodes.length>0)
oPCtl=oPCtl.childNodes[oPCtl.childNodes.length-1];}
var oPDims=new dnn.dom.positioning.dims(oPCtl);var oMDims=new dnn.dom.positioning.dims(menu);var iScrollLeft=dnn.dom.positioning.bodyScrollLeft();var iScrollTop=dnn.dom.positioning.bodyScrollTop()
var iMaxTop=dnn.dom.positioning.viewPortHeight()+iScrollTop-oPDims.rot;var iMaxLeft=dnn.dom.positioning.viewPortWidth()+iScrollLeft-oPDims.rol;var iNewTop=oPDims.t;var iNewLeft=oPDims.l;var iStartTop=oPDims.t;var iStartLeft=oPDims.l;if(this.isNodeVertical(mNode))
{iNewLeft=oPDims.l+oPDims.w;iStartTop=iMaxTop;}
else
{iNewTop=oPDims.t+oPDims.h;iStartLeft=iMaxLeft;}
if(iNewTop+oMDims.h>=iMaxTop)
{if(oPDims.rot+iStartTop-oMDims.h>iScrollTop)
iNewTop=iStartTop-oMDims.h;}
if(iNewLeft+oMDims.w>iMaxLeft)
{if(oPDims.rol+iStartLeft-oMDims.w>iScrollLeft)
iNewLeft=iStartLeft-oMDims.w;}
if(this.suborient==dnn.controls.orient.horizontal&&this.isNodeVertical(mNode)==false)
{var oRDims=new dnn.dom.positioning.dims(this.container);iNewLeft=oRDims.l;}
menu.style.top=iNewTop+'px';menu.style.left=iNewLeft+'px';},selectNode:function(mNode)
{var arg=new dnn.controls.DNNNodeEventArgs(mNode);this.invoke_handler('click',arg);if(arg.get_cancel())
return;if(this.selMNode!=null)
{this.selMNode.selected=null;this.selMNode.update('selected');this.assignCss(this.selMNode);}
mNode.selected=true;mNode.update('selected');this.assignCss(mNode);this.selMNode=mNode;if(mNode.hasNodes||mNode.hasPendingNodes)
this.expandNode(mNode,true);if(mNode.selected)
{var sJS=this.defaultJS;if(mNode.js.length>0)
sJS=mNode.js;if(sJS.length>0)
{this.update(true);if(eval(sJS)==false)
return;}
if(mNode.clickAction==dnn.controls.action.postback)
{this.update(true);eval(this.postBack.replace('[NODEID]',mNode.id));}
else if(mNode.clickAction==dnn.controls.action.nav)
dnn.dom.navigate(mNode.getUrl(this),mNode.target.length>0?mNode.target:this.target);}
return true;},assignCss:function(mNode)
{var ctr=this.getChildControl(mNode.id,'ctr');var css=this.css;if(mNode.level>0&&this.cssChild.length>0)
css=this.cssChild;if(mNode.css.length>0)
css=mNode.css;if(mNode.hover)
css+=' hov '+(mNode.cssHover.length>0?mNode.cssHover:this.cssHover);if(mNode.selected)
css+=' sel '+(mNode.cssSel.length>0?mNode.cssSel:this.cssSel);if(mNode.breadcrumb)
css+=' bc';if(mNode.isBreak)
css+=' break';css+=' mi mi'+mNode.node.getNodePath();css+=' id'+mNode.id;if(mNode.level==0)
css+=' root';if(mNode.node.getNodeIndex()==0)
css+=' first';if(mNode.node.getNodeIndex()==mNode.node.parentNode().childNodeCount()-1)
css+=' last';if((mNode.node.getNodeIndex()==0)&&(mNode.node.getNodeIndex()==mNode.node.parentNode().childNodeCount()-1))
css+=' firstlast';ctr.className=css;},update:function()
{dnn.setVar(this.ns+'_json',this.rootNode.getJSON());},_onsubmit:function()
{this.update(true);},_bodyClick:function()
{this.hideMenus();},focusHandler:function(e)
{var mNode=this.hoverMNode;if(mNode==null)
mNode=this.selMNode;if(mNode==null)
mNode=new dnn.controls.DNNMenuNode(this.nodes[0]);this.hoverNode(mNode);this.container.onfocus=null;},blurHandler:function(e)
{if(this.hoverMNode!=null)
this.hoverNode(null);dnn.cancelDelay(this.ns+'min');if(this.moutDelay>0)
dnn.doDelay(this.ns+'mout',this.moutDelay,this._hideMenusDelegate);else
this.hideMenus();},safariKeyHandler:function(e)
{if(e.charCode==KEY_RETURN)
{if(this.hoverMNode!=null&&this.hoverMNode.enabled)
this.selectNode(this.hoverMNode);return false;}},keyboardHandler:function(e)
{var code=e.keyCode;if(code==null)
code=e.charCode;if(code==KEY_RETURN)
{if(this.hoverMNode!=null&&this.hoverMNode.enabled)
this.selectNode(this.hoverMNode);return false;}
if(code==KEY_ESCAPE)
{this.blurHandler();return false;}
if(code>=KEY_LEFT_ARROW&&code<=KEY_DOWN_ARROW)
{var iDir=(code==KEY_UP_ARROW||code==KEY_LEFT_ARROW)?-1:1;var sAxis=(code==KEY_UP_ARROW||code==KEY_DOWN_ARROW)?'y':'x';var mNode=this.hoverMNode;var oNewMNode;if(mNode==null)
mNode=new dnn.controls.DNNMenuNode(this.nodes[0]);var bHor=!this.isNodeVertical(mNode);if((sAxis=='y'&&!bHor)||(bHor&&sAxis=='x'))
{this.hideMenus(new dnn.controls.DNNMenuNode(mNode.node.parentNode()));oNewMNode=this.__getNextNode(mNode,iDir);}
else
{if(iDir==-1)
{oNewMNode=new dnn.controls.DNNMenuNode(mNode.node.parentNode());if(oNewMNode.level==0&&this.orient==dnn.controls.orient.horizontal)
oNewMNode=this.__getNextNode(new dnn.controls.DNNMenuNode(mNode.node.parentNode()),iDir);this.hideMenus(oNewMNode);}
else if(iDir==1)
{if(mNode.hasNodes||mNode.hasPendingNodes)
{if(mNode.expanded!=true)
{this.expandNode(mNode);if(this.podInProgress==false)
oNewMNode=new dnn.controls.DNNMenuNode(mNode.node.nodes[0]);}}
else
{var node=mNode.node;while(node.parentNode().nodeName()!='root')
node=node.parentNode();oNewMNode=new dnn.controls.DNNMenuNode(node);oNewMNode=this.__getNextNode(oNewMNode,iDir);this.hideMenus(new dnn.controls.DNNMenuNode(oNewMNode.node.parentNode()));}}}
if(oNewMNode!=null&&oNewMNode.node.nodeName()!='root')
this.hoverNode(oNewMNode);return false;}},dispose:function()
{this._onsubmitDelegate=null;this._hideMenusDelegate=null;this._expandNodeDelegate=null;dnn.controls.DNNMenu.callBaseMethod(this,'dispose');},__getNextNode:function(mNode,iDir)
{var node;var parentNode=mNode.node.parentNode();var nodeIndex=mNode.node.getNodeIndex('id');if(nodeIndex+iDir<0)
node=parentNode.nodes[parentNode.childNodeCount()-1];else if(nodeIndex+iDir>parentNode.childNodeCount()-1)
node=parentNode.nodes[0];else
node=parentNode.nodes[nodeIndex+iDir];var oRetNode=new dnn.controls.DNNMenuNode(node);if(oRetNode.isBreak)
{nodeIndex+=iDir;if(nodeIndex+iDir<0)
node=parentNode.childNodes(parentNode.childNodeCount()-1);else if(nodeIndex+iDir>parentNode.childNodeCount()-1)
node=parentNode.childNodes(0);else
node=parentNode.childNodes(nodeIndex+iDir);return new dnn.controls.DNNMenuNode(node);}
else
return oRetNode;},callBackFail:function(result,ctx,req)
{var mNode=ctx;var menu=mNode.menu;menu.invoke_handler('callBackFail',new dnn.controls.DNNCallbackEventArgs(result,ctx,req));},callBackStatus:function(result,ctx,req)
{var mNode=ctx;var menu=mNode.menu;menu.invoke_compatHandler('callBackStatus',result,ctx,req);},callBackSuccess:function(result,ctx,req)
{var mNode=ctx;var node=mNode.node;var menu=mNode.menu;menu.showWorkImage(mNode,false);var json=dnn.evalJSON("{"+result+"}");node.nodes=json.nodes;node.setupJSONNodes(node.rootNode(),node,node.nodes);var subMenu=menu.getChildControl(mNode.id,'sub');for(var i=0;i<node.childNodeCount();i++)
menu.renderNode(node.childNodes(i),subMenu);mNode=new dnn.controls.DNNMenuNode(node);mNode.hasPendingNodes=false;mNode.hasNodes=true;subMenu=menu.getChildControl(mNode.id,'sub');menu.expandNode(mNode);menu.callBackStatus(result,ctx,req);menu.podInProgress=false;menu.invoke_handler('callBackSuccess',new dnn.controls.DNNCallbackEventArgs(result,ctx,req));},_nodeTextClick:function(evt)
{var node=this._findEventNode(evt);var mNode;if(node!=null)
{mNode=new dnn.controls.DNNMenuNode(node);this.selectNode(mNode);}
evt.stopPropagation();},_menuMOver:function(evt,element)
{dnn.cancelDelay(this.ns+'mout');},_menuMOut:function(evt,element)
{dnn.cancelDelay(this.ns+'min');if(this.moutDelay>0)
dnn.doDelay(this.ns+'mout',this.moutDelay,dnn.createDelegate(this,this.hideMenus));else
this.hideMenus();},_nodeMOver:function(evt,element)
{var node=this._findEventNode(evt);if(node!=null)
{var mNode=new dnn.controls.DNNMenuNode(node);mNode.hover=true;this.assignCss(mNode);if(mNode.expanded!=true)
this.expandNode(mNode);evt.stopPropagation();}
this._menuMOver(evt,element);},_nodeMOut:function(evt,element)
{var node=this._findEventNode(evt);if(node!=null)
{var mNode=new dnn.controls.DNNMenuNode(node);this.assignCss(mNode);this._menuMOut(evt,element);evt.stopPropagation();}},getXml:function()
{return this.rootNode.getXml();},_findEventNode:function(evt)
{if(dnn.dom.isNonTextNode(evt.target))
return this.rootNode.findNode(this.getChildControlBaseId(evt.target));},_loadNodes:function()
{if(dnn.getVar(this.ns+'_json').length>0){var json=dnn.evalJSON(dnn.getVar(this.ns+'_json'));}if(json)
{this.nodes=json.nodes;this.rootNode={};this.rootNode.nodes=this.nodes;this.rootNode.id=this.ns;this.rootNode=new dnn.controls.JSONNode(this.rootNode,'root',0);}},_setupKeyHandling:function()
{if(this.container.tabIndex<=0)
{this.container.tabIndex=0;this.addHandlers(this.container,{"keydown":this.keyboardHandler,"focus":this.focusHandler,"blur":this.blurHandler},this);}
else
{var txt=document.createElement('input');txt.type='text';txt.style.width=0;txt.style.height=0;txt.style.background='transparent';txt.style.border=0;txt.style.positioning='absolute';if(dnn.dom.browser.isType(dnn.dom.browser.Safari))
{txt.style.width='1px';txt.style.height='1px';txt.style.left='-999em';this.addHandlers(txt,{"keydown":this.keyboardHandler},this);this.addHandlers(this.container.parentNode,{"keypress":this.safariKeyHandler},this);}
else
this.addHandlers(txt,{"keypress":this.keyboardHandler},this);this.addHandlers(txt,{"focus":this.focusHandler,"blur":this.blurHandler},this);this.container.parentNode.appendChild(txt);}}}
dnn.controls.DNNMenu.registerClass('dnn.controls.DNNMenu',dnn.controls.control);dnn.controls.DNNMenuBuilder=function(menu,mNode,ctr)
{this.menu=menu;this.mNode=mNode;this.isVertical=menu.isNodeVertical(mNode);this.container=ctr;this.row=null;this.subcont=null;this.alreadyRendered=false;}
dnn.controls.DNNMenuBuilder.prototype={appendChild:function(ctl,isNewCell)
{this.subcont.appendChild(ctl);},newCell:function(){},newCont:function()
{if(this.isVertical)
this.row=this.menu.createChildControl('div',this.mNode.id,'ctr');else
this.row=this.menu.createChildControl('span',this.mNode.id,'ctr');this.subcont=this.row;this.container.appendChild(this.subcont);},newRow:function()
{},createSubMenu:function()
{return this.menu.createChildControl('DIV',this.mNode.id,'sub');}}
dnn.controls.DNNMenuBuilder.registerClass('dnn.controls.DNNMenuBuilder');dnn.controls.DNNMenuTableBuilder=function(menu,node,cont)
{dnn.controls.DNNMenuTableBuilder.initializeBase(this,[menu,node,cont]);}
dnn.controls.DNNMenuTableBuilder.prototype={appendChild:function(ctl,isNewCell)
{if(isNewCell)
this.newCell();this.subcont.appendChild(ctl);},newCont:function()
{this.subcont=this.newCell();},newCell:function()
{var td=dnn.dom.createElement('td');this.row.appendChild(td);this.subcont=td;return td;},newRow:function()
{this.row=this.menu.createChildControl('tr',this.mNode.id,'ctr');var tb=dnn.dom.getByTagName('TBODY',this.container);tb[0].appendChild(this.row);},createSubMenu:function()
{var subMenu=this.menu.createChildControl('table',this.mNode.id,'sub');subMenu.border=0;subMenu.cellPadding=0;subMenu.cellSpacing=0;subMenu.appendChild(dnn.dom.createElement('tbody'));return subMenu;}}
dnn.controls.DNNMenuTableBuilder.registerClass('dnn.controls.DNNMenuTableBuilder',dnn.controls.DNNMenuBuilder);dnn.controls.DNNMenuListBuilder=function(menu,node,cont)
{dnn.controls.DNNMenuListBuilder.initializeBase(this,[menu,node,cont]);this.alreadyRendered=true;this.row=dnn.dom.getById(this.menu.getChildControlId(this.mNode.id,'ctr'),this.menu.container);this.menu.registerChildControl(this.row,this.mNode.id);this._setStyles(this.row);this.subcont=this.row;this.subMenu=dnn.dom.getById(this.menu.getChildControlId(this.mNode.id,'sub'),this.menu.container);if(this.subMenu)
{this.menu.registerChildControl(this.subMenu,this.mNode.id);this._setStyles(this.subMenu);}}
dnn.controls.DNNMenuListBuilder.prototype={appendChild:function(ctl,isNewCell){},newCont:function(){},createSubMenu:function()
{return this.subMenu;},_setStyles:function(ctl)
{if(this.menu.isNodeVertical(this.mNode)==false)
ctl.style.display='inline';ctl.style.listStyle='none';}}
dnn.controls.DNNMenuListBuilder.registerClass('dnn.controls.DNNMenuListBuilder',dnn.controls.DNNMenuBuilder);dnn.controls.DNNMenuNode=function(node)
{dnn.controls.DNNMenuNode.initializeBase(this,[node]);this._addAbbr({breadcrumb:'bcrumb',clickAction:'ca',imageIndex:'iIdx',urlIndex:'uIdx'});this.hover=false;this.expanded=null;this.selected=node.getAttribute('selected','0')=='1'?true:null;this.breadcrumb=node.getAttribute('bcrumb','0')=='1'?true:null;this.clickAction=node.getAttribute('ca',dnn.controls.action.postback);this.imageIndex=new Number(node.getAttribute('iIdx','-1'));this.urlIndex=new Number(node.getAttribute('uIdx','-1'));this.lhtml=node.getAttribute('lhtml','');this.rhtml=node.getAttribute('rhtml','');}
dnn.controls.DNNMenuNode.prototype={childNodes:function(iIndex)
{if(this.node.nodes(iIndex)!=null)
return new dnn.controls.DNNMenuNode(this.node.nodes(iIndex));},getUrl:function(menu)
{if(this.urlIndex>-1)
return menu.urlList[this.urlIndex]+this.url;else
return this.url;}}
dnn.controls.DNNMenuNode.registerClass('dnn.controls.DNNMenuNode',dnn.controls.DNNNode);Type.registerNamespace('dnn.controls');dnn.controls.menuRenderMode=function(){};dnn.controls.menuRenderMode.prototype={normal:0,notables:1,unorderdlist:2,downlevel:3}
dnn.controls.menuRenderMode.registerEnum("dnn.controls.menuRenderMode");
