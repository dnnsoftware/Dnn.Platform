<%@ Control Language="C#" AutoEventWireup="true" Inherits="DotNetNuke.Web.UI.WebControls.DnnFilePickerUploader" %>
<%@ Register TagPrefix="dnn" Namespace="DotNetNuke.Web.UI.WebControls" Assembly="DotNetNuke.Web" %>
<asp:Panel runat="server" ID="dnnFileUploadScope" CssClass="dnnFileUploadScope">
    <div class="dnnLeft">
        <div class="dnnFormItem dnnFileUploadFolder">
            <span><%= FolderLabel  %></span>
            <dnn:DnnFolderDropDownList runat="server" ID="FoldersComboBox" />
            <asp:Label runat="server" ID="FoldersLabel" CssClass="dnnFoldersLabel"></asp:Label>
        </div>
        <div class="dnnFormItem">
            <span><%= FileLabel  %></span><dnn:DnnFileDropDownList runat="server" ID="FilesComboBox" />
        </div>
        <div class="dnnFormItem">
            <input type="file" name="postfile" multiple style="display: none" class="normalFileUpload" />
            <input type="button" name="uploadFileButton" value="<%= UploadFileLabel  %>"/>
            <dnn:DnnFileUpload runat="server" ID="FileUploadControl"></dnn:DnnFileUpload>
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
</asp:Panel>
<dnn:DnnScriptBlock runat="server">
    <script type="text/javascript">
    (function($) {
        var initDnnFileUploader = function() {
            var settings = {
                fileFilter: '<%= FileFilter %>',
                required: <%= Required? "true":"false" %>,
                foldersComboId: '<%= FoldersComboBox.ClientID %>',
                filesComboId: '<%= FilesComboBox.ClientID %>',
                fileUploadId: '<%=FileUploadControl.ClientID%>',
                folder: '<%= FolderPath %>',
                selectedFolderId: <%=FoldersComboBox.SelectedFolder != null ? FoldersComboBox.SelectedFolder.FolderID : -1 %>,
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
</dnn:DnnScriptBlock>
