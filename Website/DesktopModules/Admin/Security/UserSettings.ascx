<%@ Control Language="C#" AutoEventWireup="false" Inherits="DesktopModules.Admin.Security.UserSettings" CodeFile="UserSettings.ascx.cs" %>
<%@ Register TagPrefix="dnn" Namespace="DotNetNuke.Web.UI.WebControls" Assembly="DotNetNuke.Web" %>
<asp:Panel runat="server" ID="CannotChangeSettingsMessage" CssClass="dnnFormMessage dnnFormWarning" Visible="False"><%=LocalizeString("CannotChangeSettings") %></asp:Panel>
<div class="dnnForm dnnUserSettings dnnClear" id="dnnUserSettings" runat="server" ClientIDMode="Static">
    <div class="usUserAccounts" id="usUserAccounts">
        <fieldset>
            <dnn:DnnFormEditor ID="settingsEditor" runat="server" FormMode="Short">
                <Items>
                    <dnn:DnnFormToggleButtonItem runat="server" DataField="Column_FirstName"/>                    
                    <dnn:DnnFormToggleButtonItem runat="server" DataField="Column_LastName"/>                    
                    <dnn:DnnFormToggleButtonItem runat="server" DataField="Column_DisplayName"/>                    
                    <dnn:DnnFormToggleButtonItem runat="server" DataField="Column_Email"/>                    
                    <dnn:DnnFormToggleButtonItem runat="server" DataField="Column_Address"/>                    
                    <dnn:DnnFormToggleButtonItem runat="server" DataField="Column_Telephone"/>                    
                    <dnn:DnnFormToggleButtonItem runat="server" DataField="Column_Authorized"/>                    
                    <dnn:DnnFormToggleButtonItem runat="server" DataField="Column_CreatedDate"/>                    
                    <dnn:DnnFormToggleButtonItem runat="server" DataField="Column_LastLogin"/>
                    <dnn:DnnFormEnumItem ID="displayMode" runat="server" DataField="Display_Mode" />                    
                    <dnn:DnnFormToggleButtonItem id="manageServiceItem" runat="server" DataField="Profile_ManageServices"/>
                    <dnn:DnnFormNumericTextBoxItem runat="server" DataField="Records_PerPage"/>
                    <dnn:DnnFormEnumItem ID="usersControl" runat="server" DataField="Security_UsersControl"/>                    
                </Items>
            </dnn:DnnFormEditor>
        </fieldset>
    </div>
</div>