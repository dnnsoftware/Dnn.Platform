<%@ Control Language="C#" AutoEventWireup="false" CodeBehind="EditTermControl.ascx.cs" Inherits="DotNetNuke.Modules.Taxonomy.Views.Controls.EditTermControl" %>
<%@ Register TagPrefix="dnn" Assembly="DotNetNuke.Web" Namespace="DotNetNuke.Web.UI.WebControls" %>
<div class="dnnForm dnnEditTermControl dnnClear">
    <div class="dnnFormItem">
        <div class="dnnLabel">
            <label>
                <dnn:DnnFieldLabel id="nameFieldLabel" runat="server" Text="TermName.Text" ToolTip="TermName.ToolTip" CssClass="dnnFormRequired" />    
            </label>
        </div>        
        <asp:TextBox ID="nameTextBox" runat="server" CssClass="termNameBox" />
        <asp:RequiredFieldValidator ID="nameValidator" ControlToValidate="nameTextBox" runat="server" Display="Dynamic" SetFocusOnError="true" CssClass="dnnFormMessage dnnFormError" />
    </div>
    <div class="dnnFormItem">
        <div class="dnnLabel">
            <label>
                <dnn:DnnFieldLabel id="descriptionFieldLabel" runat="server" Text="Description.Text" ToolTip="Description.ToolTip" />        
            </label>
        </div>        
        <asp:TextBox ID="descriptionTextBox" runat="server" TextMode="MultiLine" cssClass="NormalTextBox"/>
    </div>
    <div class="dnnFormItem" id="divParentTerm" runat="server">
        <div class="dnnLabel">
            <label>
                <dnn:DnnFieldLabel id="parentTermLabel" runat="server" Text="ParentTerm.Text" ToolTip="ParentTerm.ToolTip" CssClass="dnnFormRequired" />    
            </label>
        </div>        
        <dnn:DnnComboBox ID="parentTermCombo" runat="server" DataTextField="Name" DataValueField="TermId" OnClientSelectedIndexChanged="parentTermChanged"  />
    </div>
</div>