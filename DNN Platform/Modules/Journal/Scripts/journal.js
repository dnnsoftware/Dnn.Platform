;
var journalItem = {};
journalItem.JournalType = 'status';
journalItem.Title = '';
journalItem.Summary = '';
journalItem.Body = '';
journalItem.ItemData = null;
journalItem.Security = 'E';
var cancelRequest = false;
var photoTool = null;

function attachPhoto(fileId, path, isImage) {
    $('#tbar-attach-Area').hide();
    $(".filePreviewArea").append("<div id='attachClose' class='miniclose'></div>");
    $("#attachClose").show().click(function () {
        journalItem.ItemData = null;
        journalItem.JournalType = 'status';
        $('.filePreviewArea').empty();
        $('#tbar-attach-Area').show();
    });
  
    if (photoTool.hasClass('selected') && isImage) {
        journalItem.JournalType = 'photo';
    } else {
        journalItem.JournalType = 'file';
    }
    journalItem.ItemData = {};
    journalItem.ItemData.ImageUrl = path;

    if (isImage) {
        $(".filePreviewArea").show().append($("<img src='" + path + "' />").css("width", "120px").hide().fadeIn());
        journalItem.ItemData.Url = 'fileid=' + fileId;
    } else {
        $(".filePreviewArea").show().append($("<img src='" + path + "' />").hide().fadeIn());
        journalItem.ItemData.Url = 'fileid=' + fileId;
    }
    $("#btnShare").removeClass('disabled');
    $("#btnShare").unbind('keypress', 'isDirtyHandler');

}

function bindConfirm() {
    var options = journalOptions;
    $(".journalrow .minidel, .journalrow .miniclose").each(function () {
        if ($(this).data("confirmBinded")) {
            return;
        }

        var $this = $(this);
        var oThis = this;

        $this.data("confirmBinded", true);
        var clickFuncs = [];
        if (typeof $this.attr("onclick") != "undefined" && $this.attr("onclick").length > 0) {
            var clickFunc = $this.attr("onclick").substr(0, $this.attr("onclick").indexOf("("));
            $this.attr("onclick", "");
            clickFuncs.push(eval(clickFunc));
        } else {
            for (var i = 0; i < $this.data("events").click.length; i++) {
                var handler = $this.data("events").click[i].handler;
                if (typeof handler.name != "undefined" && handler.name.length > 0) {
                    clickFuncs.push(handler);
                    break;
                }
            }

            $this.unbind("click");
        }

        $this.dnnConfirm({
            text: options.confirmText,
            yesText: options.yesText,
            noText: options.noText,
            title: options.title,
            isButton: true,
            callbackTrue: function () {
                for (var i = 0; i < clickFuncs.length; i++)
                    clickFuncs[i].call(oThis, oThis);
            }
        });
    });
}

function buildLikes(data, journalId) {
    var currLike = $('#like-' + journalId).text();
    if (currLike == resxLike) {
        $('#like-' + journalId).fadeOut(function () {
            $(this).text(resxUnLike).fadeIn();
        });
    } else {
        $('#like-' + journalId).fadeOut(function () {
            $(this).text(resxLike).fadeIn();
        });
    }
    $('#jid-' + journalId + ' .likes').fadeOut(function () {
        $(this).empty().append(data.LikeList).fadeIn();
    });
};

function getItems(sf) {
    var rows = $(".journalrow").get();
        
    data = {};
    data.ProfileId = pid;
    data.GroupId = gid;
    data.RowIndex = rows.length + 1;
    data.MaxRows = pagesize;

    $.ajax({
        type: "POST",
        url: sf.getServiceRoot('Journal') + 'Services/GetListForProfile',
        beforeSend: sf.setModuleHeaders,
        data: data,
        success: function (data) {
            if (data.length > 0) {
                $("#journalItems").append(data);
                pluginInit();
                var newRows = $(".journalrow").get();
                var diff = (newRows.length - rows.length);
                if (diff < pagesize) {
                    $("#getMore").hide();
                }
            }
            if (typeof (callback) != "undefined") {
                callback(data);
            }
        },
        error: function (xhr, status, error) {
            alert(error);
        }
    });
}

function IsImage(value) {
    if (value != null) {
        var imageTypesArray = journalOptions.imageTypes.split(",");
        return $.inArray(value, imageTypesArray) >= 0;
    } else {
        return false;
    }
}

