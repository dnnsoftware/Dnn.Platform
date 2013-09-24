(function ($) {
    var settings = {
        inEffect: { opacity: 'show' },	// in effect
        inEffectDuration: 600,				// in effect duration in miliseconds
        stayTime: 3000,				// time in miliseconds before the item has to disappear
        text: '',					// content of the item. Might be a string or a jQuery object. Be aware that any jQuery object which is acting as a message will be deleted when the toast is fading away.
        sticky: false,				// should the toast item sticky or not?
        type: 'notice', 			// notice, warning, error, success
        position: 'top-right',        // top-left, top-center, top-right, middle-left, middle-center, middle-right ... Position of the toast container holding different toast. Position can be set only once at the very first call, changing the position after the first call does nothing
        closeText: '',                 // text which will be shown as close button, set to '' when you want to introduce an image via css
        close: null                // callback function when the toastmessage is closed
    };

    var htmlSubstring = function (s, n) {
        var m, r = /<([^>\s]*)[^>]*>/g,
			stack = [],
			lasti = 0,
			result = '';

        //for each tag, while we don't have enough characters
        while ((m = r.exec(s)) && n) {
            //get the text substring between the last tag and this one
            var temp = s.substring(lasti, m.index).substr(0, n);
            //append to the result and count the number of characters added
            result += temp;
            n -= temp.length;
            lasti = r.lastIndex;

            if (n) {
                result += m[0];
                if (m[1].indexOf('/') === 0) {
                    //if this is a closing tag, than pop the stack (does not account for bad html)
                    stack.pop();
                } else if (m[1].lastIndexOf('/') !== m[1].length - 1) {
                    //if this is not a self closing tag than push it in the stack
                    stack.push(m[1]);
                }
            }
        }

        //add the remainder of the string, if needed (there are no more tags in here)
        result += s.substr(lasti, n);

        //fix the unclosed tags
        while (stack.length) {
            result += '</' + stack.pop() + '>';
        }

        return result;
    };

    var methods = {
        init: function (options) {
            if (options) {
                $.extend(settings, options);
            }
        },

        showAllToasts: function (msgOptions) {
            var messages = msgOptions.messages;
            var seeMoreText = msgOptions.seeMoreText;
            var seeMoreLink = msgOptions.seeMoreLink;

            if (!messages || !messages.length) return null;

            if ($('.toast-container').length) {
                $('.toast-container').remove();
            }

            var localSettings = {};
            $.extend(localSettings, settings);

            // declare variables
            var toastWrapAll, toastItemOuter, toastItemInner, toastItemClose, toastItemImage;

            var allToasts = $('<ul></ul>');
            var length = messages.length;
            for (var i = 0; i < length; i++) {
                var singleMsg = messages[i];
                var singleToast = $('<li></li>').addClass('toast-message');
                var subject = singleMsg.subject ? singleMsg.subject : '';
                var body = singleMsg.body ? singleMsg.body : '';
                var regex = /(<([^>]+)>)/ig;
                var stripedBody = body.replace(regex, "");

                subject = subject.length > 40 ? subject.substring(0, 40) + '...' : subject;
                body = stripedBody.length > 75 ? htmlSubstring(body, 75) + '...' : body;
                singleToast.append('<a href="' + seeMoreLink + '" >' + subject + '</a>');
                singleToast.append('<p>' + body + '</p>');
                allToasts.append(singleToast);
            }

            var seeMoreContent = $('<li></li>').addClass('toast-lastChild');
            seeMoreContent.append('<a href="' + seeMoreLink + '" class="seeMoreLink" >' + seeMoreText + '</a>');
            allToasts.append(seeMoreContent);

            toastWrapAll = $('<div></div>').addClass('toast-container').addClass('toast-position-' + localSettings.position).appendTo('body');
            var originalPos = null, top = 20, right = 80;
            // get the position from cookie
            var cookieId = 'nebula-toast-position';
            var cookieValue = dnn.dom.getCookie(cookieId);
            if (cookieValue) {
                var splitCookieValue = cookieValue.split('|');               
                top = parseInt(splitCookieValue[0], 10);
                right = parseInt(splitCookieValue[1], 10);
                toastWrapAll.css({
                    top: top,
                    right: right
                });
            }


            var mouseMove = function (e) {
                if (originalPos !== null) {
                    var x = e.pageX;
                    var y = e.pageY;

                    var offsetX = originalPos.x - x;
                    var offsetY = originalPos.y - y;
                    originalPos.x = x;
                    originalPos.y = y;

                    top -= offsetY;
                    right += offsetX;

                    toastWrapAll.css({
                        top: top,
                        right: right
                    });
                }
            };

            toastWrapAll.bind('mousedown', function (e) {
                var x = e.pageX;
                var y = e.pageY;
                originalPos = {
                    x: x,
                    y: y
                };
                $(document).bind('mousemove', mouseMove);

            }).bind('mouseup', function (e) {
                originalPos = null;
                $(document).unbind('mousemove', mouseMove);

                var cValue = top + '|' + right;
                dnn.dom.setCookie(cookieId, cValue, 20 * 365); // never expire - set 20 years...
            });

            toastItemOuter = $('<div></div>').addClass('toast-item-wrapper');
            toastItemInner = $('<div></div>').hide().addClass('toast-item toast-type-' + localSettings.type).appendTo(toastWrapAll).append(allToasts).animate(localSettings.inEffect, localSettings.inEffectDuration).wrap(toastItemOuter);
            toastItemClose = $('<div></div>').addClass('toast-item-close').prependTo(toastItemInner).html(localSettings.closeText).click(function () { $().dnnToastMessage('removeToast', toastItemInner, localSettings) });

            if (navigator.userAgent.match(/MSIE 6/i)) {
                toastWrapAll.css({ top: document.documentElement.scrollTop });
            }

            return toastItemInner;
        },

        removeToast: function (obj, options) {
            obj.animate({ opacity: '0' }, 600, function () {
                obj.parent().animate({ height: '0px' }, 300, function () {
                    obj.parent().remove();
                });
            });
            // callback
            if (options && options.close !== null) {
                options.close();
            }
        }
    };

    $.fn.dnnToastMessage = function (method) {

        // Method calling logic
        if (methods[method]) {
            return methods[method].apply(this, Array.prototype.slice.call(arguments, 1));
        } else if (typeof method === 'object' || !method) {
            return methods.init.apply(this, arguments);
        } else {
        }
    };

})(jQuery);