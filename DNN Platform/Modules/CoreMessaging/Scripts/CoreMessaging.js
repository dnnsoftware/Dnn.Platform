function CoreMessaging($, ko, settings, composeMessageOptions) {
    var profilePicHandler = settings.profilePicHandler;
    var serviceFramework = composeMessageOptions.servicesFramework;
    var baseServicepath = serviceFramework.getServiceRoot('CoreMessaging') + 'MessagingService/';
    var inboxpath = baseServicepath + "Inbox";
    var sentboxpath = baseServicepath + "Sentbox";
    var archivedboxpath = baseServicepath + "Archived";
    var notificationspath = baseServicepath + "Notifications";
    var countnotificationspath = baseServicepath + "CountNotifications";
    var gettotalspath = baseServicepath + "GetTotals";
    var pageSize = 10;
    var repliesPageSize = 2;
    var notificationsPageSize = 10;
    var containerElement = null;
	var refreshInterval = 60000; //refresh inbox status every 1 minute.

    function displayMessage(placeholderSelector, message, cssclass) {
        var messageNode = $("<div/>")
                .addClass('dnnFormMessage ' + cssclass)
                .text(message);

        $(containerElement + " " + placeholderSelector).prepend(messageNode);

        messageNode.fadeOut(3000, 'easeInExpo', function () {
            messageNode.remove();
        });
    };

    function getQuerystring(key, default_) {
        if (default_ == null) default_ = "";
        key = key.replace(/[\[]/, "\\\[").replace(/[\]]/, "\\\]");
        var regex = new RegExp("[\\?&]" + key + "=([^&#]*)");
        var qs = regex.exec(window.location.href);
        if (qs == null)
            return default_;
        else
            return qs[1];
    }

    (function (window) {
        // Check Location
        if (document.location.protocol === 'file:') {
            alert('The HTML5 History API (and thus History.js) do not work on files, please upload it to a server.');
        }

        // Establish Variables
        var History = window.History; // Note: We are using a capital H instead of a lower h

        // Bind to State Change
        History.Adapter.bind(window, 'statechange', function () { // Note: We are using statechange instead of popstate
            var state = History.getState(); // Note: We are using History.getState() instead of event.state

            if (state.data != null) {
                var data = state.data;

                var action = data.action;
                var view = data.view;
                var context = ko.contextFor(document.getElementById($(containerElement).attr("id")));

                if (view == "notifications") {
                    context.$root.loadNotificationsTabHandler();
                    $(containerElement + ' #smMainContent').dnnTabs({ selected: 1 });
                } else {
                    $(containerElement + ' #smMainContent').dnnTabs({ selected: 0 });
                    switch (action) {
                        case 'inbox':
                            context.$root.messages([]);
                            context.$root.showReplies(false);
                            context.$root.myinboxHandler();
                            break;
                        case 'sentbox':
                            context.$root.messages([]);
                            context.$root.showReplies(false);
                            context.$root.mysentboxHandler();
                            break;
                        case 'archivebox':
                            context.$root.messages([]);
                            context.$root.showReplies(false);
                            context.$root.myarchiveboxHandler();
                            break;
                        case 'thread':
                            context.$root.highlightThreads = false;
                            context.$root.conversationRead(true);
                            context.$root.messagethreads([]);
                            context.$root.sendThreadRequestHandler(data.conversationId, -1);
                            context.$root.showReplies(true);
                            break;
                        default:
                            //empty
                    }
                }
            }
        });
    })(window);

    function messageConversationView(data) {
        var self = this;

        self.MessageID = data.MessageID;
        self.To = data.To;
        self.From = data.From;
        self.Subject = data.Subject;
        self.Body = data.Body;
        self.ConversationId = data.ConversationId;
        self.ReplyAllAllowed = data.ReplyAllAllowed;
        self.SenderUserID = data.SenderUserID;
        self.RowNumber = data.RowNumber;
        self.AttachmentCount = ko.observable(data.AttachmentCount);
        self.NewThreadCount = ko.observable(data.NewThreadCount);
        self.ThreadCount = ko.observable(data.ThreadCount);
        self.MessageIDName = "messageSelect" + self.ConversationId;
        self.SenderAvatar = profilePicHandler.replace("{0}", self.SenderUserID).replace("{1}", 64).replace("{2}", 64);
        self.SenderProfileUrl = data.SenderProfileUrl;
        self.MessageAbstract = (self.Body.length < 50) ? self.Body : self.Body.substr(0, 50) + "...";
        self.messageSelected = ko.observable(false);
        self.CreatedOnDate = data.DisplayDate;

        self.Read = ko.computed(function () {
            return self.NewThreadCount() === 0;
        });

        self.HasAttachments = ko.computed(function () {
            return self.AttachmentCount() > 0;
        });
    }

    function messageThreadView(data) {
        var self = this;

        self.MessageID = data.Conversation.MessageID;
        self.To = data.Conversation.To;
        self.From = data.Conversation.From;
        self.Subject = data.Conversation.Subject;
        self.Body = data.Conversation.Body;
        self.ConversationId = data.Conversation.ConversationId;
        self.ReplyAllAllowed = data.Conversation.ReplyAllAllowed;
        self.SenderUserID = data.Conversation.SenderUserID;
        self.RowNumber = data.Conversation.RowNumber;
        self.AttachmentCount = data.Conversation.AttachmentCount;
        self.NewThreadCount = ko.observable(data.Conversation.NewThreadCount);
        self.Attachments = data.Attachments;
        self.MessageIDName = "messageSelect" + self.ConversationId;
        self.SenderAvatar = profilePicHandler.replace("{0}", self.SenderUserID).replace("{1}", 64).replace("{2}", 64);
        self.SenderProfileUrl = data.Conversation.SenderProfileUrl;
        self.CreatedOnDate = data.Conversation.DisplayDate;

        self.Read = ko.computed(function () {
            return self.NewThreadCount() === 0;
        });
    }

    function notificationActionViewModel(data, notificationId) {
        var self = this;

        self.NotificationId = notificationId;
        self.Name = data.Name;
        self.Description = data.Description;
        self.Confirm = data.Confirm;
        self.APICall = data.APICall;
    }

    function notificationViewModel(data) {
        var self = this;

        self.NotificationId = data.NotificationId;
        self.Subject = data.Subject;
        self.From = data.From;
        self.Body = data.Body;
        self.SenderAvatar = data.SenderAvatar;
        self.SenderProfileUrl = data.SenderProfileUrl;
        self.DisplayDate = data.DisplayDate;
        self.Actions = $.map(data.Actions, function (action) { return new notificationActionViewModel(action, data.NotificationId); });
    }

    function coreMessagingViewModel() {
        var self = this;

        self.messages = ko.observableArray([]);
        self.notifications = ko.observableArray([]);

        // Number displayed in Notifications tab
        self.TotalNotifications = ko.observable(0);

        // Used in box pagination
        self.TotalConversations = ko.observable(0);

        // Number displayed in Messages tab
        self.TotalNewThreads = ko.observable(0);

        // Used in Thread View to know if there are more threads to display
        self.TotalThreads = ko.observable(0);

        // Used in Thread View to display archive/unarchive links
        self.TotalArchivedThreads = ko.observable(0);

        self.messagethreads = ko.observableArray([]);

        //use an observable value to show/hide the boxes
        self.showInbox = ko.observable(true);
        self.showSentbox = ko.observable(false);
        self.showArchivebox = ko.observable(false);

        //use an observable value to show/hide the replies list
        self.showReplies = ko.observable(false);
        
        //use an observable value to decide if a reply is possible
        self.replyHasRecipients = ko.observable(true);

        self.threadSubject = ko.observable('');
        self.threadTo = ko.observable('');
        self.loadingData = ko.observable(false);
        self.isReplySelected = ko.observable(false);
        self.highlightThreads = false;
        self.conversationRead = ko.observable();

        self.selectMenuOn = ko.observable(false);
        self.actionsMenuOn = ko.observable(false);

        self.hasElementsSelected = ko.computed(function () {
            var selectedElement = ko.utils.arrayFirst(self.messages(), function (message) {
                return message.messageSelected() === true;
            });

            return selectedElement != undefined;
        });

        self.toogleSelectMenu = function () {
            self.selectMenuOn(!self.selectMenuOn());
        };

        self.toogleActionsMenu = function () {
            if (!self.hasElementsSelected()) {
                self.actionsMenuOn(false);
            } else {
                self.actionsMenuOn(!self.actionsMenuOn());
            }
        };

        $('body').bind('click.coremessaging', function (event) {
            if (!$(event.target).closest('#SelectMenu').length) {
                self.selectMenuOn(false);
            };

            if (!$(event.target).closest('#ActionsMenu').length) {
                self.actionsMenuOn(false);
            };
        });

        self.getPageNumbers = ko.computed(function () {
            if (self.TotalConversations() === 0) return '0-0';
            return '1-' + self.messages().length;
        });

        self.getNotificationPageNumbers = ko.computed(function () {
            if (self.TotalNotifications() === 0) return '0-0';
            return '1-' + self.notifications().length;
        });

        self.backToMessages = function () {
            self.messages([]);
            self.showReplies(false);
            self.reloadBoxes();
        };

        self.loadMessagesTab = function () {
            self.messages([]);
            self.showReplies(false);
            self.myinbox();
        };

        self.reloadBoxes = function () {
            self.loadingData(true);
            if (self.showInbox()) {
                self.myinbox();
            } else if (self.showSentbox()) {
                self.mysentbox();
            } else if (self.showArchivebox()) {
                self.myarchivebox();
            }
        };

        self.fetch_unix_timestamp = function () { return (new Date().getTime().toString().substring(0, 10)); };

        self.sendThreadRequest = function (message) {
            History.pushState({ view: 'messages', action: 'thread', conversationId: message.ConversationId }, "", "?view=messages&action=thread&t=" + self.fetch_unix_timestamp());
        };

        self.checkReplyHasRecipients = function (conversationId) {
            var returnValue = 0; // If the call fails, just return 0.

            $.ajax({
                type: "GET",
                beforeSend: serviceFramework.setModuleHeaders,
                url: baseServicepath + "CheckReplyHasRecipients",
                async: false,
                cache: false,
                data: { conversationId: conversationId }
            }).done(function (data) {
                if ($.type(data) === "number") {
                    returnValue = data;
                }
            });

            if (returnValue == 0) {
                self.replyHasRecipients(false);
                displayMessage("#dnnCoreMessaging", settings.replyHasNoRecipientsText, "dnnFormWarning");
            } else {
                self.replyHasRecipients(true);
            }
        };
        
        self.sendThreadRequestHandler = function (conversationId, afterMessageId) {
            self.checkReplyHasRecipients(conversationId);
            $.ajax({
                type: "GET",
                url: baseServicepath + "Thread",
                beforeSend: serviceFramework.setModuleHeaders,
                data: { conversationId: conversationId, afterMessageId: afterMessageId, numberOfRecords: repliesPageSize },
                cache: false
            }).done(function (messagethreadsView) {
                if (typeof messagethreadsView !== "undefined" &&
                    messagethreadsView != null &&
                    typeof messagethreadsView.Conversations !== "undefined") {
                    var mappedMessages = $.map(messagethreadsView.Conversations, function (item) {
                        return new messageThreadView(item);
                    });

                    if (mappedMessages.length > 0) {
                        for (var i = 0; i < mappedMessages.length; i++) {
                            self.messagethreads.unshift(mappedMessages[i]);
                        }
                    }
                    self.threadSubject(self.messagethreads()[0].Subject);
                    self.threadTo(self.messagethreads()[0].To);
                    self.highlightThreads = true;

                    //update totals
                    if (typeof messagethreadsView.TotalThreads !== "undefined" && $.type(messagethreadsView.TotalThreads) === "number") {
                        self.TotalThreads(messagethreadsView.TotalThreads);
                    }

                    if (typeof messagethreadsView.TotalNewThreads !== "undefined" && $.type(messagethreadsView.TotalNewThreads) === "number") {
                        self.TotalNewThreads(messagethreadsView.TotalNewThreads);
                    }

                    if (typeof messagethreadsView.TotalArchivedThreads !== "undefined" && $.type(messagethreadsView.TotalArchivedThreads) === "number") {
                        self.TotalArchivedThreads(messagethreadsView.TotalArchivedThreads);
                    }
                } else {
                    displayMessage("#dnnCoreMessaging", settings.serverErrorText, "dnnFormWarning");
                }
            }).fail(function (xhr, status) {
                displayMessage("#dnnCoreMessaging", settings.serverErrorWithDescriptionText + status, "dnnFormWarning");
            });
        };

        self.getReplies = function (message) {
            self.highlightThreads = false;
            if (message.Read() !== true) {
                self.markAsRead(message);
            }
            self.conversationRead(true);
            self.messagethreads([]);
            self.sendThreadRequest(message);
            self.showReplies(true);
        };

        self.getRepliesAndReply = function (message) {
            self.getReplies(message);
            self.setReplySelected();
        };

        self.showPreviousReplies = function () {
            if (self.TotalThreads() > self.messagethreads().length) {
                self.sendThreadRequestHandler(self.messagethreads()[0].ConversationId, self.messagethreads()[0].MessageID);
            }
        };

        self.showPreviousRepliesVisible = ko.computed(function () {
            return self.messagethreads().length < self.TotalThreads();
        });

        self.updateThreadCount = function (messageconversationView) {
            var previousNewThreads = self.TotalNewThreads();

            if (messageconversationView.Read() === true) {
                messageconversationView.NewThreadCount(messageconversationView.ThreadCount());
                self.TotalNewThreads(previousNewThreads + messageconversationView.NewThreadCount());
            } else {
                self.TotalNewThreads(previousNewThreads - messageconversationView.NewThreadCount());
                messageconversationView.NewThreadCount(0);
            }
        };

        self.changeState = function (messageconversationView, url) {
            $.ajax({
                type: "POST",
                url: url,
                beforeSend: serviceFramework.setModuleHeaders,
                data: { conversationId: messageconversationView.ConversationId }
            }).done(function (data) {
                if (data.Result === "success") {
                    if (messageconversationView.ThreadCount != undefined) {
                        self.updateThreadCount(messageconversationView);
                    }
                } else {
                    displayMessage("#dnnCoreMessaging", settings.serverErrorText, "dnnFormWarning");
                }
            }).fail(function (xhr, status) {
                displayMessage("#dnnCoreMessaging", settings.serverErrorWithDescriptionText + status, "dnnFormWarning");
            });
        };

        self.markAsRead = function (messageconversationView) {
            self.changeState(messageconversationView, baseServicepath + 'MarkRead');
        };

        self.markAsUnread = function (messageconversationView) {
            self.changeState(messageconversationView, baseServicepath + 'MarkUnRead');
        };

        self.toggleState = function (messageconversationView) {
            if (messageconversationView.Read() === true) {
                self.markAsUnread(messageconversationView);
            } else {
                self.markAsRead(messageconversationView);
            }
        };

        self.toggleConversationState = function () {
            var messageconversationView = self.messagethreads()[0];
            self.toggleState(messageconversationView);

            self.conversationRead(!self.conversationRead());

            if (self.conversationRead() === true) {
                $('.successMsg').text(settings.conversationSetAsReadText);
            } else {
                $('.successMsg').text(settings.conversationSetAsUnreadText);
            }
            $('.successMsg').show().fadeOut(3000, 'easeInExpo');
        };

        self.loadMore = function (data, event) {
            $(event.target).html('<img src="images/dnnanim.gif" style="vertical-align:middle" /> ' + settings.loadingText);

            var afterMessageId = self.messages().length == 0 ? -1 : self.messages()[self.messages().length - 1].MessageID;
            var url = inboxpath;
            if (self.showSentbox()) url = sentboxpath;
            else if (self.showArchivebox()) url = archivedboxpath;

            $.ajax({
                type: "GET",
                url: url,
                beforeSend: serviceFramework.setModuleHeaders,
                data: { afterMessageId: afterMessageId, numberOfRecords: pageSize },
                cache: false
            }).done(function (messageboxView) {
                if (typeof messageboxView !== "undefined" &&
                    messageboxView != null &&
                    typeof messageboxView.Conversations !== "undefined") {
                    var mappedMessages = $.map(messageboxView.Conversations, function (item) { return new messageConversationView(item); });
                    for (var i = 0; i < mappedMessages.length; i++) {
                        self.messages.push(mappedMessages[i]);
                    }

                    if (typeof messageboxView.TotalConversations !== "undefined" && $.type(messageboxView.TotalConversations) === "number") {
                        self.TotalConversations(messageboxView.TotalConversations);
                    }
                    if (typeof messageboxView.TotalNewThreads !== "undefined" && $.type(messageboxView.TotalNewThreads) === "number") {
                        self.TotalNewThreads(messageboxView.TotalNewThreads);
                    }
                } else {
                    displayMessage("#dnnCoreMessaging", settings.serverErrorText, "dnnFormWarning");
                }
            }).fail(function (xhr, status) {
                displayMessage("#dnnCoreMessaging", settings.serverErrorWithDescriptionText + status, "dnnFormWarning");
            }).always(function () {
                $(event.target).html(settings.loadMoreText);
            });
        };

        self.moveToArchive = function (message) {
            $.ajax({
                type: "POST",
                url: baseServicepath + 'MarkArchived',
                beforeSend: serviceFramework.setModuleHeaders,
                data: { conversationId: message.ConversationId }
            }).done(function (data) {
                if (data.Result === "success") {
                    if (message.ThreadCount != undefined) {
                        self.TotalNewThreads(self.TotalNewThreads() - message.NewThreadCount());
                    }

                    self.messages.remove(message);
                    self.TotalConversations(self.TotalConversations() - 1);
                } else {
                    displayMessage("#dnnCoreMessaging", settings.serverErrorText, "dnnFormWarning");
                }
            }).fail(function (xhr, status) {
                displayMessage("#dnnCoreMessaging", settings.serverErrorWithDescriptionText + status, "dnnFormWarning");
            });
        };

        self.moveSelectedToArchive = function () {
            $.each(self.messages(), function () {
                if (this.messageSelected()) self.moveToArchive(this);
            });
        };

        self.moveSelectedToRead = function () {
            $.each(self.messages(), function () {
                if (this.messageSelected()) self.markAsRead(this);
            });
        };

        self.moveSelectedToUnread = function () {
            $.each(self.messages(), function () {
                if (this.messageSelected()) self.markAsUnread(this);
            });
        };

        self.toggleArchiveConversation = function () {
            var conversationId = self.messagethreads()[0].ConversationId;
            var url;

            if (self.TotalArchivedThreads() === 0) {
                url = baseServicepath + 'MarkArchived';
                $('.successMsg').text(settings.conversationArchivedText);
            } else {
                url = baseServicepath + 'MarkUnArchived';
                $('.successMsg').text(settings.conversationUnarchivedText);
            }

            $.ajax({
                type: "POST",
                url: url,
                beforeSend: serviceFramework.setModuleHeaders,
                data: { conversationId: conversationId }
            }).done(function (data) {
                if (data.Result === "success") {
                    $('.successMsg').show().fadeOut(3000, 'easeInExpo');

                    if (self.TotalArchivedThreads() === 0) {
                        self.TotalArchivedThreads(self.TotalThreads);
                    } else {
                        self.TotalArchivedThreads(0);
                    }
                } else {
                    displayMessage("#dnnCoreMessaging", settings.serverErrorText, "dnnFormWarning");
                }
            }).fail(function (xhr, status) {
                displayMessage("#dnnCoreMessaging", settings.serverErrorWithDescriptionText + status, "dnnFormWarning");
            });
        };

        self.changeBoxes = function (inbox, sentbox, archivebox) {
            self.showInbox(inbox);
            self.showSentbox(sentbox);
            self.showArchivebox(archivebox);
        };

        self.getTotalNotifications = function () {
            var returnValue = 0; // If the call fails, just return 0.

            $.ajax({
                type: "GET",
                beforeSend: serviceFramework.setModuleHeaders,
                url: countnotificationspath,
                async: false,
                cache: false
            }).done(function (data) {
                if ($.type(data) === "number") {
                    returnValue = data;
                }
            });

            return returnValue;
        };

        self.loadNotificationsTab = function () {
            History.pushState({ view: 'notifications', action: 'notifications' }, "", "?view=notifications&action=notifications&t=" + self.fetch_unix_timestamp());
        };

        self.loadNotificationsTabHandler = function () {
            $.ajax({
                type: "GET",
                url: notificationspath,
                beforeSend: serviceFramework.setModuleHeaders,
                data: { afterNotificationId: -1, numberOfRecords: notificationsPageSize },
                cache: false
            }).done(function (notificationsViewModel) {
                if (typeof notificationsViewModel !== "undefined" &&
                    notificationsViewModel != null &&
                    typeof notificationsViewModel.Notifications !== "undefined") {
                    var mappedNotifications = $.map(notificationsViewModel.Notifications, function (notification) { return new notificationViewModel(notification); });
                    self.notifications(mappedNotifications);

                    if (typeof notificationsViewModel.TotalNotifications !== "undefined" && $.type(notificationsViewModel.TotalNotifications) === "number") {
                        self.TotalNotifications(notificationsViewModel.TotalNotifications);
                    }
                } else {
                    displayMessage("#dnnCoreNotification", settings.serverErrorText, "dnnFormWarning");
                }
            }).fail(function (xhr, status) {
                displayMessage("#dnnCoreNotification", settings.serverErrorWithDescriptionText + status, "dnnFormWarning");
            }).always(function () {
                self.loadingData(false);
            });
        };

        self.loadMoreNotifications = function () {
            var afterNotificationId = self.notifications().length == 0 ? -1 : self.notifications()[self.notifications().length - 1].NotificationId;

            $.ajax({
                type: "GET",
                url: notificationspath,
                beforeSend: serviceFramework.setModuleHeaders,
                data: { afterNotificationId: afterNotificationId, numberOfRecords: notificationsPageSize },
                cache: false
            }).done(function (notificationsViewModel) {
                if (typeof notificationsViewModel !== "undefined" &&
                    notificationsViewModel != null &&
                    typeof notificationsViewModel.Notifications !== "undefined") {
                    var mappedNotifications = $.map(notificationsViewModel.Notifications, function (notification) { return new notificationViewModel(notification); });
                    for (var i = 0; i < mappedNotifications.length; i++) {
                        self.notifications.push(mappedNotifications[i]);
                    }

                    if (typeof notificationsViewModel.TotalNotifications !== "undefined" && $.type(notificationsViewModel.TotalNotifications) === "number") {
                        self.TotalNotifications(notificationsViewModel.TotalNotifications);
                    }
                } else {
                    displayMessage("#dnnCoreNotification", settings.serverErrorText, "dnnFormWarning");
                }
            }).fail(function () {
                displayMessage("#dnnCoreNotification", settings.serverErrorWithDescriptionText, "dnnFormWarning");
            });
        };

        self.loadMoreVisible = ko.computed(function () {
            return self.messages().length < self.TotalConversations();
        });

        self.loadMoreNotificationsVisible = ko.computed(function () {
            return self.notifications().length < self.TotalNotifications();
        });

        self.isLastNotificationAction = function (notification, action) {
            return action === notification.Actions[notification.Actions.length - 1];
        };

        self.performNotificationAction = function (action) {
            if (action.Confirm.length > 0) {
                var confirmDialog = $("<div class='dnnDialog'></div>").html(action.Confirm).dialog({
                    autoOpen: false,
                    resizable: false,
                    modal: true,
                    title: settings.notificationConfirmTitleText,
                    dialogClass: 'dnnFormPopup dnnClear',
                    open: function () {
                        $('.ui-dialog-buttonpane').find('button:contains("' + settings.notificationConfirmNoText + '")').addClass('dnnConfirmCancel');
                    },
                    buttons: [
                        {
                            text: settings.notificationConfirmYesText,
                            click: function () {
                                $(this).dialog("close");
                                self.apiCallRequest(action);
                            }
                        },
                        {
                            text: settings.notificationConfirmNoText,
                            click: function () { $(this).dialog("close"); }
                        }
                    ]
                });

                if (confirmDialog.is(':visible')) {
                    confirmDialog.dialog("close");
                    return true;
                }

                confirmDialog.dialog('open');
            } else {
                self.apiCallRequest(action);
            }
        };

        self.apiCallRequest = function (action) {
            $.ajax({
                type: "POST",
                url: dnn.getVar("sf_siteRoot", "/") + action.APICall,
                beforeSend: serviceFramework.setModuleHeaders,
                data: { NotificationId: action.NotificationId }
            }).done(function (data) {
                if (data.Result === "success") {

                    var notificationToRemove = ko.utils.arrayFirst(self.notifications(), function (notification) {
                        return notification.NotificationId === action.NotificationId;
                    });

                    self.notifications.remove(notificationToRemove);

                    //remove notification from the userControl bar

                    var notifications = $("#dnn_dnnUser_notificationLink").children("span");

                    if (notifications) {
                        var totalNotificationsTxt = notifications.text();

                        if (totalNotificationsTxt != "") {
                            var totalNotifications = parseInt(totalNotificationsTxt);
                            totalNotifications--;

                            if (totalNotifications > 0) {
                                notifications.text(totalNotifications);
                            }
                            else {
                                notifications.text("");
                            }
                        }
                    }


                    self.TotalNotifications(self.TotalNotifications() - 1);
                    displayMessage("#dnnCoreNotification", settings.actionPerformedText, "dnnFormSuccess");
                    
                    if (data.Link) {
                        location.href = data.Link;
                    }
                }
                else {
                    if (typeof data.Message !== "undefined" && data.Message != null && data.Message !== '') {
                        displayMessage("#dnnCoreNotification", data.Message, "dnnFormWarning");
                    } else {
                        displayMessage("#dnnCoreNotification", settings.actionNotPerformedText, "dnnFormWarning");
                    }
                }
            }).fail(function () {
                displayMessage("#dnnCoreNotification", settings.actionNotPerformedText, "dnnFormWarning");
            });
        };

        self.hideNotification = function (elem) {
            if (elem.nodeType === 1) {
                $(elem).fadeOut('slow', function () { $(elem).remove(); });
            }
        };

        self.loadBox = function (boxPath) {
            self.loadingData(true);

            $.ajax({
                type: "GET",
                url: boxPath,
                beforeSend: serviceFramework.setModuleHeaders,
                data: { afterMessageId: -1, numberOfRecords: pageSize },
                cache: false
            }).done(function (messageboxView) {
                if (typeof messageboxView !== "undefined" &&
                    messageboxView != null &&
                    typeof messageboxView.Conversations !== "undefined") {
                    var mappedMessages = $.map(messageboxView.Conversations, function (item) { return new messageConversationView(item); });
                    self.messages(mappedMessages);

                    if (typeof messageboxView.TotalConversations !== "undefined" && $.type(messageboxView.TotalConversations) === "number") {
                        self.TotalConversations(messageboxView.TotalConversations);
                    }
                    if (typeof messageboxView.TotalNewThreads !== "undefined" && $.type(messageboxView.TotalNewThreads) === "number") {
                        self.TotalNewThreads(messageboxView.TotalNewThreads);
                    }
                } else {
                    displayMessage("#dnnCoreMessaging", settings.serverErrorText, "dnnFormWarning");
                }
            }).fail(function (xhr, status) {
                displayMessage("#dnnCoreMessaging", settings.serverErrorWithDescription + status, "dnnFormWarning");
            }).always(function () {
                self.loadingData(false);
            });
        };

        self.myinbox = function () {
            History.pushState({ view: 'messages', action: 'inbox' }, "", "?view=messages&action=inbox&t=" + self.fetch_unix_timestamp());
        };

        self.myinboxHandler = function () {
            self.changeBoxes(true, false, false);
            self.loadingData(true);
            self.loadBox(inboxpath);
        };

        self.mysentbox = function () {
            History.pushState({ view: 'messages', action: 'sentbox' }, "", "?view=messages&action=sentbox&t=" + self.fetch_unix_timestamp());
        };

        self.mysentboxHandler = function () {
            self.changeBoxes(false, true, false);
            self.loadingData(true);
            self.loadBox(sentboxpath);
        };

        self.myarchivebox = function () {
            History.pushState({ view: 'messages', action: 'archivebox' }, "", "?view=messages&action=archivebox&t=" + self.fetch_unix_timestamp());
        };

        self.myarchiveboxHandler = function () {
            self.changeBoxes(false, false, true);
            self.loadingData(true);
            self.loadBox(archivedboxpath);
        };

        self.reply = function () {
            var body = $(containerElement + " #replyMessage").val();
            if (body.length == 0) return;
            var conversationId = self.messagethreads()[0].ConversationId;
            displayMessage("#dnnCoreMessaging", "test", "dnnFormWarning");
            $.ajax({
                type: "POST",
                url: baseServicepath + "Reply",
                beforeSend: serviceFramework.setModuleHeaders,
                data: { conversationId: conversationId, body: encodeURIComponent(body), fileIds: [] }
            }).done(function (data) {
                if (typeof data !== "undefined" &&
                    data != null &&
                    typeof data.Conversation !== "undefined") {
                    $(containerElement + " #replyMessage").val('');
                    self.messagethreads.push(new messageThreadView(data));

                    if (typeof data.TotalThreads !== "undefined" && $.type(data.TotalThreads) === "number") {
                        self.TotalThreads(data.TotalThreads);
                    }
                    if (typeof data.TotalNewThreads !== "undefined" && $.type(data.TotalNewThreads) === "number") {
                        self.TotalNewThreads(data.TotalNewThreads);
                    }
                    if (typeof data.TotalArchivedThreads !== "undefined" && $.type(data.TotalArchivedThreads) === "number") {
                        self.TotalArchivedThreads(data.TotalArchivedThreads);
                    }
                } else {
                    displayMessage("#dnnCoreMessaging", settings.serverErrorText, "dnnFormWarning");
                }
            }).fail(function (xhr, status) {
                displayMessage("#dnnCoreMessaging", settings.serverErrorWithDescription + eval("(" + xhr.responseText + ")").ExceptionMessage, "dnnFormWarning");
            });
        };

        self.setReplySelected = function () {
            self.isReplySelected(true);
        };

        self.threadViewAfterAdd = function (node) {
            if (node.childNodes.length > 0 && self.highlightThreads === true) {
                $(node).effect("highlight", {}, 3000);
            }
        };

        self.selectAll = function () {
            $.each(self.messages(), function () {
                this.messageSelected(true);
            });
        };

        self.selectNone = function () {
            $.each(self.messages(), function () {
                this.messageSelected(false);
            });
        };

        self.selectRead = function () {
            $.each(self.messages(), function () {
                this.messageSelected(this.Read() === true);
            });
        };

        self.selectUnread = function () {
            $.each(self.messages(), function () {
                this.messageSelected(this.Read() === false);
            });
        };

        self.getTotals = function () {
            $.ajax({
                type: "GET",
                beforeSend: serviceFramework.setModuleHeaders,
                url: gettotalspath,
                async: false,
                cache: false
            }).done(function (totalsViewModel) {
                if (typeof totalsViewModel !== "undefined" && totalsViewModel != null) {
                    if (typeof totalsViewModel.TotalUnreadMessages !== "undefined" && $.type(totalsViewModel.TotalUnreadMessages) === "number") {
                        self.TotalNewThreads(totalsViewModel.TotalUnreadMessages);
                    }

                    if (typeof totalsViewModel.TotalNotifications !== "undefined" && $.type(totalsViewModel.TotalNotifications) === "number") {
                        self.TotalNotifications(totalsViewModel.TotalNotifications);
                    }
                }
            });
        };
	    		
    }

    this.init = function (element) {
        containerElement = element;

        var viewModel = new coreMessagingViewModel();
        ko.applyBindings(viewModel, document.getElementById($(element).attr("id")));
        $(element).dnnComposeMessage(composeMessageOptions);

        var stateview = getQuerystring('view');
        //cover case where advanced rewriter used
        if (stateview == "") {
            if (document.URL.indexOf("view/notifications") >= 0) {
                stateview = "notifications";
            }
        }
        
        if (stateview === "notifications") {
            // load initial state of notifications
            $(element + ' #smMainContent').dnnTabs({ selected: 1 });
            viewModel.loadNotificationsTabHandler();
        } else {
            //load initial state of inbox
            $(element + ' #smMainContent').dnnTabs({ selected: 0 });
            viewModel.myinboxHandler();
        }

	    setInterval(function() {
	    	viewModel.loadBox(inboxpath);
	    }, refreshInterval);

        viewModel.getTotals();
    };
};