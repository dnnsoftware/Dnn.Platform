
Type.registerNamespace('dnn.controls');dnn.extend(dnn.controls,{initTree:function(ctl)
{if(ctl)
{var tree=new dnn.controls.DNNTree(ctl);tree.initialize();return tree;}}});dnn.controls.DNNTree=function(o)
{dnn.controls.DNNTree.initializeBase(this,[o]);this.rootNode=null;this.nodes=[];this._loadNodes();this.hoverTreeNode=null;this.selTreeNode=null;this.css=this.getProp('css','');this.cssChild=this.getProp('csschild','');this.cssHover=this.getProp('csshover','');this.cssSel=this.getProp('csssel','');this.cssIcon=this.getProp('cssicon','');this.sysImgPath=this.getProp('sysimgpath','images/');this.imageList=this.getProp('imagelist','').split(',');this.expandImg=this.getProp('expimg','');this.workImg=this.getProp('workimg','dnnanim.gif');this.animf=new Number(this.getProp('animf','5'));this.collapseImg=this.getProp('colimg','');this.indentWidth=new Number(this.getProp('indentw','10'));if(this.indentWidth==0)
this.indentWidth=10;this.checkBoxes=this.getProp('checkboxes','0')=='1';this.checkBoxMode=new Number(this.getProp('cbm','0'));this.target=this.getProp('target','');this.defaultJS=this.getProp('js','');this.postBack=this.getProp('postback','');this.callBack=this.getProp('callback','');this.callBackStatFunc=this.getProp('callbackSF','');if(this.callBackStatFunc.length>0)
this.add_handler('callBackStatus',eval(this.callBackStatFunc));this.expImgWidth=new Number(this.getProp('expcolimgw','12'));kbCtl=this.container;if(this.container.tabIndex<=0)
this.container.tabIndex=0;else
{kbCtl=document.createElement('input');kbCtl.type='text';kbCtl.style.width=0;kbCtl.style.height=0;kbCtl.style.background='transparent';kbCtl.style.border=0;kbCtl.style.positioning='absolute';kbCtl.style.left='-999em';this.container.parentNode.appendChild(kbCtl);}
this.addHandlers(kbCtl,{"keydown":this.keydownHandler,"focus":this.focusHandler},this);this._onsubmitDelegate=Function.createDelegate(this,this._onsubmit);dnn.controls.submitComp.add_handler(this._onsubmitDelegate);}
dnn.controls.DNNTree.prototype={initialize:function()
{dnn.controls.DNNTree.callBaseMethod(this,'initialize');this.generateTreeHTML();},focusHandler:function(e)
{var tNode=this.hoverTreeNode;if(tNode==null)
tNode=new dnn.controls.DNNTreeNode(this.rootNode.childNodes(0));this.hoverNode(tNode);this.container.onfocus=null;},keydownHandler:function(e)
{var dir=0;var axis='';if(e.keyCode==KEY_UP_ARROW)
{dir=-1;axis='y';}
if(e.keyCode==KEY_DOWN_ARROW)
{dir=1;axis='y';}
if(e.keyCode==KEY_LEFT_ARROW)
{dir=-1;axis='x';}
if(e.keyCode==KEY_RIGHT_ARROW)
{dir=1;axis='x';}
if(dir!=0)
{var tNode=this.hoverTreeNode;var node;if(tNode==null)
tNode=new dnn.controls.DNNTreeNode(this.rootNode.childNodes(0));if(axis=='x')
{if(dir==-1)
{if(tNode.hasNodes&&tNode.expanded)
this.collapseNode(tNode);else
node=tNode.node.parentNode();}
if(dir==1)
{if(tNode.hasNodes||tNode.hasPendingNodes)
{if(tNode.expanded!=true)
this.expandNode(tNode);else
node=tNode.node.childNodes(0);}}}
else if(axis=='y')
{var iNodeIndex=tNode.node.getNodeIndex('id');var parentNode=tNode.node.parentNode();if(tNode.hasNodes&&tNode.expanded&&dir>0)
node=tNode.node.childNodes(0);else if(iNodeIndex+dir<0)
node=parentNode;else if(iNodeIndex+dir<parentNode.childNodeCount())
{node=parentNode.childNodes(iNodeIndex+dir);if(dir==-1)
{var tNode2=new dnn.controls.DNNTreeNode(node);while(tNode2.expanded)
{if(tNode2.node.childNodeCount()==0)
break;node=tNode2.node.childNodes(tNode2.node.childNodeCount()-1);tNode2=new dnn.controls.DNNTreeNode(node);}}}
else if(parentNode.nodeName()!='root')
{var iNodeIndex=parentNode.getNodeIndex('id');var tempParent=parentNode;if(dir==1)
{while(tempParent.nodeName()!='root'&&iNodeIndex+dir>=tempParent.parentNode().childNodeCount())
{tempParent=tempParent.parentNode();iNodeIndex=tempParent.getNodeIndex('id');}}
if(tempParent.nodeName()!='root')
node=tempParent.parentNode().childNodes(iNodeIndex+1);}}
if(node!=null&&node.nodeName()!='root')
this.hoverNode(new dnn.controls.DNNTreeNode(node));return false;}
if(e.keyCode==KEY_RETURN&&this.hoverTreeNode!=null)
{this.selectNode(this.hoverTreeNode);return false;}},hoverNode:function(tNode)
{if(this.hoverTreeNode!=null)
{this.hoverTreeNode.hover=false;this.assignCss(this.hoverTreeNode);}
tNode.hover=true;this.assignCss(tNode);this.hoverTreeNode=tNode;},getXml:function()
{return this.rootNode.getXml();},expandNode:function(tNode)
{var ctr=this.getChildControl(tNode.id,'pctr');var expandCol=this.getChildControl(tNode.id,'expcol');expandCol.src=this.expandImg;tNode.expanded=true;tNode.update();this.update();if(tNode.hasPendingNodes)
{var sXml=tNode.node.getXml();tNode.tree=this;if(this.workImg!=null)
{var icon=this.getChildControl(tNode.id,'icn');if(icon)
icon.src=this.sysImgPath+this.workImg;}
if(this.callBack.indexOf('[NODEXML]')>-1)
eval(this.callBack.replace('[NODEXML]',dnn.escapeForEval(sXml)));else
eval(this.callBack.replace('[NODEID]',tNode.id));tNode.hasPendingNodes=false;tNode.hasNodes=true;this.hoverTreeNode=tNode;}
else
{dnn.dom.expandElement(ctr,this.animf);}
return true;},collapseNode:function(tNode)
{var ctr=this.getChildControl(tNode.id,'pctr');var expandCol=this.getChildControl(tNode.id,'expcol');dnn.dom.collapseElement(ctr,this.animf);expandCol.src=this.collapseImg;tNode.expanded=null;tNode.update();this.update();return true;},selectNode:function(tNode)
{var arg=new dnn.controls.DNNNodeEventArgs(tNode);this.invoke_handler('click',arg);if(arg.get_cancel())
return;if(this.selTreeNode!=null&&this.checkBoxes==false)
{this.selTreeNode.selected=null;this.assignCss(this.selTreeNode);this.selTreeNode.update('selected');}
if(tNode.selected)
{tNode.selected=null;this.assignCss(tNode);}
else
{tNode.selected=true;this.hoverTreeNode=tNode;this.assignCss(tNode);}
tNode.update('selected');this.selTreeNode=tNode;this.update();var chk=this.getChildControl(tNode.id,'chk');if(chk!=null)
chk.checked=tNode.selected;if(tNode.selected)
{var js='';if(this.defaultJS.length>0)
js=this.defaultJS;if(tNode.js.length>0)
js=tNode.js;if(js.length>0)
{if(eval(js)==false)
return;}
if(tNode.clickAction==null||tNode.clickAction==dnn.controls.action.postback)
eval(this.postBack.replace('[NODEID]',tNode.id));else if(tNode.clickAction==dnn.controls.action.nav)
dnn.dom.navigate(tNode.url,tNode.target.length>0?tNode.target:this.target);else if(tNode.clickAction==dnn.controls.action.expand)
{if(tNode.hasNodes||tNode.hasPendingNodes)
{if(tNode.expanded)
this.collapseNode(tNode);else
this.expandNode(tNode);}}}
return true;},selectAllChildren:function(tNode)
{var childTNode;for(var i=0;i<tNode.childNodeCount();i++)
{childTNode=tNode.childNodes(i);if(childTNode.selected!=tNode.selected)
this.selectNode(childTNode);this.selectAllChildren(childTNode);}},assignCss:function(tNode)
{var oText=this.getChildControl(tNode.id,'t');var sNodeCss=this.css;if(tNode.level>0&&this.cssChild.length>0)
sNodeCss=this.cssChild;if(tNode.css.length>0)
sNodeCss=tNode.css;if(tNode.hover)
sNodeCss+=' '+(tNode.cssHover.length>0?tNode.cssHover:this.cssHover);if(tNode.selected)
sNodeCss+=' '+(tNode.cssSel.length>0?tNode.cssSel:this.cssSel);oText.className=sNodeCss;},update:function(force)
{if(force)
{if(this.selTreeNode)
dnn.setVar(this.ns + ':selected', this.selTreeNode.id); dnn.setVar(this.ns + '_json', dnn.decodeHTML(this.rootNode.getJSON()));
}
return true;},_onsubmit:function()
{this.update(true);},callBackStatus:function(result,ctx,req)
{var tNode=ctx;var tree=tNode.tree;tree.invoke_compatHandler('callBackStatus',result,ctx,req);},callBackSuccess:function(result,ctx,req)
{var tNode=ctx;var tree=tNode.tree;var parent=tNode.node;;var json=dnn.evalJSON("{"+result+"}");parent.nodes=json.nodes;parent.setupJSONNodes(parent.rootNode(),parent,parent.nodes);if(tree.workImg!=null)
{var icon=tree.getChildControl(tNode.id,'icn');if(tNode.image!='')
icon.src=tNode.image;else if(tNode.imageIndex>-1)
icon.src=tree.imageList[tNode.imageIndex];}
var ctr=tree.getChildControl(tNode.id,'pctr');tree.renderNode(tNode.node,ctr,true);tree.update();tree.expandNode(new dnn.controls.DNNTreeNode(tNode.node));tree.invoke_compatHandler('callBackStatus',result,ctx,req);tree.invoke_handler('callBackSuccess',new dnn.controls.DNNCallbackEventArgs(result,ctx,req));},callBackFail:function(result,ctx,req)
{var tNode=ctx;var tree=tNode.tree;tree.invoke_handler('callBackFail',new dnn.controls.DNNCallbackEventArgs(result,ctx,req));},nodeExpColClick:function(evt)
{var node=this._findEventNode(evt);if(node!=null)
{var tNode=new dnn.controls.DNNTreeNode(node);var ctr=this.getChildControl(tNode.id,'pctr');if(tNode.expanded)
this.collapseNode(tNode);else
this.expandNode(tNode);}},nodeCheck:function(evt)
{var node=this._findEventNode(evt);if(node!=null)
{var tNode=new dnn.controls.DNNTreeNode(node);if(this.checkBoxMode==2&&this.selTreeNode!=null)
this.selectNode(this.selTreeNode);this.selectNode(tNode);if(this.checkBoxMode==1)
this.selectAllChildren(tNode);}},nodeTextClick:function(evt)
{var node=this._findEventNode(evt);if(node!=null)
{this.selectNode(new dnn.controls.DNNTreeNode(node));}},nodeTextMOver:function(evt)
{var node=this._findEventNode(evt);if(node!=null)
this.hoverNode(new dnn.controls.DNNTreeNode(node));},nodeTextMOut:function(evt)
{var node=this._findEventNode(evt);if(node!=null)
this.assignCss(new dnn.controls.DNNTreeNode(node));},dispose:function()
{this._onsubmitDelegate=null;dnn.controls.DNNTree.callBaseMethod(this,'dispose');},generateTreeHTML:function()
{this.renderNode(null,this.container);},renderNode:function(node,oCont,bExists)
{var oChildCont=oCont;var tNode;if(bExists!=true)
{if(node!=null)
{tNode=new dnn.controls.DNNTreeNode(node);var oNewContainer;oNewContainer=this.createChildControl('DIV',tNode.id,'ctr');if(document.getElementById(oNewContainer.id)==null){oNewContainer.appendChild(this.renderSpacer((this.indentWidth*tNode.level)+((tNode.hasNodes||tNode.hasPendingNodes)?0:this.expImgWidth)));if(tNode.hasNodes||tNode.hasPendingNodes)
oNewContainer.appendChild(this.renderExpCol(tNode));if(this.checkBoxes)
oNewContainer.appendChild(this.renderCheckbox(tNode));var oIconCont=this.renderIconCont(tNode);oNewContainer.appendChild(oIconCont);if(tNode.imageIndex>-1||tNode.image!='')
{oIconCont.appendChild(this.renderIcon(tNode));}
oNewContainer.appendChild(this.renderText(tNode));oCont.appendChild(oNewContainer);this.assignCss(tNode);}}
else
node=this.rootNode;if(tNode!=null&&(tNode.hasNodes||tNode.hasPendingNodes))
{oChildCont=this.createChildControl('DIV',tNode.id,'pctr');if(tNode.expanded!=true)
oChildCont.style.display='none';oCont.appendChild(oChildCont);}}
for(var i=0;i<node.childNodeCount();i++)
this.renderNode(node.childNodes(i),oChildCont);},renderExpCol:function(tNode)
{var img=this.createChildControl('IMG',tNode.id,'expcol');if((tNode.hasNodes||tNode.hasPendingNodes)&&this.expandImg.length)
{if(tNode.expanded)
img.src=this.expandImg;else
img.src=this.collapseImg;}
img.style.cursor='pointer';this.addHandlers(img,{"click":this.nodeExpColClick},this);return img;},renderIconCont:function(tNode)
{var span=this.createChildControl('SPAN',tNode.id,'icnc');if(tNode.cssIcon.length>0)
span.className=tNode.cssIcon;else if(this.cssIcon.length>0)
span.className=this.cssIcon;return span;},renderIcon:function(tNode)
{var img=this.createChildControl('IMG',tNode.id,'icn');if(tNode.image!='')
img.src=tNode.image;else if(tNode.imageIndex>-1)
img.src=this.imageList[tNode.imageIndex];return img;},renderCheckbox:function(tNode)
{var chk=this.createChildControl('INPUT',tNode.id,'chk');chk.type='checkbox';chk.defaultChecked=tNode.selected;chk.checked=tNode.selected;this.addHandlers(chk,{"click":this.nodeCheck},this);return chk;},renderSpacer:function(width)
{var img=document.createElement('IMG');img.src=this.sysImgPath+'spacer.gif';img.width=width;img.height=1;img.style.width=width+'px';img.style.height='1px';return img;},renderText:function(tNode)
{var span=this.createChildControl('SPAN',tNode.id,'t');span.innerHTML=tNode.text;span.style.cursor='pointer';if(tNode.toolTip.length>0)
span.title=tNode.toolTip;if(tNode.enabled||tNode.clickAction==dnn.controls.action.expand)
{if(this.checkBoxes)
this.addHandlers(span,{"click":this.nodeCheck},this);else
this.addHandlers(span,{"click":this.nodeTextClick},this);if(this.cssHover.length>0)
{this.addHandlers(span,{"mouseover":this.nodeTextMOver,"mouseout":this.nodeTextMOut},this);}}
if(tNode.selected)
{this.selTreeNode=tNode;this.hoverTreeNode=tNode;}
return span;},_findEventNode:function(evt)
{return this.rootNode.findNode(this.getChildControlBaseId(evt.target));},_loadNodes:function()
{var json=dnn.evalJSON(dnn.getVar(this.ns+'_json'));if(json)
{this.nodes=json.nodes;this.rootNode={};this.rootNode.nodes=this.nodes;this.rootNode.id=this.ns;this.rootNode=new dnn.controls.JSONNode(this.rootNode,'root',0);}}}
dnn.controls.DNNTree.registerClass('dnn.controls.DNNTree',dnn.controls.control);dnn.controls.DNNTreeNode=function(node)
{dnn.controls.DNNTreeNode.initializeBase(this,[node]);this.hover=false;this.expanded=node.getAttribute('expanded','0')=='1'?true:null;this.selected=node.getAttribute('selected','0')=='1'?true:null;this.clickAction=node.getAttribute('ca',dnn.controls.action.postback);this.imageIndex=new Number(node.getAttribute('imgIdx','-1'));}
dnn.controls.DNNTreeNode.prototype={childNodes:function(iIndex)
{if(this.node.childNodes(iIndex)!=null)
return new dnn.controls.DNNTreeNode(this.node.childNodes(iIndex));}}
dnn.controls.DNNTreeNode.registerClass('dnn.controls.DNNTreeNode',dnn.controls.DNNNode);var DT_ACTION_POSTBACK=0;var DT_ACTION_EXPAND=1;var DT_ACTION_NONE=2;var DT_ACTION_NAV=3;function __dt_DNNTreeNode(ctl)
{var node=dnn.controls.controls[ctl.ns].rootNode.findNode(ctl.nodeid);if(node!=null)
{var tNode=new dnn.controls.DNNTreeNode(node);this.ctl=ctl;this.id=ctl.id;this.key=tNode.key;this.nodeID=ctl.nodeid;this.text=tNode.text;this.serverName=ctl.name;}}