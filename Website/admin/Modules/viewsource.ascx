<%@ Control Language="C#" AutoEventWireup="false" Inherits="DotNetNuke.Modules.Admin.Modules.ViewSource" CodeFile="viewsource.ascx.cs" %>
<%@ Register Assembly="DotnetNuke" Namespace="DotNetNuke.UI.WebControls" TagPrefix="dnn" %>
<%@ Register TagPrefix="dnn" Namespace="DotNetNuke.Web.UI.WebControls" Assembly="DotNetNuke.Web" %>
<%@ Register TagPrefix="dnn" TagName="Label" Src="~/controls/LabelControl.ascx" %>
<div class="dnnForm dnnViewSource dnnClear">
    <fieldset>
		<div class="dnnFormItem">
            <dnn:Label id="plFile" runat="Server" />
            <%--<asp:DropDownList ID="cboFile" runat="server" AutoPostBack="true" />--%>
            <dnn:DnnComboBox ID="cboFile" runat="server" AutoPostBack="true" />
        </div>
        <div class="dnnFormItem" style="text-align:center">
            <asp:Label ID="lblSourceFile" runat="server" Visible="false" />
        </div>
        <div class="dnnFormItem" id="trSource" runat="server" visible="false">
            <dnn:label id="plSource" controlname="txtSource" runat="server" />
            <asp:TextBox ID="txtSource" runat="server" TextMode="MultiLine" Rows="20" Columns="80" />
        </div>
    </fieldset>
    <ul class="dnnActions dnnClear">
        <li><asp:LinkButton id="cmdUpdate" resourcekey="cmdUpdate" runat="server" cssclass="dnnPrimaryAction" /></li>
        <li><asp:HyperLink id="cmdCancel" resourcekey="cmdCancel" runat="server" cssclass="dnnSecondaryAction" causesvalidation="False" /></li>
    </ul>
</div>