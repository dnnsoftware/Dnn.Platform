function OnClientCommandExecuting(editor, args) {
	if (typeof jQuery === "undefined" || typeof jQuery.fn.dnnControlPanel === "undefined") return;
	
    var commandName = args.get_commandName();
    if (commandName == "ToggleScreenMode") {
        if (editor._isFullScreen == true ) {
            jQuery.fn.dnnControlPanel.show();
        }
        else {
        	jQuery.fn.dnnControlPanel.hide();
        }
    }
}

function OnDnnEditorClientSubmit(editor) {
    editor.set_mode(1);
    if (editor.get_mode() != 1) {
        editor.set_mode(2);
    }
}

if (typeof(Sys) != "undefined"){
    Sys.Application.notifyScriptLoaded() ;
}
