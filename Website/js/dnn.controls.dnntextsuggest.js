
Type.registerNamespace('dnn.controls');dnn.extend(dnn.controls,{initTextSuggest:function(ctl)
{if(ctl)
{var ts=new dnn.controls.DNNTextSuggest(ctl);ts.initialize();return ts;}}});dnn.controls.DNNTextSuggest=function(o)
{dnn.controls.DNNTextSuggest.initializeBase(this,[o]);this.resultCtr=null;this.tscss=this.getProp('tscss','');this.css=this.getProp('css','');this.cssChild=this.getProp('csschild','');this.cssHover=this.getProp('csshover','');this.cssSel=this.getProp('csssel','');this.sysImgPath=this.getProp('sysimgpath','');this.workImg='dnnanim.gif';this.target=this.getProp('target','');this.defaultJS=this.getProp('js','');this.postBack=this.getProp('postback','');this.callBack=this.getProp('callback','');this.callBackStatFunc=this.getProp('callbackSF','');if(this.callBackStatFunc.length>0)
this.add_handler('callBackStatus',eval(this.callBackStatFunc));this.rootNode=null;this.selNode=null;this.selIndex=-1;this.lookupDelay=this.getProp('ludelay','500');this.lostFocusDelay=this.getProp('lfdelay','500');this.inAnimObj=null;this.inAnimType=null;this.prevText='';this.addHandlers(o,{'keyup':this.keyUp,'keypress':this.keyPress,'blur':this.onblur,'focus':this.onfocus},this);o.setAttribute('autocomplete','off');this.delimiter=this.getProp('del','');this.idtoken=this.getProp('idtok','');this.maxRows=new Number(this.getProp('maxRows','10'));if(this.maxRows==0)
this.maxRows=9999;this.minChar=new Number(this.getProp('minChar','1'));this.caseSensitive=this.getProp('casesens','0')=='1';this.prevLookupText='';this.prevLookupOffset=0;this._blurHideDelegate=dnn.createDelegate(this,this.blurHide);this._doLookupDelegate=dnn.createDelegate(this,this.doLookup);}
dnn.controls.DNNTextSuggest.prototype={keyPress:function(e)
{if(e.charCode==KEY_RETURN)
{e.stopPropagation();e.preventDefault();return false;}},onblur:function(e)
{dnn.doDelay(this.ns+'ob',this.lostFocusDelay,this._blurHideDelegate);},onfocus:function(e)
{dnn.cancelDelay(this.ns+'ob');},blurHide:function(e)
{this.clearResults(true);},keyUp:function(e)
{this.prevText=this.container.value;if(e.keyCode==KEY_UP_ARROW)
this.setNodeIndex(this.selIndex-1);else if(e.keyCode==KEY_DOWN_ARROW)
this.setNodeIndex(this.selIndex+1);else if(e.keyCode==KEY_RETURN)
{if(this.selIndex>-1)
{this.selectNode(this.getNodeByIndex(this.selIndex));this.clearResults(true);}}
else if(e.keyCode==KEY_ESCAPE)
this.clearResults(true);else
{dnn.cancelDelay(this.ns+'kd');dnn.doDelay(this.ns+'kd',this.lookupDelay,this._doLookupDelegate);}},nodeMOver:function(evt)
{var node=this._findEventNode(evt);if(node!=null)
{var tsNode=new dnn.controls.DNNTextSuggestNode(node);tsNode.hover=true;this.assignCss(tsNode);}},nodeMOut:function(evt)
{var node=this._findEventNode(evt);if(node!=null)
{var tsNode=new dnn.controls.DNNTextSuggestNode(node);tsNode.hover=false;this.assignCss(tsNode);}},nodeClick:function(evt)
{var node=this._findEventNode(evt);if(node!=null)
{var tsNode=new dnn.controls.DNNTextSuggestNode(node);this.selectNode(tsNode);this.clearResults(true);}},getTextOffset:function()
{var offset=0;if(this.delimiter.length>0)
{var ary=this.container.value.split(this.delimiter);var pos=dnn.dom.cursorPos(this.container);var len=0;for(offset=0;offset<ary.length-1;offset++)
{len+=ary[offset].length+1;if(len>pos)
break;}}
return offset;},setText:function(s,id)
{if(this.idtoken.length>0)
s+=' '+this.idtoken.replace('~',id);if(this.delimiter.length>0)
{var ary=this.container.value.split(this.delimiter);ary[this.getTextOffset()]=s;this.container.value=ary.join(this.delimiter);if(this.container.value.lastIndexOf(this.delimiter)!=this.container.value.length-1)
this.container.value+=this.delimiter;}
else
this.container.value=s;this.prevText=this.container.value;},getText:function()
{if(this.delimiter.length>0&&this.container.value.indexOf(this.delimiter)>-1)
{var ary=this.container.value.split(this.delimiter);return ary[this.getTextOffset()];}
else
return this.container.value;},formatText:function(s)
{if(this.caseSensitive)
return s;else
return s.toLowerCase();},highlightNode:function(index,bHighlight)
{if(index>-1)
{var tsNode=this.getNodeByIndex(index);tsNode.hover=bHighlight;this.assignCss(tsNode);}},getNodeByIndex:function(index)
{var element=this.resultCtr.childNodes[index];if(element)
{var node=this.rootNode.findNode(this.getChildControlBaseId(element));if(node)
return new dnn.controls.DNNTextSuggestNode(node);}},setNodeIndex:function(index)
{if(index>-1&&index<this.resultCtr.childNodes.length)
{this.highlightNode(this.selIndex,false);this.selIndex=index;this.highlightNode(this.selIndex,true);}},selectNode:function(tsNode)
{var arg=new dnn.controls.DNNNodeEventArgs(tsNode);this.invoke_handler('click',arg);if(arg.get_cancel())
return;if(this.selNode!=null)
{this.selNode.selected=null;this.assignCss(this.selNode);}
if(tsNode.selected)
{tsNode.selected=null;this.assignCss(tsNode);}
else
{tsNode.selected=true;this.assignCss(tsNode);}
this.selNode=tsNode;if(tsNode.selected)
{this.setText(tsNode.text,tsNode.id);var js='';if(this.defaultJS.length>0)
js=this.defaultJS;if(tsNode.js.length>0)
js=tsNode.js;if(js.length>0)
{if(eval(js)==false)
return;}
if(tsNode.clickAction==null||tsNode.clickAction==dnn.controls.action.postback)
eval(this.postBack.replace('[TEXT]',this.getText()));else if(tsNode.clickAction==dnn.controls.action.nav)
dnn.dom.navigate(tsNode.url,tsNode.target.length>0?tsNode.target:this.target);}
return true;},positionMenu:function()
{var dims=new dnn.dom.positioning.dims(this.container);this.resultCtr.style.left=dims.l-dnn.dom.positioning.bodyScrollLeft();this.resultCtr.style.top=dims.t+dims.h;},showResults:function()
{if(this.resultCtr)
this.resultCtr.style.display='';},hideResults:function()
{if(this.resultCtr)
this.resultCtr.style.display='none';dnn.dom.positioning.placeOnTop(this.resultCtr,false,this.sysImgPath+'spacer.gif');},clearResults:function(hide)
{if(this.resultCtr!=null)
this.resultCtr.innerHTML='';this.selIndex=-1;this.selNode=null;if(hide)
this.hideResults();},clear:function()
{this.clearResults();this.setText('','');},doLookup:function()
{if(this.getText().length>=this.minChar)
{if(this.needsLookup())
{this.prevLookupOffset=this.getTextOffset();this.prevLookupText=this.formatText(this.getText());eval(this.callBack.replace('[TEXT]',this.prevLookupText));}
else
this.renderResults(null);}
else
this.clearResults();},needsLookup:function()
{if(this.rootNode==null)
return true;if(this.prevLookupOffset!=this.getTextOffset()||this.rootNode==null)
return true;if(this.formatText(this.getText()).indexOf(this.prevLookupText)==0)
{if(this.rootNode.childNodeCount()<this.maxRows)
return false;var node;var oneMatch=false;var text=this.getText();for(var i=0;i<this.maxRows;i++)
{node=new dnn.controls.DNNTextSuggestNode(this.rootNode.childNodes(i));if(this.formatText(node.text).indexOf(text)==0)
{if(i==0)
return false;else
oneMatch=true;}
else if(oneMatch)
return false;}}
return true;},renderResults:function(result)
{if(result!=null)
{var json=dnn.evalJSON("{"+result+"}");this.nodes=json.nodes;this.rootNode={};this.rootNode.nodes=this.nodes;this.rootNode.id=this.ns;this.rootNode=new dnn.controls.JSONNode(this.rootNode,'root',0);}
if(this.rootNode!=null)
{if(this.resultCtr==null)
this.renderContainer();this.clearResults();for(var i=0;i<this.rootNode.childNodeCount();i++)
this.renderNode(this.rootNode.childNodes(i),this.resultCtr);this.showResults();}
dnn.dom.positioning.placeOnTop(this.resultCtr,true,this.sysImgPath+'spacer.gif');},renderContainer:function()
{this.resultCtr=document.createElement('DIV');this.container.parentNode.appendChild(this.resultCtr);this.resultCtr.className=this.tscss;this.resultCtr.style.position='absolute';this.positionMenu();},renderNode:function(node,ctr)
{var tsNode;tsNode=new dnn.controls.DNNTextSuggestNode(node);if(this.formatText(tsNode.text).indexOf(this.formatText(this.getText()))==0&&ctr.childNodes.length<this.maxRows)
{var newCtr=this.createChildControl('div',tsNode.id,'ctr');newCtr.appendChild(this.renderText(tsNode));if(tsNode.enabled)
{this.addHandlers(newCtr,{'click':this.nodeClick,'mouseover':this.nodeMOver,'mouseout':this.nodeMOut},this);}
if(tsNode.toolTip.length>0)
newCtr.title=tsNode.toolTip;ctr.appendChild(newCtr);this.assignCss(tsNode);}},renderText:function(tsNode)
{var span=this.createChildControl('SPAN',tsNode.id,'t');span.innerHTML=tsNode.text;span.style.cursor='pointer';return span;},assignCss:function(tsNode)
{var oCtr=this.getChildControl(tsNode.id,'ctr');var nodeCss=this.css;if(tsNode.css.length>0)
nodeCss=tsNode.css;if(tsNode.hover)
nodeCss+=' '+(tsNode.cssHover.length>0?tsNode.cssHover:this.cssHover);if(tsNode.selected)
nodeCss+=' '+(tsNode.cssSel.length>0?tsNode.cssSel:this.cssSel);oCtr.className=nodeCss;},callBackStatus:function(result,ctx,req)
{var ts=ctx;ts.invoke_compatHandler('callBackStatus',result,ctx,req);},callBackSuccess:function(result,ctx,req)
{var ts=ctx;ts.invoke_compatHandler('callBackStatus',result,ctx,req);ts.invoke_handler('callBackSuccess',new dnn.controls.DNNCallbackEventArgs(result,ctx,req));ts.renderResults(result);},callBackFail:function(result,ctx,req)
{var ts=ctx;ts.invoke_handler('callBackFail',new dnn.controls.DNNCallbackEventArgs(result,ctx,req));},_findEventNode:function(evt)
{return this.rootNode.findNode(this.getChildControlBaseId(evt.target));},dispose:function()
{this._blurHideDelegate=null;this._doLookupDelegate=null;dnn.controls.DNNTextSuggest.callBaseMethod(this,'dispose');}}
dnn.controls.DNNTextSuggest.registerClass('dnn.controls.DNNTextSuggest',dnn.controls.control);dnn.controls.DNNTextSuggestNode=function(node)
{dnn.controls.DNNTextSuggestNode.initializeBase(this,[node]);this.hover=false;this.selected=node.getAttribute('selected','0')=='1'?true:null;this.clickAction=node.getAttribute('ca',dnn.controls.action.none);}
dnn.controls.DNNTextSuggestNode.prototype={childNodes:function(index)
{if(this.node.childNodes[index]!=null)
return new dnn.controls.DNNTextSuggestNode(this.node.childNodes[index]);}}
dnn.controls.DNNTextSuggestNode.registerClass('dnn.controls.DNNTextSuggestNode',dnn.controls.DNNNode);