function journalDelete(obj) {
    var p = obj.parentNode;
    var jid = p.id.replace('jid-', '');
    var data = {};
    data.JournalId = jid;
    journalPost('SoftDelete', data, journalRemove, jid);
};

function journalPost(method, data, callback, journalId) {
    var sf = journalOptions.servicesFramework;
    $.ajax({
        type: "POST",
        url: sf.getServiceRoot('Journal') + "Services/" + method,
        beforeSend: sf.setModuleHeaders,
        data: data,
        success: function (data) {
            if (typeof (callback) != "undefined") {
                callback(data,journalId);
            }
        },
        error: function (xhr, status, error) {
            alert(error);
        }
    });
};

function journalRemove(data, jid) {
    $('#jid-' + jid).slideUp(function () {
        $(this).remove();
    });
};

function pluginInit() {
    var sf = journalOptions.servicesFramework;
    
    $('.jcmt').each(function () {
        if($(this).data("journalCommentsBinded")) {
            return;
        }
        $(this).data("journalCommentsBinded", true);
            
        $(this).journalComments(commentOpts);
    });
        
    var rows = $(".journalrow");
    if (rows.length == pagesize) {
        $("#getMore").show();
    }
        
    $('a[id^="cmtbtn-"]').each(function () {
        if($(this).data("clickBinded")) {
            return;
        }
        $(this).data("clickBinded", true);

        $(this).click(function(e) {

            e.preventDefault();
            var jid = $(this).attr('id').replace('cmtbtn-', '');
            var cmtarea = $("#jcmt-" + jid + " .cmteditarea");
            var cmtbtn = $("#jcmt-" + jid + " .cmtbtn");
            var cmtbtnlink = $("#jcmt-" + jid + " .cmtbtn a");
            if (cmtarea.css('display') == 'none') {
                cmtarea.show();
                cmtbtnlink.addClass('disabled');
                cmtbtn.show();
                $("#jcmt-" + jid + "-txt").focus();

            } else {

                var cmtedit = $("#jcmt-" + jid + " .cmteditor");
                var plh = $("#jcmt-" + jid + " .editorPlaceholder");
                cmtedit.animate({
                    height: '0'
                }, 400, function() {
                    cmtbtn.hide();
                    cmtbtnlink.addClass('disabled').hide();
                    cmtedit.text('').hide();
                    cmtarea.hide();
                    plh.show();
                });
            }
        });
    });

    $('a[id^="like-"]').each(function () {
        if($(this).data("clickBinded")) {
            return;
        }
        $(this).data("clickBinded", true);

        $(this).click(function(e) {
            e.preventDefault();
            var jid = $(this).attr('id').replace('like-', '');
            var data = { };
            data.JournalId = jid;
            journalPost('Like', data, buildLikes, jid);
        });
    });

    bindConfirm();

    if (!$("#getMore").data("clickBinded")) {
        $("#getMore").click(function(e) {
            getItems(sf);
            e.preventDefault();
        });
        $("#getMore").data("clickBinded", true);
    }
    $('#journalContent, .cmteditor').mentionsInput({servicesFramework: sf});
}

