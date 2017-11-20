<%@ Page language="c#" Codebehind="Browser.aspx.cs" AutoEventWireup="True" Inherits="DNNConnect.CKEditorProvider.Browser.Browser" %>
<%@ Register TagPrefix="dnn" Assembly="DotNetNuke.Web" Namespace="DotNetNuke.Web.UI.WebControls" %>
<%@ Register TagPrefix="wnet" Namespace="DNNConnect.CKEditorProvider.Controls" Assembly="DNNConnect.CKEditorProvider" %>
<!DOCTYPE html>
<html lang="en">
  <head>
    <meta http-equiv="content-type" content="text/html; charset=UTF-8" />
    <meta name="language" content="en" />
    <title id="title" runat="server">DNNConnect.CKEditorProvider.FileBrowser</title>
    <link rel="stylesheet" type="text/css" href="Browser.comb.min.css" />
    <asp:Placeholder id="favicon" runat="server"></asp:Placeholder>
    <script src="js/Browser.js" type="text/javascript"></script>
  </head>
  <body>
    <form id="fBrowser" method="post" runat="server">
      <asp:ScriptManager ID="scriptManager1" runat="server"></asp:ScriptManager>
      <div id="BrowserContainer">
      <h1><asp:Label id="lblModus" runat="server"></asp:Label></h1>
      <hr />
      <div class="ui-tabs ui-widget ui-widget-content ui-corner-all" style="width:820px">
      <asp:RadioButtonList id="BrowserMode" runat="server" AutoPostBack="true" RepeatDirection="Horizontal" CssClass="ui-tabs-nav ui-helper-reset ui-helper-clearfix ui-widget-header ui-corner-all">
          <asp:ListItem text="File Link" value="file" Selected="True"></asp:ListItem>
          <asp:ListItem text="Page Link" value="page"></asp:ListItem>
       </asp:RadioButtonList>
      <asp:Panel id="panPageMode" runat="server">
        <table>
          <tr>
            <td>
              <strong><asp:Label id="lblChoosetab" runat="server" Text="Choose a Page:"></asp:Label></strong>
            </td>
            <td>
              <div id="TabsBox">
                <asp:TreeView ID="dnntreeTabs" runat="server" ExpandDepth="1" Width="683" Image="Images/folder.gif" />
              </div>
            </td>
          </tr>
          <tr>
            <td>
              <strong><asp:Label id="ExtraTabOptions" runat="server" Text="Extra Tab Options:"></asp:Label></strong>
            </td>
            <td>
                <table>
                  <tr>
                    <td>
                    </td>
                    <td>
                      <asp:CheckBox ID="chkHumanFriendy" runat="server" Text="Human Friendly Url" Checked="true" />
                    </td>
                  </tr>
                  <tr>
                    <td>
                      <em><asp:Label id="LabelAnchor" runat="server" Text="Select an Page Anchor:"></asp:Label></em>
                    </td>
                    <td>
                      <asp:DropDownList ID="AnchorList" runat="server">
                        <asp:ListItem Text="None"></asp:ListItem>
                      </asp:DropDownList>
                    </td>
                  </tr>
                  <tr id="LanguageRow" runat="server">
                    <td>
                      <em><asp:Label id="LabelTabLanguage" runat="server" Text="Add Aditional Language Parameter:"></asp:Label></em>
                    </td>
                    <td>
                      
                      <asp:DropDownList ID="LanguageList" runat="server">
                        <asp:ListItem Text="None"></asp:ListItem>
                      </asp:DropDownList>
                    </td>
                  </tr>
                </table> 
              </td>
          </tr>
        </table>
      </asp:Panel>
      <asp:Panel id="panImageEdHead" runat="server" Visible="false">
        <h1><asp:Label id="lblResizeHeader" runat="server" Text="Image Editor : Resize"></asp:Label></h1>
        <div style="margin:5px 0 5px 0;">
          <asp:Label id="lblOtherTools" runat="server" Text="Other Tools:"></asp:Label>
          <asp:Button ID="cmdCrop" runat="server" Text="Crop" />
          <asp:Button ID="cmdZoom" runat="server" Text="Zoom" />
          <asp:Button ID="cmdRotate" runat="server" Text="Rotate" />
          <asp:Button ID="cmdResize2" runat="server" Text="Resize" />
        </div>
      </asp:Panel>
      <asp:Panel id="panThumb" runat="server" Visible="false">
        <div style="margin-top:5px">
          <div class="PanelSubDivs">
          <asp:Label id="lblWidth" runat="server" Text="New Width:" Font-Bold="true"></asp:Label> 
          &nbsp;<asp:TextBox ID="txtWidth" runat="server" onchange="javascript: ChangedSliderW(this);" Width="30"></asp:TextBox>px&nbsp;
          <asp:Label id="lblHeight" runat="server" Text="New Height:" Font-Bold="true"></asp:Label> 
          &nbsp;<asp:TextBox ID="txtHeight" runat="server" onchange="javascript: ChangedSliderH(this);" Width="30"></asp:TextBox>px
          &nbsp;<asp:CheckBox id="chkAspect" runat="server" Text="Maintain Aspect Ratio" Checked="true" />
          <div id="SliderHeight"></div>
          <div id="SliderWidth"></div>
          </div>
         <div class="PanelSubDivs">
            <asp:Label id="lblThumbName" runat="server" Text="New Filename:" Font-Bold="true"></asp:Label>&nbsp;
            <asp:TextBox ID="txtThumbName" runat="server" Width="500"></asp:TextBox>
          </div>
          <div class="PanelSubDivs">
            <asp:Label id="lblImgQuality" runat="server" Text="Image Quality:" Font-Bold="true"></asp:Label>&nbsp;
            <asp:DropDownList id="dDlQuality" runat="server"></asp:DropDownList>&nbsp;%
          </div>
          <div class="PanelSubDivs" style="padding-bottom:5px;">
            <asp:Button ID="cmdResizeNow" CssClass="DefaultButton" Text="Resize Now" runat="server" />&nbsp;
            <asp:Button ID="cmdResizeCancel" Text="Cancel Resize" runat="server" />
          </div>
        </div>
        </asp:Panel>
        <asp:Panel id="panImageEditor" runat="server" Visible="false">
         <div class="PanelSubDivs">
            <asp:Label id="lblCropImageName" runat="server" Text="New Filename:" Font-Bold="true"></asp:Label>&nbsp;
            <asp:TextBox ID="txtCropImageName" runat="server" Width="500"></asp:TextBox>
           </div>
           <div class="PanelSubDivs" style="padding-bottom:5px;">
              <input type="submit" name="CropNow" value='<%= DotNetNuke.Services.Localization.Localization.GetString("cmdCropNow.Text", this.ResXFile, this.LanguageCode) %>' id="CropNow" CssClass="DefaultButton" />
              <asp:Button ID="cmdCropNow" Text="Create New Image" runat="server" Style="display:none" />&nbsp;
              <asp:Button ID="cmdCropCancel" Text="Cancel Crop" runat="server" />
          </div>
          <div class="CropButtons">
            <div id="PreviewCrop">
              <img src="images/Preview.png" alt="Show Preview" />&nbsp;<asp:Label id="lblShowPreview" runat="server" Text="Show Preview"></asp:Label>
            </div>
            <div id="ClearCrop">
              <img src="images/Clear.png" alt="Clear Preview" />&nbsp;<asp:Label id="lblClearPreview" runat="server" Text="Clear Preview"></asp:Label>
            </div>
            <div style="clear:both"></div>
          </div>
        </asp:Panel>
        <asp:Panel id="panImagePreview" runat="server" Visible="false">
        <table id="ImageTable">
        <tr>
          <td><asp:Label id="lblOriginal" Text="Original:" Font-Bold="true" runat="server"></asp:Label></td>
          <td><asp:Label id="lblPreview" Text="Preview:" Font-Bold="true" runat="server"></asp:Label></td>
        </tr>
        <tr>
          <td>
            <div id="ImageOriginal">
              <asp:Image id="imgOriginal" runat="server" />
            </div>
          </td>
          <td>
            <div class="ImagePreview">
              <asp:Label id="lblCropInfo" runat="server" Text="Here you will see the cropped image" Visible="false" Font-Italic="true"></asp:Label>
              <asp:Image id="imgResized" runat="server" />
            </div>
          </td>
        </tr>
      </table>
      </asp:Panel>
      <asp:Panel id="panLinkMode" runat="server">
        <p style="margin:5px 0 5px 5px"><strong><asp:Label id="lblCurrent" runat="server" text="Current Directory:"></asp:Label></strong> <asp:Label id="lblCurrentDir" runat="server"></asp:Label></p>
      
      <asp:Panel id="panUploadDiv" runat="server" Visible="false">
      <div class="MessageBox">
        <div class="ModalDialog">
          <div class="popup">
            <div class="DialogContent">
              <div class="modalHeader">
                <h3><asp:Label id="UploadTitle" runat="server" Text="Upload a File" /></h3>
              </div>
              <div>
                  <div id="fileupload">
                      <div class="fileupload-buttonbar">
                          <div id="dropzone" class="fade ui-widget-header"><%= DotNetNuke.Services.Localization.Localization.GetString("DropHere.Text", this.ResXFile, this.LanguageCode) %></div>
                          <div class="fileupload-buttons">
                              <span class="fileinput-button">
                                  <asp:Label id="AddFiles" runat="server" Text="Add file(s)..." />
                                  <input type="file" name="files[]" multiple>
                              </span>
                              <button type="submit" class="start"><%= DotNetNuke.Services.Localization.Localization.GetString("StartUploads.Text", this.ResXFile, this.LanguageCode) %></button>
                              <span class="fileupload-process"></span>
                          </div>
                          <div class="fileupload-progress fade" style="display:none">
                              <div class="progress" role="progressbar" aria-valuemin="0" aria-valuemax="100"></div>
                              <div class="progress-extended">&nbsp;</div>
                          </div>
                      </div>
                      <div id="UploadFilesBox">
                          <table role="presentation" class="table table-striped UploadFiles"><tbody class="files"></tbody></table>
                      </div>
                  </div>
                  <script id="template-upload" type="text/x-tmpl">
                      {% for (var i=0, file; file=o.files[i]; i++) { %}
                          <tr class="template-upload fade">
                              <td>
                                  <span class="preview"></span>
                              </td>
                              <td>
                                  <p class="name">{%=file.name%}</p>
                                  <strong class="error"></strong>
                                  <p class="size">Processing...</p>
                                  <div class="progress"></div>
                              </td>
                              <td>
                                  {% if (!i && !o.options.autoUpload) { %}
                <button class="start" disabled style="display:none">Start</button>
            {% } %}
                      {% if (!i) { %}
                                      <button class="cancel"><%= DotNetNuke.Services.Localization.Localization.GetString("cmdCreateCancel.Text", this.ResXFile, this.LanguageCode) %></button>
                                  {% } %}
                              </td>
                          </tr>
                      {% } %}
                  </script>
                  
                  <script id="template-download" type="text/x-tmpl">
                  </script>
              </div>
              <div class="OverrideFile">
                  <asp:CheckBox runat="server" ID="OverrideFile" />
              </div>
              <div class="maximumFileUploadInfo ui-state-highlight ui-corner-all">
                  <span class="ui-icon ui-icon-info" style="float: left; margin-right: .3em;"></span>
                  <asp:Label ID="MaximumUploadSizeInfo" runat="server"></asp:Label>
              </div>
              <hr />
              <div class="ModalFooter">
                <asp:Button ID="cmdUploadNow" Style="display:none" runat="server" />&nbsp;
                <asp:Button ID="cmdUploadCancel" Text="Cancel Upload" runat="server" />
              </div>
            </div>
          </div>
        </div>
      </div>
      </asp:Panel>
     

      
      <asp:Panel id="panCreate" runat="server" Visible="false">
      <div class="ModalDialog_overlayBG" ></div>
      <div class="MessageBox">
        <div class="ModalDialog">
          <div class="popup">
            <div class="DialogContent">
              <div class="modalHeader">
                <h3><asp:Label id="NewFolderTitle" runat="server" Text="Create New Folder" /></h3>
              </div>
              <div><asp:Label id="lblNewFoldName" runat="server" Text="New Folder Name"></asp:Label></div>
              <asp:TextBox id="tbFolderName" runat="server" Width="100%" />
              <hr />
              <div class="ModalFooter">
                <asp:Button ID="cmdCreateFolder" CssClass="DefaultButton ui-state-focus" Text="Create Now" runat="server" />
                <asp:Button ID="cmdCreateCancel" Text="Cancel" runat="server" />
              </div>
            </div>
          </div>
        </div>
      </div>
      </asp:Panel>
      
      
      
      <table class="LinkModeTable">
        <tr>
          <td>
              <span class="Toolbar">
                  <asp:LinkButton id="cmdCreate"  runat="server" ToolTip="Create a New Sub folder" onclick="Create_Click">
                      <img src="Images/CreateFolder.gif" alt="Create Folder" title="Create a New Sub folder" /> Create New Folder
                  </asp:LinkButton>
                  <asp:LinkButton id="Syncronize"  runat="server" ToolTip="Synchronize Folder" onclick="Syncronize_Click">
                      <img src="Images/CreateFolder.gif" alt="Sync Folder" title="Synchronize Folder" /> Synchronize Folder
                  </asp:LinkButton>
             </span>
          </td>
          <td>
              <span class="Toolbar">
                  <asp:LinkButton id="cmdUpload"  runat="server" ToolTip="Upload a file" onclick="Upload_Click">
                      <img src="Images/UploadButton.gif" alt="Upload File" title="Upload a file" /> Upload File
                  </asp:LinkButton>
                  &nbsp;<asp:LinkButton ToolTip="Download selected file" id="cmdDownload"  runat="server" onclick="Download_Click">
                            <img src="Images/DownloadButton.gif" alt="Download File" title="Download selected a file" /> Download File
                        </asp:LinkButton>
                  &nbsp;<asp:LinkButton ToolTip="Delete the selected file" id="cmdDelete"  runat="server" onclick="Delete_Click">
                        <img src="Images/DeleteFile.gif" alt="Delete File" title="Delete the selected file" /> Delete File
                  </asp:LinkButton> 
                  &nbsp;<asp:LinkButton ToolTip="Image Resizer" id="cmdResizer" runat="server" onclick="Resizer_Click">
                        <img src="Images/ResizeImage.gif" alt="Image Resizer" title="Image Resizer" /> Image Resizer
                  </asp:LinkButton>
              </span>
          </td>
        </tr>
        <tr>
          <td style="padding-top:9px">
            <asp:Label id="lblSubDirs" runat="server" Text="Subdirectories:"></asp:Label><br />
            <div id="FoldersBox">
             <asp:TreeView ID="FoldersTree" runat="server" ExpandDepth="1" />
            </div>
            <asp:Label runat="server" ID="FileSpaceUsedLabel" CssClass="fileSpaceUsedLabel"></asp:Label>
          </td>
          <td style="Width:550px">
            <asp:Label id="lblConFiles" runat="server" Text="Contained Files:"></asp:Label>&nbsp;
            <asp:Label runat="server" id="SwitchDetailView">DetailView</asp:Label>&nbsp;
            <asp:Label runat="server" id="SwitchListView">ListView</asp:Label>&nbsp;
            <asp:Label runat="server" id="SwitchIconsView">IconsView</asp:Label>&nbsp;|
            <asp:LinkButton runat="server" id="SortAscending" OnClick="SortAscendingClick">Sort Ascending</asp:LinkButton>&nbsp;
            <asp:LinkButton runat="server" id="SortDescending" OnClick="SortDescendingClick">Sort Descending</asp:LinkButton>
            <br />
            <asp:HiddenField runat="server" ID="ListViewState"/>
            <div id="FilesBox">
              <asp:Repeater ID="FilesList" runat="server">
                <HeaderTemplate>
                  <ul class="FilesDetailView">
                </HeaderTemplate>
                <ItemTemplate>
                  <li class="FilesListRow" id="ListRow" runat="server" 
                      title='<%# DataBinder.Eval(Container.DataItem, "PictureURL").ToString().Substring(DataBinder.Eval(Container.DataItem, "PictureURL").ToString().LastIndexOf("/", StringComparison.Ordinal) + 1)%>'>
                    <asp:LinkButton runat="server" ID="FileListItem" CssClass="FilesListItem" 
                       CommandArgument='<%# DataBinder.Eval(Container.DataItem, "FileId").ToString()%>'>
                      <asp:Image runat="server" ID="FileThumb" CssClass="FilePreview" ImageUrl='<%# DataBinder.Eval(Container.DataItem, "PictureURL").ToString()%>'
                          AlternateText='<%# DataBinder.Eval(Container.DataItem, "FileName").ToString()%>' ToolTip='<%# DataBinder.Eval(Container.DataItem, "FileName").ToString()%>' />
                      <span class="ItemInfo"><%# DataBinder.Eval(Container.DataItem, "Info").ToString()%></span>
                    </asp:LinkButton>
                  </li>
                </ItemTemplate>
                <AlternatingItemTemplate>
                  <li class="FilesListRowAlt" id="ListRow" runat="server" 
                      title='<%# DataBinder.Eval(Container.DataItem, "PictureURL").ToString().Substring(DataBinder.Eval(Container.DataItem, "PictureURL").ToString().LastIndexOf("/", StringComparison.Ordinal) + 1)%>'>
                                        <asp:LinkButton runat="server" ID="FileListItem" CssClass="FilesListItem" 
                       CommandArgument='<%# DataBinder.Eval(Container.DataItem, "FileId").ToString()%>'>
                      <asp:Image runat="server" ID="FileThumb" CssClass="FilePreview" ImageUrl='<%# DataBinder.Eval(Container.DataItem, "PictureURL").ToString()%>'
                          AlternateText='<%# DataBinder.Eval(Container.DataItem, "FileName").ToString()%>' ToolTip='<%# DataBinder.Eval(Container.DataItem, "FileName").ToString()%>' />
                      <span class="ItemInfo"><%# DataBinder.Eval(Container.DataItem, "Info").ToString()%></span>
                    </asp:LinkButton>
                  </li>
                </AlternatingItemTemplate>
                <FooterTemplate>
                  </ul>
                </FooterTemplate>
              </asp:Repeater>
            </div>
            <wnet:Pager id="PagerFileLinks" runat="server" OnPageChanged="PagerFileLinks_PageChanged"></wnet:Pager>
          </td>
        </tr>
      </table>
      </asp:Panel>
      </div>
        <asp:Panel id="panInfo" runat="server">
          <table>
            <tr>
              <td><asp:Label id="FileId" runat="server" Visible="false"></asp:Label><asp:Label id="lblFileName" runat="server" Visible="false"></asp:Label>
                <strong><asp:Label id="lblUrlType" runat="server" Text="Choose Url Type*:"></asp:Label></strong>
              </td>
              <td>
                <asp:RadioButtonList id="rblLinkType" runat="server" ToolTip="Choose Url Type" 
                    RepeatDirection="Vertical" CssClass="ButtonList" AutoPostBack="true">
                  <asp:ListItem Text="Relative URL" Value="relLnk" Selected="True"></asp:ListItem>
                  <asp:ListItem Text="Absolute URL" Value="absLnk"></asp:ListItem>
                </asp:RadioButtonList>
            </td>
            </tr>
          </table>
        </asp:Panel>

        <asp:Button id="cmdClose" CssClass="DefaultButton ui-state-focus" runat="server" text="OK" onclick="CmdCloseClick"></asp:Button>&nbsp;<asp:Button id="cmdCancel" runat="server" text="Cancel" ></asp:Button>   
      </div>
      <!-- Loading screen -->
      <asp:Panel id="panelLoading" runat="server" style="display:none">
      <div class="ModalDialog_overlayBG" id="LoadingScreen" ></div>
      <div class="MessageBox">
        <div class="ModalDialog">
          <div class="popup">
            <div class="DialogContent LoadingContent">
              <div class="modalHeader">
                <h3><asp:Label id="Wait" runat="server" Text="Please Wait" /></h3>
              </div>
              <div class="LoadingMessage"><asp:Label id="WaitMessage" runat="server" Text="Loading Page and Parsing Anchors"></asp:Label></div>
            </div>
          </div>
        </div>
      </div>
      </asp:Panel>
      <!-- / Loading screen -->
    </form>
    <script type="text/javascript">
        $(function() {
            var overrideFile = $('#<%= this.OverrideFile.ClientID %>').is(':checked');
            var maxFileSize = <%= this.MaxUploadSize %>;
            var fileUploaderURL = "FileUploader.ashx?portalid=<%= HttpContext.Current.Request.QueryString["PortalID"] %>";

            $('#fileupload').fileupload({
                url: fileUploaderURL,
                acceptFileTypes: new RegExp('(\.|\/)(' + '<%= this.AcceptFileTypes %>' + ')', 'i'),
                maxFileSize: maxFileSize,
                done: function() {
                    __doPostBack('cmdUploadNow', '');
                },
                formData: {
                    storageFolderID: '<%= CurrentFolderId %>',
                    portalID: '<%= HttpContext.Current.Request.QueryString["PortalID"] %>',
                    overrideFiles: overrideFile
                },
                dropZone: $('#dropzone')
            });
            $(document).bind('dragover', function (e) {
                var dropZone = $('#dropzone'),
                    timeout = window.dropZoneTimeout;
                if (!timeout) {
                    dropZone.addClass('ui-state-highlight');
                } else {
                    clearTimeout(timeout);
                }
                var found = false,
                    node = e.target;
                do {
                    if (node === dropZone[0]) {
                        found = true;
                        break;
                    }
                    node = node.parentNode;
                } while (node != null);
                if (found) {
                    dropZone.addClass('ui-widget-content');
                } else {
                    dropZone.removeClass('ui-widget-content');
                }
                window.dropZoneTimeout = setTimeout(function () {
                    window.dropZoneTimeout = null;
                    dropZone.removeClass('ui-state-highlight ui-widget-content');
                }, 100);
            });
        });
    </script>
  </body>
</html>

