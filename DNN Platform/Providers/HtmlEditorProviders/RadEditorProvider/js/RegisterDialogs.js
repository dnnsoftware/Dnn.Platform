var initToolCommands = function() {
	if (typeof Telerik !== "undefined" && typeof Telerik.Web.UI.Editor !== "undefined") {
		Telerik.Web.UI.Editor.CommandList["SaveTemplate"] = function(commandName, editor, args) {
			var htmlText = editor.get_html();

			var args;
			args[0] = htmlText;
			argument = args;

			var myCallbackFunction = function(sender, args) { };

			editor.showExternalDialog(
				__textEditorSaveTemplateDialog,
				argument,
				375,
				290,
				myCallbackFunction,
				null,
				'Save as Template',
				true,
				Telerik.Web.UI.WindowBehaviors.Close + Telerik.Web.UI.WindowBehaviors.Move + Telerik.Web.UI.WindowBehaviors.Resize,
				false,
				true);
		};
	}
};

var createToolCommands = function () {
    initToolCommands();
    if(typeof Sys !== "undefined") {
	Sys.WebForms.PageRequestManager.getInstance().add_endRequest(function () {
		initToolCommands();
	});
}
}

if (window.attachEvent) {
	window.attachEvent("onload", createToolCommands);
}
else 
{
	if (window.addEventListener) 
	{
		window.addEventListener("load", createToolCommands, false);
	}            
}

if (typeof(Sys) != "undefined"){
    Sys.Application.notifyScriptLoaded() ;
}




