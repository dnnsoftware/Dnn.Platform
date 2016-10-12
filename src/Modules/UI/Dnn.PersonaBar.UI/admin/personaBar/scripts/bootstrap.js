(function () {
	var regex = /\.mobi\.html/;
    var mobi = regex.test(location.href);
    
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
    var mainJs = mobi ? 'scripts/main.mobi.js' : 'scripts/main.js';
    var mainCss = mobi ? 'css/main.mobi.css' : 'css/main.css';
    if (skin) {
        mainCss = mobi ? 'css/' + skin + '.mobi.css' : 'css/' + skin + '.css';
    }

    styles.push(mainCss);
    styles.push('css/graph.css');

    

    if (mobi) {
        styles.push('scripts/contrib/owl-carousel/owl.carousel.css');
        styles.push('scripts/contrib/owl-carousel/owl.theme.css');
    }
    addCssToHead(styles, version);
    addJsToBody(mainJs, version);
})();