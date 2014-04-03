<%@ Control Language="C#" AutoEventWireup="false" Inherits="DotNetNuke.Modules.Admin.Scheduler.ViewScheduleStatus" CodeFile="ViewScheduleStatus.ascx.cs" %>
<%@ Register TagPrefix="dnn" Assembly="DotNetNuke" Namespace="DotNetNuke.UI.WebControls"%>
<%@ Register TagPrefix="dnnweb" Assembly="DotNetNuke.Web" Namespace="DotNetNuke.Web.UI.WebControls" %>
<%@ Register TagPrefix="dnn" Namespace="Telerik.Web.UI" %>
<%@ Register TagPrefix="dnn" TagName="Label" Src="~/controls/LabelControl.ascx" %>
<div class="dnnForm dnnScheduleStatus dnnClear" id="dnnScheduleStatus">
    <div class="dnnFormItem">
        <%--<asp:Label ID="lblStatusLabel" resourcekey="lblStatusLabel" runat="server" CssClass="dnnFormLabel" />--%>
        <dnn:Label ID="lblStatusLabel" resourcekey="lblStatusLabel" runat="server"/>
        <asp:Label ID="lblStatus" runat="server" />
    </div>
    <div class="dnnFormItem">
        <%--<asp:Label ID="lblMaxThreadsLabel" resourcekey="lblMaxThreadsLabel" runat="server" CssClass="dnnFormLabel" />--%>
        <dnn:Label ID="lblMaxThreadsLabel" resourcekey="lblMaxThreadsLabel" runat="server" />
        <asp:Label ID="lblMaxThreads" runat="server" />
    </div>
     <div class="dnnFormItem">
            <%--<asp:Label ID="lblActiveThreadsLabel" resourcekey="lblActiveThreadsLabel" runat="server" CssClass="dnnFormLabel" />--%>
         <dnn:Label ID="lblActiveThreadsLabel" resourcekey="lblActiveThreadsLabel" runat="server" />
         <asp:Label ID="lblActiveThreads" runat="server" />
    </div>
     <div class="dnnFormItem">
        <%--<asp:Label ID="lblFreeThreadsLabel" resourcekey="lblFreeThreadsLabel" runat="server" CssClass="dnnFormLabel" />--%>
        <dnn:Label ID="lblFreeThreadsLabel" resourcekey="lblFreeThreadsLabel" runat="server"/>
        <asp:Label ID="lblFreeThreads" runat="server" />
    </div>
    <asp:PlaceHolder ID="placeCommands" runat="server">
        <div class="dnnFormItem">
            <%--<asp:Label ID="lblCommand" resourcekey="lblCommand" runat="server" CssClass="dnnFormLabel" />--%>
            <dnn:label ID="lblCommand" resourcekey="lblCommand" runat="server" />
            <asp:LinkButton ID="cmdStart" resourcekey="cmdStart" CssClass="dnnSecondaryAction" runat="server" />
            <asp:LinkButton ID="cmdStop" resourcekey="cmdStop" CssClass="dnnSecondaryAction" runat="server" />
        </div>
    </asp:PlaceHolder>
	<asp:Panel ID="pnlScheduleProcessing" runat="server" CssClass="dnnScheduleProcessing">
		<h2><asp:Label ID="lblProcessing" runat="server" resourcekey="lblProcessing" /></h2>
		<dnnweb:DnnGrid id="dgScheduleProcessing" runat="server" AutoGenerateColumns="false" AllowSorting="false" CssClass="dnnScheduleHistoryGrid">
			<MasterTableView DataKeyNames="ScheduleID">
				<Columns>
					<dnnweb:DnnGridBoundColumn DataField="ScheduleID" HeaderText="ServerScheduleID" />
					<dnnweb:DnnGridBoundColumn DataField="TypeFullName" HeaderText="Type" />
					<dnnweb:DnnGridBoundColumn DataField="StartDate" HeaderText="Started" />
					<dnnweb:DnnGridBoundColumn DataField="ElapsedTime" HeaderText="Duration" />
					<dnnweb:DnnGridBoundColumn DataField="ObjectDependencies" HeaderText="ObjectDependencies" />
					<dnnweb:DnnGridBoundColumn DataField="ScheduleSource" HeaderText="TriggeredBy" />
					<dnnweb:DnnGridBoundColumn DataField="ThreadID" HeaderText="Thread" />
					<dnnweb:DnnGridTemplateColumn UniqueName="Servers" HeaderText="Servers">
						<ItemTemplate>
							<%# DataBinder.Eval(Container.DataItem,"Servers") %>
					</ItemTemplate>
					</dnnweb:DnnGridTemplateColumn>
                    <dnnweb:DnnGridTemplateColumn>
                            <ItemTemplate>
                                <asp:LinkButton id="cmdStopSchedule" CommandArgument='<%# DataBinder.Eval(Container.DataItem, "ScheduleID").ToString() %>' CommandName="ScheduleID" runat="server" CssClass="dnnSecondaryAction" ResourceKey="cmdStop" OnClick="CmdStopScheduleClick"  />
                            </ItemTemplate>
                    </dnnweb:DnnGridTemplateColumn>
				</Columns>
			</MasterTableView>
		</dnnweb:DnnGrid>
	</asp:Panel>
	<asp:Panel ID="pnlScheduleQueue" runat="server" CssClass="dnnScheduleQueue">
		<h2><asp:Label ID="lblQueue" runat="server" resourcekey="lblQueue" /></h2>
		<dnnweb:DnnGrid id="dgScheduleQueue" runat="server" AutoGenerateColumns="false" AllowSorting="false" CssClass="dnnScheduleHistoryGrid">
			<MasterTableView DataKeyNames="ScheduleID,OverdueBy">
				<Columns>
					<dnnweb:DnnGridBoundColumn DataField="ScheduleID" HeaderText="ScheduleID" />
					<dnnweb:DnnGridBoundColumn DataField="FriendlyName" HeaderText="Name" />
					<dnnweb:DnnGridBoundColumn DataField="NextStart" HeaderText="NextStart" />
					<dnnweb:DnnGridTemplateColumn UniqueName="Overdue" HeaderText="Overdue">
						<ItemTemplate>
							<asp:Label id="lblOverdue" runat="server" />
						</ItemTemplate>
					</dnnweb:DnnGridTemplateColumn>
					<dnnweb:DnnGridBoundColumn DataField="RemainingTime" HeaderText="TimeRemaining" />
					<dnnweb:DnnGridBoundColumn DataField="ObjectDependencies" HeaderText="ObjectDependencies" />
					<dnnweb:DnnGridBoundColumn DataField="ScheduleSource" HeaderText="TriggeredBy" />
					<dnnweb:DnnGridBoundColumn DataField="ThreadID" HeaderText="Thread" />
					<dnnweb:DnnGridTemplateColumn UniqueName="Servers" HeaderText="Servers">
						<ItemTemplate>
							<%# DataBinder.Eval(Container.DataItem,"Servers") %>
						</ItemTemplate>
					</dnnweb:DnnGridTemplateColumn>
				</Columns>
			</MasterTableView>
		</dnnweb:DnnGrid>
	</asp:Panel>
	<ul class="dnnActions dnnClear">
		<li><asp:HyperLink id="cmdCancel" runat="server" CssClass="dnnPrimaryAction" resourcekey="cmdReturn" /></li>
	</ul>
</div>