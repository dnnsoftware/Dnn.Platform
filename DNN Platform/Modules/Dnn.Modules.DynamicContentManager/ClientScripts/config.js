var cf = {
    init: function() {
        var inIframe = window != window.top;

        // I dont know why platform sends me those strange json incompatible value and pretends it is json...
        // so now I need to do some dirty work here -- clean the dnnVariable and make it json compatible
        var v = inIframe ? window.parent.document.getElementById('__dnnVariable').value : '';

        // remove the first `
        v = v.replace('`', '');
        // change the rest of ` to "
        var m = new RegExp('`', 'g');
        v = v.replace(m, '"');
        var userSettings = window.parent['__userSettings'];

        return {
            userSettings: userSettings
        };
    }
};