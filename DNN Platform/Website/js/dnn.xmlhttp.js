
Type.registerNamespace('dnn.xmlhttp');dnn.xmlhttp.callbackType=function(){};dnn.xmlhttp.callbackType.prototype={simple:0,processPage:1,callBackMethod:2,processPageCallbackMethod:3}
dnn.xmlhttp.callbackType.registerEnum("dnn.xmlhttp.callbackType");dnn.xmlhttp.callbackStatus=function(){};dnn.xmlhttp.callbackStatus.prototype={ok:200,genericFailure:400,controlNotFound:404,interfactNotSupported:501}
dnn.xmlhttp.callbackStatus.registerEnum("dnn.xmlhttp.callbackStatus");dnn.extend(dnn.xmlhttp,{pns:'dnn',ns:'xmlhttp',parserName:null,contextId:0,requests:[],cleanUpTimer:null,callBackMethods:null,init:function()
{this.parserName=this._getParser();},onload:function()
{dnn.xmlhttp._fillCallBackMethods();},doCallBack:function(sControlId,sArg,pSuccessFunc,sContext,pFailureFunc,pStatusFunc,bAsync,sPostChildrenId,iType)
{var oReq=dnn.xmlhttp.createRequestObject();var sURL=document.location.href;oReq.successFunc=pSuccessFunc;oReq.failureFunc=pFailureFunc;oReq.statusFunc=pStatusFunc;oReq.context=sContext;if(bAsync==null)
bAsync=true;if(sURL.indexOf('#')!=-1)
sURL=sURL.substring(0,sURL.indexOf('#'));oReq.open('POST',sURL,bAsync);if(this.parserName=='JS')
sArg=dnn.encode(sArg,false);else
sArg=dnn.encode(sArg,true);if(sPostChildrenId)
sArg+='&'+dnn.dom.getFormPostString($get(sPostChildrenId));if(iType!=0)
sArg+='&__DNNCAPISCT='+iType;oReq.send('__DNNCAPISCI='+sControlId+'&__DNNCAPISCP='+sArg);return oReq;},callControlMethod:function(ns,method,args,successFunc,failFunc,context,type)
{if(this.callBackMethods==null)
this._fillCallBackMethods();if(type==null)
type=dnn.xmlhttp.callbackType.callBackMethod;if(this.callBackMethods[ns])
{if(args==null)
args={};var callContext={context:context,success:successFunc,fail:failFunc};var payload=Sys.Serialization.JavaScriptSerializer.serialize({method:method,args:args});dnn.xmlhttp.doCallBack(this.callBackMethods[ns],payload,dnn.xmlhttp.callBackMethodComplete,callContext,dnn.xmlhttp.callBackMethodError,null,true,null,type);}
else
{alert('Namespace not registered');}},callBackMethodComplete:function(result,context,req)
{result=Sys.Serialization.JavaScriptSerializer.deserialize(result);if(context.success)
context.success(result.result,context.context,req);},callBackMethodError:function(message,context,req)
{if(context.fail)
context.fail(message,context.context,req);},createRequestObject:function()
{if(this.parserName=='ActiveX')
{var o=new ActiveXObject('Microsoft.XMLHTTP');dnn.xmlhttp.requests[dnn.xmlhttp.requests.length]=new dnn.xmlhttp.XmlHttpRequest(o);return dnn.xmlhttp.requests[dnn.xmlhttp.requests.length-1];}
else if(this.parserName=='Native')
{return new dnn.xmlhttp.XmlHttpRequest(new XMLHttpRequest());}
else
{var oReq=new dnn.xmlhttp.XmlHttpRequest(new dnn.xmlhttp.JsXmlHttpRequest());dnn.xmlhttp.requests[oReq._request.contextId]=oReq;return oReq;}},_getParser:function()
{if(dnn.xmlhttp.JsXmlHttpRequest!=null)
return'JS';if(dnn.dom.browser.isType(dnn.dom.browser.InternetExplorer))
return'ActiveX';else if(typeof(XMLHttpRequest)!="undefined")
return'Native';else
return'JS';},_fillCallBackMethods:function()
{this.callBackMethods=[];var methods=dnn.getVar('__dnncbm','').split(' ');var pair;if(methods)
{for(var i=0;i<methods.length;i++)
{if(methods[i].length>0)
{pair=methods[i].split('=');this.callBackMethods[pair[0]]=pair[1];}}}},_cleanupxmlhttp:function()
{for(var i=0;i<dnn.xmlhttp.requests.length;i++)
{if(dnn.xmlhttp.requests[i]!=null)
{if(dnn.xmlhttp.requests[i].completed)
{dnn.xmlhttp.requests[i].dispose();if(dnn.xmlhttp.requests.length==1)
dnn.xmlhttp.requests=new Array();else
dnn.xmlhttp.requests.splice(i,i);}}}}});dnn.xmlhttp.XmlHttpRequest=function(o)
{this._request=o;this.successFunc=null;this.failureFunc=null;this.statusFunc=null;this._request.onreadystatechange=dnn.dom.getObjMethRef(this,'onreadystatechange');this.context=null;this.completed=false;}
dnn.xmlhttp.XmlHttpRequest.prototype={dispose:function()
{if(this._request!=null)
{this._request.onreadystatechange=new function(){};this._request.abort();this._request=null;this.successFunc=null;this.failureFunc=null;this.statusFunc=null;this.context=null;this.completed=null;this.postData=null;}},open:function(sMethod,sURL,bAsync)
{this._request.open(sMethod,sURL,bAsync);if(typeof(this._request.setRequestHeader)!='undefined')
this._request.setRequestHeader("Content-type","application/x-www-form-urlencoded; charset=UTF-8");return true;},send:function(postData)
{this.postData=postData;if(dnn.xmlhttp.parserName=='ActiveX')
this._request.send(postData);else
this._request.send(postData);return true;},onreadystatechange:function()
{if(this.statusFunc!=null)
this.statusFunc(this._request.readyState,this.context,this);if(this._request.readyState=='4')
{this.complete(this._request.responseText);if(dnn.xmlhttp.parserName=='ActiveX')
window.setTimeout(dnn.xmlhttp._cleanupxmlhttp,1);}},complete:function(res)
{var statusCode=this.getResponseHeader('__DNNCAPISCSI');this.completed=true;if(new Number(statusCode)==dnn.xmlhttp.callbackStatus.ok)
{var ret=Sys.Serialization.JavaScriptSerializer.deserialize(res);this.successFunc(ret.d,this.context,this);}
else
{var statusDesc=this.getResponseHeader('__DNNCAPISCSDI');if(this.failureFunc!=null)
this.failureFunc(statusCode+' - '+statusDesc,this.context,this);else
alert(statusCode+' - '+statusDesc);}},getResponseHeader:function(key)
{return this._request.getResponseHeader(key);}}
dnn.xmlhttp.XmlHttpRequest.registerClass('dnn.xmlhttp.XmlHttpRequest');dnn.xmlhttp.init();Sys.Application.add_load(dnn.xmlhttp.onload);
