<%@ Control Language="C#" AutoEventWireup="false" Inherits="DotNetNuke.Modules.UrlManagement.PortalAliases" Codebehind="PortalAliases.ascx.cs" %>
<%@ Register TagPrefix="dnn" Namespace="DotNetNuke.Web.UI.WebControls" Assembly="DotNetNuke.Web" %>
<%@ Register TagPrefix="dnn" TagName="label" Src="~/controls/labelcontrol.ascx" %>

<div class="dnnAliasesHeader">
    <asp:LinkButton ID="addAliasButton" runat="server" ResourceKey="cmdAdd" CssClass="dnnSecondaryAction" />
</div>
<asp:DataGrid ID="portalAiasesGrid" Runat="server" AutoGenerateColumns="false" width="100%" GridLines="None" CssClass="dnnGrid">
    <headerstyle CssClass="dnnGridHeader" />
    <itemstyle CssClass="dnnGridItem" horizontalalign="Left" />
    <alternatingitemstyle CssClass="dnnGridAltItem" />
    <edititemstyle />
    <selecteditemstyle />
    <footerstyle />
	<Columns>
        <asp:TemplateColumn HeaderText="Primary" HeaderStyle-Width="50px" ItemStyle-HorizontalAlign="Center">
            <ItemTemplate>
                <asp:Image ID="Image1" runat="server" ImageUrl="~/Icons/Sigma/Checked_16x16_Standard(dark).png" 
                        Visible='<%#DataBinder.Eval(Container.DataItem, "IsPrimary") %>' />
            </ItemTemplate>
            <EditItemTemplate>
                <asp:CheckBox ID="isPrimary" runat="server"
                    Checked='<%#DataBinder.Eval(Container.DataItem, "IsPrimary") %>' 
                    Enabled='<%#Container.ItemIndex == portalAiasesGrid.EditItemIndex %>'/>
            </EditItemTemplate>
        </asp:TemplateColumn>
        <asp:TemplateColumn HeaderText="HTTPAlias" HeaderStyle-Width="510px" >
		    <HeaderStyle  HorizontalAlign="Left" />
		    <ItemStyle  HorizontalAlign="Left" />
		    <ItemTemplate>
                <asp:label runat="server" Text='<%#DataBinder.Eval(Container.DataItem, "HTTPAlias") %>' ID="lbHTTPAlias" />
		    </ItemTemplate>
		    <EditItemTemplate>
                <asp:textbox runat="server" Text='<%#DataBinder.Eval(Container.DataItem, "HTTPAlias") %>' ID="txtHTTPAlias" CssClass="dnnFormInput" />
		    </EditItemTemplate>
		</asp:TemplateColumn>
       <asp:TemplateColumn HeaderText="Language" HeaderStyle-Width="110px" >
		    <ItemTemplate>
                <asp:label runat="server" Text='<%#(DataBinder.Eval(Container.DataItem, "CultureCode")) %>' ID="cultureCodeLabel" />
		    </ItemTemplate>
            <EditItemTemplate>
                <dnn:DnnComboBox runat="server" id="cultureCodeDropDown" DataTextField="Value" DataValueField="Value" 
                            Visible ='<%#Locales.Count > 1 %>'/>
                <asp:label runat="server" 
                            Text='<%#DataBinder.Eval(Container.DataItem, "CultureCode") %>' ID="cultureCodeEditLabel" 
                            Visible ='<%#Locales.Count == 1 %>'/>
            </EditItemTemplate>
        </asp:TemplateColumn>        
        <asp:TemplateColumn HeaderText="Action" HeaderStyle-Width="60px" ItemStyle-HorizontalAlign="Center">
            <ItemTemplate>
                <dnn:Label ID="currentAliasHelpLabel" runat="server"  helpKey="currentAliasHelp"/>
	            <dnn:DnnImageButton Runat="server" ID="editButton" resourcekey="editRule" OnCommand="EditAiasesGrid" CommandArgument='<% # Container.ItemIndex %>' IconKey="Edit" />
	            <dnn:DnnImageButton Runat="server" ID="deleteButton" resourcekey="Delete" OnCommand="DeleteAiasesGrid" CommandArgument='<% # Container.ItemIndex %>' IconKey="Delete" />
            </ItemTemplate>
            <EditItemTemplate>
	            <dnn:DnnImageButton Runat="server" ID="saveButton" resourcekey="saveRule" OnCommand="SaveAliasesGrid" IconKey="Save" />
	            <dnn:DnnImageButton Runat="server" ID="cancelButton" resourcekey="Cancel" OnCommand="CancelEdit" IconKey="Cancel" />
            </EditItemTemplate>
        </asp:TemplateColumn>
	</Columns>
</asp:DataGrid>
<asp:Label ID="lblError" runat="server" Visible="false" CssClass="dnnFormMessage dnnFormError" />
<script  language="javascript">
    function SelectOne(rdo, gridName) {
        /* Getting an array of all the "INPUT" controls on the form.*/
        var all = document.getElementsByTagName("input");
        
        for (var i = 0; i < all.length; i++) {
            if (all[i].type == "radio")/*Checking if it is a radio button*/ {
                var count = all[i].id.indexOf(gridName);
                if (count != -1) {
                    all[i].checked = false;
                }
            }
        }
        
        rdo.checked = true;/* Finally making the clicked radio button CHECKED */
    }
</script>