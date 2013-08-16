/*
  DotNetNuke - http://www.dotnetnuke.com
  Copyright (c) 2002-2007
  by DotNetNuke Corporation
 
  Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated 
  documentation files (the "Software"), to deal in the Software without restriction, including without limitation 
  the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and 
  to permit persons to whom the Software is furnished to do so, subject to the following conditions:
 
  The above copyright notice and this permission notice shall be included in all copies or substantial portions 
  of the Software.
 
  THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED 
  TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL 
  THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF 
  CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
  DEALINGS IN THE SOFTWARE.

	''' -----------------------------------------------------------------------------
	''' <summary>
	''' This script contains initialization and helper classes and should be loaded
	''' prior to any other DNN Ajax script.
	''' </summary>
	''' <remarks>
	''' </remarks>
	''' <history>
	'''     Version 1.0.0: Feb. 28, 2007, Nik Kalyani, nik.kalyani@dotnetnuke.com 
	'''     Version 1.0.1: Oct. 28, 2007, Nik Kalyani, nik.kalyani@dotnetnuke.com 
	'''     Version 1.1.0: Nov. 06, 2008, Nik Kalyani, nik.kalyani@dotnetnuke.com
	''' </history>
	''' -----------------------------------------------------------------------------
*/
var $dnn = (typeof($dnn) === "undefined" ? new Object() : $dnn);
$dnn.pageScripts = document.getElementsByTagName("script");
$dnn.scriptUrl = $dnn.pageScripts[$dnn.pageScripts.length - 1].src;
$dnn.hostUrl = (typeof($dnn.hostUrl) == "undefined" ? $dnn.scriptUrl.toLowerCase().replace("resources/shared/scripts/init.js","") : $dnn.hostUrl);
if (!$dnn.hostUrl.endsWith("/")) $dnn.hostUrl += "/";
$dnn.baseDnnScriptUrl = $dnn.hostUrl + "Resources/Shared/scripts/";
$dnn.baseResourcesUrl = $dnn.hostUrl + "Resources/";

// jQuery dependency
if (typeof(Sys) === "undefined")
    $.getScript($dnn.baseDnnScriptUrl + "MSAJAX/MicrosoftAjax.js", Type.registerNamespace("DotNetNuke.UI.WebControls"));

