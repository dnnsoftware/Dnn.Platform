<%@ Control Language="C#" AutoEventWireup="false" Inherits="DotNetNuke.Modules.Admin.Scheduler.ViewScheduleHistory" CodeFile="ViewScheduleHistory.ascx.cs" %>
<%@ Register TagPrefix="dnn" Assembly="DotNetNuke.Web" Namespace="DotNetNuke.Web.UI.WebControls" %>
<%@ Register TagPrefix="dnn" Namespace="Telerik.Web.UI" %>
<div class="dnnForm dnnScheduleHistory dnnClear" id="dnnScheduleHistory">
    <dnn:DnnGrid id="dgHistory" runat="server" AutoGenerateColumns="false" AllowSorting="true" CssClass="dnnGrid">
        <MasterTableView DataKeyNames="FriendlyName,LogNotes,StartDate,EndDate,NextStart">
            <Columns>
                <dnn:DnnGridTemplateColumn UniqueName="Description" HeaderText="Description">
                    <ItemTemplate>
                        <asp:Literal ID="litName" runat="server" Text='<%#DataBinder.Eval(Container.DataItem, "FriendlyName")%>' />
                        <asp:Literal ID="litNotes" runat="server" Visible='<%# DataBinder.Eval(Container.DataItem,"LogNotes").ToString() != ""%>' Text='<%# DataBinder.Eval(Container.DataItem,"LogNotes")%>' />
                    </ItemTemplate>
                </dnn:DnnGridTemplateColumn>
                <dnn:DnnGridBoundColumn DataField="Server" HeaderText="Server" />
                <dnn:DnnGridBoundColumn DataField="ElapsedTime" HeaderText="Duration" />
                <dnn:DnnGridCheckBoxColumn DataField="Succeeded" HeaderText="Succeeded" />
                <dnn:DnnGridTemplateColumn UniqueName="Start" HeaderText="Start">
                    <ItemTemplate>
                        <ul>
                            <li>S:&nbsp;<asp:Literal ID="litStartDate" runat="server" Text='<%# DataBinder.Eval(Container.DataItem,"StartDate")%>' /></li>
                            <li>E:&nbsp;<asp:Literal ID="litEndDate" runat="server" Text='<%# DataBinder.Eval(Container.DataItem,"EndDate")%>' /></li>
                            <li>N:&nbsp;<asp:Literal ID="litNextStart" runat="server" Text='<%# DataBinder.Eval(Container.DataItem,"NextStart")%>' /></li>
                        </ul>
                    </ItemTemplate>
                </dnn:DnnGridTemplateColumn>
            </Columns>
        </MasterTableView>
    </dnn:DnnGrid>
    <ul class="dnnActions dnnClear">
        <li><asp:HyperLink id="cmdCancel" runat="server" CssClass="dnnPrimaryAction" resourcekey="cmdReturn" /></li>
    </ul>
</div>