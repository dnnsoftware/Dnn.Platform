<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="Settings.ascx.cs" Inherits="DotNetNuke.Modules.MemberDirectory.Settings" %>
<%@ Import Namespace="DotNetNuke.Services.Localization" %>
<%@ Register TagPrefix="dnn" TagName="label" Src="~/controls/LabelControl.ascx" %>

<div class="dnnForm dnnMemberDirectorySettings dnnClear" id="dnnMemberDirectorySettings">
    <h2 id="dnnPanel-Templates" class="dnnFormSectionHead"><a href="" class="dnnSectionExpanded"><%=LocalizeString("Templates")%></a></h2>
    <fieldset>
        <div class="dnnFormItem">
            <dnn:label id="itemTemplateLabel" runat="server" controlname="itemTemplate" />
            <asp:TextBox ID="itemTemplate" runat="server" TextMode="MultiLine" Rows="3" />
        </div>        
        <div class="dnnFormItem">
            <dnn:label id="alternateItemTemplateLabel" runat="server" controlname="alternateItemTemplate" />
            <asp:TextBox ID="alternateItemTemplate" runat="server" TextMode="MultiLine" Rows="3" />
        </div>
        <div class="dnnFormItem">
            <dnn:label id="enablePopUpLabel" runat="server" controlname="enablePopUp" />
            <asp:CheckBox ID="enablePopUp" runat="server" />
        </div>        
        <div class="dnnFormItem">
            <dnn:label id="popUpTemplateLabel" runat="server" controlname="popUpTemplate" />
            <asp:TextBox ID="popUpTemplate" runat="server" TextMode="MultiLine" Rows="3" />
        </div>        
    </fieldset>
    <h2 id="dnnPanel-Filter" class="dnnFormSectionHead"><a href="" class="dnnSectionExpanded"><%=LocalizeString("Filters")%></a></h2>
    <fieldset>
        <div class="dnnFormItem">
            <dnn:label id="filterBySelectorLabel" runat="server" controlname="filterBySelector" />
            <div class="mdFilterBy">
                <asp:RadioButtonList runat="server" ID="filterBySelector" CssClass="mdFilters" RepeatDirection="Horizontal" RepeatColumns="5">
                    <asp:ListItem Value="None" resourcekey="None"/>
                    <asp:ListItem Value="User" resourcekey="User"/>
				    <asp:ListItem Value="Group" resourcekey="Group"/>
				    <asp:ListItem Value="Relationship" resourcekey="Relationship"/>
				    <asp:ListItem Value="ProfileProperty" resourcekey="ProfileProperty"/>
                </asp:RadioButtonList>
                <div class="mdFilterLists">
                    <asp:DropDownList runat="server" ID="groupList" DataTextField="RoleName" DataValueField="RoleID"/>
                    <asp:DropDownList runat="server" ID="relationShipList" DataTextField="Name" DataValueField="RelationshipTypeId"/>
                    <asp:DropDownList runat="server" ID="propertyList" DataTextField="Text" DataValueField="Value"/>
                    <asp:TextBox runat="server" ID="propertyValue" />
                </div>
            </div>
        </div>
         <div class="dnnFormItem">
            <dnn:label id="sortFieldListLabel" runat="server" controlname="sortFieldList" />
            <asp:DropDownList runat="server" ID="sortFieldList">
            </asp:DropDownList>
        </div>        
         <div class="dnnFormItem">
            <dnn:label id="sortOrderListLabel" runat="server" controlname="sortOrderList" />
            <asp:DropDownList runat="server" ID="sortOrderList">
                <asp:ListItem Value="ASC" resourcekey="Ascending"/>
                <asp:ListItem Value="DESC" resourcekey="Descending"/>
            </asp:DropDownList>
        </div>        
        <div class="dnnFormItem">
            <dnn:label id="ExcludeHostUsersLabel" runat="server" controlname="ExcludeHostUsersCheckBox" />
            <asp:CheckBox ID="ExcludeHostUsersCheckBox" runat="server" />
        </div>            
   </fieldset>
    <h2 id="H1" class="dnnFormSectionHead"><a href="" class="dnnSectionExpanded"><%=LocalizeString("Search")%></a></h2>
    <fieldset>
        <div class="dnnFormItem">
            <dnn:label id="displaySearchLabel" runat="server" controlname="displaySearch" />
            <asp:DropDownList runat="server" ID="displaySearchList">
	            <Items>
		            <asp:ListItem Value="None" resourcekey="DisplaySearch_None"></asp:ListItem>
					<asp:ListItem Value="Simple" resourcekey="DisplaySearch_Simple"></asp:ListItem>
					<asp:ListItem Value="Both" resourcekey="DisplaySearch_Both"></asp:ListItem>
	            </Items>
			</asp:DropDownList>
        </div>        
        <div class="dnnFormItem">
            <dnn:label id="searchField1Label" runat="server" controlname="searchField1List" />
            <asp:DropDownList runat="server" ID="searchField1List" DataTextField="Text" DataValueField="Value"/>
        </div>
        <div class="dnnFormItem">
            <dnn:label id="searchField2Label" runat="server" controlname="searchField2List" />
            <asp:DropDownList runat="server" ID="searchField2List" DataTextField="Text" DataValueField="Value"/>
        </div>
        <div class="dnnFormItem">
            <dnn:label id="searchField3Label" runat="server" controlname="searchField3List" />
            <asp:DropDownList runat="server" ID="searchField3List" DataTextField="Text" DataValueField="Value"/>
        </div>
        <div class="dnnFormItem">
            <dnn:label id="searchField4Label" runat="server" controlname="searchField4List" />
            <asp:DropDownList runat="server" ID="searchField4List" DataTextField="Text" DataValueField="Value"/>
        </div>
    </fieldset>
    <h2 id="H2" class="dnnFormSectionHead"><a href="" class="dnnSectionExpanded"><%=LocalizeString("Paging")%></a></h2>
    <fieldset>
        <div class="dnnFormItem">
            <dnn:label id="disablePagerLabel" runat="server" controlname="disablePager" />
            <asp:CheckBox ID="disablePager" runat="server" />
        </div>        
        <div class="dnnFormItem">
            <dnn:label id="pageSizeLabel" runat="server" controlname="pageSize" />
            <asp:TextBox ID="pageSize" runat="server" />
            <asp:RangeValidator runat="server" ControlToValidate="pageSize" CssClass="dnnFormMessage dnnFormError" MaximumValue="100" MinimumValue="1" Type="Integer" resourcekey="pageSize.Error"></asp:RangeValidator>
        </div>
    </fieldset>

