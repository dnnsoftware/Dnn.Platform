<%@ Control language="vb" AutoEventWireup="false" Explicit="True" Inherits="DotNetNuke.UI.Containers.Container" %>
<%@ Register TagPrefix="dnn" TagName="ICON" Src="~/Admin/Containers/Icon.ascx" %>
<%@ Register TagPrefix="dnn" TagName="TITLE" Src="~/Admin/Containers/Title.ascx" %>
<%@ Register TagPrefix="dnn" TagName="VISIBILITY" Src="~/Admin/Containers/Visibility.ascx" %>
<div class="c_DNN6_Footer c_DNN6">
    <h2 class="Title"><div class="dnnIcon"><dnn:ICON runat="server" /></div><dnn:TITLE runat="server" id="dnnTITLE" /></h2>
    <div id="ContentPane" runat="server"></div>
</div>
