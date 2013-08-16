<%@ Control language="vb" AutoEventWireup="false" Explicit="True" Inherits="DotNetNuke.UI.Containers.Container" %>
<%@ Register TagPrefix="dnn" TagName="ICON" Src="~/Admin/Containers/Icon.ascx" %>
<%@ Register TagPrefix="dnn" TagName="TITLE" Src="~/Admin/Containers/Title.ascx" %>
<%@ Register TagPrefix="dnn" TagName="VISIBILITY" Src="~/Admin/Containers/Visibility.ascx" %>
<div class="RotatorWrapper" id="<%=ModuleControl.ModuleContext.ModuleId%>_Rotator"><div id="ContentPane" runat="server"></div></div>
<script type="text/javascript">
    jQuery(document).ready(function ($) {
        /*  Banner Rotator Script
            This script will automatically create a banner rotator based on the content entered into container.
            The script will automatically cycle through all the sibling elements and create a full rotator including navigation.
            Banner sizes and other styles are located in container.css.  The page titles for the navigation row are
			created from each display elements title attribute.
            See http://jquery.malsup.com/cycle/ for more info on plugin.
        */
        $('#<%=ModuleControl.ModuleContext.ModuleId%>_Rotator div.DNNModuleContent > div.Normal').after('<ul class="RotatorNav">').cycle({
            fx: 'fade', //effect to apply to rotation
            speed: 1000, // speed of the transition (any valid fx speed value) 
            timeout: 5000, // milliseconds between slide transitions (0 to disable auto advance) 
            pager: '#<%=ModuleControl.ModuleContext.ModuleId%>_Rotator .RotatorNav', //selector for rotator navigation
            // callback fn that creates a navigation to use as pager anchor 
            pagerAnchorBuilder: function (idx, slide) {
                return '<li><a href="#">' + slide.title + '</a></li>';
            }
        });
    });
</script>