</div>
<script type="text/javascript" language="javascript">
    (function ($, Sys) {
        function setUpSettings() {
            toggleFilter();
            $('#<%= filterBySelector.ClientID %>').change(function () {
                toggleFilter();
            });
        }

        function toggleFilter() {
            var filterValue = $('#<%= filterBySelector.ClientID %> input:checked').val();
            switch (filterValue) {
                case "Group":
                    $('select[name$=groupList]').show();
                    $('select[name$=relationShipList]').hide();
                    $('select[name$=propertyList]').hide();
                    $('input[name$=propertyValue]').hide();
                    break;
                case "Relationship":
                    $('select[name$=groupList]').hide();
                    $('select[name$=relationShipList]').show();
                    $('select[name$=propertyList]').hide();
                    $('input[name$=propertyValue]').hide();
                    break;
                case "ProfileProperty":
                    $('select[name$=groupList]').hide();
                    $('select[name$=relationShipList]').hide();
                    $('select[name$=propertyList]').show();
                    $('input[name$=propertyValue]').show();
                    break;
                default:
                    $('select[name$=groupList]').hide();
                    $('select[name$=relationShipList]').hide();
                    $('select[name$=propertyList]').hide();
                    $('input[name$=propertyValue]').hide();
                    break;
            }
        }

        $(document).ready(function () {
            setUpSettings();
            Sys.WebForms.PageRequestManager.getInstance().add_endRequest(function () {
                setUpSettings();
            });
        });
    } (jQuery, window.Sys));
</script>
