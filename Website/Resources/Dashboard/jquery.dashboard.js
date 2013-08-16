(function($) {

    $.fn.zebratable = function(options) {

        var defaults = {
            tblClass: 'dashboardGrid',
            altClass: 'dashboardTableAlt'
        };
        var options = $.extend(defaults, options);

        return this.each(function() {
            // all of our tables need common spacing - this overcomes bug
            // in PropertyGrid which prevents you from assigning a CssClass in ascx file
            $(this).addClass('dashboardGrid');

            // if the table has headers then start with odd rows so we don't highlight the header
            $(this).find(':has(th)').find('tr:odd').addClass('dashboardTableAlt');

            // if the table doesn't have headers then start highlighting even rows so the top row is highlighted
            $(this).find(':not(:has(th))').find('tr:even').addClass('dashboardTableAlt');

        });
    };

    /* 
    Delegate function allows us to use Selectors to handle events.
    This allows us to use event bubbling and single event handler
    which improves perf and makes eventhandling more dynamic.
    */
    $.delegate = function(rules) {
        return function(e) {
            var target = jQuery(e.target);
            for (var selector in rules)
                if (target.is(selector)) {
                return rules[selector].apply(this, jQuery.makeArray(arguments));
            }
        }
    }

    /*
    // Simple JavaScript Templating
    // John Resig - http://ejohn.org/ - MIT Licensed
    */
    var _cache = {};

    $.template = function template(str, data) {
        // Figure out if we're getting a template, or if we need to
        // load the template - and be sure to cache the result.
        var fn = !/\W/.test(str) ?
        _cache[str] = cache[str] ||
        template(document.getElementById(str).innerHTML) :

        // Generate a reusable function that will serve as a template
        // generator (and which will be cached).
      new Function("obj",
        "var p=[],print=function(){p.push.apply(p,arguments);};" +

        // Introduce the data as local variables using with(){}
        "with(obj){p.push('" +

        // Convert the template into pure JavaScript
        str
          .replace(/[\r\t\n]/g, " ")
          .split("<#").join("\t")
          .replace(/((^|#>)[^\t]*)'/g, "$1\r")
          .replace(/\t=(.*?)#>/g, "',$1,'")
          .split("\t").join("');")
          .split("#>").join("p.push('")
          .split("\r").join("\\'")
      + "');}return p.join('');");

        // Provide some basic currying to the user
        return data ? fn(data) : fn;
    };
})(jQuery);


jQuery(document).ready(function($) {
    var dlg = $.template('<div id="<#=id#>" title="<#=title#> - <#=name#>" >Empty Dialog</div>')
    
    $('#dashboardTabs div[id$="-tab"]').hide();
    $('#dashboardTabs div[id$="-tab"]:first').show();
    $('#tablist li:first a').addClass('active');
    $('#tablist li:first a').addClass('SubHead');

    $('#dashboardTabs').click($.delegate({
        '.dashboardTab': function(e) {
            $('#tablist li a').removeClass('active');
            $('#tablist li a').removeClass('SubHead');
            $(e.target).addClass('active');
            $(e.target).addClass('SubHead');
            var currentTab = $(e.target).attr('href');
            $('#dashboardTabs div[id$="-tab"]').hide();
            $(currentTab).show();
            return false;
        }
    }));
    
    $('#dashboardTabs div[id$="-tab"] table').zebratable();

    // clean up to avoid memory leaks in certain versions of IE 6
    $(window).bind('unload', function() {
        $('#dashboardTabs').unbind('click');
    });

});

