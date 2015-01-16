/// <reference name="MicrosoftAjax.js" />
/// <reference name="dnn.js" assembly="DotNetNuke.WebUtility" />

//--- dnn.diagnostics
Type.registerNamespace('dnn.diagnostics');

dnn.extend(dnn.diagnostics, {
	pns: 'dnn',
	ns: 'diagnostics',
	parserName: null,
	debugCtl: null,
	debugWait: (document.all != null),
	debugArray: [],

    clearDebug: function()
    {
	    if (this.debugCtl != null)
	    {
		    this.debugCtl.value = '';
		    return true;
	    }
	    return false;
    },

    displayDebug: function(sText)
    {
	    if (this.debugCtl == null)
	    {
		    if (dnn.dom.browser.type == dnn.dom.browser.InternetExplorer)
		    {
			    var oBody = dnn.dom.getByTagName("body")[0];
			    if (this.debugWait && oBody.readyState != 'complete')
			    {
				    dnn.debugWait = true;
				    this.debugArray[this.debugArray.length] = sText;
				    //document.attachEvent('onreadystate', __dnn_documentLoaded);
				    if (oBody.onload == null || oBody.onload.toString().indexOf('__dnn_documentLoaded') == -1)
					    oBody.onload = dnn.dom.appendFunction(oBody.onload, '__dnn_documentLoaded()');
				    return;
			    }
		    }
		    this.debugCtl = dnn.dom.getById('__dnnDebugOutput');
		    if (this.debugCtl == null)
		    {
			    this.debugCtl = dnn.dom.createElement('TEXTAREA');
			    this.debugCtl.id = '__dnnDebugOutput';
			    this.debugCtl.rows=10;
			    this.debugCtl.cols=100;
			    dnn.dom.appendChild(oBody, this.debugCtl);
		    }
		    this.debugCtl.style.display = 'block';
	    }
    	
	    if (dnn.diagnostics.debugCtl == null)
		    alert(sText);
	    else
		    dnn.diagnostics.debugCtl.value += sText + '\n';
    	
	    return true;
    },

    assertCheck: function(sCom, bVal, sMsg)
    {
	    if (!bVal)
		    this.displayDebug(sCom + ' - FAILED (' + sMsg + ')');
	    else if (this.verbose)
		    this.displayDebug(sCom + ' - PASSED');
    },

    assert: function(sCom, bVal) 
    {
      this.assertCheck(sCom, bVal == true, 'Testing assert(boolean) for true');
    },

    assertTrue: function(sCom, bVal)
    {
      this.assertCheck(sCom, bVal == true, 'Testing assert(boolean) for true');
    },

    assertFalse: function(sCom, bVal)
    {
      this.assertCheck(sCom, bVal == false, 'Testing assert(boolean) for false');
    },

    assertEquals: function(sCom, sVal1, sVal2)
    {
      this.assertCheck(sCom, sVal1 == sVal2, 'Testing Equals: ' + this._safeString(sVal1) + ' (' + typeof(sVal1) + ') != ' + this._safeString(sVal2) + ' (' + typeof(sVal2) + ')');
    },

    assertNotEquals: function(sCom, sVal1, sVal2)
    {
      this.assertCheck(sCom, sVal1 != sVal2, 'Testing NotEquals: ' + this._safeString(sVal1) + ' (' + typeof(sVal1) + ') == ' + this._safeString(sVal2) + ' (' + typeof(sVal2) + ')');
    },

    assertNull: function (sCom, sVal1)
    {
	    this.assertCheck(sCom, sVal1 == null, 'Testing null: ' + this._safeString(sVal1) + ' (' + typeof(sVal1) + ') != null');
    },

    assertNotNull: function (sCom, sVal1)
    {
	    this.assertCheck(sCom, sVal1 != null, 'Testing for null: ' + this._safeString(sVal1) + ' (' + typeof(sVal1) + ') == null');
    },

    assertStringLength: function (sCom, sVal1)
    {
	    this.assertCheck(sCom, ((sVal1 == null) ? false : sVal1.length > 0), 'Testing for string length: ' + this._safeString(sVal1) + ' (' + ((sVal1 == null) ? 'null' : sVal1.length) + ')');
    },

    assertNaN: function (sCom, sVal1)
    {
	    this.assertCheck(sCom, isNaN(sVal1), 'Testing for NaN: ' + this._safeString(sVal1) + ' (' + typeof(sVal1) + ') is a number');
    },

    assertNotNaN: function (sCom, sVal1)
    {
	    this.assertCheck(sCom, isNaN(sVal1) == false, 'Testing for NotNaN: ' + this._safeString(sVal1) + ' (' + typeof(sVal1) + ') is NOT a number');
    },
    
    _safeString: function(s)
    {
	    if (typeof(s) == 'string' || typeof(s) == 'number')
		    return s;
	    else
		    return typeof(s);
    }
});


//--- End dnn.diagnostics
//dnn_diagnosticTests(dnn);

var __dnn_m_aryHandled=new Array();
function dnn_diagnosticTests(oParent)
{
	if (oParent.ns == 'dnn')
		dnn.diagnostics.clearDebug();
	if (typeof(oParent.UnitTests) == 'function')
	{
		dnn.diagnostics.displayDebug('------- Starting ' + oParent.pns + '.' + oParent.ns + ' tests (v.' + (oParent.apiversion ? oParent.apiversion : dnn.apiversion) + ') ' + new Date().toString() + ' -------');
		oParent.UnitTests();
	}
	
	for (var obj in oParent)
	{
		if (oParent[obj] != null && typeof(oParent[obj]) == 'object' && __dnn_m_aryHandled[obj] == null)
		{
			//if (obj != 'debugCtl')	//what is this IE object???
			if (oParent[obj].pns != null)
				dnn_diagnosticTests(oParent[obj]);
		}
		//__dnn_m_aryHandled[obj] = true;
	}
}

function __dnn_documentLoaded()
{
	dnn.diagnostics.debugWait = false;
	dnn.diagnostics.displayDebug('document loaded... avoiding Operation Aborted IE bug');
	dnn.diagnostics.displayDebug(dnn.diagnostics.debugArray.join('\n'));
	
}
