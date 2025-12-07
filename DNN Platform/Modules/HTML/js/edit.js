$(function () {

    var portalId = $('#dnnEditHtml').attr('data-portalid');
    var tabId = $('#dnnEditHtml').attr('data-tabid');
    var moduleId = $('#dnnEditHtml').attr('data-moduleid');
    var urlpars = 'tabid=' + tabId + '&PortalID=' + portalId + '&mid=' + moduleId;

    // window.CKEDITOR_BASEPATH = '/Providers/HtmlEditorProviders/DNNConnect.CKE/js/ckeditor/4.18.0/';

    var editorConfigeditortxtContent = {
        //allowedContent: false,
        //allowedContent: true,
        justifyClasses: ['text-left', 'text-center', 'text-right', 'text-justify'],
        allowedContent: 'h1 h2 strong u sub sup em ol label hr blockquote table theader th tbody tr td; a[!href](*); img[!src,alt,width,height](*); div(*);ul(*); li(*);span(*);p(*)',
        autoGrow_bottomSpace: 0,
        autoGrow_maxHeight: 0,
        autoGrow_minHeight: 200, autoGrow_onStartup: false, autoParagraph: true,
        autosave_delay: 25, autoUpdateElement: true, baseFloatZIndex: 10000,
        basicEntities: true,
        blockedKeystrokes: [CKEDITOR.CTRL + 66, CKEDITOR.CTRL + 73, CKEDITOR.CTRL + 85],
        browserContextMenuOnCtrl: true, clipboard_defaultContentType: 'html',
        codemirror: {
            theme: 'default', lineNumbers: true, lineWrapping: true,
            matchBrackets: true, autoCloseTags: false, enableSearchTools: true,
            enableCodeFolding: true, enableCodeFormatting: true, autoFormatOnStart: false,
            autoFormatOnUncomment: true, highlightActiveLine: true,
            highlightMatches: true, showTabs: false, showFormatButton: true,
            showCommentButton: true, showUncommentButton: true
        },
        colorButton_colors: '00923E,F8C100,28166F',
        colorButton_enableMore: true,
        contentsLangDirection: '',
        dataIndentationChars: ' ',
        defaultLanguage: 'en', defaultLinkType: 'url',
        dialog_backgroundCoverColor: 'white', dialog_backgroundCoverOpacity: '0.5',
        dialog_buttonsOrder: 'OS', dialog_magnetDistance: 20, dialog_noConfirmCancel: 0,
        dialog_startupFocusTab: false, disableNativeSpellChecker: true,
        disableNativeTableHandles: true, disableObjectResizing: false,
        disableReadonlyStyling: false, div_wrapTable: false,
        docType: '<!DOCTYPE html>', enableTabKeyTools: true,
        enterMode: 1, entities: true, entities_additional: '#39',
        entities_greek: false, entities_latin: false,
        entities_processNumerical: false,
        extraPlugins: 'dnnpages,wordcount,notification',
        filebrowserWindowFeatures: 'location=no,menubar=no,toolbar=no,dependent=yes,minimizable=no,modal=yes,alwaysRaised=yes,resizable=yes,scrollbars=yes',
        fillEmptyBlocks: true, flashAddEmbedTag: false,
        flashConvertOnEdit: false, flashEmbedTagOnly: false,
        floatSpaceDockedOffsetX: 0, floatSpaceDockedOffsetY: 0, floatSpacePinnedOffsetX: 0,
        floatSpacePinnedOffsetY: 0, fontSize_sizes: '12px;2.3em;130%;larger;x-small',
        font_names: 'Arial;Times New Roman;Verdana', forceEnterMode: false,
        forcePasteAsPlainText: false, forceSimpleAmpersand: false,
        format_tags: 'p;h1;h2;h3;h4;h5;h6;pre;address;div', fullPage: false,
        htmlEncodeOutput: false, ignoreEmptyParagraph: true,
        image_previewText: 'Lorem ipsum dolor...', image_removeLinkByEmptyURL: true,
        indentOffset: 40, indentUnit: 'px', linkJavaScriptLinksAllowed: false,
        linkShowAdvancedTab: true, linkShowTargetTab: true, magicline_color: '#FF0000',
        magicline_holdDistance: '0.5', magicline_putEverywhere: false,
        magicline_triggerOffset: 30,
        menu_groups: 'clipboard,tablecell,tablecellproperties,tablerow,tablecolumn,table,anchor,link,image,flash,checkbox,radio,textfield,hiddenfield,imagebutton,button,select,textarea,div', menu_subMenuDelay: 400,
        oembed_maxWidth: 0, oembed_maxHeight: 0, pasteFromWordNumberedHeadingToList: false,
        pasteFromWordPromptCleanup: false, pasteFromWordRemoveFontStyles: true,
        pasteFromWordRemoveStyles: true,
        protectedSource: [(/<i class[\s\S]*?>[\s\S]*?<\/i>/gi), (/<span class[\s\S]*?>[\s\S]*?<\/span>/gi), (/<em class[\s\S]*?>[\s\S]*?<\/em>/gi), (/<button class[\s\S]*?>[\s\S]*?<\/button>/gi)], readOnly: false,
        removeFormatAttributes: 'class,style,lang,width,height,align,hspace,valign',
        removeFormatTags: 'b,big,code,del,dfn,em,font,i,ins,kbd,q,samp,small,span,strike,strong,sub,sup,tt,u,var',
        resize_dir: 'both',
        resize_enabled: true,
        resize_maxHeight: 600,
        resize_maxWidth: 3000,
        resize_minHeight: 250,
        resize_minWidth: 750, scayt_autoStartup: false,
        scayt_maxSuggestions: 0, shiftEnterMode: 2, smiley_columns: 8, sourceAreaTabSize: 20,
        startupFocus: false, startupMode: 'wysiwyg', startupOutlineBlocks: false, startupShowBorders: true,
        tabIndex: 0, tabSpaces: 0, templates: 'default',
        templates_replaceContent: true, toolbarCanCollapse: false, toolbarGroupCycling: true, toolbarLocation: 'top',
        toolbarStartupExpanded: true, undoStackSize: 20,
        useComputedState: true,
        wordcount: { showCharCount: false, showWordCount: true },
        language: 'fr-fr', scayt_sLang: 'fr_FR', customConfig: '',
        skin: 'moono', linkDefaultProtocol: 'https://',
        contentsCss: ["/DesktopModules/HTML/CkEditorContents.css"],
        toolbar: [
            { name: 'document', items: ['Source', '-', 'Preview', 'Print', '-', 'Templates'] },
            {
                name: 'clipboard',
                items: ['Cut', 'Copy', 'Paste', 'PasteText', 'PasteFromWord', '-', 'Undo', 'Redo']
            },
            {
                name: 'editing',
                items: ['Find', 'Replace', '-', 'SelectAll']
            },
            { name: 'tools', items: ['Maximize', 'ShowBlocks', '-', 'About'] },
            '/',
            { name: 'paragraph', items: ['NumberedList', 'BulletedList', '-', 'Outdent', 'Indent', 'Blockquote', 'CreateDiv', '-', 'JustifyLeft', 'JustifyCenter', 'JustifyRight', 'JustifyBlock', '-', 'BidiLtr', 'BidiRtl'] }, { name: 'links', items: ['Link', 'Unlink', 'Anchor'] },
            { name: 'insert', items: ['Image', 'Mathjax', 'oembed', 'syntaxhighlight', 'Table', 'HorizontalRule', 'Smiley', 'SpecialChar', 'PageBreak', 'Iframe'] }, '/',
            { name: 'basicstyles', items: ['Bold', 'Italic', 'Underline', 'Strike', 'Subscript', 'Superscript', '-', 'RemoveFormat'] }, { name: 'styles', items: ['Styles', 'Format', 'Font', 'FontSize'] }, { name: 'colors', items: ['TextColor', 'BGColor'] }
        ],
        removePlugins: 'easyimage',
        cloudServices_tokenUrl: '/API/CKEditorProvider/CloudServices/GetToken',
        width: '99%',
        height: 200,
        maxFileSize: 29360128,
        filebrowserBrowseUrl: '/Providers/HtmlEditorProviders/DNNConnect.CKE/Browser/Browser.aspx?Type=Link&' + urlpars + '&ckid=txtContent&mode=Default&lang=fr-FR',
        filebrowserImageBrowseUrl: '/Providers/HtmlEditorProviders/DNNConnect.CKE/Browser/Browser.aspx?Type=Image&' + urlpars + '&ckid=txtContent&mode=Default&lang=fr-FR',
        filebrowserFlashBrowseUrl: '/Providers/HtmlEditorProviders/DNNConnect.CKE/Browser/Browser.aspx?Type=Flash&' + urlpars + '&ckid=txtContent&mode=Default&lang=fr-FR',
        filebrowserUploadUrl: '/Providers/HtmlEditorProviders/DNNConnect.CKE/Browser/Browser.aspx?Command=FileUpload&' + urlpars + '&ckid=txtContent&mode=Default&lang=fr-FR',
        filebrowserFlashUploadUrl: '/Providers/HtmlEditorProviders/DNNConnect.CKE/Browser/Browser.aspx?Command=FlashUpload&' + urlpars + '&ckid=txtContent&mode=Default&lang=fr-FR',
        filebrowserImageUploadUrl: '/Providers/HtmlEditorProviders/DNNConnect.CKE/Browser/Browser.aspx?Command=ImageUpload&' + urlpars + '&ckid=txtContent&mode=Default&lang=fr-FR',
        filebrowserWindowWidth: 870, filebrowserWindowHeight: 800
    };
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
        $('.js-history-rollback').click(function () {
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
    }
    initPage();
});

