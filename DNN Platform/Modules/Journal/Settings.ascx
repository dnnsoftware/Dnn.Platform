<%@ Control Language="C#" AutoEventWireup="false" Inherits="DotNetNuke.Modules.Journal.Settings"
    CodeBehind="Settings.ascx.cs" %>
    <%@ Register TagPrefix="dnn" TagName="Label" Src="~/controls/LabelControl.ascx" %>
    <style>
        .dnnFormItem td label{white-space:nowrap;text-align:left;}
    </style>
<div class="dnnFormItem">
    <dnn:label controlname="chkEnableEditor" resourcekey="EnableEditor" Text="Enable Editor" Suffix=":" runat="server" />
    <asp:CheckBox id="chkEnableEditor" runat="server" />
</div>
<div class="dnnFormItem">
    <dnn:label controlname="chkAllowFiles" resourcekey="AllowFiles" Text="Allow Files" Suffix=":" runat="server" />
    <asp:CheckBox id="chkAllowFiles" runat="server" />
</div>

<div class="dnnFormItem">
    <dnn:label controlname="chkAllowPhotos" resourcekey="AllowPhotos" Text="Allow Photos" Suffix=":" runat="server" />
    <asp:CheckBox id="chkAllowPhotos" runat="server" />
</div>

<div class="dnnFormItem">
    <dnn:label controlname="chkAllowResize" resourcekey="AllowResize" Text="Allow Resize Photos" Suffix=":" runat="server" />    
    <asp:CheckBox id="chkAllowResize" runat="server" />
</div>

<div class="dnnFormItem">
    <dnn:label controlname="drpDefaultPageSize" resourcekey="DefaultPageSize" Suffix=":" runat="server" />
    <asp:DropDownList ID="drpDefaultPageSize" runat="server">
        <asp:ListItem Value="5">5</asp:ListItem>
        <asp:ListItem Value="10">10</asp:ListItem>
        <asp:ListItem Value="20">20</asp:ListItem>
        <asp:ListItem Value="30">30</asp:ListItem>
        <asp:ListItem Value="40">40</asp:ListItem>
        <asp:ListItem Value="50">50</asp:ListItem>
        <asp:ListItem Value="75">75</asp:ListItem>
        <asp:ListItem Value="100">100</asp:ListItem>
    </asp:DropDownList>
</div>
<div class="dnnFormItem">
    <dnn:label controlname="drpMaxMessageLength" resourcekey="MaxMessageLength" Suffix=":" runat="server" />
    <asp:DropDownList ID="drpMaxMessageLength" runat="server">
        <asp:ListItem Value="140">140</asp:ListItem>
        <asp:ListItem Value="250">250</asp:ListItem>
        <asp:ListItem Value="500">500</asp:ListItem>
        <asp:ListItem Value="1000">1000</asp:ListItem>
        <asp:ListItem Value="2000">2000</asp:ListItem>
        <asp:ListItem Value="-1" resourcekey="MessageLength_Unlimited"></asp:ListItem>
    </asp:DropDownList>
  
</div>
<div class="dnnFormItem">
    <dnn:label  resourcekey="JournalFilters" Suffix=":" runat="server" />
    <asp:CheckBoxList ID="chkJournalFilters" runat="server" />
</div>
<script type="text/javascript">
    $(document).ready(function () {        
        $('#<%=chkEnableEditor.ClientID %>').click(function (event) {
            if (this.checked) {
                $('#<%=chkAllowFiles.ClientID %>').removeAttr("disabled");
                $('#<%=chkAllowPhotos.ClientID %>').removeAttr("disabled");
                if ($('#<%=chkAllowPhotos.ClientID %>')[0].checked) {
                    $('#<%=chkAllowResize.ClientID %>').removeAttr("disabled");                    
                }
            } else {
                $('#<%=chkAllowFiles.ClientID %>').attr("disabled", true);
                $('#<%=chkAllowPhotos.ClientID %>').attr("disabled", true);
                $('#<%=chkAllowResize.ClientID %>').attr("disabled", true);
            }
        });

        $('#<%=chkAllowPhotos.ClientID %>').click(function(event) {
            if (this.checked) {
                $('#<%=chkAllowResize.ClientID %>').removeAttr("disabled");                
            } else {                
                $('#<%=chkAllowResize.ClientID %>').attr("disabled", true);
            }
        });
    });

</script>