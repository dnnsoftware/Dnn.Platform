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

    var editBarSettings = window.parent['editBarSettings'];
    var debugMode = editBarSettings['debugMode'] === true;
    var cdv = editBarSettings['buildNumber'];
    var version = (cdv ? '?cdv=' + cdv : '') + (debugMode ? '&t=' + Math.random(): '');
    var styles = [];
    var mainJs = mobi ? 'scripts/main.mobi.js' : 'scripts/main.js';
    var themeCss = 'css/theme.css';
    var mainCss = mobi ? 'css/main.mobi.css' : 'css/main.css';

    var hasCustomEditBarTheme = editBarSettings['editBarTheme'];
    if (hasCustomEditBarTheme){
        styles.push('../../../../Portals/_default/EditBarTheme.css');
    }
    else{
        styles.push(themeCss);
    }

    styles.push(mainCss);

    addCssToHead(styles, version);
    addJsToBody(mainJs, version);
})();