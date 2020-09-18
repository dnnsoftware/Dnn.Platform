<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="View.ascx.cs" Inherits="DotNetNuke.Modules.MemberDirectory.View" %>
<%@ Import Namespace="DotNetNuke.Common.Utilities" %>
<%@ Import Namespace="DotNetNuke.Services.Localization" %>
<%@ Register TagPrefix="dnn" Namespace="DotNetNuke.Web.Client.ClientResourceManagement" Assembly="DotNetNuke.Web.Client"%>

<dnn:DnnJsInclude ID="DnnJsInclude1" runat="server" FilePath="~/Resources/Shared/Components/ComposeMessage/ComposeMessage.js" Priority="101" AddTag="false" />
<dnn:DnnCssInclude ID="DnnCssInclude1" runat="server" FilePath="~/Resources/Shared/Components/ComposeMessage/ComposeMessage.css" AddTag="false" />
<dnn:DnnJsInclude ID="DnnJsInclude2" runat="server" FilePath="~/Resources/Shared/Components/UserFileManager/UserFileManager.js" Priority="102" AddTag="false" />
<dnn:DnnCssInclude ID="DnnCssInclude2" runat="server" FilePath="~/Resources/Shared/Components/UserFileManager/UserFileManager.css" AddTag="false" />
<dnn:DnnCssInclude ID="DnnCssInclude3" runat="server" FilePath="~/Resources/Shared/Components/UserFileManager/UserFileManager.css" AddTag="false" />
<dnn:DnnJsInclude ID="DnnJsInclude3" runat="server" FilePath="~/Resources/Shared/Components/Tokeninput/jquery.tokeninput.js" Priority="103" AddTag="false" />
<dnn:DnnCssInclude ID="DnnCssInclude4" runat="server" FilePath="~/Resources/Shared/Components/Tokeninput/Themes/token-input-facebook.css" AddTag="false" />
<dnn:DnnJsInclude ID="DnnJsInclude6" runat="server" FilePath="~/Resources/Shared/Components/UserFileManager/jquery.dnnUserFileUpload.js" Priority="104" AddTag="false" />

<div id="memberDirectory" runat="server"  class="dnnForm dnnMemberDirectory">
	<div id="searchBar" class="mdSearch dnnClear" runat="server" ViewStateMode="Disabled">
    	<div class="mdSearchBar" id="mdBasicSearchBar">
        	<div class="searchWrapper">
                <a href="#" id="refreshResults" data-bind="visible: ResetEnabled, click: resetSearch" title="<%=LocalizeString("Refresh") %>"><span><%=LocalizeString("Refresh") %></span></a>
            	<input type="text" id="mdBasicSearch" data-bind="value: SearchTerm" placeholder="<%=LocalizeString("SearchConnections") %>" />
            </div>
            <a href="" title="search" data-bind="click: basicSearch" class="dnnPrimaryAction"><%=LocalizeString("Search") %></a>
        </div>
        <span class="selectDrop" id="advancedSearchBar" runat="server">
        	<a href="#" id="mdAdvancedSearch" class="mdAdvancedSearch dnnTertiaryAction" title="advanced search"><%=LocalizeString("AdvancedSearch") %></a>
        	<div class="mdAdvancedSearchForm" id="mdAdvancedSearchForm">
                <div class="dnnFormItem">
                    <label for="<%=SearchField1 %>"><%=Localization.GetString("ProfileProperties_" + SearchField1, ProfileResourceFile)%></label>
                    <input type="text" placeholder="" id="" name="<%=SearchField1 %>"  data-bind="value: AdvancedSearchTerm1"/>
                </div>
                <div class="dnnFormItem">
                    <label for="<%=SearchField2 %>"><%=Localization.GetString("ProfileProperties_" + SearchField2, ProfileResourceFile)%></label>
                    <input type="text" placeholder="" id="" name="<%=SearchField2 %>"  data-bind="value: AdvancedSearchTerm2"/>
                </div>
                <div class="dnnFormItem">
                    <label for="<%=SearchField3 %>"><%=Localization.GetString("ProfileProperties_" + SearchField3, ProfileResourceFile)%></label>
                    <input type="text" placeholder="" id="" name="<%=SearchField3 %>"  data-bind="value: AdvancedSearchTerm3"/>
                </div>
                <div class="dnnFormItem">
                    <label for="<%=SearchField4 %>"><%=Localization.GetString("ProfileProperties_" + SearchField4, ProfileResourceFile)%></label>
                    <input type="text" placeholder="" id="" name="<%=SearchField4 %>"  data-bind="value: AdvancedSearchTerm4"/>
                </div>
                <a href="" class="dnnPrimaryAction" data-bind="click: advancedSearch" ><%=LocalizeString("Search") %></a>
       		</div><!--close mdAdvancedSearchForm-->
        </span>
        
    </div>
    <div id="loading" data-bind="visible: loadingData">
        <img src='<%= ResolveUrl("images/ajax-loader.gif") %>' alt='<%=LocalizeString("Loading") %>' /><%=LocalizeString("Loading") %>
    </div>    
    <div class="dnnFormMessage dnnFormInfo" style="display:none" data-bind="visible: !HasMembers()"><%=LocalizeString("NoMembers") %></div>
    <ul id="mdMemberList" class="mdMemberList dnnClear" style="display:none" data-bind="foreach: { data: Members, afterRender: handleAfterRender }, css: { mdMemberListVisible : Visible }, visible: HasMembers()">
        <li>
            <div data-bind="visible: $parent.isEven($data)">
                <%=ItemTemplate %>
            </div>            
            <div data-bind="visible: !$parent.isEven($data)">
                <%=AlternateItemTemplate %>
            </div>            
            <div id="popUpPanel" runat="Server" class="mdHoverContent dnnClear" ViewStateMode="Disabled">
            	<div class="mdHoverContentTop">
            	    <%=PopUpTemplate %>
                </div>
                <div class="mdHoverContentBt"></div>
                <span class="tooltipArrow"></span>                    
            </div>
        </li>
    </ul>
    <div id="loadMore" runat="server" ViewStateMode="Disabled" Visible="False" class="mdLoadMore" style="display:none" data-bind="visible: CanLoadMore()"><a href="" class="dnnTertiaryAction" title="" data-bind="click: loadMore">&darr; <%=LocalizeString("LoadMore")%></a></div>
