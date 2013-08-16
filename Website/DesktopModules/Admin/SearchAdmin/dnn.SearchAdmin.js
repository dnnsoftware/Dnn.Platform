(function ($, Sys, dnn) {
    if (typeof dnn == 'undefined' || dnn == null) dnn = {};
    dnn.searchAdmin = dnn.searchAdmin || {};
    dnn.searchAdmin.init = function(settings) {
        var currentSynonymsGroupTagList = [],
            currentStopwordsList = [],
            currentSynonymsPortalId = parseInt($('#' + settings.synonymsSelectedPortalIdCtrl).val(), 10),
            currentSynonymsCulture = $('#' + settings.synonymsSelectedCultureCodeCtrl).val(),
            currentStopwordsPortalId = parseInt($('#' + settings.stopwordsSelectedPortalIdCtrl).val(), 10),
            currentStopwordsCulture = $('#' + settings.stopwordsSelectedCultureCodeCtrl).val();

        var supportRgba = function() {
            var rgba = null;
            var testSupportRgba = function() {
                if (rgba == null) {
                    var scriptElement = document.getElementsByTagName('script')[0];
                    var prevColor = scriptElement.style.color;
                    var testColor = 'rgba(0, 0, 0, 0.5)';
                    if (prevColor == testColor) {
                        rgba = true;
                    } else {
                        try {
                            scriptElement.style.color = testColor;
                        } catch(e) {
                        }
                        rgba = scriptElement.style.color != prevColor;
                        scriptElement.style.color = prevColor;
                    }
                }
                return rgba;
            };
            return testSupportRgba();
        };
        
        var flashOnElement = function (element, flashColor, fallbackColor) {
            if (supportRgba()) { // for moden browser, I use RGBA
                var color = flashColor.join(',') + ',',
                transparency = 1,
                timeout = setInterval(function () {
                    if (transparency >= 0) {
                        element.style.backgroundColor = 'rgba(' + color + (transparency -= 0.015) + ')';
                        // (1 / 0.015) / 25 = 2.66 seconds to complete animation
                    } else {
                        clearInterval(timeout);
                    }
                }, 40); // 1000/40 = 25 fps
            } else { // for IE8, I use hex color fallback
                element.style.backgroundColor = fallbackColor;
                setTimeout(function () {
                    element.style.backgroundColor = 'transparent';
                }, 1000);
            }
        };

        var getDnnService = function () {
            if (typeof $.dnnSF != 'undefined')
                return $.dnnSF(settings.moduleId);
            return null;
        };
        
        var getSearchServiceUrl = function (service) {
            service = service || getDnnService();
            if (service)
                return service.getServiceRoot('internalservices') + 'searchservice/';

            return null;
        };
        
        // Synonyms container setup
        var cancelSynonymsGroupOnClick = function () {
            $('#synonymsGroupTable tr.synonymsGroupEditRow').remove();
            $('#synonymsGroupTable tr').show();
            return false;
        };

        var editSynonymsGroupOnClick = function () {
            // remove all editRow;
            $('#synonymsGroupTable tr.synonymsGroupEditRow').remove();
            $('#synonymsGroupTable tr').show();
            
            // hide current row and show edit row
            var currentRow = $(this).parents('tr');
            var synonymsId = parseInt(currentRow.attr('data-synonymsid'), 10);
            var editTags = currentRow.find('span.synonymsGroupTags').html().replace(/\, /g, ',');
            currentSynonymsGroupTagList = editTags.split(',');
            var editRow = '<tr class="synonymsGroupEditRow">';
            editRow += '<td><div class="editSynonymsContainer">';
            editRow += '<input type="text" class="synonymsGroupTagsInput" />';
            editRow += '<span class="dnnFormMessage dnnFormError"></span></div></td>';
            editRow += '<td><a href="javascript:void(0)" class="btnSaveSynonymsGroup"></a>';
            editRow += '<a href="javascript:void(0)" class="btnCancelSynonymsGroup"></a>';
            editRow += '</td></tr>';
            editRow = $(editRow).insertBefore(currentRow);
            currentRow.hide();

            editRow.find('input.synonymsGroupTagsInput').val(editTags).dnnTagsInput({
                width: '95%',
                onAddTag: function (t) {
                    currentSynonymsGroupTagList.push(t);
                },
                onRemoveTag: function (t) {
                    var index = currentSynonymsGroupTagList.indexOf(t);
                    if (index >= 0)
                        currentSynonymsGroupTagList.splice(index, 1);
                },
                maxTags: 50
            });

            //cancel edit
            editRow.find('a.btnCancelSynonymsGroup').on('click', cancelSynonymsGroupOnClick);

            // save edit
            editRow.find('a.btnSaveSynonymsGroup').on('click', function () {
                if (!currentSynonymsGroupTagList || !currentSynonymsGroupTagList.length) {
                    editRow.find('span.dnnFormError').html(settings.msgSynonymsTagRequired).show();
                    return false;
                }

                var service = getDnnService();
                var serviceUrl = getSearchServiceUrl(service);
                if (!serviceUrl) return false;

                var tags = currentSynonymsGroupTagList.join(',');
                $.ajax({
                    url: serviceUrl + 'UpdateSynonymsGroup',
                    type: 'POST',
                    data: { tags: tags, portalId: currentSynonymsPortalId, id: synonymsId, culture: currentSynonymsCulture },
                    beforeSend: service.setModuleHeaders,
                    success: function (data) {
                        if (data && data.Id > 0) {
                            $('#synonymsGroupTable tr.synonymsGroupEditRow').remove();
                            currentRow.show();
                            currentRow.find('span.synonymsGroupTags').html(tags.replace(/\,/g, ', '));
                            
                            flashOnElement(currentRow.get(0), [255, 255, 102], '#FFFF66');
                        } else {
                            var duplicateWord = data.DuplicateWord;
                            editRow.find('span.dnnFormError').html('[' + duplicateWord + '] ' + settings.msgSynonymsTagDuplicated).show();
                        }
                    },
                    error: function () {
                    }
                });

                return false;
            });

            return false;
        };

        var deleteSynonymsGroupOnClick = function () {
            // remove all editRow;
            $('#synonymsGroupTable tr.synonymsGroupEditRow').remove();
            $('#synonymsGroupTable tr').show();

            var service = getDnnService();
            var serviceUrl = getSearchServiceUrl(service);
            if (!serviceUrl) return false;

            var currentRow = $(this).parents('tr');
            var synonymsId = parseInt(currentRow.attr('data-synonymsid'), 10);

            $.ajax({
                url: serviceUrl + 'DeleteSynonymsGroup',
                type: 'POST',
                data: { tags: '', portalId: currentSynonymsPortalId, id: synonymsId },
                beforeSend: service.setModuleHeaders,
                success: function () {
                    currentRow.remove();
                },
                error: function () {
                }
            });

            return false;
        };

        var addSynonymsGroupOnClick = function () {
            // remove all editRow;
            $('#synonymsGroupTable tr.synonymsGroupEditRow').remove();
            $('#synonymsGroupTable tr').show();

            var thead = $('#synonymsGroupTable tr:first');
            var editRow = '<tr class="synonymsGroupEditRow">';
            editRow += '<td><div class="editSynonymsContainer">';
            editRow += '<input type="text" class="synonymsGroupTagsInput" />';
            editRow += '<span class="dnnFormMessage dnnFormError"></span></div></td>';
            editRow += '<td><a href="javascript:void(0)" class="btnSaveSynonymsGroup"></a>';
            editRow += '<a href="javascript:void(0)" class="btnCancelSynonymsGroup"></a>';
            editRow += '</td></tr>';
            editRow = $(editRow).insertAfter(thead);
            currentSynonymsGroupTagList = [];

            editRow.find('input.synonymsGroupTagsInput').dnnTagsInput({
                width: '95%',
                onAddTag: function (t) {
                    currentSynonymsGroupTagList.push(t);
                    editRow.find('span.dnnFormError').hide();
                },
                onRemoveTag: function (t) {
                    var index = currentSynonymsGroupTagList.indexOf(t);
                    if (index >= 0)
                        currentSynonymsGroupTagList.splice(index, 1);
                },
                maxTags: 50
            });

            //cancel edit
            editRow.find('a.btnCancelSynonymsGroup').on('click', cancelSynonymsGroupOnClick);

            // save edit
            editRow.find('a.btnSaveSynonymsGroup').on('click', function () {
                if (!currentSynonymsGroupTagList || !currentSynonymsGroupTagList.length) {
                    editRow.find('span.dnnFormError').html(settings.msgSynonymsTagRequired).show();
                    return false;
                }

                var service = getDnnService();
                var serviceUrl = getSearchServiceUrl(service);
                if (!serviceUrl) return false;

                var tags = currentSynonymsGroupTagList.join(',');

                $.ajax({
                    url: serviceUrl + 'AddSynonymsGroup',
                    type: 'POST',
                    data: { tags: tags, portalId: currentSynonymsPortalId, id: 0, culture: currentSynonymsCulture },
                    beforeSend: service.setModuleHeaders,
                    success: function (data) {
                        if (data && data.Id > 0) {
                            var newRow = '<tr data-synonymsid="' + data.Id + '">' +
                                '<td><span class="synonymsGroupTags">' + tags.replace(/\,/g, ', ') + '</span></td>' +
                                '<td><a href="javascript:void(0)" class="btnEditSynonymsGroup"></a>' +
                                '<a href="javascript:void(0)" class="btnDeleteSynonymsGroup"></a></td></tr>';

                            newRow = $(newRow).insertAfter(editRow);
                            $('#synonymsGroupTable tr.synonymsGroupEditRow').remove();

                            newRow.find('a.btnEditSynonymsGroup').on('click', editSynonymsGroupOnClick);
                            newRow.find('a.btnDeleteSynonymsGroup').on('click', deleteSynonymsGroupOnClick);

                            flashOnElement(newRow.get(0), [255, 255, 102], '#FFFF66');

                        } else {
                            var duplicateWord = data.DuplicateWord;
                            editRow.find('span.dnnFormError').html('[' + duplicateWord + '] ' + settings.msgSynonymsTagDuplicated).show();
                        }
                    },
                    error: function () {
                    }
                });

                return false;
            });

            return false;
        };

        var synonymsContainerSetup = function () {
            $('#synonymsGroupTable a.btnEditSynonymsGroup').on('click', editSynonymsGroupOnClick);
            $('#synonymsGroupTable a.btnDeleteSynonymsGroup').on('click', deleteSynonymsGroupOnClick);
            $('a#btnAddSynonymsGroup').on('click', addSynonymsGroupOnClick);
        };
        // End Synonyms container setup
        
        // Stopwords container setup
        var cancelEditStopwordsOnClick = function() {
            $('#stopwordsTable tr.stopwordsEditRow').remove();
            $('#stopwordsTable tr').show();
            return false;
        };
        
        var cancelAddStopwordsOnClick = function () {
            $('#stopwordsTable tr.stopwordsEditRow').remove();
            $('#stopwordsTable tr').show();
            return false;
        };

        var editStopwordsOnClick = function() {
            // hide current row and show edit row
            var currentRow = $(this).parents('tr');
            var stopwordsId = parseInt(currentRow.attr('data-stopwordsid'), 10);
            var editTags = currentRow.find('span.stopwordsTags').html().replace(/\, /g, ',');
            currentStopwordsList = editTags.split(',');
            var editRow = '<tr class="stopwordsEditRow">';
            editRow += '<td><div class="editStopwordsContainer">';
            editRow += '<input type="text" class="stopwordsTagsInput" />';
            editRow += '<span class="dnnFormMessage dnnFormError"></span></div>';
            editRow += '</td>';
            editRow += '<td>';
            editRow += '<a href="javascript:void(0)" class="btnSaveStopwords"></a>';
            editRow += '<a href="javascript:void(0)" class="btnCancelStopwords"></a>';
            editRow += '</td></tr>';
          
            editRow = $(editRow).insertBefore(currentRow);
            currentRow.hide();

            editRow.find('input.stopwordsTagsInput').val(editTags).dnnTagsInput({
                width: '95%',
                onAddTag: function (t) {
                    currentStopwordsList.push(t);
                },
                onRemoveTag: function (t) {
                    var index = currentStopwordsList.indexOf(t);
                    if (index >= 0)
                        currentStopwordsList.splice(index, 1);
                },
                maxTags: 300
            });

            //cancel edit
            editRow.find('a.btnCancelStopwords').on('click', cancelEditStopwordsOnClick);

            // save edit
            editRow.find('a.btnSaveStopwords').on('click', function () {
                if (!currentStopwordsList || !currentStopwordsList.length) {
                    editRow.find('span.dnnFormError').html('At least one word required').show();
                    return false;
                }

                var service = getDnnService();
                var serviceUrl = getSearchServiceUrl(service);
                if (!serviceUrl) return false;

                var words = currentStopwordsList.join(',');
                $.ajax({
                    url: serviceUrl + 'UpdateStopwords',
                    type: 'POST',
                    data: { words: words, portalId: currentStopwordsPortalId, id: stopwordsId, culture: currentStopwordsCulture },
                    beforeSend: service.setModuleHeaders,
                    success: function (data) {
                        if (data && data.Id > 0) {
                            $('#stopwordsTable tr.stopwordsEditRow').remove();
                            currentRow.show();
                            currentRow.find('span.stopwordsTags').html(words.replace(/\,/g, ', '));

                            flashOnElement(currentRow.get(0), [255, 255, 102], '#FFFF66');
                        } else {
                            editRow.find('span.dnnFormError').html('some error').show();
                        }
                    },
                    error: function () {
                    }
                });

                return false;
            });

            return false;
        };
        
        var addStopwordsOnClick = function () {
            // check current state
            if ($('#stopwordsTable tr.stopwordsEditRow').length > 0) return false;
            // check edit or add
            var trCount = $('#stopwordsTable tr').length;
            if (trCount > 1) {
                // edit
                $('#stopwordsTable tr a.btnEditStopwords').trigger('click');
                return false;
            }
            // add
            var thead = $('#stopwordsTable tr:first');
            var editRow = '<tr class="stopwordsEditRow">';
            editRow += '<td><div class="editStopwordsContainer">';
            editRow += '<input type="text" class="stopwordsTagsInput" />';
            editRow += '<span class="dnnFormMessage dnnFormError"></span></div></td>';
            editRow += '<td><a href="javascript:void(0)" class="btnSaveStopwords"></a>';
            editRow += '<a href="javascript:void(0)" class="btnCancelStopwords"></a>';
            editRow += '</td></tr>';
            editRow = $(editRow).insertAfter(thead);
            currentStopwordsList = [];

            editRow.find('input.stopwordsTagsInput').dnnTagsInput({
                width: '95%',
                onAddTag: function (t) {
                    currentStopwordsList.push(t);
                },
                onRemoveTag: function (t) {
                    var index = currentStopwordsList.indexOf(t);
                    if (index >= 0)
                        currentStopwordsList.splice(index, 1);
                },
                maxTags: 300
            });

            //cancel edit
            editRow.find('a.btnCancelStopwords').on('click', cancelAddStopwordsOnClick);

            // save edit
            editRow.find('a.btnSaveStopwords').on('click', function () {
                if (!currentStopwordsList || !currentStopwordsList.length) {
                    editRow.find('span.dnnFormError').html('At least one word required').show();
                    return false;
                }

                var service = getDnnService();
                var serviceUrl = getSearchServiceUrl(service);
                if (!serviceUrl) return false;

                var words = currentStopwordsList.join(',');
                $.ajax({
                    url: serviceUrl + 'AddStopwords',
                    type: 'POST',
                    data: { words: words, portalId: currentStopwordsPortalId, id: 0, culture: currentStopwordsCulture },
                    beforeSend: service.setModuleHeaders,
                    success: function (data) {
                        if (data && data.Id > 0) {
                            var newRow = '<tr data-stopwordsid="' + data.Id + '">' +
                                '<td><span class="stopwordsTags">' + words.replace(/\,/g, ', ') + '</span></td>' +
                                '<td><a href="javascript:void(0)" class="btnEditStopwords"></a>' +
                                '<a href="javascript:void(0)" class="btnDeleteStopwords"></a></td>' +
                                '</tr>';

                            newRow = $(newRow).insertAfter(editRow);
                            $('#stopwordsTable tr.stopwordsEditRow').remove();

                            newRow.find('a.btnEditStopwords').on('click', editStopwordsOnClick);
                            newRow.find('a.btnDeleteStopwords').on('click', deleteStopwordsOnClick);

                            flashOnElement(newRow.get(0), [255, 255, 102], '#FFFF66');
                        } else {
                            editRow.find('span.dnnFormError').html('some error').show();
                        }
                    },
                    error: function () {
                    }
                });

                return false;
            });

            return false;
        };

        var deleteStopwordsOnClick = function() {
            var service = getDnnService();
            var serviceUrl = getSearchServiceUrl(service);
            if (!serviceUrl) return false;

            var currentRow = $(this).parents('tr');
            var stopwordsId = parseInt(currentRow.attr('data-stopwordsid'), 10);

            $.ajax({
                url: serviceUrl + 'DeleteStopwords',
                type: 'POST',
                data: { words: '', portalId: currentStopwordsPortalId, id: stopwordsId, culture: currentStopwordsCulture },
                beforeSend: service.setModuleHeaders,
                success: function () {
                    currentRow.remove();
                },
                error: function () {
                }
            });
            return false;
        };
        
        var stopwordsContainerSetup = function() {
            $('#stopwordsTable a.btnEditStopwords').on('click', editStopwordsOnClick);
            $('#stopwordsTable a.btnDeleteStopwords').on('click', deleteStopwordsOnClick);
            $('a#btnAddStopwords').on('click', addStopwordsOnClick);
        };
        // End Stopwords container setup
        
        var searchAdminSetup = function () {
            var options = {
                validationTriggerSelector: '.dnnPrimaryAction2'
            };
            $('#dnnSearchAdmin').dnnTabs(options).dnnPanels(options);

            // reset some vaule cause MS AJAX post back
            currentSynonymsGroupTagList = [],
            currentSynonymsPortalId = parseInt($('#' + settings.synonymsSelectedPortalIdCtrl).val(), 10),
            currentStopwordsPortalId = parseInt($('#' + settings.stopwordsSelectedPortalIdCtrl).val(), 10),
            currentStopwordsCulture = $('#' + settings.stopwordsSelectedCultureCodeCtrl).val();
            
            synonymsContainerSetup();
            stopwordsContainerSetup();
            
            // binding confirmatin on reindex
            $('#' + settings.btnReIndex).dnnConfirm({
                text: settings.msgReIndexConfirmation,
                yesText: settings.reIndexConfirmationYes,
                noText: settings.reIndexConfirmationCancel,
                title: settings.titleReIndexConfirmation
            });
        };
        
        $(document).ready(searchAdminSetup);
        Sys.WebForms.PageRequestManager.getInstance().add_endRequest(searchAdminSetup);
    };
})(jQuery, window.Sys, window.dnn);
