
function __dnn_toolbarHandler(toolBarId,controlId,behaviorId,namespacePrefix,loadHandler,eventName,hideEventName)
{var status=dnn.dom.scriptStatus('dnn.controls.dnntoolbar.js');if(status=='complete')
{var tb=new dnn.controls.DNNToolBar(controlId,behaviorId);var ctl=$get(controlId);tb.loadDefinition(toolBarId,namespacePrefix,ctl,ctl.parentNode,ctl,loadHandler);dnn.dom.addSafeHandler(ctl,eventName,tb,'show');dnn.dom.addSafeHandler(ctl,hideEventName,tb,'beginHide');tb.show();}
else if(status=='')
dnn.dom.loadScript(dnn.dom.getScriptPath()+'dnn.controls.dnntoolbar.js','',function(){__dnn_toolbarHandler(toolBarId,controlId,behaviorId,namespacePrefix,loadHandler,eventName,hideEventName)});}