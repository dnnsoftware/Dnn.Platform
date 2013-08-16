<%@ Control Language="C#" AutoEventWireup="false" Inherits="DotNetNuke.Modules.Admin.Security.MemberServices" CodeFile="MemberServices.ascx.cs" %>
<%@ Register TagPrefix="dnn" TagName="Label" Src="~/controls/LabelControl.ascx" %>
<%@ Register TagPrefix="dnn" Namespace="DotNetNuke.UI.WebControls" Assembly="DotNetNuke" %>
<div class="dnnForm dnnServices dnnClear" id="dnnServices">
    <fieldset>
        <div class="dnnFormItem">
            <asp:Label ID="lblServicesHelp" runat="server" resourcekey="ServicesHelp" />
        </div>
        <div id="ServicesRow" runat="server" class="dnnFormItem">
 			<asp:datagrid id="grdServices" runat="server" GridLines="None" enableviewstate="true" autogeneratecolumns="false" cellpadding="4" border="0" summary="Register Design Table">
	            <headerstyle cssclass="dnnGridHeader" verticalalign="Top" horizontalalign="Center" />
	            <itemstyle cssclass="dnnGridItem" horizontalalign="Left" />
	            <alternatingitemstyle cssclass="dnnGridAltItem" />
	            <edititemstyle cssclass="dnnFormInput" />
	            <selecteditemstyle cssclass="dnnFormError" />
	            <footerstyle cssclass="dnnGridFooter" />
	            <pagerstyle cssclass="dnnGridPager" />
				<columns>
					<asp:templatecolumn>
						<itemtemplate>
							<asp:LinkButton 
							    text='<%# ServiceText((bool)DataBinder.Eval(Container.DataItem,"Subscribed"), (DateTime)DataBinder.Eval(Container.DataItem, "ExpiryDate")) %>'
							    CommandName='<%# ServiceText((bool)DataBinder.Eval(Container.DataItem,"Subscribed"), (DateTime)DataBinder.Eval(Container.DataItem, "ExpiryDate")) %>'
							    CommandArgument = '<%# DataBinder.Eval(Container.DataItem,"RoleID") %>'
							    cssclass="dnnSecondaryAction" 
							    runat="server" 
							    visible =  '<%# ShowSubscribe((int)DataBinder.Eval(Container.DataItem,"RoleID")) %>'
							    id="lnkSubscribe" />
						</itemtemplate>
					</asp:templatecolumn>
					<asp:templatecolumn>
						<itemtemplate>
							<asp:LinkButton 
							    CommandName="UseTrial"
							    CommandArgument = '<%# DataBinder.Eval(Container.DataItem,"RoleID") %>'
							    cssclass="dnnSecondaryAction" 
							    runat="server"
							    resourcekey = "UseTrial"
							    visible =  '<%# ShowTrial((int)DataBinder.Eval(Container.DataItem,"RoleID")) %>'
							    id="lnkTrial" />
						</itemtemplate>
					</asp:templatecolumn>
					<asp:boundcolumn headertext="Name" datafield="RoleName" />
					<asp:boundcolumn headertext="Description" datafield="Description" />
					<asp:templatecolumn headertext="Fee">
						<itemtemplate>
							<asp:label runat="server" text='<%#FormatPrice((float)DataBinder.Eval(Container.DataItem, "ServiceFee"), (int)DataBinder.Eval(Container.DataItem, "BillingPeriod"), (string)DataBinder.Eval(Container.DataItem, "BillingFrequency")) %>' id="Label2" />
						</itemtemplate>
					</asp:templatecolumn>
					<asp:templatecolumn headertext="Trial">
						<itemtemplate>
							<asp:label runat="server" text='<%#FormatTrial((float)DataBinder.Eval(Container.DataItem, "TrialFee"), (int)DataBinder.Eval(Container.DataItem, "TrialPeriod"), (string)DataBinder.Eval(Container.DataItem, "TrialFrequency")) %>' id="Label4" />
						</itemtemplate>
					</asp:templatecolumn>
					<asp:templatecolumn headertext="ExpiryDate">
						<itemtemplate>
							<asp:label runat="server" text='<%#FormatExpiryDate((DateTime)DataBinder.Eval(Container.DataItem, "ExpiryDate")) %>' id="Label1" />
						</itemtemplate>
					</asp:templatecolumn>
				</columns>
			</asp:datagrid>
            <asp:label id="lblServices" runat="server" visible="False"/>
       </div>
        <div class="dnnFormItem">
            <asp:Label ID="lblRSVPHelp" runat="server" resourcekey="RSVPHelp" />
        </div>
        <div class="dnnFormItem">
            <dnn:label id="plRSVPCode" runat="server" controlname="txtRSVPCode" />
		    <asp:textbox id="txtRSVPCode" runat="server" width="100" maxlength="50" columns="30" />
		    <asp:linkbutton class="dnnSecondaryAction" id="cmdRSVP" runat="server" resourcekey="cmdRSVP" />
        </div>
        <div class="dnnFormItem">
            <asp:Label ID="lblRSVP" runat="server" />
        </div>
    </fieldset>
</div>