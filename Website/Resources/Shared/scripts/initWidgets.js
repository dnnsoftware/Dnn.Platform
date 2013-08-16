function loadWidgets() {
    if (typeof (DotNetNuke) === "undefined")
        Type.registerNamespace("DotNetNuke.UI.WebControls");

    if (typeof (DotNetNuke.UI.WebControls.Utility) === "undefined")
        jQuery.getScript($dnn.baseResourcesUrl + "Shared/scripts/DotNetNukeAjaxShared.js",
                    function() { jQuery.getScript($dnn.baseResourcesUrl + "Shared/scripts/widgets.js"); });
    else
        jQuery.getScript($dnn.baseResourcesUrl + "Shared/scripts/widgets.js");
}

if (typeof ($dnn) === "undefined") {
    $dnn = new Object();
    $dnn.pageScripts = document.getElementsByTagName("script");
    $dnn.scriptUrl = $dnn.pageScripts[$dnn.pageScripts.length - 1].src;
    $dnn.hostUrl = (typeof ($dnn.hostUrl) == "undefined" ? $dnn.scriptUrl.toLowerCase().replace("resources/shared/scripts/initwidgets.js", "") : $dnn.hostUrl);
    if (!$dnn.hostUrl.endsWith("/")) $dnn.hostUrl += "/";
    $dnn.baseDnnScriptUrl = $dnn.hostUrl + "Resources/Shared/scripts/";
    $dnn.baseResourcesUrl = $dnn.hostUrl + "Resources/";
}

// jQuery dependency
if (typeof (Sys) === "undefined")
    jQuery.getScript($dnn.baseDnnScriptUrl + "MSAJAX/MicrosoftAjax.js", loadWidgets());
else
    loadWidgets();

if (Sys && Sys.Application) {
    Sys.Application.notifyScriptLoaded();
}