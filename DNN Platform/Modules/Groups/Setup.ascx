<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="Setup.ascx.cs" Inherits="DotNetNuke.Modules.Groups.Setup" %>
<div class="dnnForm">
    <fieldset>
        <div class="dnnFormItem">
            <p><%=LocalizeString("SetupIntro") %></p>
        </div>
    </fieldset>
    <ul class="dnnActions dnnClear">
        <li>
            <asp:LinkButton ID="btnGo" runat="server" resourceKey="AutoConfigure" CssClass="dnnPrimaryAction" />
        </li>
    </ul>


</div>

