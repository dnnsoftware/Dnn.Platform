<%@ Control Language="C#" AutoEventWireup="false" Inherits="Dnn.Modules.Lists.ListEditor" Codebehind="ListEditor.ascx.cs" %>
<%@ Import Namespace="DotNetNuke.Services.Localization" %>
<%@ Register TagPrefix="dnn" Namespace="DotNetNuke.Web.UI.WebControls" Assembly="DotNetNuke.Web" %>
<%@ Register TagPrefix="dnn" Namespace="DotNetNuke.Web.UI.WebControls" Assembly="DotNetNuke.Web.Deprecated" %>
<%@ Register TagPrefix="dnn" TagName="ListEntries" Src="ListEntries.ascx" %>
<div class="dnnForm dnnListEditor dnnClear" id="dnnListEditor">
   
        <div class="dnnFormItem">
            <div class="dnnListEditorTree">
                <dnn:DnnTreeView ID="listTree" runat="server"></dnn:DnnTreeView>
                <asp:LinkButton id="cmdAddList" runat="server" resourcekey="AddList" CssClass="dnnSecondaryAction" causesvalidation="False" />
            </div>
            <div id="divNoList" runat="server" class="dnnListEditorNoList">
                <%= Localization.GetString("NoList", LocalResourceFile) %>
            </div>
			<div id="divDetails" runat="server" class="dnnListEditorDetails">
                <dnn:ListEntries id="lstEntries" runat="Server" />
			</div>
        </div>
  
</div>