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
                    <li><b><%: LocalizeText("WhoWillSee.Text") %></b></li>
                    <% if (IsGroup && !IsPublicGroup)
                    { %>
                    <li><input type="radio" name="privacy" value="R" checked="checked" aria-label="Security" /><%: LocalizeText("GroupMembers.Text") %></li>
                    <% }
                    else
                    { %>
                    <li><input type="radio" name="privacy" value="E" checked="checked" aria-label="Security" /><%: LocalizeText("Everyone.Text") %></li>
                    <li><input type="radio" name="privacy" value="C" aria-label="Security" /><%: LocalizeText("Community.Text") %></li>
                    <li><input type="radio" name="privacy" value="F" aria-label="Security" /><%: LocalizeText("Friends.Text") %></li>
                    <% if (!IsGroup) { %>
                    <li><input type="radio" name="privacy" value="U" aria-label="Security" /><%: LocalizeText("Private.Text") %></li>
                    <% } %>
                    <% } %>
                </ul>
            </div>
        </div>
        <a href="#" id="btnShare" aria-label="Share"><%: LocalizeText("Share.Text") %></a>
        <div id="journalPlaceholder"><%: LocalizeText("SharePlaceHolder.Text") %></div>
        <div class="dnnClear"></div>
    </div>
    <div id="journalOptionArea">
        <% if (AllowFiles || AllowPhotos) { %>
        <div class="fileUploadArea">
            <div class="jpa" id="tbar-attach-Area">
                <div class="journal_onlineFileShare">
                    <span id="tbar-photoText"><%: LocalizeText("SelectPhoto.Text") %></span> 
                    <span id="tbar-fileText"><%: LocalizeText("SelectFile.Text") %></span>
                    <div>
                        <a href="javascript:void(0)" id="photoFromSite" class="dnnSecondaryAction"><%: LocalizeText("BrowseFromSite.Text") %></a> 
                    </div>
                </div>
                <div class="journal_localFileShare">
                    <span class="browser-upload-btn"><%: LocalizeText("UploadFromLocal.Text") %></span>
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
                <span id="imgPrev"><<</span><span id="imgCount">1 <%: LocalizeText("Of.Text") %> 10</span><span id="imgNext">>></span>
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
<a href="#" style="display:none;" id="getMore" class="dnnPrimaryAction"><%: LocalizeText("GetMore.Text") %></a>

<script type="text/javascript">
    var InputFileNS = {};
    InputFileNS.initilizeInput = function() {
        var $fileUpload = $('.fileUploadArea :file');
        $fileUpload.data("text", '<%: LocalizeJsString("ChooseFile.Text")%>');
        $fileUpload.dnnFileInput();
    };
    $(document).ready(function () {
        InputFileNS.initilizeInput();
    });

    var pagesize = <%: PageSize.ToString()%>;
    var profilePage ='<%: ProfilePage%>';
    var maxlength = <%: MaxMessageLength.ToString()%>;
    
    var baseUrl = '<%: JavaScriptStringEncode(BaseUrl) %>'; 
    var resxLike ='<%: LocalizeJsString("{resx:like}")%>';
    var resxUnLike ='<%: LocalizeJsString("{resx:unlike}")%>';

    var pid = <%: Pid.ToString()%>;
    var gid = <%: Gid.ToString()%>;
    var ispublicgroup = <%: IsPublicGroup ? "true" : "false" %>;

    var journalOptions = {};
    journalOptions.servicesFramework = $.ServicesFramework(<%:ModuleId %>);
    journalOptions.confirmText = '<%: LocalizeJsString("DeleteItem") %>';
    journalOptions.yesText = '<%: LocalizeJsString("Yes.Text") %>';
    journalOptions.noText = '<%: LocalizeJsString("No.Text") %>';
    journalOptions.title = '<%: LocalizeJsString("Confirm.Text") %>';
    journalOptions.imageTypes = '<%: JavaScriptStringEncode(DotNetNuke.Common.Globals.ImageFileTypes) %>';

    var commentOpts = {};
    commentOpts.servicesFramework = $.ServicesFramework(<%:ModuleId %>);
    
    pluginInit();
    
    function setupJournal() {
        var sf = journalOptions.servicesFramework;
        var journalServiceBase = sf.getServiceRoot('Journal');
        
        $('.juser').click(function() {
            var uid =  $(this).attr('id').replace('user-', '');
            window.location.href = profilePage.replace('xxx',uid);
        });
        var maxUploadSize = <%: MaxUploadSize %>;
        
        $('.fileUploadArea').dnnUserFileUpload({
            maxFileSize: maxUploadSize,
            serverErrorMessage: '<%: LocalizeJsString("ServerError.Text") %>',
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
            title: '<%: LocalizeJsString("Title.Text") %>',
            cancelText: '<%: LocalizeJsString("Cancel.Text") %>',
            attachText: '<%: LocalizeJsString("Attach.Text") %>',
            getItemsServiceUrl: sf.getServiceRoot('InternalServices') + 'UserFile/GetItems',
            nameHeaderText: '<%: LocalizeJsString("Name.Header") %>',
            typeHeaderText: '<%: LocalizeJsString("Type.Header") %>',
            lastModifiedHeaderText: '<%: LocalizeJsString("LastModified.Header") %>',
            fileSizeText: '<%: LocalizeJsString("FileSize.Header") %>',
            templatePath: '<%: JavaScriptStringEncode(Page.ResolveUrl("~/Resources/Shared/Components/UserFileManager/Templates/")) %>',
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
