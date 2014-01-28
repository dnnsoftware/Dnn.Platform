if (typeof dnn === 'undefined') dnn = {};
dnn.hotkeysManager = dnn.hotkeysManager || {};

(function($) {
    //special key
    dnn.hotkeysManager.specialKeys = {
        shift: { name: 'shift', code: 16 },
        ctrl: { name: 'ctrl', code: 17 },
        alt: { name: 'alt', code: 18 }
    };

    dnn.hotkeysManager.specialKey = dnn.hotkeysManager.specialKeys["ctrl"];
    dnn.hotkeysManager.setSpecialKey = function(name) {
        dnn.hotkeysManager.specialKey = dnn.hotkeysManager.specialKeys[name];
    };

    var $document = $(document.body);
    var hotkeys = [];

    var targetAcceptInput = function(type) {
        return $.inArray(type, ["text", "textarea", "password", "number", "email", "url", "range", "date", "month", "week", "time", "datetime", "datetime-local", "search", "color", "tel"]) > -1;
    };

    var buildMessageBar = function() {
        var $bar = $('<div id="HotkeysBar"><ul></ul></div>');
        $bar.appendTo($document);
        for (var i = 0; i < hotkeys.length; i++) {
            var k = hotkeys[i];
            var $item = $('<li title="' + k.description + '"><span>' + k.key + '</span><p>' + k.title + '</p></li>');
            $bar.find('ul').append($item);
        }

        return $bar;
    };

    var showMessageBar = function () {
        
        if ($('#HotkeysBar').length > 0 || hotkeys.length == 0) {
            return;
        }
        var messageBar = buildMessageBar();
        messageBar.animate({ bottom: 0, opacity: 1 }, 'fast');
        $document.on('keyup', hideMessageBar);
    };

    var hideMessageBar = function (e) {
        $('#HotkeysBar').animate({ opacity: 0 }, 'fast', function() {
            $('#HotkeysBar').remove();
        });
        $document.off(e);
    };
    
    $document.on('keydown', function(e) {
        if (targetAcceptInput(e.target.type)) {
            e.stopImmediatePropagation();
            return;
        }
        
        if (e.keyCode == dnn.hotkeysManager.specialKey.code) {
            showMessageBar();
        }
    });

    dnn.hotkeysManager.map = function (key, title, description, action) {
        hotkeys.push({key: key, title: title, description: description});
        $document.jkey(dnn.hotkeysManager.specialKey.name + "+" + key, function(e) {
            action.call();
        });
    };
}(jQuery));