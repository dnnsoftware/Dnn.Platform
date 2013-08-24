<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="ProfileManagerView.ascx.cs"
	Inherits="DotNetNuke.Modules.PreviewProfileManagement.Views.ProfileManagerView" %>
<%@ Import Namespace="DotNetNuke.Services.Localization" %>
<%@ Register TagPrefix="dnn" Namespace="DotNetNuke.UI.WebControls" Assembly="DotNetNuke" %>
<%@ Register TagPrefix="dnn" Assembly="DotNetNuke.Web" Namespace="DotNetNuke.Web.UI.WebControls" %>
<div class="dnnForm dnnClear">
	<h2 class="dnnFormSectionHead">
		<%=LocalizeString("CreateNew")%></h2>
	<div id="AddProfile" runat="server" class="dnnForm dnnClear dnnAddProfile">	
		<div class="dnnFormItem">
            <div class="dnnLabel">
                <label for="<%= cbName.ClientID %>">
			        <asp:Label ID="lblName" runat="server" resourcekey="lblName" CssClass="dnnFormRequired" ></asp:Label>
                </label>
            </div>
			<dnn:DnnComboBox ID="cbName" runat="server" AllowCustomText="true"
				MarkFirstMatch="true" ShowDropDownOnTextboxClick="true" CloseDropDownOnBlur="true" CollapseDelay="0" Filter="Contains"
                OnClientSelectedIndexChanged="OnClientSelectedIndexChanged" />
			<asp:CustomValidator ID="CustomValidator1" runat="server" ValidateEmptyText="true" ClientValidationFunction="notEmpty" CssClass="dnnFormMessage dnnFormError"
				OnServerValidate="ValidateName" ControlToValidate="cbName" Display="Dynamic" resourcekey="valName"
				ValidationGroup="AddProfile" />
		</div>
		<div class="dnnFormItem">
            <div class="dnnLabel">
                <label for="<%= txtWidth.ClientID %>">
                    <asp:Label ID="lblWidth" runat="server" resourcekey="lblWidth" CssClass="dnnFormRequired"></asp:Label>
                </label>
            </div>			
			<asp:TextBox ID="txtWidth" runat="server" MaxLength="4"  />
			<asp:CustomValidator ID="CustomValidator2" runat="server" ValidateEmptyText="true" ClientValidationFunction="isNumeric" CssClass="dnnFormMessage dnnFormError"
				ControlToValidate="txtWidth" Display="Dynamic" resourcekey="valWidth" ValidationGroup="AddProfile" />
		</div>
		<div class="dnnFormItem">
            <div class="dnnLabel">
                <label for="<%= txtHeight.ClientID %>">
                    <asp:Label ID="lblHeight" runat="server" resourcekey="lblHeight" CssClass="dnnFormRequired"></asp:Label>
                </label>
            </div>
			<asp:TextBox ID="txtHeight" runat="server" MaxLength="4" />
			<asp:CustomValidator ID="CustomValidator3" runat="server" ValidateEmptyText="true" ClientValidationFunction="isNumeric" CssClass="dnnFormMessage dnnFormError"
				ControlToValidate="txtHeight" Display="Dynamic" resourcekey="valHeight" ValidationGroup="AddProfile" />
		</div>
		<div class="dnnFormItem">
            <div class="dnnLabel">
                <label for="<%= txtUserAgent.ClientID %>">
                    <asp:Label ID="lblUserAgent" runat="server" resourcekey="lblUserAgent"  CssClass="dnnFormRequired"></asp:Label>
                </label>
            </div>			
			<asp:TextBox ID="txtUserAgent" runat="server" MaxLength="255" />
			<asp:CustomValidator ID="CustomValidator4" runat="server" ValidateEmptyText="true" ClientValidationFunction="notEmpty" CssClass="dnnFormMessage dnnFormError"
				ControlToValidate="txtUserAgent" Display="Dynamic" resourcekey="valUserAgent" ValidationGroup="AddProfile" />
		</div>
		<div class="dnnFormItem">
			<asp:LinkButton ID="btnSave" runat="server" resourcekey="Save" CssClass="dnnPrimaryAction"
				ValidationGroup="AddProfile" />
		</div>
	</div>
	<fieldset>
		<asp:ValidationSummary ID="valEditSummary" runat="server" CssClass="dnnFormMessage dnnFormValidationSummary" AllowSorting="true" 
			DisplayMode="List" ShowSummary="true" ValidationGroup="EditProfile" />
        <dnn:DnnGrid ID="ProfilesList" AutoGenerateColumns="false" runat="server" AllowSorting="true">             
              <MasterTableView  EditMode="InPlace">
                    <Columns>
                        	<dnn:DnnGridTemplateColumn HeaderText="" HeaderStyle-Width="30px">
					            <ItemTemplate>
						            <asp:Image ID="Image1" runat="server" ImageUrl="~/Icons/Sigma/DragDrop_15x15_Standard.png" AlternateText=""/>
					            </ItemTemplate>
				            </dnn:DnnGridTemplateColumn>
                            <dnn:DnnGridTemplateColumn HeaderText="" HeaderStyle-Width="50px">
					            <ItemTemplate>
						            <dnn:DnnImageButton ID="lnkEdit" runat="server" IconKey="Edit" CommandName="Edit" />
					            </ItemTemplate>
					            <EditItemTemplate>
                                    <div style="width:50px">
						            <dnn:DnnImageButton runat="server" ID="lnkSave" resourcekey="saveRule" CommandName="Save"
							            CommandArgument='<%#Eval("Id") %>' IconKey="Save" ValidationGroup="EditProfile"
							            CausesValidation="true" />
						            <dnn:DnnImageButton runat="server" ID="lnkCancelEdit" resourcekey="cmdCancel" CommandName="Cancel"
							            IconKey="Cancel" />
                                        </div>
					            </EditItemTemplate>
				            </dnn:DnnGridTemplateColumn>
                            <dnn:DnnGridTemplateColumn HeaderText="" HeaderStyle-Width="30px">
					            <ItemTemplate>
						            <asp:LinkButton ID="btnDel" runat="server" CssClass="delete" CausesValidation="false"
							            CommandArgument='<%#Eval("Id") %>' CommandName="Delete"></asp:LinkButton>
					            </ItemTemplate>
				            </dnn:DnnGridTemplateColumn>
                            <dnn:DnnGridTemplateColumn HeaderText="Device Name" HeaderStyle-Width="300px" UniqueName="DeviceName">
					            <ItemTemplate>
						            <%#Eval("Name") %>
					            </ItemTemplate>
					            <EditItemTemplate>						         
							        <asp:TextBox ID="txtEditName" runat="server" MaxLength="50"
								        Text='<%#Eval("Name") %>' />
							        <asp:CustomValidator ID="CustomValidator5" runat="server" ValidateEmptyText="true" ClientValidationFunction="notEmpty"
								        OnServerValidate="ValidateName" ControlToValidate="txtEditName" Display="None"
								        resourcekey="valName" ValidationGroup="EditProfile" InitValue='<%#Eval("Name") %>' />						          
					            </EditItemTemplate>
				            </dnn:DnnGridTemplateColumn>
				            <dnn:DnnGridTemplateColumn HeaderText="Width" HeaderStyle-Width="100px" UniqueName="Width">
					            <ItemTemplate>
						            <%#Eval("Width") %>
					            </ItemTemplate>
					            <EditItemTemplate>
						        
							        <asp:TextBox ID="txtEditWidth" runat="server" MaxLength="4"  Width="50"
								        Text='<%#Eval("Width") %>' />
							        <asp:CustomValidator ID="CustomValidator6" runat="server" ValidateEmptyText="true" ClientValidationFunction="isNumeric"
								        ControlToValidate="txtEditWidth" Display="None" resourcekey="valWidth" ValidationGroup="EditProfile" />
						          
					            </EditItemTemplate>
				            </dnn:DnnGridTemplateColumn>
				            <dnn:DnnGridTemplateColumn HeaderText="Height" HeaderStyle-Width="100px" UniqueName="Height">
					            <ItemTemplate>
						            <%#Eval("Height") %>
					            </ItemTemplate>
					            <EditItemTemplate>
						           
							        <asp:TextBox ID="txtEditHeight" runat="server" MaxLength="4"  CssClass="dnnFormRequired" Width="50"
								        Text='<%#Eval("Height") %>' />
							        <asp:CustomValidator ID="CustomValidator7" runat="server" ValidateEmptyText="true" ClientValidationFunction="isNumeric"
								        ControlToValidate="txtEditHeight" Display="None" resourcekey="valHeight" ValidationGroup="EditProfile" />
						          
					            </EditItemTemplate>
				            </dnn:DnnGridTemplateColumn>
				            <dnn:DnnGridTemplateColumn HeaderText="USER AGENT" UniqueName="UserAgent">
					            <ItemTemplate>
						            <div class="userAgent"><%#Eval("UserAgent") %></div>
					            </ItemTemplate>
					            <EditItemTemplate>						            
							        <asp:TextBox ID="txtEditUserAgent" runat="server" MaxLength="255" CssClass="dnnFormRequired"
								        Text='<%#Eval("UserAgent") %>' />
							        <asp:CustomValidator ID="CustomValidator8" runat="server" ValidateEmptyText="true" ClientValidationFunction="notEmpty"
								        ControlToValidate="txtEditUserAgent" Display="None" resourcekey="valUserAgent" ValidationGroup="EditProfile" />						            
					            </EditItemTemplate>
				            </dnn:DnnGridTemplateColumn>
                    </Columns>
              </MasterTableView>
        </dnn:DnnGrid>
	</fieldset>
