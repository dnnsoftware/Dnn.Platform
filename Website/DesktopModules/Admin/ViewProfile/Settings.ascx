<%@ Control Language="C#" AutoEventWireup="false" CodeFile="Settings.ascx.cs" Inherits="DotNetNuke.Modules.Admin.Users.ViewProfileSettings" %>
<%@ Register TagPrefix="dnn" TagName="Label" Src="~/controls/LabelControl.ascx" %>
<div class="dnnForm dnnViewProfileSettings dnnClear">
	<div class="dnnFormItem">
		<dnn:label id="lblIncludeButton" runat="server" ControlName="IncludeParent" ResourceKey="IncludeButton" />
		 <asp:Checkbox ID="IncludeButton" runat="server" Checked="True" />
	</div>
    <div class="dnnFormItem">
        <dnn:Label ID="plTemplate" runat="server" ControlName="txtTemplate" />
        <asp:TextBox ID="txtTemplate" Columns="60" TextMode="MultiLine" Rows="25" MaxLength="2000" runat="server" Width="300px" />
        <asp:LinkButton ID="cmdLoadDefault" runat="server" CausesValidation="False" CssClass="dnnSecondaryAction" resourcekey="LoadDefault" />
    </div>
</div>