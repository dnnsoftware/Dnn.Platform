<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="GroupEdit.ascx.cs" Inherits="DotNetNuke.Modules.Groups.GroupEdit" %>
<div class="dnnForm  DnnModule-groupsWizard">   
    <div class="dnnFormItem">
        <div class="dnnLabel">
            <label>
                <span class="dnnFormRequired"><%=LocalizeString("GroupName.Text")%></span>
            </label>
        </div>
        <asp:Literal ID="litGroupName" runat="server" />
        <asp:TextBox ID="txtGroupName" runat="server" MaxLength="50" class="dnnFormRequired" />
        <asp:RequiredFieldValidator ID="reqGroupName" runat="server" ControlToValidate="txtGroupName" CssClass="dnnFormMessage dnnFormError" ResourceKey="GroupName.Required" />
        <asp:RegularExpressionValidator ID="valGroupName" CssClass="dnnFormMessage dnnFormError" runat="server" resourcekey="GroupName.Invalid" ControlToValidate="txtGroupName" Display="Dynamic" ValidationExpression="[^&\$\+,/?~#<>\(\)¿¡«»!\.:\*'\[\]]*" />
        <asp:Label ID="lblInvalidGroupName" runat="server" CssClass="dnnFormMessage dnnFormError" resourcekey="GroupNameDuplicate" Visible="false" />
    </div>
    <div class="dnnFormItem">
        <div class="dnnLabel">
            <label>
                <span><%=LocalizeString("Description.Text")%></span>
            </label>
        </div>
        <asp:TextBox ID="txtDescription" Columns="20" Rows="2" TextMode="MultiLine" runat="server" />
    </div>
    <div class="dnnFormItem">
        <div class="dnnLabel">
            <label>
                <span><%=LocalizeString("GroupPicture.Text")%></span>
            </label>
        </div>
        <div class="thumb">
            <span>
                <img id="imgGroup" runat="server" width="50" src=""
                    alt="Group Thumbnail" />
                <%--<a href="#" class="removeThumb"><%=LocalizeString("Remove.Text")%></a>--%>
            </span>
            <p>
                <%=LocalizeString("GroupPicture.Help")%>
            </p>
            <br />
            <asp:FileUpload ID="inpFile" runat="server" />
        </div>
    </div>

    <div class="dnnFormItem">
        <div class="dnnLabel">
            <label>
                <span><%=LocalizeString("Accessibility.Text")%></span>
            </label>
        </div>
        <table>
            <tr>
                <td>
                    <asp:RadioButton ID="rdAccessTypePublic" GroupName="AccessType" runat="server" Checked="true" />
                    <label><%=LocalizeString("Public.Text")%></label>
                    <span><%=LocalizeString("Public.Help")%></span>
                </td>
            </tr>
            <tr id="trMem">
                <td>
                    <div style="margin-left: 16px; font-size: 11px;">
                        <asp:CheckBox ID="chkMemberApproved" runat="server" />
                        <label><%=LocalizeString("MembersMustBeApproved") %></label>
                    </div>
                </td>
            </tr>
            <tr>
                <td>
                    <asp:RadioButton ID="rdAccessTypePrivate" GroupName="AccessType" runat="server" />
                    <label><%=LocalizeString("Private.Text")%></label>
                    <span><%=LocalizeString("Private.Help")%></span>
                </td>
            </tr>
        </table>
    </div>
    <!--close dnnFormItem-->
    <ul class="dnnActions dnnClear">
        <li>
            <asp:LinkButton ID="btnSave" runat="server" CssClass="dnnPrimaryAction" Text="Save Group"
                resourcekey="SaveGroupButton" />
        </li>
        <li>
            <asp:LinkButton ID="btnCancel" runat="server" CssClass="dnnSecondaryAction" Text="Cancel"
                resourcekey="Cancel" CausesValidation="False" /></li>
    </ul>
    <!--close dnnActions-->
</div>
<script type="text/javascript">
    jQuery(document).ready(function ($) {
        $('#<%=rdAccessTypePublic.ClientID %>').change(function () {
            $('#trMem').show();

        });
        $('#<%=rdAccessTypePrivate.ClientID %>').change(function () {
            $('#trMem').hide();

        });
    });
</script>