(function ($, window) {
    "use strict";
    function resetJournalItem() {
        journalItem = {};
        journalItem.JournalType = 'status';
        journalItem.Title = '';
        journalItem.Summary = '';
        journalItem.Body = '';
        journalItem.ItemData = null;
        journalItem.Security = 'E';
        journalItem.Tags = [];
        journalItem.Users = [];
    }


    
    $.fn.journalTools = function (options) {
        $.fn.journalTools.defaultOptions = {
            placeHolder: '#journalPlaceholder',
            shareButton: '#btnShare',
            closeButton: '#journalClose',
            optionsArea: '#journalOptionArea',
            content: '#journalContent',
            photoTool: '#tbar-photo',
            attachTool: '#tbar-attach',
            permTool: '#tbar-perm',
            securityMenu: '.securityMenu',
            photoText: '#tbar-photoText',
            fileText: '#tbar-fileText',
            maxLength: 250,
            fileManagerSelector: '#userFileManager'
        };
        var opts = $.extend({}, $.fn.journalTools.defaultOptions, options),
            $wrap = this,
            $placeHolder = $wrap.find(opts.placeHolder),
            $shareButton = $wrap.find(opts.shareButton),
            $closeButton = $wrap.find(opts.closeButton),
            $optionsArea = $wrap.find(opts.optionsArea),
            $content = $wrap.find(opts.content),
            $photoTool = $wrap.find(opts.photoTool),
            $attachTool = $wrap.find(opts.attachTool),
            $permTool = $wrap.find(opts.permTool),
            $photoArea = $wrap.find(opts.photoTool + '-Area'),
            $attachArea = $wrap.find(opts.attachTool + '-Area'),
            $photoText = $wrap.find(opts.photoText),
            $fileText = $wrap.find(opts.fileText),
            $contentShow = false,
            $maxLength = opts.maxLength,
            $securityMenu = $wrap.find(opts.securityMenu),
            $userFileManager = $wrap.find(opts.fileManagerSelector);
        
        photoTool = $photoTool;

        var isDirtyHandler = function (event) {
            $shareButton.removeClass('disabled');
            $shareButton.unbind('keypress', isDirtyHandler);
        };
        $permTool.click(function () {
            $securityMenu.toggle();
        });
        $securityMenu.find('.handle').click(function () {
            $securityMenu.toggle();
        });
        $(opts.securityMenu + ' input').click(function () {
            journalItem.Security = $(this).val();
        });

        $photoTool.click(function () {
            photoToolClick();
        });
        $attachTool.click(function () {
            attachToolClick();
        });

        $placeHolder.click(function () {
            showContent();
        });

        $closeButton.click(function () {
            closeEditor();
        });
        $content.bind('keypress', function(event) {
            if (event.keyCode == 8 || event.keyCode == 46) {
                return;
            }
            if ($content.val().length >= $maxLength && $maxLength > 0) {
                return false;
            }
        });
        $content.bind('paste', function (e) {
            setTimeout(function () {
                $content.val($content.val());
                if ($content.val().length>=$maxLength && $maxLength > 0) {
                    var txt = $content.val().substring(0,$maxLength);
                    $content.val(txt);
                }
            }, 100);

        });
        function closeEditor() {
            $closeButton.hide();
            $contentShow = false;
            $securityMenu.hide();
            $shareButton.addClass('disabled').hide();
            $photoArea.hide();
            $attachArea.hide();
            $optionsArea.hide();
            $('#tbar span').removeClass('selected');
            
            var linkArea = $('#linkArea');
            $('#linkArea #imagePreviewer').hide();
            linkArea.find('#linkInfo b').text('');
            linkArea.find('#linkInfo p').text('');
            $content.data('linkedUp', false);
            linkArea.hide();
            linkArea.data('url','');

            $attachTool.unbind('click');
            $photoTool.unbind('click');
            $photoTool.click(function () {
                photoToolClick();
            });
            $attachTool.click(function () {
                attachToolClick();
            });
            $(".filePreviewArea").empty();
            $content.unbind('keypress', isDirtyHandler);
            $content.animate({
                height: '0'
            }, 400, function () {
                $content.val('').hide();
                $placeHolder.show();
                resetJournalItem();
            });
        }
        var showContent = function () {
            $placeHolder.hide();
            $shareButton.addClass('disabled').show();
            $content.show().animate({
                height: '+=65'
            }, 400, function () {
                $contentShow = true;
                $content.focus();
                $content.bind('keypress', isDirtyHandler);
                $closeButton.show();


            });
        };
        var photoToolClick = function() {
            $('#tbar-attach-Area').show();
            if ($photoTool.hasClass('selected')) {
                $photoTool.removeClass('selected');
                $optionsArea.hide();
                $attachArea.hide();
                $attachTool.unbind('click');
                 $('.filePreviewArea').empty();
                resetJournalItem();
                $attachTool.click(function() {
                    attachToolClick();
                });
            } else {
                if ($contentShow == false) {
                    showContent();
                }
                $userFileManager.userFileManager.setFileExtensions(journalOptions.imageTypes);
                $photoTool.addClass('selected');
                $attachTool.unbind('click');
                $attachTool.bind('click', clickDisable);
                $optionsArea.show();
                $attachArea.show();
                $fileText.hide();
                $photoText.show();
            }
        };
        var attachToolClick = function () {
            $('#tbar-attach-Area').show();
            if ($attachTool.hasClass('selected')) {
                $attachTool.removeClass('selected');
                $photoTool.unbind('click');
                $photoTool.click(function () {
                    photoToolClick();
                });
                $attachArea.hide();
                $optionsArea.hide();
                resetJournalItem();
                 journalItem.ItemData = null;
            } else {
                if ($contentShow == false) {
                    showContent();
                };
                $userFileManager.userFileManager.setFileExtensions('');
                $attachTool.addClass('selected');
                $photoTool.unbind('click');
                $photoTool.bind('click', clickDisable);
                $attachArea.show();
                $optionsArea.show();
                $fileText.show();
                $photoText.hide();
            }
        };
        var clickDisable = function (e) {
            e.preventDefault();
            //return false;
        };
        $shareButton.click(function (e) {
            cancelRequest = true;
            e.preventDefault();
            var data = {};
            data.text = encodeURIComponent($content.val());
           
            data.profileId = pid;
            data.groupId = gid;
            data.journalType = journalItem.JournalType;
            data.securitySet = journalItem.Security;
            var ItemData = journalItem.ItemData;
            if (ItemData != null) {
                data.itemData = JSON.stringify(ItemData);
            } else {
                data.itemData = '';
            }
            if ((data.text == '' && data.itemData == '') || data.text == '%3Cbr%3E') {
                return false;
            }
            //Check for a value
            var tmpValue = $content.val();
            tmpValue = tmpValue.replace(/<(?:.|\n)*?>/gm, '').replace(/\s+/g, '').replace(/&nbsp;/g,'');
            if (tmpValue == '' && data.itemData == '') {
                return false;
            }
            data.mentions = $content.data("mentions");
            $.ajax({
                type: "POST",
                url: opts.servicesFramework.getServiceRoot('Journal') + "Services/Create",
                data: data,
                beforeSend: opts.servicesFramework.setModuleHeaders,
                success: function (data) {

                    var req = {};
                    req.ProfileId = pid;
                    req.GroupId = gid;
                    req.RowIndex = 0;
                    req.MaxRows = 1;
                    closeEditor();
                    getNewItem(req, insertNewItem);

                },
                error: function (xhr, status, error) {
                    alert(error);
                }
            });

        });
        function getNewItem(data, callback) {
            $.ajax({
                type: "POST",
                url: opts.servicesFramework.getServiceRoot('Journal') + "Services/GetListForProfile",
                beforeSend: opts.servicesFramework.setModuleHeaders,
                data: data,
                success: function (data) {

                    if (typeof (callback) != "undefined") {
                        callback(data);
                    }

                },
                error: function (xhr, status, error) {
                    alert(error);
                }
            });
        }
        function insertNewItem(html) {
            $(html).hide().prependTo("#journalItems").fadeIn();
            pluginInit();
            //.slideDown();

        }

    };
    $.fn.previewify = function (options) {
        var images = [];
        var currImage = 0;
        var opts = $.extend({}, $.fn.previewify.defaultOptions, options),
            $wrap = this,
            $textArea = $wrap.find(opts.textareaSelector),
            $clearLink = $wrap.find(opts.clearPreviewSelector),
            $previewArea = $wrap.find(opts.previewSelector);
        // updates the preview area
        function updatePageValues(url) {

            if ($previewArea.data('url') === url) {
                return;
            }
            cancelRequest = false;
            $textArea.data('linkedUp', true);
            $previewArea.data('url', url);
            $clearLink.show();

            var data = { Url: url };
            $.ajax({
                type: "POST",
                url: opts.servicesFramework.getServiceRoot('Journal') + 'Services/PreviewURL',
                beforeSend: opts.servicesFramework.setModuleHeaders,
                data: data,
                success: function (data) {
                    buildLinkPreview(data);
                },
                error: function (xhr, status, error) {
                    alert(error);
                }
            });
        }
        function prevImg() {
            currImage = currImage - 1;
            if (currImage < 0) {
                currImage = images.length - 1;
            };
            if (currImage > images.length - 1) {
                currImage = 0;
            };
            $(opts.previewSelector + ' #imgCount').text(currImage + 1 + ' of ' + images.length);
            $previewArea.find('#image img').fadeOut(function () {
                $(this).hide().attr('src', '').attr('src', images[currImage].URL).fadeIn();
            });
            journalItem.ItemData.ImageUrl = images[currImage].URL;
        };
        function nextImg() {
            currImage = currImage + 1;
            if (currImage < 0) {
                currImage = images.length - 1;
            };
            if (currImage > images.length - 1) {
                currImage = 0;
            };
            $(opts.previewSelector + ' #imgCount').text(currImage + 1 + ' of ' + images.length);
            $previewArea.find('#image img').fadeOut(function () {
                $(this).hide().attr('src', '').attr('src', images[currImage].URL).fadeIn();
            });
            journalItem.ItemData.ImageUrl = images[currImage].URL;
        };
        function buildLinkPreview(link) {
            //$previewArea.empty();
            if (link.URL == null) {
                return;
            }
            if (cancelRequest) {
                removePageValues();
                cancelRequest = false;
                return;
            }
            journalItem.ItemData = {};
            if (link.Images.length > 0) {
                images = link.Images;
                journalItem.ItemData.ImageUrl = link.Images[0].URL;
                $previewArea.find('#image img').attr('src', link.Images[0].URL);
                $(opts.previewSelector + ' #imgPrev').bind('click', prevImg);
                $(opts.previewSelector + ' #imgNext').bind('click', nextImg);
                $(opts.previewSelector + ' #imgCount').text(currImage + 1 + ' of ' + images.length);
                $(opts.previewSelector + ' #imagePreviewer').show();
            } else {
                $(opts.previewSelector + ' #imagePreviewer').hide();
            }
            if (link.Description == null) {
                link.Description = '';
            };
            link.Description = link.Description.replace('&amp;', '&');
            link.Description = decodeURIComponent(link.Description);
            $previewArea.find('#linkClose').show();
            $previewArea.find('#linkInfo b').text(link.Title);
            $previewArea.find('#linkInfo p').text(link.Description);
            $previewArea.show();
            $previewArea.parent().show();
            journalItem.JournalType = 'link';

            journalItem.ItemData.Title = link.Title;
            journalItem.ItemData.Description = escape(link.Description);
            journalItem.ItemData.URL = link.URL;

        }
        // resets the preview area (but keeps the associated url in a data field for future use)
        function removePageValues() {
            $(opts.previewSelector + ' #imagePreviewer').hide();
            images = [];
            currImage = 0;
            $previewArea.find('#linkInfo b').text('');
            $previewArea.find('#linkInfo p').text('');
            $textArea.data('linkedUp', false);
            $previewArea.hide();
            $clearLink.hide();
            journalItem.JournalType = 'status';
            journalItem.ItemData = null;
            $previewArea.data('url','');
        }

        // returns true if link already found, false if not
        function isLinkedUp() {
            return $textArea.data('linkedUp');
        }

        // parses the first url out of a block of text
        function findUrl(enteredText) {
            if (typeof (enteredText) == 'undefined') {
                enteredText = $textArea.val();
            }
            var replacePattern1, replacePattern2, replacePattern3, url;
            //URLs starting with "www."
            replacePattern1 = /(^|[^\/])(www\.[\S]+(\b|$))/gim;
            url = replacePattern1.exec(enteredText);
            if (!url) {
                //URLs starting with http://, https://, or ftp://
                replacePattern2 = /(\b(https?|ftp):\/\/[-A-Z0-9+&@#\/%?=~_|!:,.;]*[-A-Z0-9+&@#\/%=~_|])/gim;
                url = replacePattern2.exec(enteredText);
            }
            if (url) {
                url = (url.toString().split(',')[0]);
                updatePageValues(url);
            }
        }

        // set default state, link not found.
        $textArea.data('linkedUp', false);

        // key event
        $textArea.keyup(function (e) {

            var code = e.keyCode || e.which;
            if (code !== 13 && code !== 32) { // space, enter key
                return;
            }
            if (!isLinkedUp()) {
                findUrl($(this).val());
            }
        });

        // paste event
        $textArea.bind('paste', function (e) {

            setTimeout(function () {
                $textArea.val($textArea.val());
                if (!isLinkedUp()) {
                    findUrl();
                }
            }, 100);

        });

        // removes the preview
        $clearLink.click(function (e) {
            e.preventDefault();
            removePageValues();
        });

    };

    $.fn.previewify.defaultOptions = {
        textareaSelector: 'textarea',
        previewSelector: 'div',
        clearPreviewSelector: 'a'
    };



} (jQuery, window));
