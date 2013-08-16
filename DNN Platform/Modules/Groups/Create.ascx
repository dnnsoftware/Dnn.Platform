<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="Create.ascx.cs" Inherits="DotNetNuke.Modules.Groups.Create" %>
<%@ Register TagPrefix="dnn" TagName="label" Src="~/controls/LabelControl.ascx" %>
<% if (CanCreate) { %>
<div class="dnnForm  DnnModule-groupsWizard">
    <div class="content">
        <fieldset class="group-wizard-step-1 wizardGroup" id="step1">
            <h2 class="WizardStepTitle">
               <strong><%= LocalizeString("CreateGroup.Text")%></strong>
            </h2>
            <div class="wizardStepBody">               
                <div class="dnnFormItem">
                    <div class="dnnLabel">
                        <label>
                            <span class="dnnFormRequired"><%=LocalizeString("GroupName.Text")%></span>
                        </label>
                    </div>
                    <asp:TextBox ID="txtGroupName" runat="server" MaxLength="50"/>
                    <asp:RequiredFieldValidator ID="reqGroupName" runat="server" ControlToValidate="txtGroupName" CssClass="dnnFormMessage dnnFormError" ResourceKey="GroupName.Required" Display="Dynamic" />
                    <asp:RegularExpressionValidator ID="valGroupName" CssClass="dnnFormMessage dnnFormError" runat="server" resourcekey="GroupName.Invalid" ControlToValidate="txtGroupName" Display="Dynamic" ValidationExpression="[A-Za-z0-9\.\s_-]*" />
                    <asp:Label id="lblInvalidGroupName" runat="server" CssClass="dnnFormMessage dnnFormError" resourcekey="GroupNameDuplicate" Visible="false" />
                </div>
                <!--close dnnFormItem-->
                <div class="dnnFormItem">
                    <div class="dnnLabel">
                        <label>
                            <span><%=LocalizeString("Description.Text")%></span>
                        </label>
                    </div>
                    <asp:TextBox ID="txtDescription" Columns="20" Rows="2" TextMode="MultiLine" runat="server" />               
                </div>
                <!--close dnnFormItem-->
                <div class="dnnFormItem">
                    <div class="dnnLabel">
                        <label>
                            <span><%=LocalizeString("GroupPicture.Text")%></span>
                        </label>
                    </div>
                    <div class="thumb">
                        <span>
                            <img src="<%=Page.ResolveUrl("~/DesktopModules/SocialGroups/Images/") %>sample-group-profile.jpg" alt="Group Thumbnail" />
                            <%--<a href="#" class="removeThumb"><%=LocalizeString("Remove.Text")%></a>--%> </span>
                        <p><%=LocalizeString("GroupPicture.Help")%>
                            </p>
                        <br />
                        <asp:FileUpload ID="inpFile" runat="server" />
                  
                    </div>
                </div>
                <!--close dnnFormItem-->
                <hr />
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
                            <td><div style="margin-left:16px;font-size:11px;">
                                <asp:CheckBox ID="chkMemberApproved" runat="server" /><label><%=LocalizeString("MembersMustBeApproved") %></label>
                            </div></td>
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
                    <li><asp:LinkButton ID="btnCreate" runat="server" CssClass="dnnPrimaryAction" Text="Create Group" resourcekey="CreateGroupButton" /> </li>
                    <li><asp:LinkButton ID="btnCancel" runat="server" CssClass="dnnSecondaryAction" Text="Cancel" resourcekey="Cancel" CausesValidation="False" /></li>
                </ul>
                <!--close dnnActions-->
            </div>
            <!--close wizardStepBody-->
        </fieldset>
      
    </div>
    <!--close content-->
</div>
<!--close dnnForm DnnModule-Groups-Creation-->
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
<% } %>