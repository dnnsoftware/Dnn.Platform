<%@ Control Language="C#" AutoEventWireup="false" Inherits="DotNetNuke.Modules.CoreMessaging.View" CodeBehind="View.ascx.cs" %>
<%@ Import Namespace="DotNetNuke.Common.Utilities" %>
<%@ Import Namespace="DotNetNuke.Services.Localization" %>
<%@ Register TagPrefix="dnn" Namespace="DotNetNuke.Web.Client.ClientResourceManagement" Assembly="DotNetNuke.Web.Client" %>
<%@ Register src="Subscriptions.ascx" tagPrefix="dnn" tagName="Subscriptions" %>

<asp:Panel runat="server" ID="CoreMessagingContainer">

<dnn:DnnJsInclude ID="DnnJsInclude1" runat="server" PathNameAlias="SharedScripts" FilePath="knockout.js" />
<dnn:DnnJsInclude ID="DnnJsInclude2" runat="server" FilePath="~/Resources/Shared/Components/ComposeMessage/ComposeMessage.js" Priority="101" />
<dnn:DnnCssInclude ID="DnnCssInclude1" runat="server" FilePath="~/Resources/Shared/Components/ComposeMessage/ComposeMessage.css" />
<dnn:DnnJsInclude ID="DnnJsInclude3" runat="server" FilePath="~/Resources/Shared/Components/UserFileManager/UserFileManager.js" Priority="102" />
<dnn:DnnCssInclude ID="DnnCssInclude2" runat="server" FilePath="~/Resources/Shared/Components/UserFileManager/UserFileManager.css" />
<dnn:DnnJsInclude ID="DnnJsInclude4" runat="server" FilePath="~/Resources/Shared/Components/Tokeninput/jquery.tokeninput.js" Priority="103" />
<dnn:DnnCssInclude ID="DnnCssInclude3" runat="server" FilePath="~/Resources/Shared/Components/Tokeninput/Themes/token-input-facebook.css" />
<dnn:DnnJsInclude ID="DnnJsInclude5" runat="server" FilePath="~/Resources/Shared/Scripts/jquery.history.js" Priority="104" />
<dnn:DnnJsInclude ID="DnnJsInclude6" runat="server" FilePath="~/Resources/Shared/Components/UserFileManager/jquery.dnnUserFileUpload.js" Priority="106" />

