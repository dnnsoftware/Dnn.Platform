<%@ Control Language="C#" AutoEventWireup="true" Inherits="DotNetNuke.Web.UI.WebControls.DnnFilePickerUploader" %>
<%@ Register TagPrefix="dnn" Namespace="DotNetNuke.Web.UI.WebControls" Assembly="DotNetNuke.Web" %>
<asp:Panel runat="server" ID="dnnFileUploadScope" CssClass="dnnFileUploadScope">
    <div class="dnnLeft">
        <div class="dnnFormItem dnnFileUploadFolder">
            <span><%= FolderLabel  %></span><dnn:DnnComboBox runat="server" ID="FoldersComboBox" OnClientSelectedIndexChanged="dnn.dnnFileUpload.Folders_Changed"/>
        </div>
        <div class="dnnFormItem">
            <span><%= FileLabel  %></span><dnn:DnnComboBox runat="server" ID="FilesComboBox" OnClientSelectedIndexChanged="dnn.dnnFileUpload.Files_Changed" DataTextField="Text" DataValueField="Value"/>
        </div>
        <div class="dnnFormItem">
            <input type="file" name="postfile" multiple data-text="<%= UploadFileLabel  %>" />
        </div>
    </div>
    <div class="dnnLeft">
        <asp:Panel id="dnnFileUploadDropZone" runat="server" CssClass="dnnFileUploadDropZone">
            <span><%= DropFileLabel  %></span>
        </asp:Panel>
    </div>
    <div class="dnnClear"></div>
    <div class="dnnFormItem">
         <asp:Panel runat="server" ID="dnnFileUploadProgressBar" CssClass="ui-progressbar">
             <div class="ui-progressbar-value"></div>
         </asp:Panel>
    </div>
    <asp:HiddenField runat="server" ID="dnnFileUploadFilePath" />
    <asp:HiddenField runat="server" ID="dnnFileUploadFileId" />
</asp:Panel>
<script type="text/javascript">
    (function($) {
        var initDnnFileUploader = function() {
            var settings = {
                fileFilter: '<%= FileFilter %>',
                required: <%= Required? "true":"false" %>,
                foldersComboId: '<%= FoldersComboBox.ClientID %>',
                filesComboId: '<%= FilesComboBox.ClientID %>',
                folder: '<%= FolderPath %>',
                filePathId: '<%= dnnFileUploadFilePath.ClientID %>',
                fileIdId: '<%= dnnFileUploadFileId.ClientID %>',
                progressBarId: '<%= dnnFileUploadProgressBar.ClientID %>',
                dropZoneId: '<%= dnnFileUploadDropZone.ClientID %>'
            };
            $('#<%= dnnFileUploadScope.ClientID %>').dnnFileUpload(settings);
        };
        $(initDnnFileUploader);

        // microsoft ajax registered - to fix microsoft ajax update panel post back
        if(typeof Sys != 'undefined') 
            Sys.WebForms.PageRequestManager.getInstance().add_endRequest(initDnnFileUploader);

    })(jQuery);
</script>