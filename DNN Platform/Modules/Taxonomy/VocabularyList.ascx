<%@ Control Language="C#" AutoEventWireup="false" CodeBehind="VocabulariesList.ascx.cs" Inherits="DotNetNuke.Modules.Taxonomy.Views.VocabularyList" %>
<%@ Register TagPrefix="dnn" Assembly="DotNetNuke.Web" Namespace="DotNetNuke.Web.UI.WebControls" %>
<%@ Register TagPrefix="dnn" Namespace="Telerik.Web.UI" %>
<div class="dnnForm dnnVocabularyList dnnClear">
    <h3><asp:Label ID="titleLabel" runat="server" resourceKey="Title" /></h3>
    <dnn:DnnGrid id = "vocabulariesGrid" runat="server" AutoGenerateColumns="false" AllowSorting="true" CssClass="dnnVocabularyGrid">
        <MasterTableView DataKeyNames="VocabularyId">
            <Columns>
                <dnn:DnnGridTemplateColumn UniqueName="EditItem">
                    <ItemTemplate>
                        <asp:HyperLink ID="hlEdit" runat="server">
                            <dnn:DnnImage IconKey="Edit" id="imgEdit" runat="server" />
                        </asp:HyperLink>
                    </ItemTemplate>
                </dnn:DnnGridTemplateColumn>
                <dnn:DnnGridBoundColumn DataField="Name" HeaderText="Name" />
                <dnn:DnnGridBoundColumn DataField="Description" HeaderText="Description" />
                <dnn:DnnGridBoundColumn DataField="Type" HeaderText="Type" />
				<dnn:DnnGridTemplateColumn HeaderText="Scope">
                    <ItemTemplate>
                        <%#LocalizeString(Eval("ScopeType").ToString()) %>
                    </ItemTemplate>
                </dnn:DnnGridTemplateColumn>
            </Columns>
        </MasterTableView>
    </dnn:DnnGrid>
    <ul class="dnnActions dnnClear">
        <li><asp:HyperLink ID="hlAddVocab" runat="server" resourcekey="Create" CssClass="dnnPrimaryAction" /></li>
    </ul>
</div>