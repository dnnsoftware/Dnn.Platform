
Type.registerNamespace('dnn.controls');dnn.controls.orient=function(){};dnn.controls.orient.prototype={horizontal:0,vertical:1}
dnn.controls.orient.registerEnum("dnn.controls.orient");dnn.controls.action=function(){};dnn.controls.action.prototype={postback:0,expand:1,none:2,nav:3}
dnn.controls.action.registerEnum("dnn.controls.action");dnn.extend(dnn.controls,{version:new Number('02.03'),pns:'dnn',ns:'controls',isLoaded:false,controls:[],toolbars:[],_behaviorIDs:[],find:function(behaviorID)
{return this.controls[this._behaviorIDs[behaviorID]];}});dnn.controls.control=function(ctl)
{dnn.controls.control.initializeBase(this,[ctl]);dnn.controls.controls[ctl.id]=this;this.behaviorID='';this.ns=ctl.id;this.container=ctl;this._props=null;this._childControls=[];this._childControlIDs=[];this._handlerControls=[];}
dnn.controls.control.prototype={initialize:function(behaviorID)
{dnn.controls.control.callBaseMethod(this,'initialize');if(behaviorID)
this.behaviorID=behaviorID;else
this.behaviorID=this.getProp('bid','');if(this.behaviorID.length>0)
dnn.controls._behaviorIDs[this.behaviorID]=this.ns;},getProp:function(name,defVal)
{if(this._props==null)
{this._props={};var p=dnn.getVar(this.ns+'_p');if(p)
{this._props=dnn.evalJSON(p);if(dnn.dom.browser.isType(dnn.dom.browser.Mozilla)==false)
dnn.setVar(this.ns+'_p','');}}
var val=this._props[name];if(val==undefined||val=='')
return defVal;else
return val;},addHandlers:function(element,events,handlerOwner)
{this._handlerControls.push(element);$addHandlers(element,events,handlerOwner);},getChildControlId:function(id,prefix)
{return this.ns+prefix+id;},createChildControl:function(tag,id,prefix)
{var ctl=dnn.dom.createElement(tag);ctl.ns=this.ns;ctl.id=this.getChildControlId(id,prefix);this.registerChildControl(ctl,id);return ctl;},registerChildControl:function(ctl,id)
{this._childControlIDs[ctl.id]=id;this._childControls[ctl.id]=ctl;},getChildControl:function(id,prefix)
{var newId=this.ns+prefix+id;if(this._childControls[newId]!=null)
return this._childControls[newId];else
return $get(newId);},getChildControlBaseId:function(ctl)
{while(ctl.id.length==0&&ctl.parentNode)
{ctl=ctl.parentNode;}
return this._childControlIDs[ctl.id];},add_handler:function(name,handler)
{this.get_events().addHandler(name,handler);},remove_handler:function(name,handler)
{this.get_events().removeHandler(name,handler);},invoke_handler:function(name,args)
{var h=this.get_events().getHandler(name);if(args==null)
args=Sys.EventArgs.Empty;if(h)
h(this,args);},invoke_compatHandler:function(name)
{var ret=true;var h;var evts=this.get_events()._getEvent(name);if(evts)
{var argString='';for(var i=1;i<arguments.length;i++)
{if(i>1)
argString+=',';argString+='arguments['+i+']';}
for(var i=0;i<evts.length;i++)
{h=evts[i];ret=(eval('h('+argString+')')!=false);if(ret==false)
return ret;}}
return true;},getDynamicEventObject:function(name,handler)
{var eh={};eh[name]=handler;return eh;},callBackFail:function(result,ctx,req)
{this.invoke_handler('callBackFail',new dnn.controls.DNNCallbackEventArgs(result,ctx,req));alert(result);},dispose:function()
{this._childControls=null;this._childControlIDs=null;for(var i=0;i<this._handlerControls.length;i++)
{$clearHandlers(this._handlerControls[i]);this._handlerControls[i]=null;}
this.container=null;this._handlerControls=null;dnn.controls.control.callBaseMethod(this,'dispose');},_getEventName:function(s)
{if(s.indexOf('on')==0)
return s.substring(2);return s;}}
dnn.controls.control.registerClass('dnn.controls.control',Sys.UI.Control);dnn.controls.DNNNode=function(node)
{this._abbr={target:'tar',toolTip:'tTip',imageIndex:'imgIdx',image:'img'};if(node!=null)
{this.node=node;this.id=node.getAttribute('id','');this.key=node.getAttribute('key','');this.text=node.getAttribute('txt','');this.url=node.getAttribute('url','');this.js=node.getAttribute('js','');this.target=node.getAttribute('tar','');this.toolTip=node.getAttribute('tTip','');this.enabled=node.getAttribute('enabled','1')!='0';this.css=node.getAttribute('css','');this.cssSel=node.getAttribute('cssSel','');this.cssHover=node.getAttribute('cssHover','');this.cssIcon=node.getAttribute('cssIcon','');this.hasNodes=node.childNodeCount()>0;this.hasPendingNodes=(node.getAttribute('hasNodes','0')=='1'&&this.hasNodes==false);this.imageIndex=new Number(node.getAttribute('imgIdx','-1'));this.image=node.getAttribute('img','');this.level=this.getNodeLevel();this.isBreak=node.getAttribute('isBreak','0')=='1'?true:false;}}
dnn.controls.DNNNode.prototype={_getAbbr:function(name)
{if(this._abbr[name])
return this._abbr[name];return name;},_addAbbr:function(dict)
{for(var prop in dict)
this._abbr[prop]=dict[prop];},childNodeCount:function()
{return this.node.childNodeCount();},getNodeLevel:function()
{return this.getParentNodes().length;},getParentNodes:function()
{var nodes=[];var node=this.node;while(node!=null)
{node=node.parentNode();if(node==null||node.nodeName()=='root')
break;nodes.push(node);}
return nodes;},update:function(prop)
{if(prop!=null)
{var type=typeof(this[prop]);var key=prop;if(this._abbr[prop])
key=this._abbr[prop];if(type=='string'||type=='number'||this[prop]==null)
this.node.setAttribute(prop,this[prop]);else if(type=='boolean')
this.node.setAttribute(prop,new Number(this[prop]));}
else
{for(prop in this)
this.update(prop);}}}
dnn.controls.DNNNode.registerClass('dnn.controls.DNNNode');dnn.controls.JSONNode=function(node,nodeName,nodeIndex,path)
{dnn.extend(this,node);this._nodeName=nodeName;this._nodeDictionary=null;this._nodeIndex=nodeIndex;this._nodePath=nodeIndex.toString();if(path==null)
this._nodePath='';else if(path.length>0)
this._nodePath=path+'-'+nodeIndex;if(nodeName=='root')
{this._nodeDictionary=[];this.setupJSONNodes(this,this,node.nodes);}}
dnn.controls.JSONNode.prototype={getAttribute:function(name,def)
{def=(def)?def:'';return this[name]==null?def:this[name];},setAttribute:function(name,val)
{this[name]=val;},parentNode:function()
{return this._parentNode;},hasChildNodes:function()
{return this.nodes.length>0;},getNodeIndex:function()
{return this._nodeIndex;},getNodePath:function()
{return this._nodePath;},childNodeCount:function()
{return this.nodes.length;},childNodes:function(idx)
{return this.nodes[idx];},nodeName:function()
{return this._nodeName;},rootNode:function()
{return this._parentNode==null?this:this._parentNode.rootNode();},findNode:function(id)
{if(arguments.length==3)
id=arguments[2];return this.rootNode()._nodeDictionary[id];},getJSON:function(node)
{if(node==null)
node=this;var json='{';json+=this.getJSONAttributes(node,':',',')+',nodes:[';for(var i=0;i<node.childNodeCount();i++)
{if(i>0)
json+=',';json+=this.getJSON(node.childNodes(i));}
json+=']}';return json;},getXml:function(node)
{if(node==null)
node=this;var xml='';xml='<'+node.nodeName()+this.getXmlAttributes(node)+'>';for(var i=0;i<node.childNodeCount();i++)
{xml+=this.getXml(node.childNodes(i));}
xml=xml+'</'+node.nodeName()+'>';return xml;},getJSONAttributes:function(node)
{var ret='';for(var attr in node)
{if(typeof(node[attr])!='function'&&attr.substring(0,1)!='_'&&attr!='nodes')
{if(ret.length>0)
ret+=',';ret+=' '+attr+':"'+dnn.encodeJSON(node.getAttribute(attr).toString())+'"';}}
return ret;},getXmlAttributes:function(node)
{var ret='';for(var attr in node)
{if(typeof(node[attr])!='function'&&attr.substring(0,1)!='_'&&attr!='nodes')
{if(ret.length>0)
ret+=' ';ret+=' '+attr+'="'+dnn.encodeHTML(node.getAttribute(attr))+'"';}}
return ret;},setupJSONNodes:function(root,parent,nodes)
{var jnode;for(var i=0;i<nodes.length;i++)
{jnode=new dnn.controls.JSONNode(nodes[i],'n',i,parent.getNodePath());jnode._parentNode=parent;root._nodeDictionary[jnode.id]=jnode;nodes[i]=jnode;this.setupJSONNodes(root,jnode,jnode.nodes);}}}
dnn.controls.JSONNode.registerClass('dnn.controls.JSONNode');dnn.controls.DNNNodeEventArgs=function(node)
{dnn.controls.DNNNodeEventArgs.initializeBase(this);this._node=node;}
dnn.controls.DNNNodeEventArgs.prototype={get_node:function()
{return this._node;}}
dnn.controls.DNNNodeEventArgs.registerClass('dnn.controls.DNNNodeEventArgs',Sys.CancelEventArgs);dnn.controls.DNNTabStripEventArgs=function(tab)
{dnn.controls.DNNTabStripEventArgs.initializeBase(this);this._tab=tab;}
dnn.controls.DNNTabStripEventArgs.prototype={get_tab:function()
{return this._tab;}}
dnn.controls.DNNTabStripEventArgs.registerClass('dnn.controls.DNNTabStripEventArgs',Sys.CancelEventArgs);dnn.controls.DNNToolBarEventArgs=function(btn)
{dnn.controls.DNNToolBarEventArgs.initializeBase(this);this._btn=btn;}
dnn.controls.DNNToolBarEventArgs.prototype={get_button:function()
{return this._btn;}}
dnn.controls.DNNToolBarEventArgs.registerClass('dnn.controls.DNNToolBarEventArgs',Sys.CancelEventArgs);dnn.controls.DNNCallbackEventArgs=function(result,ctx,req)
{dnn.controls.DNNCallbackEventArgs.initializeBase(this);this._result=result;this._context=ctx;this._req=req;}
dnn.controls.DNNCallbackEventArgs.prototype={get_result:function()
{return this._result;},get_context:function()
{return this._context;},get_request:function()
{return this._req;}}
dnn.controls.DNNCallbackEventArgs.registerClass('dnn.controls.DNNCallbackEventArgs',Sys.EventArgs);dnn.controls._submitComponent=function()
{dnn.controls._submitComponent.initializeBase(this);}
dnn.controls._submitComponent.prototype={onsubmit:function()
{var h=this.get_events().getHandler('submit');if(h)
h(this,Sys.EventArgs.Empty);},add_handler:function(handler)
{this.get_events().addHandler('submit',handler);},remove_handler:function(handler)
{this.get_events().removeHandler('submit',handler);},dispose:function()
{dnn.controls._submitComponent.callBaseMethod(this,'dispose');}}
dnn.controls._submitComponent.registerClass('dnn.controls._submitComponent',Sys.Component);if(dnn.controls.submitComp==null)
dnn.controls.submitComp=new dnn.controls._submitComponent();