<div id="coreMessaging" runat="server">
    <div id="smMainContent" class="dnnForm DnnModule-Messaging-Notifications">
        <ul class="dnnAdminTabNav">
            <li><a href="#dnnCoreMessaging" data-bind="click: loadMessagesTab, attr: { title: TotalNewThreads() + '<%=LocalizeString("NewUnreadMessages") %>    '}"><span data-bind="    text: TotalNewThreads, visible: TotalNewThreads() > 0"></span><%=LocalizeString("Messages") %></a></li>
            <li><a href="#dnnCoreNotification" data-bind="click: loadNotificationsTab, attr: {title: TotalNotifications() + '<%=LocalizeString("TotalNotifications") %>    '}"><span data-bind="    text: TotalNotifications, visible: TotalNotifications() > 0"></span><%=LocalizeString("Notifications") %></a></li>            
        </ul>
        <!-- start core messaging -->
        <div class="coreMessaging" id="dnnCoreMessaging">
            <div class="dnnFormExpandContent"><a href="#" class="ComposeMessage"><%=LocalizeString("ComposeNewMessage")%></a></div>
            <!-- ko ifnot: showReplies -->
            <div class="dnnCoreMessagingContent dnnClear">
                <div class="messageControls dnnClear">
                    <!-- GROUP ACTIONS: ARRANGE | ORDER | FILTER -->
                    <div class="messageFolders">
                        <p><strong data-bind="text: getPageNumbers()"></strong><%=LocalizeString("Of")%><strong data-bind="    text: TotalConversations"></strong></p>
                        <ul class="dnnButtonGroup">
							<li class="dnnButtonGroup-first"></li>
                            <li class="alpha">
                                <a href="#" title='<%=LocalizeString("Conversations")%>' data-bind="click: $root.myinbox, css: {'active': showInbox}"><span><%=LocalizeString("Conversations") %></span></a>
                            </li>
                            <li>
                                <a href="#" title='<%=LocalizeString("SentMessages")%>' data-bind="click: $root.mysentbox, css: {'active': showSentbox}"><span><%=LocalizeString("Sent")%></span></a>
                            </li>
                            <li class="omega">
                                <a href="#" title='<%=LocalizeString("ArchivedMessages")%>' data-bind="click: $root.myarchivebox, css: {'active': showArchivebox}"><span><%=LocalizeString("Archived")%></span></a>
                            </li>
                        </ul>
                    </div>
                    <!-- MESSAGE SELECT: SELECT | ACTIONS | ARCHIVE -->
                    <div class="messageSelect">
                        <ul class="dnnButtonGroup">
							<li class="dnnButtonGroup-first"></li>
                            <li id="SelectMenu" class="alpha selectDrop" data-bind="css: { 'active': selectMenuOn }, click: toogleSelectMenu, event: { blur: toogleSelectMenu }, visible: showInbox()">
                                <a href="#" title='<%=LocalizeString("SelectMessages")%>'><span><%=LocalizeString("Select")%></span></a>
                                <ul>
                                    <li><a href="#" data-bind="click: $root.selectAll"><%=LocalizeString("All")%></a></li>
                                    <li><a href="#" data-bind="click: $root.selectNone"><%=LocalizeString("None")%></a></li>
                                    <li><a href="#" data-bind="click: $root.selectRead"><%=LocalizeString("Read")%></a></li>
                                    <li><a href="#" data-bind="click: $root.selectUnread"><%=LocalizeString("Unread")%></a></li>
                                </ul>
                            </li>
                            <li id="ActionsMenu" class="selectDrop" data-bind="css: { 'active': actionsMenuOn }, click: toogleActionsMenu, event: { blur: toogleActionsMenu }, visible: showInbox()">
                                <a href="#" title='<%=LocalizeString("ApplyActionToMessages")%>' data-bind="css: { 'disabled': !hasElementsSelected() }"><span><%=LocalizeString("Actions")%></span></a>
                                <ul>
                                    <li><a href="#" data-bind="click: moveSelectedToRead"><%=LocalizeString("MarkRead")%></a></li>
                                    <li><a href="#" data-bind="click: moveSelectedToUnread"><%=LocalizeString("MarkUnread")%></a></li>
                                    <li><a href="#" data-bind="click: moveSelectedToArchive"><%=LocalizeString("MarkArchive")%></a></li>
                                </ul>
                            </li>
							<li id="ArchiveButton" class="omega" data-bind="visible: showInbox()">
								<a href="#" title='<%=LocalizeString("ArchiveMessages")%>' class="ArchiveItems" data-bind="click: moveSelectedToArchive"><span><%=LocalizeString("MarkArchive")%></span></a>
							</li>
                        </ul>
                        
                    </div>
                    <!-- MESSAGE ACTIONS: ARRANGE | ORDER | FILTER -->
                    <div class="messageActions" style="display:none">
                        <ul>
                            <li class="selectDrop">
                                <a href="#" title="Arrange Messages" class="dnnTertiaryAction"><span><%=LocalizeString("ArrangeMessages")%></span></a>
                                <ul>
                                    <li><a href="#"><%=LocalizeString("ArrangeDate")%></a></li>
                                    <li><a href="#"><%=LocalizeString("ArrangeFrom")%></a></li>
                                    <li><a href="#"><%=LocalizeString("ArrangeConversation")%></a></li>
                                </ul>
                            </li>
                        </ul>
                        <a href="#" title="Change Order By Ascending Or Descending" class="dnnTertiaryAction ToggleOrder ascend"><span>Acending</span></a>
                        <ul>
                            <li class="selectDrop">
                                <a href="#" title="Select A Filter Property" class="dnnTertiaryAction"><span><%=LocalizeString("Filter")%></span></a>
                                <ul>
                                    <li><a href="#"><%=LocalizeString("FilterUnread")%></a></li>
                                    <li><a href="#"><%=LocalizeString("FilterFriends")%></a></li>
                                    <li><a href="#"><%=LocalizeString("FilterGroups")%></a></li>
                                    <li><a href="#"><%=LocalizeString("FilterCategory")%></a></li>
                                    <li><a href="#"><%=LocalizeString("FilterFrom")%></a></li>
                                    <li><a href="#"><strong><%=LocalizeString("FilterViewAll")%></strong></a></li>
                                </ul>
                            </li>
                        </ul>
                    </div>
                    <div class="dnnClear"></div>
                </div>
                <div id="loadingMessages" data-bind="visible: loadingData"><img src='<%= ResolveUrl("images/ajax-loader.gif") %>' alt="" /> <%=LocalizeString("LoadingContent") %></div>
                <div class="smListings" data-bind="visible: !loadingData()">
                    <ul id="inbox" class="messages" data-bind="foreach: messages">
                        <li data-bind="css: {'active': !Read() && $root.showInbox()}">
                            <ul>
                                <li class="ListCol-1">
                                    <label data-bind="for: MessageIDName"></label>
                                    <input type="checkbox" data-bind="checked: messageSelected, id: MessageIDName, visible: $root.showInbox()" class="normalCheckBox" />
                                </li>
                                <li class="ListCol-2"><a class="profileImg" data-bind="attr: { href: SenderProfileUrl }"><span><em>
                                    <img alt="" data-bind="attr: { src: SenderAvatar, alt: From, title: From }" /></em></span></a>
                                </li>
                                <li class="ListCol-3">
                                    <dl>
                                        <dt class="subject"><a href="#" data-bind="text: Subject, click: $root.getReplies"></a></dt>
                                        <dd class="meta"><em><%=LocalizeString("From")%>: <a data-bind="text: From, attr: { href: SenderProfileUrl }"></a></em><br/><em><%=LocalizeString("SentTo")%>: <span data-bind="    text: To"></span></em></dd>
                                        <dd class="message" data-bind="text: MessageAbstract"></dd>
                                    </dl>
                                </li>
                                <li class="ListCol-4">
                                    <ul class="msgActionItems">
                                        <li><span data-bind="text: CreatedOnDate" class="smTimeStamped"></span><a href="#" class="ActiveToggle" data-bind="    click: $root.toggleState, visible: $root.showInbox(), attr: { title: (Read() === true ? '<%=LocalizeString("MarkUnread")%>    ' : '<%=LocalizeString("MarkRead")%>    ') }"><span><%=LocalizeString("ReadStatusToggle")%></span></a></li>
                                        <li class="hoverControls"><div><a href="#" data-bind="click: $root.moveToArchive, visible: $root.showInbox()"><%=LocalizeString("MarkArchive")%></a> | <a href="#" data-bind="    click: $root.getRepliesAndReply"><%=LocalizeString("Reply")%></a></div></li>
                                        <li><span class="attachmentsIcon" data-bind="if: AttachmentCount"><img src='<%= ResolveUrl("images/paperClip.png") %>' alt="" /></span></li>
                                    </ul>
                                </li>
                            </ul>
                        </li>
                    </ul>
                </div>
            </div>
            <div class="dnnCoreMessagingFooter"><a href="#" class="dnnPrimaryAction ComposeMessage dnnRight"><%=LocalizeString("ComposeNewMessage")%></a> <a href="#" class="dnnTertiaryAction" data-bind="click: loadMore, visible: loadMoreVisible"><%=LocalizeString("LoadMore")%></a><div class="dnnClear"></div></div>
            <!-- /ko -->
            <!-- ko if: showReplies -->
            <div class="dnnForm DnnModule-Messaging-Details dnnClear">
                <div class="dnnCoreMessagingContent dnnClear">
                    <div class="previousMessages">
                        <div class="dnnFormMessage dnnFormSuccess successMsg" style="display: none"></div>
                        <div class="messageHeader">
                            <p><strong><%=LocalizeString("Subject")%>:</strong> <span data-bind="text: threadSubject"></span><br/><strong><%=LocalizeString("SentTo")%>:</strong> <span data-bind="    text: threadTo"></span></p>
                            <div class="returnLink">
                                <a href="#" data-bind="click: toggleArchiveConversation, text: (TotalArchivedThreads() === 0 ? '<%=Localization.GetSafeJSString(LocalizeString("MarkArchive"))%>    ' : '<%=Localization.GetSafeJSString(LocalizeString("MarkUnarchive"))%>    ')"></a> | 
                                <a href="#" data-bind="click: toggleConversationState, text: (conversationRead() === true ? '<%=Localization.GetSafeJSString(LocalizeString("MarkUnread"))%>    ' : '<%=Localization.GetSafeJSString(LocalizeString("MarkRead"))%>    '), visible: TotalArchivedThreads() === 0"></a> | 
                                <a href="#" data-bind="click: backToMessages"><%=LocalizeString("BackToMessages")%></a>
                            </div>
                            <div class="dnnClear"></div>
                        </div>
                        <div class="morePrevMsgButton" data-bind="visible: showPreviousRepliesVisible">
                            <a href="#" class="replyView" data-bind="click: showPreviousReplies"><strong><%=LocalizeString("ShowPreviousReplies")%></strong></a>
                        </div>
                        <div class="smListings">
                            <ul class="messages" data-bind="foreach: { data: messagethreads, afterAdd: threadViewAfterAdd }">
                                <li>
                                    <ul>
                                        <li class="ListCol-1">
										    <a class="profileImg" data-bind="attr: { href: SenderProfileUrl }">
											    <span><em><img alt="" data-bind="attr: { src: SenderAvatar, alt: From, title: From }" /></em></span>
										    </a>
                                        </li>
                                        <li class="ListCol-2">
                                            <dl>
                                                <dd class="meta">
                                                    <em><a data-bind="text: From, attr: { href: SenderProfileUrl }"></a></em>
                                                </dd>
                                                <dd class="message" data-bind="text: Body"></dd>
                                                <dd class="attachements" data-bind="if: AttachmentCount > 0">
                                                    <strong><%=LocalizeString("Attachments")%>:</strong>
                                                    <ul data-bind="foreach: Attachments">
                                                        <li><a href="#" target="_blank" data-bind="attr: { href: Url }"><span data-bind="    text: Name"></span> (<span data-bind="    text: Size"></span>)</a></li>
                                                    </ul>
                                                </dd>
                                            </dl>
                                        </li>
                                        <li class="ListCol-3">
                                            <ul>
                                                <li data-bind="text: CreatedOnDate"></li>
                                                <li class="hoverControls"><a href="#" data-bind="click: $root.setReplySelected"><%=LocalizeString("Reply")%></a></li>
                                            </ul>
                                        </li>
                                    </ul>
                                </li>
                            </ul>
                            <div class="dnnClear"></div>
                        </div>
                    </div>
                </div>
                <div class="dnnCoreMessagingFooter" data-bind="visible:replyHasRecipients">
                    <textarea name="replyMessage" id="replyMessage" data-bind="hasfocus: isReplySelected"></textarea>
                    <a href="#" class="dnnPrimaryAction" data-bind="click: $root.reply"><%=LocalizeString("Reply")%></a>
                    <div class="dnnClear"></div>
                </div>
            </div>
            <!-- /ko -->
        </div>
        <!-- end core messaging -->

        <!-- start core notification -->
        <div class="coreNotifications" id="dnnCoreNotification">
            <div class="dnnCoreMessagingContent dnnClear">
                <div class="messageControls dnnClear">
                    <div class="messageFolders">
                        <p><strong data-bind="text: getNotificationPageNumbers()"></strong><%=LocalizeString("Of")%><strong data-bind="    text: TotalNotifications"></strong></p>
                    </div>
                </div>
                <div id="loadingNotifications" data-bind="visible: loadingData"><img src='<%= ResolveUrl("images/ajax-loader.gif") %>' alt="" /> <%=LocalizeString("LoadingContent") %></div>
                <div class="smListings" data-bind="visible: !loadingData()">
                    <ul class="messages" data-bind="foreach: { data: notifications, beforeRemove: hideNotification }">
                        <li class="active">
                            <ul>
                                <li class="ListCol-1"></li>
                                <li class="ListCol-2">
                                    <a class="profileImg" data-bind="attr: { href: SenderProfileUrl }"><span><em><img alt="" data-bind="    attr: { src: SenderAvatar, alt: From, title: From }" /></em></span></a>
                                </li>
                                <li class="ListCol-3">
                                    <dl>
                                        <dt class="subject"><span data-bind="text: Subject"></span></dt>
                                        <dd class="meta"><em><a data-bind="text: From, attr: { href: SenderProfileUrl }"></a></em></dd>
                                        <dd class="message" data-bind="html: Body"></dd>
                                        <dd class="notificationControls" data-bind="foreach: Actions">
                                            <a href="#" data-bind="text: Name, click: $root.performNotificationAction, attr: { title: Description }"></a><span data-bind="    visible: !$root.isLastNotificationAction($parent, $data)"> | </span>
                                        </dd>
                                    </dl>
                                </li>
                                <li class="ListCol-4">
                                    <ul>
                                        <li><span data-bind="text: DisplayDate" class="smTimeStamped"></span></li>
                                    </ul>
                                </li>
                            </ul>
                        </li>
                    </ul>
                </div>
            </div>
            <div class="dnnCoreMessagingFooter"><a href="#" class="dnnTertiaryAction" data-bind="click: loadMoreNotifications, visible: loadMoreNotificationsVisible"><%=LocalizeString("LoadMore")%></a></div>
        </div>
        <!-- end core notification -->
		
    </div>
