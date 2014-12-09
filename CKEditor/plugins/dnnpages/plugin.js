(function() {
    CKEDITOR.plugins.add('dnnpages', {
        lang: ['de', 'en', 'pl']
    });
	
	CKEDITOR.scriptLoader.load(CKEDITOR.basePath + 'Tabs.ashx?PortalID=' + document.getElementById('CKDNNporid').value);
	
	// Loads the parameters in a selected link to the link dialog fields.
    var javascriptProtocolRegex = /^javascript:/,
        emailRegex = /^mailto:([^?]+)(?:\?(.+))?$/,
        emailSubjectRegex = /subject=([^;?:@&=$,\/]*)/,
        emailBodyRegex = /body=([^;?:@&=$,\/]*)/,
        anchorRegex = /^#(.*)$/,
        urlRegex = /^((?:http|https|ftp|news):\/\/)?(.*)$/,
        selectableTargets = /^(_(?:self|top|parent|blank))$/,
        encodedEmailLinkRegex = /^javascript:void\(location\.href='mailto:'\+String\.fromCharCode\(([^)]+)\)(?:\+'(.*)')?\)$/,
        functionCallProtectedEmailLinkRegex = /^javascript:([^(]+)\(([^)]+)\)$/;
    
    var popupRegex = /\s*window.open\(\s*this\.href\s*,\s*(?:'([^']*)'|null)\s*,\s*'([^']*)'\s*\)\s*;\s*return\s*false;*\s*/;
    var popupFeaturesRegex = /(?:^|,)([^=]+)=(\d+|yes|no)/gi;
    
    function protectEmailAddressAsEncodedString(address) {
        var charCode,
			length = address.length,
			encodedChars = [];
        for (var i = 0; i < length; i++) {
            charCode = address.charCodeAt(i);
            encodedChars.push(charCode);
        }
        return 'String.fromCharCode(' + encodedChars.join(',') + ')';
    }
    
    function unescapeSingleQuote(str) {
        return str.replace(/\\'/g, '\'');
    }

    function escapeSingleQuote(str) {
        return str.replace(/'/g, '\\$&');
    }
    
    // Make a string's first character uppercase.
    //
    // @param {String}
    //            str String.
    function ucFirst(str) {
        str += '';
        var f = str.charAt(0).toUpperCase();
        return f + str.substr(1);
    }

    var parseLink = function(editor, element) {
        var href = (element && (element.data('cke-saved-href') || element.getAttribute('href'))) || '',
            javascriptMatch, emailMatch, anchorMatch, urlMatch,
            retval = {};

        var emailProtection = editor.config.emailProtection || '';

        if ((javascriptMatch = href.match(javascriptProtocolRegex))) {

            if (emailProtection == 'encode') {
                href = href.replace(encodedEmailLinkRegex, function(match, protectedAddress, rest) {

                    var decodedEmail = 'mailto:' +
                        String.fromCharCode.apply(String, protectedAddress.split(','));

                    if (rest && rest !== 'undefined') {
                        decodedEmail += unescapeSingleQuote(rest);
                    }

                    return decodedEmail;

                });
            }

                // Protected email link as function call.
            else if (emailProtection) {
                href.replace(functionCallProtectedEmailLinkRegex, function(match, funcName, funcArgs) {
                    if (funcName == compiledProtectionFunction.name) {
                        retval.type = 'email';
                        var email = retval.email = {};

                        var paramRegex = /[^,\s]+/g,
                            paramQuoteRegex = /(^')|('$)/g,
                            paramsMatch = funcArgs.match(paramRegex),
                            paramsMatchLength = paramsMatch.length,
                            paramName, paramVal;

                        for (var i = 0; i < paramsMatchLength; i++) {
                            paramVal = decodeURIComponent(unescapeSingleQuote(paramsMatch[i].replace(paramQuoteRegex, '')));
                            paramName = compiledProtectionFunction.params[i].toLowerCase();
                            email[paramName] = paramVal;
                        }
                        email.address = [email.name, email.domain].join('@');
                    }
                });
            }
        }

        if (!retval.type) {

            if ((anchorMatch = href.match(anchorRegex))) {
                retval.type = 'anchor';
                retval.anchor = {};
                retval.anchor.name = retval.anchor.id = anchorMatch[1];
            }
                // Protected email link as encoded string.
            else if ((emailMatch = href.match(emailRegex))) {
                var subjectMatch = href.match(emailSubjectRegex),
                    bodyMatch = href.match(emailBodyRegex);

                retval.type = 'email';
                var email = (retval.email = {});
                email.address = emailMatch[1];
                subjectMatch && (email.subject = decodeURIComponent(subjectMatch[1]));
                bodyMatch && (email.body = decodeURIComponent(bodyMatch[1]));
            }
                // urlRegex matches empty strings, so need to check for href as well.
            else if (href && (urlMatch = href.match(urlRegex))) {

                var isLocalPage = false;
                // check for portal page
                for (var iPage = 0; iPage < dnnpagesSelectBox.length; iPage++) {
                    if (dnnpagesSelectBox[iPage][1] == href) {
                        isLocalPage = true;
                    }
                }

                isLocalPage ? retval.type = 'localPage' : retval.type = 'url';
                retval.url = {};
                retval.url.protocol = urlMatch[1];
                retval.url.url = urlMatch[2];
            } else {
                if (editor.config.defaultLinkType && editor.config.defaultLinkType === "url" || editor.config.defaultLinkType === "localPage" || editor.config.defaultLinkType === "anchor" || editor.config.defaultLinkType === "email") {
                    retval.type = editor.config.defaultLinkType;
                } else {
                    retval.type = 'url';
                }
            }
        }

        // Load target and popup settings.
        if (element) {
            var target = element.getAttribute('target');
            retval.target = {};
            retval.adv = {};

            // IE BUG: target attribute is an empty string instead of null in IE if it's not set.
            if (!target) {
                var onclick = element.data('cke-pa-onclick') || element.getAttribute('onclick'),
                    onclickMatch = onclick && onclick.match(popupRegex);
                if (onclickMatch) {
                    retval.target.type = 'popup';
                    retval.target.name = onclickMatch[1];

                    var featureMatch;
                    while ((featureMatch = popupFeaturesRegex.exec(onclickMatch[2]))) {
                        // Some values should remain numbers (#7300)
                        if ((featureMatch[2] == 'yes' || featureMatch[2] == '1') && !(featureMatch[1] in { height: 1, width: 1, top: 1, left: 1 }))
                            retval.target[featureMatch[1]] = true;
                        else if (isFinite(featureMatch[2]))
                            retval.target[featureMatch[1]] = featureMatch[2];
                    }
                }
            } else {
                var targetMatch = target.match(selectableTargets);
                if (targetMatch)
                    retval.target.type = retval.target.name = target;
                else {
                    retval.target.type = 'frame';
                    retval.target.name = target;
                }
            }

            var me = this;
            var advAttr = function(inputName, attrName) {
                var value = element.getAttribute(attrName);
                if (value !== null)
                    retval.adv[inputName] = value || '';
            };
            advAttr('advId', 'id');
            advAttr('advLangDir', 'dir');
            advAttr('advAccessKey', 'accessKey');

            retval.adv.advName = element.data('cke-saved-name') || element.getAttribute('name') || '';
            advAttr('advLangCode', 'lang');
            advAttr('advTabIndex', 'tabindex');
            advAttr('advTitle', 'title');
            advAttr('advContentType', 'type');
            CKEDITOR.plugins.link.synAnchorSelector ? retval.adv.advCSSClasses = getLinkClass(element) : advAttr('advCSSClasses', 'class');
            advAttr('advCharset', 'charset');
            advAttr('advStyles', 'style');
            advAttr('advRel', 'rel');
        }

        // Find out whether we have any anchors in the editor.
        var anchors = retval.anchors = [],
            i, count, item;

        // For some browsers we set contenteditable="false" on anchors, making document.anchors not to include them, so we must traverse the links manually (#7893).
        if (CKEDITOR.plugins.link.emptyAnchorFix) {
            var links = editor.document.getElementsByTag('a');
            for (i = 0, count = links.count(); i < count; i++) {
                item = links.getItem(i);
                if (item.data('cke-saved-name') || item.hasAttribute('name'))
                    anchors.push({ name: item.data('cke-saved-name') || item.getAttribute('name'), id: item.getAttribute('id') });
            }
        } else {
            var anchorList = new CKEDITOR.dom.nodeList(editor.document.$.anchors);
            for (i = 0, count = anchorList.count(); i < count; i++) {
                item = anchorList.getItem(i);
                anchors[i] = { name: item.getAttribute('name'), id: item.getAttribute('id') };
            }
        }

        if (CKEDITOR.plugins.link.fakeAnchor) {
            var imgs = editor.document.getElementsByTag('img');
            for (i = 0, count = imgs.count(); i < count; i++) {
                if ((item = CKEDITOR.plugins.link.tryRestoreFakeAnchor(editor, imgs.getItem(i))))
                    anchors.push({ name: item.getAttribute('name'), id: item.getAttribute('id') });
            }
        }

        // Record down the selected element in the dialog.
        this._.selectedElement = element;
        return retval;
    };
    CKEDITOR.on('dialogDefinition', function (ev) {
        InjectQuickUploadFileSizeCheck(ev);

        var dialogName = ev.data.name;
        var dialogDefinition = ev.data.definition;

        if (dialogName == 'link') {
            if (typeof (dnnpagesSelectBox) === 'undefined') {
                return;
            }

            for (var i = 0; i < dnnpagesSelectBox.length; i++) {
                dnnpagesSelectBox[i][0] = unescape(dnnpagesSelectBox[i][0]);
            }

            var editor = ev.editor;
            var linkTypeChanged = function () {
                var dialog = this.getDialog();
                var partIds = ['urlOptions', 'localPageOptions', 'anchorOptions', 'emailOptions'],
                    typeValue = this.getValue(),
                    uploadTab = dialog.definition.getContents('upload'),
                    uploadInitiallyHidden = uploadTab && uploadTab.hidden;
                if (typeValue == 'url') {
                    if (editor.config.linkShowTargetTab) dialog.showPage('target');
                    if (!uploadInitiallyHidden) dialog.showPage('upload');
                } else {
                    dialog.hidePage('target');
                    if (!uploadInitiallyHidden) dialog.hidePage('upload');
                }
                for (var i = 0; i < partIds.length; i++) {
                    var element = dialog.getContentElement('info', partIds[i]);
                    if (!element) continue;
                    element = element.getElement().getParent().getParent();
                    if (partIds[i] == typeValue + 'Options') {
                        element.show();
                    } else {
                        element.hide();
                    }
                }
                dialog.layout();
            };

            dialogDefinition.onShow = function () {

                var plugin = CKEDITOR.plugins.link;

                var editor = this.getParentEditor(),
                    selection = editor.getSelection(),
                    element;

                // Fill in all the relevant fields if there's already one link selected.
                if ((element = plugin.getSelectedLink(editor)) && element.hasAttribute('href'))
                    selection.selectElement(element);
                else
                    element = null;

                this.setupContent(parseLink.apply(this, [editor, element]));
            };
            dialogDefinition.onOk = function () {
                var attributes = {},
                    removeAttributes = [],
                    data = {},
                    editor = this.getParentEditor();

                var emailProtection = editor.config.emailProtection || '';

                // Compile the protection function pattern.
                if (emailProtection && emailProtection != 'encode') {
                    var compiledProtectionFunction = {};

                    emailProtection.replace(/^([^(]+)\(([^)]+)\)$/, function (match, funcName, params) {
                        compiledProtectionFunction.name = funcName;
                        compiledProtectionFunction.params = [];
                        params.replace(/[^,\s]+/g, function (param) {
                            compiledProtectionFunction.params.push(param);
                        });
                    });
                }

                this.commitContent(data);
                // Compose the URL.
                switch (data.type || 'url') {
                    case 'url':
                        var protocol = (data.url && data.url.protocol != undefined) ? data.url.protocol : 'http://',
                            url = (data.url && CKEDITOR.tools.trim(data.url.url)) || '';
                        attributes['data-cke-saved-href'] = (url.indexOf('/') === 0) ? url : protocol + url;
                        break;
                    case 'localPage':
                        attributes['data-cke-saved-href'] = data.localPage;
                        break;
                    case 'anchor':
                        var name = (data.anchor && data.anchor.name),
                            id = (data.anchor && data.anchor.id);
                        attributes['data-cke-saved-href'] = '#' + (name || id || '');
                        break;
                    case 'email':
                        var linkHref,
                            email = data.email,
                            address = email.address;
                        switch (emailProtection) {
                            case '':
                            case 'encode':
                                {
                                    var subject = encodeURIComponent(email.subject || ''),
                                        body = encodeURIComponent(email.body || '');
                                    // Build the e-mail parameters first.
                                    var argList = [];
                                    subject && argList.push('subject=' + subject);
                                    body && argList.push('body=' + body);
                                    argList = argList.length ? '?' + argList.join('&') : '';
                                    if (emailProtection == 'encode') {
                                        linkHref = ["javascript:void(location.href='mailto:'+",
                                            protectEmailAddressAsEncodedString(address)];
                                        // parameters are optional.
                                        argList && linkHref.push('+\'', escapeSingleQuote(argList), '\'');
                                        linkHref.push(')');
                                    } else linkHref = ['mailto:', address, argList];
                                }

                                break;
                            default:
                                {
                                    // Separating name and domain.
                                    var nameAndDomain = address.split('@', 2);
                                    email.name = nameAndDomain[0];
                                    email.domain = nameAndDomain[1];
                                    linkHref = ["javascript:", CKEDITOR.plugins.link.protectEmailLinkAsFunction(email)];
                                }
                        }
                        attributes['data-cke-saved-href'] = linkHref.join('');
                        break;
                }
                // Popups and target.
                if (data.target) {
                    if (data.target.type == 'popup') {
                        var onclickList = ['window.open(this.href, \'',
                            data.target.name || '', '\', \''];
                        var featureList = ['resizable', 'status', 'location', 'toolbar', 'menubar', 'fullscreen', 'scrollbars', 'dependent'];
                        var featureLength = featureList.length;
                        var addFeature = function (featureName) {
                            if (data.target[featureName]) featureList.push(featureName + '=' + data.target[featureName]);
                        };
                        for (var ia = 0; ia < featureLength; ia++) {
                            featureList[ia] = featureList[ia] + (data.target[featureList[i]] ? '=yes' : '=no');
                        }
                        addFeature('width');
                        addFeature('left');
                        addFeature('height');
                        addFeature('top');
                        onclickList.push(featureList.join(','), '\'); return false;');
                        attributes['data-cke-pa-onclick'] = onclickList.join('');
                        // Add the "target" attribute. (#5074)
                        removeAttributes.push('target');
                    } else {
                        if (data.target.type != 'notSet' && data.target.name) attributes.target = data.target.name;
                        else removeAttributes.push('target');
                        removeAttributes.push('data-cke-pa-onclick', 'onclick');
                    }
                }
                // Advanced attributes.
                if (data.adv) {
                    var advAttr = function (inputName, attrName) {
                        var value = data.adv[inputName];
                        if (value) attributes[attrName] = value;
                        else removeAttributes.push(attrName);
                    };
                    advAttr('advId', 'id');
                    advAttr('advLangDir', 'dir');
                    advAttr('advAccessKey', 'accessKey');
                    if (data.adv["advName"]) attributes["name"] = attributes['data-cke-saved-name'] = data.adv["advName"];
                    else removeAttributes = removeAttributes.concat(['data-cke-saved-name', 'name']);
                    advAttr('advLangCode', 'lang');
                    advAttr('advTabIndex', 'tabindex');
                    advAttr('advTitle', 'title');
                    advAttr('advContentType', 'type');
                    advAttr('advCSSClasses', 'class');
                    advAttr('advCharset', 'charset');
                    advAttr('advStyles', 'style');
                    advAttr('advRel', 'rel');
                }
                var selection = editor.getSelection();
                // Browser need the "href" fro copy/paste link to work. (#6641)
                attributes.href = attributes['data-cke-saved-href'];
                if (!this._.selectedElement) {
                    var range = selection.getRanges(1)[0];
                    // Use link URL as text with a collapsed cursor.
                    if (range.collapsed) {
                        // Short mailto link text view (#5736).
                        var text = new CKEDITOR.dom.text(data.type == 'email' ? data.email.address : attributes['data-cke-saved-href'], editor.document);
                        range.insertNode(text);
                        range.selectNodeContents(text);
                    }
                    // Apply style.
                    var style = new CKEDITOR.style({
                        element: 'a',
                        attributes: attributes
                    });
                    style.type = CKEDITOR.STYLE_INLINE; // need to override... dunno why.
                    style.applyToRange(range);
                    range.select();
                } else {
                    // We're only editing an existing link, so just overwrite the attributes.
                    var element = this._.selectedElement,
                        href = element.data('cke-saved-href'),
                        textView = element.getHtml();
                    element.setAttributes(attributes);
                    element.removeAttributes(removeAttributes);
                    if (data.adv && data.adv.advName && CKEDITOR.plugins.link.synAnchorSelector) element.addClass(element.getChildCount() ? 'cke_anchor' : 'cke_anchor_empty');
                    // Update text view when user changes protocol (#4612).
                    if (href == textView || data.type == 'email' && textView.indexOf('@') != -1) {
                        // Short mailto link text view (#5736).
                        element.setHtml(data.type == 'email' ? data.email.address : attributes['data-cke-saved-href']);
                    }
                    selection.selectElement(element);
                    delete this._.selectedElement;
                }
            };
            var infoTab = dialogDefinition.getContents('info');
            var linkType = infoTab.get('linkType');

            // check if filebrowser is enabled
            var urlTitle = editor.config.filebrowserBrowseUrl ? editor.lang.dnnpages.portalUrl : editor.lang.common.url;

            linkType.items = [
                [urlTitle, 'url'],
                [editor.lang.dnnpages.dnnpages, 'localPage'],
                [editor.lang.link.toAnchor, 'anchor'],
                [editor.lang.link.toEmail, 'email']
            ];
            linkType.onChange = linkTypeChanged;
            infoTab.add({
                type: 'vbox',
                id: 'localPageOptions',
                children: [{
                    type: 'select',
                    label: editor.lang.dnnpages.localPageLabel,
                    id: 'localPage',
                    title: editor.lang.dnnpages.localPageTitle,
                    items: dnnpagesSelectBox,
                    setup: function (data) {
                        if (data.localPage) {
                            this.setValue(data.localPage);

                        }
                        this.allowOnChange = false;
                        this.setValue(data.url ? data.url.url : '');
                        this.allowOnChange = true;
                    },
                    commit: function (data) {
                        if (!data.localPage) data.localPage = {};
                        data.localPage = this.getValue();
                    }
                }]
            });
            dialogDefinition.onLoad = function () {
                var internField = this.getContentElement('info', 'localPage');
                internField.reset();
            };
        }
    });

    function getLinkClass(ele) {
        var className = ele.getAttribute('class');
        return className ? className.replace(/\s*(?:cke_anchor_empty|cke_anchor)(?:\s*$)?/g, '') : '';
    }

    function InjectQuickUploadFileSizeCheck(evt) {
        var maxFileSize = evt.editor.config.maxFileSize ? evt.editor.config.maxFileSize : null,
            definition = evt.data.definition,
            element;

        for (var i = 0; i < definition.contents.length; ++i) {
            if ((element = definition.contents[i])) {
                attachFileBrowser(maxFileSize, evt.editor, evt.data, evt.data.name, definition, element.elements);

            }
        }
    }

    function attachFileBrowser(maxFileSize, editor, dialog, dialogName, definition, elements) {
        if (!elements || !elements.length)
            return;

        var element;

        for (var i = elements.length; i--;) {
            element = elements[i];

            if (element.type == 'hbox' || element.type == 'vbox' || element.type == 'fieldset')
                attachFileBrowser(maxFileSize, editor, dialog, dialogName, definition, element.children);

            if (!element.filebrowser)
                continue;

            if (element.filebrowser.action == 'QuickUpload' && element['for']) {
                var url = element.filebrowser.url;
                if (url === undefined) {
                    url = editor.config['filebrowser' + ucFirst(dialogName) + 'UploadUrl'];
                    if (url === undefined) {
                        url = editor.config.filebrowserUploadUrl;
                    }
                }

                if (url) {
                    element.onClick = function() {
                        var editorInstance = editor,
                            input = definition.dialog.getContentElement(this['for'][0], this['for'][1]).getInputElement().$;
                        editorInstance._.filebrowserSe = this;

                        var fileSize = null;

                        if (input.files) {
                            var file = input.files[0];

                            fileSize = file.size;
                        } else {
                            // Check for IE < 9
                            if (CKEDITOR.env.ie) {
                               fileSize = null;
                            }
                        }
                        
                        if (fileSize != null) {
                            if (maxFileSize && fileSize > maxFileSize) {
                                alert(editor.lang.dnnpages.fileToBig);
                                return false;
                            }
                        }
                    };
                }
            }
        }
    }
})();