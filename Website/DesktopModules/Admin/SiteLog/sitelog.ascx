<%@ Control Inherits="DotNetNuke.Modules.Admin.SiteLog.SiteLog" Language="C#" AutoEventWireup="false" CodeFile="SiteLog.ascx.cs" %>
<%@ Register TagPrefix="dnn" TagName="Label" Src="~/controls/LabelControl.ascx" %>
<%@ Register TagPrefix="dnn" Namespace="DotNetNuke.Web.UI.WebControls" Assembly="DotNetNuke.Web" %>
<div class="dnnForm dnnSiteLog dnnClear" id="dnnSiteLog">
    <div class="slContent dnnClear">
        <fieldset>
            <div class="dnnFormItem dnnClear"><asp:Label ID="lblMessage" runat="server" Visible="false" resourcekey="NoRecords" CssClass="dnnFormMessage dnnFormWarning" /></div>
            <div class="dnnFormItem">
                <dnn:label id="plReportType" runat="server" controlname="cboReportType" suffix=":" />
                <%--<asp:DropDownList ID="cboReportType" Runat="server" DataValueField="value" DataTextField="text" />--%>
                <dnn:DnnComboBox ID="cboReportType" Runat="server" DataValueField="value" DataTextField="text" />
            </div>
            <div class="dnnFormItem">
                <dnn:label id="plStartDate" runat="server" controlname="txtStartDate" suffix=":" />
                <dnn:dnndatepicker ID="diStartDate" runat="server" />
                <asp:comparevalidator id="valStartDate" cssclass="NormalRed" runat="server" resourcekey="valStartDate" display="Dynamic" type="Date" operator="DataTypeCheck" controltovalidate="diStartDate" />

            </div>
            <div class="dnnFormItem">
                <dnn:label id="plEndDate" runat="server" controlname="txtEndDate" suffix=":" />
                <dnn:dnndatepicker ID="diEndDate" runat="server" />
                <asp:comparevalidator id="valEndDate" cssclass="NormalRed" runat="server" resourcekey="valEndDate" display="Dynamic" type="Date" operator="DataTypeCheck" controltovalidate="diEndDate" />

            </div>
            <div class="dnnFormItem">
                <asp:comparevalidator id="valDates" cssclass="NormalRed" runat="server" resourcekey="valDates" display="Dynamic" type="Date" operator="GreaterThan" controltovalidate="diEndDate" controltocompare="diStartDate" />
            </div>
            <ul class="dnnActions dnnClear">
                <li><asp:LinkButton id="cmdDisplay" resourcekey="cmdDisplay" cssclass="dnnPrimaryAction" Text="Display" runat="server"  /></li>
            </ul>
        </fieldset>        
       <%-- <asp:datagrid id="grdLog" Runat="server" cellpadding="4" Border="0" AutoGenerateColumns="true" BorderStyle="None" GridLines="None">
        	    <headerstyle cssclass="dnnGridHeader" verticalalign="Top"/>
	            <footerstyle cssclass="dnnGridFooter" />
	            <pagerstyle cssclass="dnnGridPager" />
        </asp:datagrid>--%>
        <dnn:DnnGrid ID="grdLog" runat="server">
        </dnn:DnnGrid>
    </div>
</div>