</div>

<script language="javascript" type="text/javascript">
    $(document).ready(function () {
        var md = new MemberDirectory($, ko, {
            userId: <% = (ProfileUserId == -1) ? ModuleContext.PortalSettings.UserId: ProfileUserId %>,
            viewerId: <% = ModuleContext.PortalSettings.UserId %>,
            groupId:<% = GroupId %>,
            pageSize: <% = PageSize %>,
            profileUrl: "<% = ViewProfileUrl %>",
            profileUrlUserToken: "<% = ProfileUrlUserToken %>",
            profilePicHandler: '<% = DotNetNuke.Common.Globals.UserProfilePicRelativeUrl() %>',
            addFriendText: '<%=LocalizeSafeJsString("AddFriend") %>',
            acceptFriendText: '<%=LocalizeSafeJsString("AcceptFriend") %>',
            friendPendingText:'<%=LocalizeSafeJsString("FriendPending") %>',
            removeFriendText:'<%=LocalizeSafeJsString("RemoveFriend") %>',
            followText:'<%=LocalizeSafeJsString("Follow") %>',
            unFollowText:'<%=LocalizeSafeJsString("UnFollow") %>',
            sendMessageText:'<%=LocalizeSafeJsString("SendMessage") %>',
            userNameText: '<%=LocalizeSafeJsString("UserName") %>',
            emailText: '<%=LocalizeSafeJsString("Email") %>',
            cityText: '<%=LocalizeSafeJsString("City") %>',
            searchErrorText: '<%=LocalizeSafeJsString("SearchError")%>',
            serverErrorText: '<%=LocalizeSafeJsString("ServerError")%>',
            serverErrorWithDescriptionText: '<%=LocalizeSafeJsString("ServerErrorWithDescription")%>',
        	servicesFramework: $.ServicesFramework(<%=ModuleContext.ModuleId %>),
			disablePrivateMessage: <%= DisablePrivateMessage.ToString().ToLowerInvariant() %>
        }, {
	        title: '<%= LocalizeSafeJsString("Title") %>',
            toText: '<%= LocalizeSafeJsString("To") %>',
            subjectText: '<%= LocalizeSafeJsString("Subject") %>',
            messageText: '<%= LocalizeSafeJsString("Message") %>',
            sendText: '<%= LocalizeSafeJsString("Send") %>',
            cancelText: '<%= LocalizeSafeJsString("Cancel") %>',
            attachmentsText: '<%= LocalizeSafeJsString("Attachments") %>',
            browseText: '<%= LocalizeSafeJsString("Browse") %>',
            uploadText: '<%= LocalizeSafeJsString("Upload") %>',
            maxFileSize: <%=Config.GetMaxUploadSize()%>,
            removeText: '<%= LocalizeSafeJsString("Remove") %>',
            messageSentTitle: '<%= LocalizeSafeJsString("MessageSentTitle") %>',
            messageSentText: '<%= LocalizeSafeJsString("MessageSent") %>',
            dismissThisText: '<%= LocalizeSafeJsString("DismissThis") %>',
            throttlingText: '<%= LocalizeSafeJsString("Throttling") %>',
            noResultsText: '<%= LocalizeSafeJsString("NoResults") %>',
            searchingText: '<%= LocalizeSafeJsString("Searching") %>',
            createMessageErrorText: '<%=LocalizeSafeJsString("CreateMessageError")%>',
            createMessageErrorWithDescriptionText: '<%=LocalizeSafeJsString("CreateMessageErrorWithDescription")%>',
            autoSuggestErrorText: '<%=LocalizeSafeJsString("AutoSuggestError")%>'
        });
    	md.init('#<%= memberDirectory.ClientID %>');

    	$("#mdBasicSearchBar input[type=text]").keydown(function(e) {
			if (e.which == 13) {
				$("#mdBasicSearchBar a[class*=dnnPrimaryAction]").focus().click();
				e.preventDefault();
			}
	    });
	    
	    $("#mdAdvancedSearchForm input[type=text]").keydown(function(e) {
	    	if (e.which == 13) {
	    		$("#mdAdvancedSearchForm a[class*=dnnPrimaryAction]").focus().click();
	    		e.preventDefault();
	    	}
	    });
    });
</script>