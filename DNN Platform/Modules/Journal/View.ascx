<%@ Control language="C#" Inherits="DotNetNuke.Modules.Journal.View" AutoEventWireup="false"  Codebehind="View.ascx.cs" %>
<%@ Import Namespace="System.Web.Configuration" %>
<%@ Register Namespace="DotNetNuke.Modules.Journal.Controls" Assembly="DotNetNuke.Modules.Journal" TagPrefix="dnnj" %>
<%@ Register TagPrefix="dnn" Namespace="DotNetNuke.Web.Client.ClientResourceManagement" Assembly="DotNetNuke.Web.Client" %>

<div id="userFileManager"></div>

<dnn:DnnJsInclude runat="server" FilePath="~/Resources/Shared/Components/UserFileManager/jquery.dnnUserFileUpload.js" Priority="102" AddTag="false" />
<dnn:DnnJsInclude runat="server" FilePath="~/Resources/Shared/Components/UserFileManager/UserFileManager.js" Priority="105" AddTag="false"></dnn:DnnJsInclude>
<dnn:DnnCssInclude runat="server" FilePath="~/Resources/Shared/Components/UserFileManager/UserFileManager.css" AddTag="false"></dnn:DnnCssInclude>

<% if (ShowEditor) {  %>
<div class="journalTools">
    <div id="journalEditor">
        <div id="journalClose"></div>
        <textarea id="journalContent" aria-label="Content"></textarea>
        <div id="tbar">
            <span id="tbar-perm"></span>
            <% if (AllowFiles) {  %>
            <span id="tbar-attach"></span>
            <% } %>
            <% if (AllowPhotos) {  %>
            <span id="tbar-photo"></span>
            <%}%>
            <div class="securityMenu dnnClear">
                <div class="handle"></div>
                <ul>
                    <li><b><%= LocalizeString("WhoWillSee.Text") %></b></li>
                    <% if (IsGroup && !IsPublicGroup)
                    { %>
                    <li><input type="radio" name="privacy" value="R" checked="checked" aria-label="Security" /><%= LocalizeString("GroupMembers.Text") %></li>
                    <% }
                    else
                    { %>
                    <li><input type="radio" name="privacy" value="E" checked="checked" aria-label="Security" /><%= LocalizeString("Everyone.Text") %></li>
                    <li><input type="radio" name="privacy" value="C" aria-label="Security" /><%= LocalizeString("Community.Text") %></li>
                    <li><input type="radio" name="privacy" value="F" aria-label="Security" /><%= LocalizeString("Friends.Text") %></li>
                    <% if (!IsGroup) { %>
                    <li><input type="radio" name="privacy" value="U" aria-label="Security" /><%= LocalizeString("Private.Text") %></li>
                    <% } %>
                    <% } %>
                </ul>
            </div>
        </div>
        <a href="#" id="btnShare" aria-label="Share"><%= LocalizeString("Share.Text") %></a>
        <div id="journalPlaceholder"><%= LocalizeString("SharePlaceHolder.Text") %></div>
        <div class="dnnClear"></div>
    </div>
    <div id="journalOptionArea">
        <% if (AllowFiles || AllowPhotos) { %>
        <div class="fileUploadArea">
            <div class="jpa" id="tbar-attach-Area">
                <div class="journal_onlineFileShare">
                    <span id="tbar-photoText"><%= LocalizeString("SelectPhoto.Text") %></span> 
                    <span id="tbar-fileText"><%= LocalizeString("SelectFile.Text") %></span>
                    <div>
                        <a href="javascript:void(0)" id="photoFromSite" class="dnnSecondaryAction"><%= LocalizeString("BrowseFromSite.Text") %></a> 
                    </div>
                </div>
                <div class="journal_localFileShare">
                    <span class="browser-upload-btn"><%= LocalizeString("UploadFromLocal.Text") %></span>
                    <input id="uploadFileId" type="file" name="files[]" aria-label="Upload" />
                </div>
                <div style="clear:both; padding: 0; margin: 0;"></div>
            </div>
            <div id="itemUpload">
                <div class="fileupload-error dnnFormMessage dnnFormValidationSummary" style="display:none;"></div>
                <div class="progress_bar_wrapper">
                        <div class="progress_context" style="margin:10px 0px; display:none;">
                            <div class="upload_file_name" style="margin-top:5px; margin-bottom:-5px;"></div>
                            <div class="progress-bar green">
                                <div style="width:0px;">
                                    <span></span> 
                                </div>
                            </div>
                        </div>
                    </div>
                
                <div class="filePreviewArea"></div>
            </div>
        </div>
        <% } %>
        <div class="jpa" id="linkArea">
            <div id="linkClose"></div>
            <div id="imagePreviewer">
                <div id="image">
                    <img src='' alt="Preview" />
                </div>
                <span id="imgPrev"><<</span><span id="imgCount">1 <%= LocalizeString("Of.Text") %> 10</span><span id="imgNext">>></span>
            </div>
            <div id="linkInfo">
                <b></b>
                <p></p>
            </div>
            <div class="dnnClear"></div>
        </div>
    </div>
</div>
<div class="dnnClear"></div>


<%} %>

