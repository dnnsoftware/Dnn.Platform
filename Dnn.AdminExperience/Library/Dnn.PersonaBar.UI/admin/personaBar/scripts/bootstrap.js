(function () {
    function addCssToHead(css, version) {
        var head = document.getElementsByTagName('head')[0];
        for (var i = 0; i < css.length; i++) {
            var link = document.createElement('link');
            link.setAttribute('rel', 'stylesheet');
            link.setAttribute('type', 'text/css');
            link.setAttribute('href', css[i] + version);
            head.appendChild(link);
        }
    };

    function addJsToBody (js, version) {
        var body = document.getElementsByTagName('body')[0];
        var script = document.createElement('script');
        script.setAttribute('src', 'scripts/contrib/require.js' + version);
        script.setAttribute('data-main', js + version);
        body.appendChild(script);
    };

    var personaBarSettings = window.parent['personaBarSettings'];
    var debugMode = personaBarSettings['debugMode'] === true;
    var cdv = personaBarSettings['buildNumber'];
    var skin = personaBarSettings['skin'];
    var version = (cdv ? '?cdv=' + cdv : '') + (debugMode ? '&t=' + Math.random(): '');
    var styles = [];
    var mainJs = 'scripts/main.js';
    var mainCss = 'css/main.css';

    //START persian-dnnsoftware
    if (window.parent['personaBarSettings']['culture'] == 'fa-IR' || window.parent['personaBarSettings']['culture'].startsWith("ar-")) {
        mainJs = 'scripts/main.rtl.js';
        mainCss = 'css/main.rtl.css';
        
    }
    //END persian-dnnsoftware

    if (skin) {
        mainCss = 'css/' + skin + '.css';
    }

    styles.push(mainCss);

    //START persian-dnnsoftware
    if (window.parent['personaBarSettings']['culture'] == 'fa-IR' || window.parent['personaBarSettings']['culture'].startsWith("ar-")) {
        styles.push('css/graph.rtl.css');
    } else {
        styles.push('css/graph.css');
    }
    //END persian-dnnsoftware


    addCssToHead(styles, version);
    addJsToBody(mainJs, version);
})();