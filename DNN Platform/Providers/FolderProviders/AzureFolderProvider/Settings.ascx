<%@ Control Language="C#" CodeBehind="Settings.ascx.cs" Inherits="DotNetNuke.Providers.FolderProviders.AzureFolderProvider.Settings, DotNetNuke.Providers.FolderProviders" %>
<%@ Register TagPrefix="dnn" TagName="Label" Src="~/controls/LabelControl.ascx" %>
<div class="dnnFormItem">
    <dnn:Label ID="plAccountName" runat="server" controlname="tbAccessKeyId" />
    <asp:TextBox ID="tbAccountName" runat="server" CssClass="dnnFormRequired" />
    <asp:RequiredFieldValidator id="valAccountName" runat="server" ControlToValidate="tbAccountName" Display="Dynamic" CssClass="dnnFormMessage dnnFormError" resourcekey="valAccountName.ErrorMessage" />
</div>
<div class="dnnFormItem">
    <dnn:Label ID="plAccountKey" runat="server" />
    <asp:TextBox ID="tbAccountKey" runat="server" CssClass="dnnFormRequired" />
    <asp:RequiredFieldValidator ID="valAccountKey" runat="server" ControlToValidate="tbAccountKey" Display="Dynamic" CssClass="dnnFormMessage dnnFormError" resourcekey="valAccountKey.ErrorMessage" />
</div>
<div class="dnnFormItem">
    <dnn:Label ID="plContainerName" runat="server" />
    <asp:Panel ID="SelectContainerPanel" runat="server" CssClass="dnnLeft">
        <asp:DropDownList ID="ddlContainers" runat="server" AutoPostBack="true" onselectedindexchanged="ddlContainers_SelectedIndexChanged" CausesValidation="false">
            <asp:ListItem value="SelectContainer" resourcekey="SelectContainer" />
            <asp:ListItem value="Refresh" resourcekey="RefreshContainerList" />
        </asp:DropDownList>
        <asp:LinkButton ID="btnNewContainer" runat="server" CssClass="dnnSecondaryAction" Text="New Container" resourcekey="NewContainer" onclick="btnNewContainer_Click" CausesValidation="false" />
    </asp:Panel>
    <asp:Panel ID="CreateContainerPanel" runat="server" Visible="false" CssClass="dnnLeft">
        <asp:TextBox ID="tbContainerName" runat="server" CssClass="dnnFormRequired" />
        <asp:LinkButton ID="btnSelectExistingContainer" runat="server" CssClass="dnnSecondaryAction" resourcekey="SelectExistingContainer" OnClick="btnSelectExistingContainer_Click" CausesValidation="false" />
    </asp:Panel>
    <asp:CustomValidator ID="valContainerName" runat="server" Display="Dynamic" CssClass="dnnFormMessage dnnFormError" onservervalidate="valContainerName_ServerValidate" />
</div>
<div class="dnnFormItem">
    <dnn:Label ID="plUseHttps" runat="server" />
    <asp:CheckBox ID="chkUseHttps" runat="server" />
</div>
<div class="dnnFormItem">
    <dnn:Label ID="plDirectLink" runat="server" ControlName="chkDirectLink" />
    <asp:CheckBox runat="server" ID="chkDirectLink" Checked="True"/>
</div>
<div class="dnnFormItem">
    <dnn:Label ID="plCustomDomain" runat="server" />
    <asp:TextBox ID="tbCustomDomain" runat="server" MaxLength="200" Width="200" />
    <asp:RegularExpressionValidator ID="rgBlobEndpoint" CssClass="dnnFormMessage dnnFormError"
            runat="server" ControlToValidate="tbCustomDomain" Display="Dynamic" ResourceKey="tbCustomDomain.Error" ValidationExpression="^http[s]?://([a-zA-Z0-9\-]+\.)*([a-zA-Z]{3,61}|[a-zA-Z]{1,}\.[a-zA-Z]{2,3})$"></asp:RegularExpressionValidator>
</div>
<div class="dnnFormItem">
    <dnn:Label ID="plSyncBatchSize" runat="server" />
    <asp:TextBox ID="tbSyncBatchSize" runat="server" MaxLength="6" Width="200" />
    <asp:RegularExpressionValidator ID="rgInteger" CssClass="dnnFormMessage dnnFormError"
            runat="server" ControlToValidate="tbSyncBatchSize" Display="Dynamic" ResourceKey="tbSyncBatchSize.Error" ValidationExpression="^\d+$"></asp:RegularExpressionValidator>
</div>