<div id="journalItems">
    <dnnj:JournalListControl ID="ctlJournalList" runat="server"/>
</div>
<a href="#" style="display:none;" id="getMore" class="dnnPrimaryAction"><%= LocalizeString("GetMore.Text") %></a>

<script type="text/javascript">
    var InputFileNS = {};
    InputFileNS.initilizeInput = function() {
        var $fileUpload = $('.fileUploadArea :file');
        $fileUpload.data("text", '<%=LocalizeSafeJsString("ChooseFile.Text")%>');
        $fileUpload.dnnFileInput();
    };
    $(document).ready(function () {
        InputFileNS.initilizeInput();
    });

    var pagesize = <%= PageSize.ToString()%>;
    var profilePage ='<%= ProfilePage%>';
    var maxlength = <%= MaxMessageLength.ToString()%>;
    
    var baseUrl = '<%= BaseUrl %>'; 
    var resxLike ='<%= LocalizeSafeJsString("{resx:like}")%>';
    var resxUnLike ='<%= LocalizeSafeJsString("{resx:unlike}")%>';

    var pid = <%= Pid.ToString()%>;
    var gid = <%= Gid.ToString()%>;
    var ispublicgroup = <%= IsPublicGroup ? "true" : "false" %>;

    var journalOptions = {};
    journalOptions.servicesFramework = $.ServicesFramework(<%=ModuleId %>);
    journalOptions.confirmText = '<%= LocalizeSafeJsString("DeleteItem") %>';
    journalOptions.yesText = '<%= LocalizeSafeJsString("Yes.Text") %>';
    journalOptions.noText = '<%= LocalizeSafeJsString("No.Text") %>';
    journalOptions.title = '<%= LocalizeSafeJsString("Confirm.Text") %>';
    journalOptions.imageTypes = '<%= DotNetNuke.Common.Globals.glbImageFileTypes %>';

    var commentOpts = {};
    commentOpts.servicesFramework = $.ServicesFramework(<%=ModuleId %>);
    
    pluginInit();
    
    function setupJournal() {
        var sf = journalOptions.servicesFramework;
        var journalServiceBase = sf.getServiceRoot('Journal');
        
        $('.juser').click(function() {
            var uid =  $(this).attr('id').replace('user-', '');
            window.location.href = profilePage.replace('xxx',uid);
        });
        var maxUploadSize = <%=MaxUploadSize %>;
        
        $('.fileUploadArea').dnnUserFileUpload({
            maxFileSize: maxUploadSize,
            serverErrorMessage: '<%= LocalizeSafeJsString("ServerError.Text") %>',
            addImageServiceUrl: journalServiceBase + 'FileUpload/UploadFile',
            beforeSend: sf.setModuleHeaders,
            callback: function (result) {
                if (IsImage(result.extension)) {
                    attachPhoto(result.file_id, result.url, true);
                }else{
                    attachPhoto(result.file_id, result.thumbnail_url, false);
                }
            },
            complete: function () {
                InputFileNS.initilizeInput();
            }
        });
        var jopts = {};
        jopts.maxLength = maxlength;
        jopts.servicesFramework = sf;
        $('body').journalTools(jopts);
        
        $('#userFileManager').userFileManager({
            title: '<%= LocalizeSafeJsString("Title.Text") %>',
            cancelText: '<%= LocalizeSafeJsString("Cancel.Text") %>',
            attachText: '<%= LocalizeSafeJsString("Attach.Text") %>',
            getItemsServiceUrl: sf.getServiceRoot('InternalServices') + 'UserFile/GetItems',
            nameHeaderText: '<%= LocalizeSafeJsString("Name.Header") %>',
            typeHeaderText: '<%= LocalizeSafeJsString("Type.Header") %>',
            lastModifiedHeaderText: '<%= LocalizeSafeJsString("LastModified.Header") %>',
            fileSizeText: '<%= LocalizeSafeJsString("FileSize.Header") %>',
            templatePath: '<%=Page.ResolveUrl("~/Resources/Shared/Components/UserFileManager/Templates/") %>',
            templateName: 'Default',
            templateExtension: '.html',
            attachCallback: function(file) {
                if (IsImage(file.type)) {
                    attachPhoto(file.id, file.thumb_url, true);
                } else {
                    attachPhoto(file.id, file.thumb_url, false);
                }
            }
        });
       
        var opts = {};
        opts.textareaSelector = '#journalContent';
        opts.clearPreviewSelector = '#linkClose';
        opts.previewSelector = '#linkArea';
        opts.servicesFramework = sf;

        $('body').previewify(opts);
    }

    $(document).ready(function () {
        setupJournal();
    });
</script>
