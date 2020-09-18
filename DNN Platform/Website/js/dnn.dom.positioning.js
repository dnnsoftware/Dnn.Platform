
Type.registerNamespace('dnn.dom.positioning');dnn.extend(dnn.dom.positioning,{pns:'dnn.dom',ns:'positioning',dragCtr:null,dragCtrDims:null,bodyScrollLeft:function()
{if(window.pageYOffset)
return window.pageYOffset;var oBody=(document.compatMode&&document.compatMode!="BackCompat")?document.documentElement:document.body;return oBody.scrollLeft;},bodyScrollTop:function()
{if(window.pageXOffset)
return window.pageXOffset;var oBody=(document.compatMode&&document.compatMode!="BackCompat")?document.documentElement:document.body;return oBody.scrollTop;},viewPortHeight:function()
{if(window.innerHeight)
return window.innerHeight;var oBody=(document.compatMode&&document.compatMode!="BackCompat")?document.documentElement:document.body;return oBody.clientHeight;},viewPortWidth:function()
{if(window.innerWidth)
return window.innerWidth;var oBody=(document.compatMode&&document.compatMode!="BackCompat")?document.documentElement:document.body;return oBody.clientWidth;},dragContainer:function(oCtl,e)
{var iNewLeft=0;var iNewTop=0;var oCont=dnn.dom.getById(oCtl.contID);var oTitle=dnn.dom.positioning.dragCtr;var iScrollTop=this.bodyScrollTop();var iScrollLeft=this.bodyScrollLeft();if(oCtl.startLeft==null)
oCtl.startLeft=e.clientX-this.elementLeft(oCont)+iScrollLeft;if(oCtl.startTop==null)
oCtl.startTop=e.clientY-this.elementTop(oCont)+iScrollTop;if(oCont.style.position=='relative')
oCont.style.position='absolute';iNewLeft=e.clientX-oCtl.startLeft+iScrollLeft;iNewTop=e.clientY-oCtl.startTop+iScrollTop;if(iNewLeft>this.elementWidth(document.forms[0]))
iNewLeft=this.elementWidth(document.forms[0]);if(iNewTop>this.elementHeight(document.forms[0]))
iNewTop=this.elementHeight(document.forms[0]);oCont.style.left=iNewLeft+'px';oCont.style.top=iNewTop+'px';if(oTitle!=null&&oTitle.dragOver!=null)
eval(oCtl.dragOver);},elementHeight:function(eSrc)
{if(eSrc.offsetHeight==null||eSrc.offsetHeight==0)
{if(eSrc.offsetParent==null)
return 0;if(eSrc.offsetParent.offsetHeight==null||eSrc.offsetParent.offsetHeight==0)
{if(eSrc.offsetParent.offsetParent!=null)
return eSrc.offsetParent.offsetParent.offsetHeight;else
return 0;}
else
return eSrc.offsetParent.offsetHeight;}
else
return eSrc.offsetHeight;},elementLeft:function(eSrc)
{return this.elementPos(eSrc).l;},elementOverlapScore:function(oDims1,oDims2)
{var iLeftScore=0;var iTopScore=0;if(oDims1.l<=oDims2.l&&oDims2.l<=oDims1.r)
iLeftScore+=(oDims1.r<oDims2.r?oDims1.r:oDims2.r)-oDims2.l;if(oDims2.l<=oDims1.l&&oDims1.l<=oDims2.r)
iLeftScore+=(oDims2.r<oDims1.r?oDims2.r:oDims1.r)-oDims1.l;if(oDims1.t<=oDims2.t&&oDims2.t<=oDims1.b)
iTopScore+=(oDims1.b<oDims2.b?oDims1.b:oDims2.b)-oDims2.t;if(oDims2.t<=oDims1.t&&oDims1.t<=oDims2.b)
iTopScore+=(oDims2.b<oDims1.b?oDims2.b:oDims1.b)-oDims1.t;return iLeftScore*iTopScore;},elementTop:function(eSrc)
{return this.elementPos(eSrc).t;},elementPos:function(eSrc)
{var oPos=new Object();oPos.t=0;oPos.l=0;oPos.at=0;oPos.al=0;var eParent=eSrc;var style;var srcId=eSrc.id;if(srcId!=null&&srcId.length==0)
srcId=null;if(eSrc.style.position=='absolute')
{oPos.t=eParent.offsetTop;oPos.l=eParent.offsetLeft;}
while(eParent!=null)
{oPos.at+=eParent.offsetTop;oPos.al+=eParent.offsetLeft;if(eSrc.style.position!='absolute')
{if(eParent.currentStyle)
style=eParent.currentStyle;else
style=Sys.UI.DomElement._getCurrentStyle(eParent);if(eParent.id==srcId||style.position!='relative')
{oPos.t+=eParent.offsetTop;oPos.l+=eParent.offsetLeft;}}
eParent=eParent.offsetParent;if(eParent==null||(eParent.tagName.toUpperCase()=="BODY"&&dnn.dom.browser.isType(dnn.dom.browser.Konqueror)))
break;}
return oPos;},elementWidth:function(eSrc)
{if(eSrc.offsetWidth==null||eSrc.offsetWidth==0)
{if(eSrc.offsetParent==null)
return 0;if(eSrc.offsetParent.offsetWidth==null||eSrc.offsetParent.offsetWidth==0)
{if(eSrc.offsetParent.offsetParent!=null)
return eSrc.offsetParent.offsetParent.offsetWidth;else
return 0;}
else
return eSrc.offsetParent.offsetWidth}
else
return eSrc.offsetWidth;},enableDragAndDrop:function(oContainer,oTitle,sDragCompleteEvent,sDragOverEvent)
{dnn.dom.addSafeHandler(document.body,'onmousemove',dnn.dom.positioning,'__dnn_bodyMouseMove');dnn.dom.addSafeHandler(document.body,'onmouseup',dnn.dom.positioning,'__dnn_bodyMouseUp');dnn.dom.addSafeHandler(oTitle,'onmousedown',dnn.dom.positioning,'__dnn_containerMouseDownDelay');if(dnn.dom.browser.type==dnn.dom.browser.InternetExplorer)
oTitle.style.cursor='hand';else
oTitle.style.cursor='pointer';if(oContainer.id.length==0)
oContainer.id=oTitle.id+'__dnnCtr';oTitle.contID=oContainer.id;if(sDragCompleteEvent!=null)
oTitle.dragComplete=sDragCompleteEvent;if(sDragOverEvent!=null)
oTitle.dragOver=sDragOverEvent;return true;},placeOnTop:function(oCont,bShow,sSrc)
{if(dnn.dom.browser.isType(dnn.dom.browser.Opera,dnn.dom.browser.Mozilla,dnn.dom.browser.Netscape,dnn.dom.browser.Safari)||(dnn.dom.browser.isType(dnn.dom.browser.InternetExplorer)&&dnn.dom.browser.version>=7))
return;var oIFR=dnn.dom.getById('ifr'+oCont.id);if(oIFR==null)
{var oIFR=document.createElement('iframe');oIFR.id='ifr'+oCont.id;if(sSrc!=null)
oIFR.src=sSrc;oIFR.style.top='0px';oIFR.style.left='0px';oIFR.style.filter="progid:DXImageTransform.Microsoft.Alpha(opacity=0)";oIFR.scrolling='no';oIFR.frameBorder='no';oIFR.style.display='none';oIFR.style.position='absolute';oCont.parentNode.appendChild(oIFR);}
var oDims=new dnn.dom.positioning.dims(oCont);oIFR.style.width=oDims.w;oIFR.style.height=oDims.h;oIFR.style.top=oDims.t+'px';oIFR.style.left=oDims.l+'px';var iIndex=dnn.dom.getCurrentStyle(oCont,'zIndex');if(iIndex==null||iIndex==0||isNaN(iIndex))
iIndex=1;oCont.style.zIndex=iIndex;oIFR.style.zIndex=iIndex-1;if(bShow)
oIFR.style.display="block";else if(oIFR!=null)
oIFR.style.display='none';},__dnn_containerMouseDown:function(oCtl)
{while(oCtl.contID==null)
{oCtl=oCtl.parentNode;if(oCtl.tagName.toUpperCase()=='BODY')
return;}
dnn.dom.positioning.dragCtr=oCtl;oCtl.startTop=null;oCtl.startLeft=null;var oCont=dnn.dom.getById(oCtl.contID);if(oCont.style.position==null||oCont.style.position.length==0)
oCont.style.position='relative';dnn.dom.positioning.dragCtrDims=new dnn.dom.positioning.dims(oCont);if(oCont.getAttribute('_b')==null)
{oCont.setAttribute('_b',oCont.style.backgroundColor);oCont.setAttribute('_z',oCont.style.zIndex);oCont.setAttribute('_w',oCont.style.width);oCont.setAttribute('_d',oCont.style.border);oCont.style.zIndex=9999;oCont.style.backgroundColor=DNN_HIGHLIGHT_COLOR;oCont.style.border='4px outset '+DNN_HIGHLIGHT_COLOR;oCont.style.width=dnn.dom.positioning.elementWidth(oCont);if(dnn.dom.browser.type==dnn.dom.browser.InternetExplorer)
oCont.style.filter='progid:DXImageTransform.Microsoft.Alpha(opacity=80)';}},__dnn_containerMouseDownDelay:function(e)
{var oTitle=e.srcElement;if(oTitle==null)
oTitle=e.target;dnn.doDelay('__dnn_dragdrop',500,this.__dnn_containerMouseDown,oTitle);},__dnn_bodyMouseUp:function()
{dnn.cancelDelay('__dnn_dragdrop');var oCtl=dnn.dom.positioning.dragCtr;if(oCtl!=null&&oCtl.dragComplete!=null)
{eval(oCtl.dragComplete);var oCont=dnn.dom.getById(oCtl.contID);oCont.style.backgroundColor=oCont.getAttribute('_b');oCont.style.zIndex=oCont.getAttribute('_z');oCont.style.width=oCont.getAttribute('_w');oCont.style.border=oCont.getAttribute('_d');oCont.setAttribute('_b',null);oCont.setAttribute('_z',null);if(dnn.dom.browser.type==dnn.dom.browser.InternetExplorer)
oCont.style.filter=null;}
dnn.dom.positioning.dragCtr=null;},__dnn_bodyMouseMove:function(e)
{if(this.dragCtr!=null)
this.dragContainer(this.dragCtr,e);}});dnn.dom.positioning.dims=function(eSrc)
{var bHidden=(eSrc.style.display=='none');if(bHidden)
eSrc.style.display="";this.w=dnn.dom.positioning.elementWidth(eSrc);this.h=dnn.dom.positioning.elementHeight(eSrc);var oPos=dnn.dom.positioning.elementPos(eSrc);this.t=oPos.t;this.l=oPos.l;this.at=oPos.at;this.al=oPos.al;this.rot=this.at-this.t;this.rol=this.al-this.l;this.r=this.l+this.w;this.b=this.t+this.h;if(bHidden)
eSrc.style.display="none";}
dnn.dom.positioning.dims.registerClass('dnn.dom.positioning.dims');
