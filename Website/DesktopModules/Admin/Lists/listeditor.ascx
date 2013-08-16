<%@ Control Language="C#" AutoEventWireup="false" Inherits="DotNetNuke.Common.Lists.ListEditor" CodeFile="ListEditor.ascx.cs" %>
<%@ Register TagPrefix="dnn" Namespace="DotNetNuke.Web.UI.WebControls" Assembly="DotNetNuke.Web" %>
<%@ Register TagPrefix="dnn" TagName="ListEntries" Src="~/DesktopModules/Admin/Lists/ListEntries.ascx" %>
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