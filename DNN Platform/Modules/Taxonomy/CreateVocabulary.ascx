<%@ Control Language="C#" AutoEventWireup="false" CodeBehind="CreateVocabulary.ascx.cs" Inherits="DotNetNuke.Modules.Taxonomy.Views.CreateVocabulary" %>
<%@ Register TagPrefix="dnn" Assembly="DotNetNuke.Web" Namespace="DotNetNuke.Web.UI.WebControls" %>
<%@ Register TagPrefix="dnn" TagName="EditVocabularyControl" Src="Controls/EditVocabularyControl.ascx" %>
<div class="dnnForm dnnCreateVocab dnnClear">
    <dnn:EditVocabularyControl ID="editVocabularyControl" runat="server" IsAddMode="true" />
    <ul class="dnnActions dnnClear">
        <li><asp:LinkButton ID="saveVocabulary" runat="server" resourceKey="saveVocabulary" CssClass="dnnPrimaryAction" /></li>
        <li><asp:HyperLink ID="cancelCreate" runat="server" resourceKey="cancelCreate" CssClass="dnnSecondaryAction" /></li>
    </ul>
</div>