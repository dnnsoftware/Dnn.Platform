(function ($, window) {
    $.fn.journalComments = function (options) {
        $.fn.journalComments.defaultOptions = {
            maxLength: 2000
        };
        var opts = $.extend({}, $.fn.journalComments.defaultOptions, options),
            $wrap = this,
            $id = $(this).attr('id'),
            $maxLength = opts.maxLength,
            $textarea = $('#' + $id + '-txt'),
            $placeHolder = $('#' + $id + ' .editorPlaceholder'),
            $button = $('#' + $id + ' .cmtbtn a'),
            $delete = $('#' + $id + ' .miniclose');

        $placeHolder.click(function () {
            $placeHolder.hide();
            $button.addClass('disabled').show();
            $textarea.show().animate({
                height: '+=45'
            }, 400, function () {

                $textarea.focus();
                $textarea.on('keypress', isDirtyHandler);
            });
        });
        $delete.on('click', deleteComment);

        $textarea.on('paste', function (e) {
            setTimeout(function () {
                $textarea.val($textarea.val());
                if ($textarea.val().length > $maxLength) {
                    var txt = $textarea.val().substring(0, $maxLength);
                    $textarea.val(txt);
                }
            }, 100);
            isDirtyHandler(e);

        });
        function deleteComment() {
            var data = {};
            data.JournalId = $id.replace('jcmt-', '');
            data.CommentId = $(this).parent().attr('id').replace('cmt-', '');
            Post('CommentDelete', data);
            $(this).parent().fadeOut(function () {
                var p = $(this).parent();
                var id = $(this).attr('id');
                $(this).animate({
                    height: '0'
                }, 400, function () {
                    p.remove('#' + id);
                });

            });
        }
        var isDirtyHandler = function (event) {
            $button.removeClass('disabled');
            $textarea.off('keypress', isDirtyHandler);
        };

        $button.click(function (event) {
            event.preventDefault();
            var jid = $id.replace('jcmt-', '');
            var data = {};
            data.JournalId = jid;
            data.Comment = encodeURIComponent($textarea.val());
            data.mentions = $textarea.data("mentions");
            var tmpValue = $textarea.val();
            tmpValue = tmpValue.replace(/<(?:.|\n)*?>/gm, '').replace(/\s+/g, '').replace(/&nbsp;/g, '');
            if (tmpValue == '') {
                return false;
            }
            if (data.Comment == '' || data.Comment == '%3Cbr%3E') {
                return false;
            }
            Post('CommentSave', data, commentComplete, jid);
        });
        function commentComplete(data, journalId) {
            $textarea.animate({
                height: '0'
            }, 400, function () {
                $button.addClass('disabled').hide();
                $textarea.val('').hide();
                $placeHolder.show();

            });
            var li = $(data);
            li.insertBefore('#' + $id + ' .cmteditarea');
            $(li).find('.miniclose').on('click', deleteComment);
            bindConfirm();
        }
        function Post(method, data, callback, journalId) {
            var sf = opts.servicesFramework;
            
            $.ajax({
                type: "POST",
                url: sf.getServiceRoot('Journal') + 'Services/' + method,
                beforeSend: sf.setModuleHeaders,
                data: data,
                success: function (data) {
                    if (typeof (callback) != "undefined") {
                        callback(data, journalId);

                    }
                },
                error: function (xhr, status, error) {
                    alert(error);
                }
            });
        };
    }
} (jQuery, window));