<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="View.ascx.cs" Inherits="Dnn.Modules.TelerikRemoval.View" %>
<%@ Register Assembly="DotNetNuke.Web" Namespace="DotNetNuke.Web.UI.WebControls" TagPrefix="cc1" %>
<%@ Register Assembly="Dnn.Modules.TelerikRemoval" Namespace="Dnn.Modules.TelerikRemoval.UserControls" TagPrefix="cc2" %>

<asp:UpdatePanel ID="MainUpdatePanel" runat="server">
    <ContentTemplate>
        <div id="telerikRemoval">
            <div class="loading"></div>
            <asp:MultiView ID="MainMultiView" runat="server">
                <asp:View ID="RegularUserView" runat="server">
                    <h1>
                        <cc1:DnnLabel ID="SuperUserRequiredHeadingLabel" runat="server" CssClass="dnnFormLabel">SuperUserRequiredHeading</cc1:DnnLabel>
                    </h1>
                    <p>
                        <cc1:DnnLabel ID="SuperUserRequiredInfoLabel" runat="server" CssClass="dnnFormLabel">SuperUserRequiredInfo</cc1:DnnLabel>
                    </p>
                </asp:View>
                <asp:View ID="NotInstalledView" runat="server">
                    <h1>
                        <cc1:DnnLabel ID="TelerikNotInstalledHeadingLabel" runat="server" CssClass="dnnFormLabel">TelerikNotInstalledHeading</cc1:DnnLabel>
                    </h1>
                    <p>
                        <cc1:DnnLabel ID="TelerikNotInstalledInfoLabel" runat="server" CssClass="dnnFormLabel">TelerikNotInstalledInfo</cc1:DnnLabel>
                    </p>
                </asp:View>
                <asp:View ID="InstalledView" runat="server">
                    <h1>
                        <cc1:DnnLabel ID="TelerikInstalledButNotUsedHeadingLabel" runat="server" CssClass="dnnFormLabel">TelerikInstalledHeading</cc1:DnnLabel>
                    </h1>
                    <p>
                        <cc1:DnnLabel ID="TelerikInstalledDetectedLabel" runat="server" CssClass="dnnFormLabel">TelerikInstalledDetected</cc1:DnnLabel>
                        <cc1:DnnLabel ID="TelerikInstalledVersionLabel" runat="server" CssClass="dnnFormLabel telerikVersion" Localize="False"></cc1:DnnLabel>
                    </p>
                    <p>
                        <cc1:DnnLabel ID="TelerikInstalledButNotUsedBulletinLabel" runat="server" CssClass="dnnFormLabel">TelerikInstalledBulletin</cc1:DnnLabel>
                    </p>
                    <asp:MultiView ID="InstalledMultiView" runat="server">
                        <asp:View ID="InstalledButNotUsedView" runat="server">
                            <p>
                                <cc1:DnnLabel ID="TelerikInstalledButNotUsedInfoLabel" runat="server" CssClass="dnnFormLabel">TelerikInstalledButNotUsedInfo</cc1:DnnLabel>
                            </p>
                            <p>
                                <cc2:DnnCheckBox ID="BackupConfirmationCheckBox" runat="server" AutoPostBack="True" OnCheckedChanged="BackupConfirmationCheckBox_CheckedChanged" Text="BackupConfirmation" />
                            </p>
                            <div>
                                <cc2:DnnTextButton ID="RemoveTelerikButton" runat="server" CssClass="dnnPrimaryAction" DisabledCssClass="dnnPrimaryAction dnnDisabledAction" OnClick="RemoveTelerikButton_Click" Text="RemoveTelerik" OnClientClick="this.disabled = true;" UseSubmitBehavior="False" />
                            </div>
                        </asp:View>
                        <asp:View ID="InstalledAndUsedView" runat="server">
                            <p>
                                <cc1:DnnLabel ID="TelerikInstalledAndUsedInfoLabel" runat="server" CssClass="dnnFormLabel"></cc1:DnnLabel>
                            </p>
                            <div>
                                <asp:PlaceHolder ID="AssemblyListPlaceHolder" runat="server" />
                            </div>
                            <p>
                                <cc1:DnnLabel ID="TelerikInstalledAndUsedWarningLabel" runat="server" CssClass="dnnFormLabel"></cc1:DnnLabel>
                            </p>
                        </asp:View>
                    </asp:MultiView>
                </asp:View>
                <asp:View ID="UninstallReportView" runat="server">
                    <h1>
                        <cc1:DnnLabel ID="UninstallReportHeadingLabel" runat="server" CssClass="dnnFormLabel">UninstallReportHeading</cc1:DnnLabel>
                    </h1>
                    <p>
                        <cc1:DnnLabel ID="UninstallReportDetailLabel" runat="server" CssClass="dnnFormLabel" Localize="False"></cc1:DnnLabel>
                    </p>
                    <div>
                        <table id="uninstallReport">
                            <thead>
                                <tr>
                                    <th>
                                        <cc1:DnnLabel ID="UninstallReportStepColumnLabel" runat="server" CssClass="dnnFormLabel">UninstallReportStepColumn</cc1:DnnLabel></th>
                                    <th>
                                        <cc1:DnnLabel ID="UninstallReportResultColumnLabel" runat="server" CssClass="dnnFormLabel">UninstallReportResultColumn</cc1:DnnLabel></th>
                                    <th>
                                        <cc1:DnnLabel ID="UninstallReportNotesColumnLabel" runat="server" CssClass="dnnFormLabel">UninstallReportNotesColumn</cc1:DnnLabel></th>
                                </tr>
                            </thead>
                            <tbody>
                                <asp:Repeater ID="UninstallReportRepeater" runat="server">
                                    <ItemTemplate>
                                        <tr>
                                            <td><%# Eval("StepName") %></td>
                                            <td><%# this.ConvertBooleanToIcon(Eval("Success")) %></td>
                                            <td><%# Eval("Notes") %></td>
                                        </tr>
                                    </ItemTemplate>
                                </asp:Repeater>
                            </tbody>
                        </table>
                    </div>
                </asp:View>
            </asp:MultiView>
        </div>
    </ContentTemplate>
</asp:UpdatePanel>
