<%@ Control Language="c#" AutoEventWireup="false" Explicit="True" CodeFile="ModuleLocalization.ascx.cs" Inherits="DotNetNuke.Admin.Modules.ModuleLocalization" %>
<%@ Register TagPrefix="dnnweb" Namespace="DotNetNuke.Web.UI.WebControls" Assembly="DotNetNuke.Web" %>
<%@ Register TagPrefix="dnn" Assembly="DotNetNuke.Web" Namespace="DotNetNuke.Web.UI.WebControls" %>
<div class="dnnForm dnnModuleLocalization dnnClear">
	<dnnweb:DnnGrid ID="localizedModulesGrid" runat="server" AutoGenerateColumns="false" AllowMultiRowSelection="true" 
            CssClass="dnnModuleLocalizationGrid">
        <ClientSettings >
            <Selecting AllowRowSelect="true" />
        </ClientSettings>
        <MasterTableView DataKeyNames="ModuleId, TabId">
			<Columns>
                <dnnweb:DnnGridClientSelectColumn HeaderStyle-HorizontalAlign="Center" ItemStyle-HorizontalAlign="Center" ItemStyle-Width="40px" />
 				<dnnweb:DnnGridTemplateColumn UniqueName="Language" HeaderText="Language" ItemStyle-VerticalAlign="Middle" ItemStyle-Width="200px">
					<ItemTemplate>
						<%# Convert.ToBoolean(Eval("IsDefaultLanguage")) ? "" : "&nbsp;&nbsp;&nbsp;&nbsp;"%>
						<dnnweb:DnnLanguageLabel ID="moduleLanguageLabel" runat="server" Language='<%# Eval("CultureCode") %>'  />
					</ItemTemplate>
				</dnnweb:DnnGridTemplateColumn>
				<dnnweb:DnnGridBoundColumn HeaderText="ModuleType" DataField="DesktopModule.FriendlyName" ItemStyle-Width="80px" ItemStyle-VerticalAlign="Middle" />
				<dnnweb:DnnGridBoundColumn HeaderText="ModuleTitle" DataField="ModuleTitle" ItemStyle-Width="200px" ItemStyle-VerticalAlign="Middle"  />
				<dnnweb:DnnGridTemplateColumn UniqueName="Edit" HeaderText="Edit"  ItemStyle-Width="40px" ItemStyle-HorizontalAlign="Center" HeaderStyle-HorizontalAlign="Center" ItemStyle-VerticalAlign="Middle">
					<ItemTemplate>
						<a href='<%# DotNetNuke.Common.Globals.NavigateURL(Convert.ToInt32(Eval("TabId")), Null.NullBoolean, PortalSettings, "Module", Eval("CultureCode").ToString(), "ModuleId=" + Eval("ModuleID")) %>' >
							<dnn:DnnImage ID="editCultureImage" runat="server" ResourceKey="edit" IconKey="Edit" />
						</a>
					</ItemTemplate>
				</dnnweb:DnnGridTemplateColumn>
				<dnnweb:DnnGridTemplateColumn UniqueName="IsLocalized" HeaderText="UnBound" ItemStyle-Width="100px" ItemStyle-HorizontalAlign="Center" ItemStyle-VerticalAlign="Middle">
					<ItemTemplate>
						<asp:Label ID="defaultLocalizedLabel" runat="server" resourcekey="NA" Visible='<%# Eval("IsDefaultLanguage")%>' />
						<dnnweb:DnnImage ID="localizedImage" runat="server" IconKey="Grant" Visible='<%# Convert.ToBoolean(Eval("IsLocalized")) && !Convert.ToBoolean(Eval("IsDefaultLanguage"))%>' />
						<dnnweb:DnnImage ID="notLocalizedImage" runat="server" IconKey="Deny" Visible='<%# !Convert.ToBoolean(Eval("IsLocalized")) && !Convert.ToBoolean(Eval("IsDefaultLanguage"))%>' />
					</ItemTemplate>
				</dnnweb:DnnGridTemplateColumn>
				<dnnweb:DnnGridTemplateColumn UniqueName="IsTranslated" HeaderText="Translated" ItemStyle-Width="60px" ItemStyle-HorizontalAlign="Center" ItemStyle-VerticalAlign="Middle">
					<ItemTemplate>
						<asp:Label ID="defaultTranslatedLabel" runat="server" resourcekey="NA" Visible='<%# Eval("IsDefaultLanguage")%>' />
						<dnnweb:DnnImage ID="translatedImage" runat="server" IconKey="Grant" Visible='<%# Convert.ToBoolean(Eval("IsTranslated")) && !Convert.ToBoolean(Eval("IsDefaultLanguage"))%>' />
						<dnnweb:DnnImage ID="notTranslatedImage" runat="server" IconKey="Deny" Visible='<%# !Convert.ToBoolean(Eval("IsTranslated")) && !Convert.ToBoolean(Eval("IsDefaultLanguage"))%>' />
					</ItemTemplate>
				</dnnweb:DnnGridTemplateColumn>
			</Columns>
		</MasterTableView>
	</dnnweb:DnnGrid>
	<asp:PlaceHolder ID="footerPlaceHolder" runat="server">
	    <ul class="dnnActions dnnClear">
		    <li><asp:LinkButton ID="localizeModuleButton" resourcekey="unbindModule" runat="server" CssClass="dnnSecondaryAction" CausesValidation="False" /></li>
		    <li><asp:LinkButton ID="delocalizeModuleButton" resourcekey="bindModule" runat="server" CssClass="dnnSecondaryAction" CausesValidation="False" /></li>
		    <ul><asp:LinkButton ID="markModuleTranslatedButton" resourcekey="markModuleTranslated" runat="server" CssClass="dnnSecondaryAction" CausesValidation="False" />
		    <li><asp:LinkButton ID="markModuleUnTranslatedButton" resourcekey="markModuleUnTranslated" runat="server" CssClass="dnnSecondaryAction" CausesValidation="False" /></li>
	    </ul>    
	</asp:PlaceHolder>
</div>