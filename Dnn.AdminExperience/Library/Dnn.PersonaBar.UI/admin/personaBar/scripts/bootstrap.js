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
    var version = (cdv ? '?cdv=' + cdv : '') + (debugMode ? '&t=' + Math.random(): '');
    var styles = [];
    var mainJs = 'scripts/main.js';
    var themeCss = 'css/theme.css';
    var mainCss = 'css/main.css';

    var hasCustomPersonaBarTheme = personaBarSettings['personaBarTheme'];
    if (hasCustomPersonaBarTheme){
        styles.push('../../../../Portals/_default/PersonaBarTheme.css');
    }
    else{
        styles.push(themeCss);
    }

    styles.push(mainCss);
    styles.push('css/graph.css');

    addCssToHead(styles, version);
    addJsToBody(mainJs, version);
})();