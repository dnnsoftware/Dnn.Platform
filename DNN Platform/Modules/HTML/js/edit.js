$(function () {

    var portalId = $('#dnnEditHtml').attr('data-portalid');
    var tabId = $('#dnnEditHtml').attr('data-tabid');
    var moduleId = $('#dnnEditHtml').attr('data-moduleid');

    var editorConfigeditortxtContent = {};
    if (window['editorConfigeditor' + moduleId])
        editorConfigeditortxtContent = window['editorConfigeditor' + moduleId];

    CKEDITOR.replace('EditorContent', editorConfigeditortxtContent);

    var initPage = function () {

        $('#dnnEditHtml form').ajaxForm({
            success: function () {
                window.location = $('#dnnEditHtml').attr('data-returnurl');
            },
            beforeSerialize: function () {
                for (var instanceName in CKEDITOR.instances)
                    CKEDITOR.instances[instanceName].updateElement();
            }
        });

        $('#cmdHistory').click(function () {
            var action = $(this).attr('data-action');
            $('#dnnEditHtml form').ajaxSubmit({
                url: action,
                target: '#dnnEditHtml',
                success: initPage,
                beforeSerialize: function () {
                    for (var instanceName in CKEDITOR.instances)
                        CKEDITOR.instances[instanceName].updateElement();
                }
            });
            // return false to prevent normal browser submit and page navigation
            return false;
        });

        $('#cmdPreview').click(function () {
            var action = $(this).attr('data-action');
            $('#dnnEditHtml form').ajaxSubmit({
                url: action,
                //target: '.ehccContent',
                target: '#dnnEditHtml',
                success: initPage,
                beforeSerialize: function () {
                    for (var instanceName in CKEDITOR.instances)
                        CKEDITOR.instances[instanceName].updateElement();
                }
            });
            // return false to prevent normal browser submit and page navigation
            return false;
        });

        $('#cmdEdit').click(function () {
            var action = $(this).attr('data-action');
            $('#dnnEditHtml form').ajaxSubmit({
                url: action,
                target: '#dnnEditHtml',
                success: function () {
                    initPage();
                    CKEDITOR.replace('EditorContent', editorConfigeditortxtContent);
                },
            });
            // return false to prevent normal browser submit and page navigation
            return false;
        });

        $('.js-history-remove').click(function () {
            var itemId = $(this).attr('data-itemid');
            var action = $(this).attr('data-action');
            $('#dnnEditHtml form').ajaxSubmit({
                url: action,
                beforeSubmit: function (formData, jqForm, options) {
                    formData.push({ name: 'ItemID', value: itemId });
                },
                //target: '.ehccContent',
                target: '#dnnEditHtml',
                success: function () {
                    initPage();
                    CKEDITOR.replace('EditorContent', editorConfigeditortxtContent);
                },
            });
            // return false to prevent normal browser submit and page navigation
            return false;
        });
        $('.js-history-preview').click(function () {
            var itemId = $(this).attr('data-itemid');
            var action = $(this).attr('data-action');
            $('#dnnEditHtml form').ajaxSubmit({
                url: action,
                beforeSubmit: function (formData, jqForm, options) {
                    formData.push({ name: 'ItemID', value: itemId });
                },
                target: '#dnnEditHtml',
                success: function () {
                    initPage();
                },
            });
            // return false to prevent normal browser submit and page navigation
            return false;
        });
        $('.js-history-rollback').click(function () {
            var itemId = $(this).attr('data-itemid');
            var action = $(this).attr('data-action');
            $('#dnnEditHtml form').ajaxSubmit({
                url: action,
                beforeSubmit: function (formData, jqForm, options) {
                    formData.push({ name: 'ItemID', value: itemId });
                },
                target: '#dnnEditHtml',
                success: function () {
                    window.location = window.location.href;
                },
            });
            // return false to prevent normal browser submit and page navigation
            return false;
        });
    }
    initPage();
});

