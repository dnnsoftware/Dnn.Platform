
Type.registerNamespace('dnn.controls');dnn.extend(dnn.controls,{initTabStrip:function(ctl)
{if(ctl&&dnn.controls.controls[ctl.id]==null)
{var ts=new dnn.controls.DNNTabStrip(ctl);ts.initialize();return ts;}}});dnn.controls.DNNTabStrip=function(o)
{dnn.controls.DNNTabStrip.initializeBase(this,[o]);this.contentContainer=$get(o.id+'_c');this.resultCtr=null;this.workImg=this.getProp('workImage','');;this.tabRenderMode=new Number(this.getProp('trm','0'));this.callBack=this.getProp('callback','');this.callBackStatFunc=this.getProp('callbackSF','');if(this.callBackStatFunc.length>0)
this.add_handler('callBackStatus',eval(this.callBackStatFunc));this.callbackType=this.getProp('cbtype','0');this.tabClickFunc=this.getProp('tabClickF','');if(this.tabClickFunc.length>0)
this.add_handler('tabClick',eval(this.tabClickFunc));this.selectedIndexFunc=this.getProp('selIdxF','');if(this.selectedIndexFunc.length>0)
this.add_handler('selectedIndexChanged',eval(this.selectedIndexFunc));this.lblCss=this.getProp('css','');this.lblCssSel=this.getProp('csssel',this.lblCss);this.lblCssHover=this.getProp('csshover','');this.lblCssDisabled=this.getProp('cssdisabled','');this.pendingLookupId=null;var json=dnn.evalJSON(dnn.getVar(this.ns+'_json'));if(json)
{this.tabData=json.tabs;this.tabLabelData=json.tablabels;}
this.tabs=[];this.tabIds=[];this.selectedIndex=-1;this.selectedTab=null;var clientId;var tabId;var tab;for(var i=0;i<this.tabData.length;i++)
{clientId=this.tabData[i].tcid;tabId=this.tabData[i].tid;tab=new dnn.controls.DNNTab(this,i,tabId,clientId);tab.selected=(tab.container!=null&&tab.container.style.display!='none');this.tabIds.push(tab.tabId);this.tabs[tabId]=tab;this.addHandlers(tab.tab,{'click':this.tabClick,'mouseover':this.tabMouseOver,'mouseout':this.tabMouseOut},tab);if(tab.selected)
{this.selectedIndex=i;this.selectedTab=tab;}}
this.update();}
dnn.controls.DNNTabStrip.prototype={tabClick:function(evt,element)
{var tab=this;var ts=tab.strip;if(tab.enabled)
{var arg=new dnn.controls.DNNTabStripEventArgs(tab);ts.invoke_handler('tabClick',arg)
if(arg.get_cancel()==false)
ts.showTab(tab.tabId);}},tabMouseOver:function(evt,element)
{if(typeof(dnn)!='undefined')
{this.hovered=true;this.assignCss();}},tabMouseOut:function(evt,element)
{if(typeof(dnn)!='undefined')
{this.hovered=false;this.assignCss();}},setSelectedIndex:function(index)
{return this.showTab(this.tabIds[index]);},showTab:function(tabId)
{var tab;if(this.needsLookup(tabId))
{if(this.pendingLookupId==null)
{tab=this.tabs[tabId];tab.showWork(true);this.pendingLookupId=tabId;if(this.tabRenderMode=='1')
{for(var i=0;i<this.tabIds.length;i++)
{tab=this.tabs[this.tabIds[i]];if(tab.tabId==tabId)
{tab.selected=true;this.selectedIndex=i;this.selectedTab=tab;this.invoke_compatHandler('selectedIndexChanged',null,this);}
else
tab.selected=false;}
this.update();}
eval(this.callBack.replace('[TABID]',tabId).replace("'[POST]'",tab.postMode).replace("[CBTYPE]",tab.callbackType));}}
else
{for(var i=0;i<this.tabIds.length;i++)
{tab=this.tabs[this.tabIds[i]];if(tab.tabId==tabId)
{tab.showWork(false);tab.selected=true;tab.rendered=true;tab.hovered=false;tab.container.style.display='';tab.assignCss();this.selectedIndex=i;this.selectedTab=tab;this.invoke_compatHandler('selectedIndexChanged',null,this);}
else
{if(tab.container!=null)
tab.container.style.display='none';tab.selected=false;tab.assignCss();}}
this.update();}
return tab;},raiseEvent:function(sFunc,evt,element)
{if(this[sFunc].length>0)
{var oPointerFunc=eval(this[sFunc]);return oPointerFunc(evt,element)!=false;}
return true;},update:function()
{var ary=[];for(var i=0;i<this.tabIds.length;i++)
ary[ary.length]=this.tabs[this.tabIds[i]].serialize();dnn.setVar(this.ns+'_tabs',ary.join(','));},createTab:function(html,id)
{var span=dnn.dom.createElement('span');span.innerHTML=html;var ctr=span.childNodes[0];if(ctr.id!=this.tabs[id].clientId)
ctr=dnn.dom.getById(this.tabs[id].clientId,span);this.tabs[id].container=ctr;this.contentContainer.appendChild(ctr);return this.tabs[id];},resetTab:function(id)
{var tab=this.tabs[id];if(tab.container!=null)
{dnn.dom.removeChild(tab.container);tab.container=null;}},needsLookup:function(tabId)
{return this.tabs[tabId].container==null;},callBackStatus:function(result,ctx,req)
{var ts=ctx;ts.invoke_compatHandler('callBackStatus',result,ctx,req);},callBackSuccess:function(result,ctx,req)
{var ts=ctx;ts.invoke_compatHandler('callBackStatus',result,ctx,req);ctx.invoke_handler('callBackSuccess',new dnn.controls.DNNCallbackEventArgs(result,ctx,req));var ret=dnn.evalJSON(result);var tab=ts.createTab(ret.text,ts.pendingLookupId);var vars=dnn.evalJSON(ret.vars);for(var key in vars)
dnn.setVar(key,vars[key]);if(ret.scripts!=null)
{var scripts=dnn.evalJSON(ret.scripts);var inlineScripts=[];var refScripts=[];for(var s in scripts)
{if(scripts[s].src&&scripts[s].src.length>0)
{if(dnn.dom.scriptStatus(scripts[s].src)=='')
refScripts.push(scripts[s].src);}
else
inlineScripts.push(scripts[s].__text);}
dnn.dom.loadScripts(refScripts,inlineScripts,dnn.createDelegate(ts,ts.callBackScriptComplete));}
else
ts.callBackScriptComplete();},callBackScriptComplete:function()
{var tabId=this.pendingLookupId;this.pendingLookupId=null;this.showTab(tabId);},callBackFail:function(result,ctx,req)
{ctx.invoke_handler('callBackFail',new dnn.controls.DNNCallbackEventArgs(result,ctx,req));},_getTabFromTabElement:function(element)
{},_getLabelData:function(index,prop,def)
{var data=this.tabLabelData[index];if(data[prop]!=null&&data[prop].length>0)
return data[prop];else
return def;},_getTabData:function(index,prop,def)
{var data=this.tabData[index];if(data[prop]!=null&&data[prop].length>0)
return data[prop];else
return def;}}
dnn.controls.DNNTabStrip.registerClass('dnn.controls.DNNTabStrip',dnn.controls.control);dnn.controls.DNNTab=function(strip,index,id,clientId)
{var tab=$get(clientId+'_l');if(tab!=null)
{this.tab=tab;this.container=$get(clientId);this.icon=$get(clientId+'_i');this.work=$get(clientId+'_w');this.rendered=(this.container!=null);this.selected=false;this.hovered=false;this.tabIndex=index;this.strip=strip;this.text=this.tab.innerHTML;this.clientId=clientId;this.tabId=id;this.css=this.strip._getLabelData(index,'css',strip.lblCss);this.cssSel=this.strip._getLabelData(index,'csssel',strip.lblCssSel);this.cssHover=this.strip._getLabelData(index,'csshover',strip.lblCssHover);this.cssDisabled=this.strip._getLabelData(index,'cssdisabled',strip.lblCssDisabled);this.postMode=this.strip._getTabData(index,'postmode',null);this.enabled=(this.strip._getTabData(index,'enabled',"1")=="1");this.callbackType=this.strip._getTabData(index,'cbtype',strip.callbackType);if(this.postMode)
this.postMode='\''+this.postMode+'\'';}}
dnn.controls.DNNTab.prototype={serialize:function()
{return this.tabId+'='+((this.rendered?1:0)+(this.selected?2:0)+(this.enabled?4:0));},showWork:function(show)
{if(this.work!=null)
{if(show)
{this.work.style.display='';if(this.icon!=null)
this.icon.style.display='none';}
else
{this.work.style.display='none';if(this.icon!=null)
this.icon.style.display='';}}},enable:function()
{this.enabled=true;},disable:function()
{this.enabled=false;},assignCss:function()
{var sCss='';if(this.enabled==false&&this.cssDisabled.length>0)
sCss=this.cssDisabled;else
{if(this.hovered&&this.cssHover.length>0)
sCss=this.cssHover;else
sCss=(this.selected?this.cssSel:this.css);}
this.tab.className=sCss;}}
dnn.controls.DNNTab.registerClass('dnn.controls.DNNTab');