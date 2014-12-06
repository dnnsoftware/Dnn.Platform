function showNotification(params) {
    var options = {
        'showAfter': 0,
        'duration': 0,
        'autoClose': false,
        'type': 'success',
        'message': '',
        'link_notification': '',
        'description': '',
        'imagepath': ''
    };
    $.extend(true, options, params);
    var msgclass = 'succ_bg';
    if (options['type'] == 'error') {
        msgclass = 'error_bg'
    } else if (options['type'] == 'information') {
        msgclass = 'info_bg'
    } else if (options['type'] == 'warning') {
        msgclass = 'warn_bg'
    }
    var icon = 'tick.png';
    if (options['type'] == 'error') {
        icon = 'error.png'
    } else if (options['type'] == 'information') {
        icon = 'information.png'
    } else if (options['type'] == 'warning') {
        icon = 'warning.png'
    }
    var container = '<div id="info_message" class="notification_background ' + msgclass + '" onclick="return closeNotification();" title="Click to Hide Notification"><div class="center_auto"><div class="info_message_text message_area">';
    container += '<img class="message_icon" src="' + options['imagepath'] + icon + '" alt="' + options['type'] + '" title="' + options['type'] + '" />&nbsp;';
    container += options['message'].replaceAll('\\n', '<br />');
    container += '</div><div class="info_progress"></div><div class="clearboth"></div>';
    container += '</div>';
    $notification = $(container);
    $('body').append($notification);
    var divHeight = $('div#info_message').height();
    $('div#info_message').css({
        top: '-' + divHeight + 'px'
    });
    $('div#info_message').show();
    slideDownNotification(options['showAfter'], options['autoClose'], options['duration']);
    var animationDuration = options['duration'] + "s";
    var progressDuration = (options['duration'] - 1) + "s";
    $('div#info_message').css("-webkit-animation-duration", animationDuration).css("-moz-animation-duration", animationDuration).css("-o-animation-duration", animationDuration).css("-ms-animation-duration", animationDuration).css("animation-duration", animationDuration);
    $('#info_message .info_progress').css("-webkit-animation-duration", progressDuration).css("-moz-animation-duration", progressDuration).css("-o-animation-duration", progressDuration).css("-ms-animation-duration", progressDuration).css("animation-duration", progressDuration);
    $('.link_notification').on('click', function () {
        $('.info_more_descrption').html(options['description']).slideDown('fast')
    })
}
String.prototype.replaceAll = function (token, newToken, ignoreCase) {
    var str, i = -1,
        _token;
    if ((str = this.toString()) && typeof token === "string") {
        _token = ignoreCase === true ? token.toLowerCase() : undefined;
        while ((i = (_token !== undefined ? str.toLowerCase().indexOf(_token, i >= 0 ? i + newToken.length : 0) : str.indexOf(token, i >= 0 ? i + newToken.length : 0))) !== -1) {
            str = str.substring(0, i).concat(newToken).concat(str.substring(i + token.length))
        }
    }
    return str
};
function closeNotification(duration) {
    var divHeight = $('div#info_message').height();
    setTimeout(function () {
        $('div#info_message').animate({
            top: '-' + divHeight
        });
        setTimeout(function () {
            $('div#info_message').remove()
        }, 200)
    }, parseInt(duration * 1000))
}
function slideDownNotification(startAfter, autoClose, duration) {
    setTimeout(function () {
        $('div#info_message').animate({
            top: 0
        });
        if (autoClose) {
            setTimeout(function () {
                closeNotification(duration)
            }, duration)
        }
    }, parseInt(startAfter * 1000))
}