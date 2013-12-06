<%@ Control Language="C#" AutoEventWireup="false" Inherits="DotNetNuke.Modules.Admin.Security.SecurityRoles" CodeFile="SecurityRoles.ascx.cs" %>
<%@ Register TagPrefix="dnn" TagName="Label" Src="~/controls/LabelControl.ascx" %>
<%@ Register TagPrefix="dnn" Assembly="DotNetNuke" Namespace="DotNetNuke.UI.WebControls"%>
<%@ Register TagPrefix="dnn" Assembly="DotNetNuke.Web" Namespace="DotNetNuke.Web.UI.WebControls" %>
<div class="dnnForm dnnManageSecurityRoles">
    <asp:Panel ID="pnlRoles" runat="server" Visible="True">
        <h2 class="dnnFormSectionHead"><asp:Label ID="lblTitle" runat="server" /></h2>
             <div class="dnnFormItem">
        <table cellspacing="0" cellpadding="0" border="0" class="dnnSecurityRoles dnnClear">
            <tr>
                <td valign="top" width="250">                  
                    <div class="dnnFormItem" style="z-index: 10;">
                    <dnn:Label ID="plRoles" runat="server" />  
                    <dnn:Label ID="plUsers" runat="server" /> 
                    </div>               
                </td>
                <td width="30"></td>
                <td valign="top" width="100">
                    <dnn:Label ID="plEffectiveDate" runat="server"/>
                 </td>
                <td width="30"></td>
                <td valign="top" width="100">
                    <dnn:Label ID="plExpiryDate" runat="server" />
                </td>
                
                <asp:Placeholder runat="server" ID="placeIsOwnerHeader" Visible="false">
                    <td width="30"></td>
                    <td valign="top" width="150"><dnn:Label ID="lblIsOwner" runat="server" /></td>
                </asp:Placeholder>

                <td width="30"></td>
                <td valign="top" width="200"></td>
            </tr>
            <tr>
                <td valign="top" width="220">
                    <asp:TextBox ID="txtUsers" runat="server" Width="150" />
                    <asp:LinkButton ID="cmdValidate" runat="server" CssClass="dnnSecondaryAction" resourceKey="cmdValidate" />
                    <dnn:DnnComboBox ID="cboUsers" runat="server" AutoPostBack="True" />
                    <dnn:DnnComboBox ID="cboRoles" runat="server" AutoPostBack="True" DataValueField="RoleID" DataTextField="RoleName" />
                </td>
                <td width="30"></td>
                <td valign="top" width="100" nowrap="nowrap">
                    <dnn:DnnDatePicker ID="effectiveDatePicker" runat="server"/>
                </td>
                <td width="30"></td>
                <td valign="top" width="100" nowrap="nowrap">
                    <dnn:DnnDatePicker ID="expiryDatePicker" runat="server"/>
                </td>
                <asp:PlaceHolder runat="server" ID="placeIsOwner" Visible="false">
                    <td width="30"></td>
                    <td valign="top" width="150" nowrap="nowrap" align="right"><asp:CheckBox runat="server" ID="chkIsOwner"/></td>
                </asp:PlaceHolder>
                <td width="30"></td>
                <td valign="top" width="" nowrap="nowrap">
                    <asp:LinkButton ID="cmdAdd" CssClass="dnnPrimaryAction" runat="server"  CausesValidation="true" ValidationGroup="SecurityRole" />
                    <asp:CheckBox ID="chkNotify" resourcekey="SendNotification" runat="server" Checked="True" />
                </td>
            </tr>
        </table>
        <asp:CompareValidator ID="valEffectiveDate" CssClass="dnnFormError" runat="server" resourcekey="valEffectiveDate" Display="Dynamic" Type="Date" Operator="DataTypeCheck" ControlToValidate="effectiveDatePicker" ValidationGroup="SecurityRole" />
        <asp:CompareValidator ID="valExpiryDate" CssClass="dnnFormError" runat="server" resourcekey="valExpiryDate" Display="Dynamic" Type="Date" Operator="DataTypeCheck" ControlToValidate="expiryDatePicker" ValidationGroup="SecurityRole" />
        <asp:CompareValidator ID="valDates" CssClass="dnnFormError" runat="server" resourcekey="valDates" Display="Dynamic" Type="Date" Operator="GreaterThan" ControlToValidate="expiryDatePicker" ControlToCompare="effectiveDatePicker" ValidationGroup="SecurityRole" />
         </div>
    </asp:Panel>
    <asp:Panel ID="pnlUserRoles" runat="server" CssClass="WorkPanel" Visible="True">
        <asp:DataGrid ID="grdUserRoles" runat="server" Width="100%" GridLines="None" DataKeyField="UserRoleID" EnableViewState="false" AutoGenerateColumns="false" CellSpacing="0" CellPadding="0" CssClass="dnnGrid">
            <headerstyle cssclass="dnnGridHeader" verticalalign="Top"/>
            <itemstyle cssclass="dnnGridItem" horizontalalign="Left" />
            <alternatingitemstyle cssclass="dnnGridAltItem" />
            <edititemstyle cssclass="dnnFormInput" />
            <selecteditemstyle cssclass="dnnFormError" />
            <footerstyle cssclass="dnnGridFooter" />
            <pagerstyle cssclass="dnnGridPager" />
            <Columns>
                <asp:TemplateColumn>
                    <ItemTemplate>
                        <!-- [DNN-4285] Hide the button if the user cannot be removed from the role -->
                        <dnn:DnnImageButton ID="cmdDeleteUserRole" runat="server" AlternateText="Delete" CausesValidation="False" CommandName="Delete" IconKey="Delete" resourcekey="cmdDelete"  Visible='<%# DeleteButtonVisible(Convert.ToInt32(Eval("UserID")), Convert.ToInt32(Eval("RoleID")))  %>' OnClick="cmdDeleteUserRole_click">
                        </dnn:DnnImageButton>
                    </ItemTemplate>
                </asp:TemplateColumn>
                <asp:TemplateColumn HeaderText="UserName">
                    <ItemTemplate>
                         <a href='<%# DotNetNuke.Common.Globals.LinkClick("userid=" + Eval("UserID").ToString(), TabId, ModuleId) %>' class=""> <%# Eval("FullName").ToString()%> </a>
                    </ItemTemplate>
                </asp:TemplateColumn>
                <asp:BoundColumn DataField="RoleName" HeaderText="SecurityRole" />
                <asp:TemplateColumn HeaderText="EffectiveDate">
                    <ItemTemplate>
                         <%#FormatDate(Convert.ToDateTime(Eval("EffectiveDate"))) %>
                    </ItemTemplate>
                </asp:TemplateColumn>
                <asp:TemplateColumn HeaderText="ExpiryDate">
                    <ItemTemplate>
                         <%#FormatDate(Convert.ToDateTime(Eval("ExpiryDate"))) %>
                         </ItemTemplate>
                </asp:TemplateColumn>
                <asp:TemplateColumn HeaderText="IsOwner">
                    <ItemTemplate>
                        <dnn:DnnImage Runat="server" ID="imgApproved" IconKey="Checked" Visible='<%# (bool)DataBinder.Eval(Container.DataItem,"IsOwner") %>' />
                    </ItemTemplate>
                </asp:TemplateColumn>
            </Columns>
        </asp:DataGrid>
        <dnn:pagingcontrol id="ctlPagingControl" runat="server"></dnn:pagingcontrol>
        
    </asp:Panel>
    <ul id="actionsRow" runat="server" class="dnnActions dnnClear">
        <li><asp:HyperLink id="cmdCancel" runat="server" CssClass="dnnPrimaryAction" resourcekey="Close" /></li>
    </ul>
</div>