</div>
<script type="text/javascript">
	(function ($, Sys) {
	    $().ready(function () {
	        var setUp = function () {
	            $('a.delete').dnnConfirm({
	                text: '<%= Localization.GetSafeJSString(LocalizeString("DeleteItem")) %>',
	                yesText: '<%= Localization.GetSafeJSString("Yes.Text", Localization.SharedResourceFile) %>',
	                noText: '<%= Localization.GetSafeJSString("No.Text", Localization.SharedResourceFile) %>',
	                title: '<%= Localization.GetSafeJSString("Confirm.Text", Localization.SharedResourceFile) %>'
	            });

	            var listContainer = $("#<%=ProfilesList.ClientID %> tbody");
	            var itemFilter = ".rgRow, .rgAltRow, .rgEditRow";

	            if (listContainer.find(itemFilter).length == 0) {
	                togglePreview(false);
	            }
	            else {
	                togglePreview(true);
	                listContainer.sortable({
	                    stop: function (event, ui) {
	                        var moveId = ui.item.attr("data");
	                        var nextId = "-1";
	                        if (ui.item.next().length > 0) {
	                            nextId = ui.item.next().attr("data");
	                        }
	                        var action = "\"action=sort&moveId=" + moveId + "&nextId=" + nextId + "\"";
	                        eval(dnn.getVar('ActionCallback').replace("[ACTIONTOKEN]", action));
	                    }
					, items: itemFilter
					, helper: function (event, ui) {
					    var helper = ui.clone(false);
					    helper.find("td").each(function (index) {
					        $(this).width(ui.find("td").eq(index).width());
					    });

					    return helper;
					}
					, placeholder: "dnnGridItem"
	                });
	            }

	        };

	        var togglePreview = function (enabled) {
	            var ddlViewMode = $(".dnnControlPanel").find("select[id$=ddlMode]")[0];
	            var found = false;
	            if (ddlViewMode) {
	                for (var i = 0; i < ddlViewMode.options.length; i++) {
	                    if (ddlViewMode.options[i].value === "PREVIEW") {
	                        found = true;

	                        if (!enabled) {
	                            ddlViewMode.options.remove(i);
	                        }

	                        break;
	                    }
	                }


	                if (!found && enabled) {
	                    ddlViewMode.options.add(new Option("Preview", "PREVIEW"));
	                }
	            }
	        };

	        setUp();

	        Sys.WebForms.PageRequestManager.getInstance().add_endRequest(function () {
	            setUp();
	        });
	    });

	    window.isNumeric = function (source, arg) {
	        var val = arg.Value;
	        var parseVal = parseInt(val);
	        arg.IsValid = /^\d{1,}$/.test(val) && !isNaN(parseVal) && parseVal > 0;
	    };

	    window.notEmpty = function (source, arg) {
	        var val = arg.Value;
	        arg.IsValid = val != "";
	    };

	    window.success = function (result, ctx) {
	        var listContainer = $("#<%=ProfilesList.ClientID %> tbody");
	        //var itemFilter = ".dnnGridItem,.dnnGridAltItem,.dnnFormEditItem";
	        var itemFilter = ".rgRow, .rgAltRow, .rgEditRow";

	        listContainer.find(itemFilter).each(function (index) {
	            var rowNo = index + 2;
	            if (rowNo < 10) {
	                rowNo = "0" + rowNo;
	            }
	            var lnkEdit = $(this).find("input[name$=lnkEdit]");
	            lnkEdit.attr("id", lnkEdit.attr("id").replace(/lnkEdit_\d+/, "lnkEdit_" + index));
	            lnkEdit.attr("name", lnkEdit.attr("name").replace(/ctl\d+\$lnkEdit/, "ctl" + rowNo + "$lnkEdit"));

	            var btnDel = $(this).find("a[id*=btnDel]");
	            btnDel.attr("id", btnDel.attr("id").replace(/btnDel_\d+/, "btnDel_" + index));
	            btnDel.attr("href", btnDel.attr("href").replace(/ctl\d+\$btnDel/, "ctl" + rowNo + "$btnDel"));

	            //	            if ($(this).attr("class").indexOf("dnnFormEditItem") == -1) {
	            //	                $(this).removeClass("dnnGridItem").removeClass("dnnGridAltItem").addClass(index % 2 == 0 ? "dnnGridItem" : "dnnGridAltItem");
	            //	            }

	            if ($(this).attr('class').indexOf("rgEditRow") == -1) {
	                $(this).removeClass("rgRow").removeClass("rgAltRow").addClass(index % 2 == 0 ? "rgRow" : "rgAltRow");
	            }
	        });
	    };

	    window.error = function (result, ctx) {
	        location.reload(true);
	    };

	    window.OnClientSelectedIndexChanged = function (sender, args) {
	        var selectedItem = args.get_item();
	        var txtWidth = $("#<%=txtWidth.ClientID %>");
	        var txtHeight = $("#<%=txtHeight.ClientID %>");
	        var txtUserAgent = $("#<%=txtUserAgent.ClientID %>");

	        if (selectedItem != null) {
	            var selectedValue = selectedItem.get_value();
	            var size = eval("({" + selectedValue + "})");

	            txtWidth.val(size.width);
	            txtHeight.val(size.height);
	            txtUserAgent.val(size.userAgent);
	        }
	    };
	})(jQuery, window.Sys);
</script>
