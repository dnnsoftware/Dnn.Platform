<%@ Control Language="C#" AutoEventWireup="false" Inherits="DesktopModules.Admin.Console.Settings" CodeFile="Settings.ascx.cs" %>
<%@ Register TagPrefix="dnn" TagName="Label" Src="~/controls/LabelControl.ascx" %>
<%@ Register TagPrefix="dnn" Namespace="DotNetNuke.Web.UI.WebControls" Assembly="DotNetNuke.Web" %>
<div class="dnnForm dnnConsole dnnClear">
	<div class="dnnFormItem">
		<dnn:label id="Label1" runat="server" ControlName="ParentTab" ResourceKey="Mode" Suffix=":" />
	<%--	<asp:DropDownList ID="modeList" runat="server" AutoPostBack="True">
		    <asp:ListItem Value="Normal" ResourceKey="Normal" />
		    <asp:ListItem Value="Profile" ResourceKey="Profile" />
		    <asp:ListItem Value="Group" ResourceKey="Group" />
		</asp:DropDownList>--%>
        <dnn:DnnComboBox ID="modeList" runat="server" AutoPostBack="true">
            <Items>
                <dnn:DnnComboBoxItem Value="Normal" Text="Normal" ResourceKey="Normal" />
                <dnn:DnnComboBoxItem Value="Profile" Text="Profile" ResourceKey="Profile" />
                <dnn:DnnComboBoxItem Value="Group" Text="Group" ResourceKey="Group" />
            </Items>
        </dnn:DnnComboBox>
	</div>
	<div id="parentTabRow" runat="server" class="dnnFormItem">
		<dnn:label id="lblParentTab" runat="server" ControlName="ParentTab" ResourceKey="ParentTab" Suffix=":" />
		<%--<asp:DropDownList ID="ParentTab" runat="server" AutoPostBack="True" />--%>
        <dnn:DnnComboBox ID="ParentTab" runat="server" AutoPostBack="true" />
	</div>
	<div id="includeParentRow" runat="server" class="dnnFormItem">
		<dnn:label id="lblIncludeParent" runat="server" ControlName="IncludeParent" ResourceKey="IncludeParent" Suffix=":" />
		<asp:Checkbox ID="IncludeParent" runat="server" />
	</div>
    <div id="tabVisibilityRow" runat="server" class="dnnFormItem">
		<dnn:label id="tabsLabel" runat="server" ControlName="groupTabs" /><br/>
        <div class="dnnLeft tabVisibilityRow">
        <asp:Repeater ID="tabs" runat="server">
            <ItemTemplate>
	            <div class="tabVisibilityRowItem">
		            <asp:label id="tabLabel" runat="server" CssClass="dnnFormLabel"></asp:label>
	                <asp:HiddenField id="tabPath" runat="server"></asp:HiddenField>
	             <%--   <asp:DropDownList ID="tabVisibility" runat="server" />--%>
                    <dnn:DnnComboBox ID="tabVisibility" runat="server" />
	            </div>
            </ItemTemplate>
        </asp:Repeater>
        </div>
    </div>
	<div class="dnnFormItem">
		<dnn:label id="lblDefaultSize" runat="server" ControlName="DefaultSize" ResourceKey="DefaultSize" Suffix=":" />
		<%--<asp:DropDownList ID="DefaultSize" runat="server" />--%>
        <dnn:DnnComboBox ID="DefaultSize" runat="server" />
	</div>
	<div class="dnnFormItem">
		<dnn:label id="lblAllowResize" runat="server" ControlName="AllowResize" ResourceKey="AllowResize" Suffix=":" />
		 <asp:Checkbox ID="AllowResize" runat="server" Checked="true" />
	</div>
	<div class="dnnFormItem">
		<dnn:label id="lblDefaultView" runat="server" ControlName="DefaultView" ResourceKey="DefaultView" Suffix=":" />
		<%--<asp:DropDownList ID="DefaultView" runat="server" />--%>
        <dnn:DnnComboBox ID="DefaultView" runat="server" />
	</div>
	<div class="dnnFormItem">
		<dnn:label id="lblAllowViewChange" runat="server" ControlName="AllowViewChange" ResourceKey="AllowViewChange" Suffix=":" />
		<asp:Checkbox ID="AllowViewChange" runat="server" Checked="true" />
	</div>
	<div class="dnnFormItem">
		<dnn:label id="lblShowTooltip" runat="server" ControlName="ShowTooltip" ResourceKey="ShowTooltip" Suffix=":" />
		<asp:Checkbox ID="ShowTooltip" runat="server" Checked="true" />
	</div>
	<div class="dnnFormItem">
		<dnn:label id="lblOrderTabsByHierarchy" runat="server" ControlName="OrderTabsByHierarchy" ResourceKey="OrderTabsByHierarchy" Suffix=":" />
		<asp:Checkbox ID="OrderTabsByHierarchy" runat="server" />
	</div>
	<div class="dnnFormItem">
		<dnn:label id="lblConsoleWidth" runat="server" ControlName="ConsoleWidth" ResourceKey="ConsoleWidth" Suffix=":" />
		<asp:TextBox ID="ConsoleWidth" runat="server" Text="" />
	</div>
</div>