</div>
<script type="text/javascript">
	jQuery(document).ready(function ($) {		
        var sm = new CoreMessaging($, ko, {
            profilePicHandler: '<% = DotNetNuke.Common.Globals.UserProfilePicFormattedUrl() %>',
            conversationSetAsReadText: '<%=DotNetNuke.UI.Utilities.ClientAPI.GetSafeJSString(LocalizeString("ConversationSetAsRead"))%>',
            conversationSetAsUnreadText: '<%=DotNetNuke.UI.Utilities.ClientAPI.GetSafeJSString(LocalizeString("ConversationSetAsUnread"))%>',
            loadingText: '<%=DotNetNuke.UI.Utilities.ClientAPI.GetSafeJSString(LocalizeString("Loading"))%>',
            loadMoreText: '<%=DotNetNuke.UI.Utilities.ClientAPI.GetSafeJSString(LocalizeString("LoadMore"))%>',
            conversationArchivedText: '<%=DotNetNuke.UI.Utilities.ClientAPI.GetSafeJSString(LocalizeString("ConversationArchived"))%>',
            conversationUnarchivedText: '<%=DotNetNuke.UI.Utilities.ClientAPI.GetSafeJSString(LocalizeString("ConversationUnarchived"))%>',
            notificationConfirmTitleText: '<%=DotNetNuke.UI.Utilities.ClientAPI.GetSafeJSString(LocalizeString("NotificationConfirmTitle"))%>',
            notificationConfirmYesText: '<%=DotNetNuke.UI.Utilities.ClientAPI.GetSafeJSString(LocalizeString("NotificationConfirmYes"))%>',
            notificationConfirmNoText: '<%=DotNetNuke.UI.Utilities.ClientAPI.GetSafeJSString(LocalizeString("NotificationConfirmNo"))%>',
            actionPerformedText: '<%=DotNetNuke.UI.Utilities.ClientAPI.GetSafeJSString(LocalizeString("ActionPerformed"))%>',
            actionNotPerformedText: '<%=DotNetNuke.UI.Utilities.ClientAPI.GetSafeJSString(LocalizeString("ActionNotPerformed"))%>',
            replyHasNoRecipientsText: '<%=DotNetNuke.UI.Utilities.ClientAPI.GetSafeJSString(LocalizeString("ReplyHasNoRecipients"))%>',
            serverErrorText: '<%=DotNetNuke.UI.Utilities.ClientAPI.GetSafeJSString(LocalizeString("ServerError"))%>',
            serverErrorWithDescriptionText: '<%=DotNetNuke.UI.Utilities.ClientAPI.GetSafeJSString(LocalizeString("ServerErrorWithDescription"))%>'
        }, {
            openTriggerScope: '#<%= coreMessaging.ClientID %>',
            openTriggerSelector: '.ComposeMessage',
            title: '<%= DotNetNuke.UI.Utilities.ClientAPI.GetSafeJSString(LocalizeString("Title")) %>',
            toText: '<%= DotNetNuke.UI.Utilities.ClientAPI.GetSafeJSString(LocalizeString("To")) %>',
            subjectText: '<%= DotNetNuke.UI.Utilities.ClientAPI.GetSafeJSString(LocalizeString("Subject")) %>',
            messageText: '<%= DotNetNuke.UI.Utilities.ClientAPI.GetSafeJSString(LocalizeString("Message")) %>',
            sendText: '<%= DotNetNuke.UI.Utilities.ClientAPI.GetSafeJSString(LocalizeString("Send")) %>',
            cancelText: '<%= DotNetNuke.UI.Utilities.ClientAPI.GetSafeJSString(LocalizeString("Cancel")) %>',
            attachmentsText: '<%= DotNetNuke.UI.Utilities.ClientAPI.GetSafeJSString(LocalizeString("Attachments")) %>',
            browseText: '<%= DotNetNuke.UI.Utilities.ClientAPI.GetSafeJSString(LocalizeString("Browse")) %>',
            uploadText: '<%= DotNetNuke.UI.Utilities.ClientAPI.GetSafeJSString(LocalizeString("Upload")) %>',
            maxFileSize: <%=Config.GetMaxUploadSize()%>,
            removeText: '<%= DotNetNuke.UI.Utilities.ClientAPI.GetSafeJSString(LocalizeString("Remove")) %>',
            messageSentTitle: '<%= DotNetNuke.UI.Utilities.ClientAPI.GetSafeJSString(LocalizeString("MessageSentTitle")) %>',
            messageSentText: '<%= DotNetNuke.UI.Utilities.ClientAPI.GetSafeJSString(LocalizeString("MessageSent")) %>',
            dismissThisText: '<%= DotNetNuke.UI.Utilities.ClientAPI.GetSafeJSString(LocalizeString("DismissThis")) %>',
            throttlingText: '<%= DotNetNuke.UI.Utilities.ClientAPI.GetSafeJSString(LocalizeString("Throttling")) %>',
            noResultsText: '<%= DotNetNuke.UI.Utilities.ClientAPI.GetSafeJSString(LocalizeString("NoResults")) %>',
            searchingText: '<%= DotNetNuke.UI.Utilities.ClientAPI.GetSafeJSString(LocalizeString("Searching")) %>',
            createMessageErrorText: '<%=DotNetNuke.UI.Utilities.ClientAPI.GetSafeJSString(LocalizeString("CreateMessageError"))%>',
            createMessageErrorWithDescriptionText: '<%=DotNetNuke.UI.Utilities.ClientAPI.GetSafeJSString(LocalizeString("CreateMessageErrorWithDescription"))%>',
            autoSuggestErrorText: '<%=DotNetNuke.UI.Utilities.ClientAPI.GetSafeJSString(LocalizeString("AutoSuggestError"))%>',
            showAttachments: <%= ShowAttachments %>,
            onMessageSent: function () {
                var context = ko.contextFor(document.getElementById($("#<%= coreMessaging.ClientID %>").attr("id")));
                if (!context.$root.showReplies()) context.$root.reloadBoxes();
            },
            userFileManagerOptions: {
                title: '<%= DotNetNuke.UI.Utilities.ClientAPI.GetSafeJSString(LocalizeString("UserFileManagerTitle")) %>',
                cancelText: '<%= DotNetNuke.UI.Utilities.ClientAPI.GetSafeJSString(LocalizeString("Cancel")) %>',
                attachText: '<%= DotNetNuke.UI.Utilities.ClientAPI.GetSafeJSString(LocalizeString("Attach")) %>',
                getItemsServiceUrl: $.ServicesFramework(<%=ModuleId %>).getServiceRoot('InternalServices') + 'UserFile/GetItems',
                templatePath: '<%=Page.ResolveUrl("~/Resources/Shared/Components/UserFileManager/Templates/") %>',
                templateName: 'Default',
                templateExtension: '.html',
                nameHeaderText: '<%= DotNetNuke.UI.Utilities.ClientAPI.GetSafeJSString(LocalizeString("Name.Header")) %>',
                typeHeaderText: '<%= DotNetNuke.UI.Utilities.ClientAPI.GetSafeJSString(LocalizeString("Type.Header")) %>',
                lastModifiedHeaderText: '<%= DotNetNuke.UI.Utilities.ClientAPI.GetSafeJSString(LocalizeString("LastModified.Header")) %>',
                fileSizeText: '<%= DotNetNuke.UI.Utilities.ClientAPI.GetSafeJSString(LocalizeString("FileSize.Header")) %>'
            },
            servicesFramework: $.ServicesFramework(<%=ModuleId %>)
        });
        
        sm.init('#<%= coreMessaging.ClientID %>');
        $("#<%=coreMessaging.ClientID %>").parentsUntil("div[id$=ContentPane]").parent().find(".dnnActionMenu").css({left: "232px", top: "1px"});
    });
</script>
</asp:Panel>