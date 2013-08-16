<%@ Control Language="C#" %>
<%@ Register Assembly="Telerik.Web.UI" Namespace="Telerik.Web.UI" TagPrefix="telerik" %>
<%@ Register Assembly="Telerik.Web.UI" Namespace="Telerik.Web.UI.Editor.DialogControls" TagPrefix="dc" %>
<script type="text/javascript">
    Type.registerNamespace("Telerik.Web.UI.Widgets");

    Telerik.Web.UI.Widgets.DocumentPreviewer = function (element) {
        Telerik.Web.UI.Widgets.DocumentPreviewer.initializeBase(this, [element]);

        this._currentItem = null;
        this._linkManager = null;
        this._originalText = "";
    }

    Telerik.Web.UI.Widgets.DocumentPreviewer.prototype = {
        initialize: function () {
            Telerik.Web.UI.Widgets.DocumentPreviewer.callBaseMethod(this, 'initialize');
            this._initializeChildren();
        },

        _initializeChildren: function () {
            this._linkManager = $find("linkManager");
            this._linkManager._trackingDiv.style.display = "none";
            this.get_element().style.display = "none";
        },

        _setLinkManagerItem: function (item, linkManagerParameters) {
            var link = linkManagerParameters.get_value();
            link.href = item.get_url().replace("#", "%23");
            if (this._originalText && this._originalText == link.innerHTML) {
                link.innerHTML = "";
            }
            if (linkManagerParameters.showText && link.innerHTML == "") {
                var fileName = item.get_name();
                link.innerHTML = fileName;
                this._originalText = link.innerHTML;
            }
            this._linkManager.clientInit(linkManagerParameters);
        },

        setItem: function (item) {
            this._currentItem = item;

            if (item.get_type() == Telerik.Web.UI.FileExplorerItemType.Directory) {
                this.get_element().style.display = "none";
            }
            else {
                this.get_element().style.display = "";
                var linkManagerParameters = this.get_browser().get_clientParameters();
                if (!linkManagerParameters) {
                    //call function with a small timeout, waiting for clientInit.
                    //this can happen after upload when the FileExplorer selects an item before the browser is initialized;
                    window.setTimeout(Function.createDelegate(this, function () {
                        var linkManagerParameters = this.get_browser().get_clientParameters();
                        this._setLinkManagerItem(item, linkManagerParameters);
                    }), 150);
                }
                else {
                    this._setLinkManagerItem(item, linkManagerParameters);
                }
            }
        },

        getResult: function () {

            var newLink = this._linkManager.getModifiedLink();
            return newLink;
        },

        getItemLink: function () {
            if (this._currentItem && this._currentItem.get_type() == Telerik.Web.UI.FileExplorerItemType.File) {
                var returnLink = document.createElement("A");
                returnLink.innerHTML = this._currentItem.get_name();
                returnLink.href = this._currentItem.get_url();
                return returnLink;
            }
            return null;
        },

        dispose: function () {
            this._linkManager = null;
            this._currentItem = null;
            Telerik.Web.UI.Widgets.DocumentPreviewer.callBaseMethod(this, 'dispose');
        }
    }

    Telerik.Web.UI.Widgets.DocumentPreviewer.registerClass('Telerik.Web.UI.Widgets.DocumentPreviewer', Telerik.Web.UI.Widgets.FilePreviewer);
</script>
<div id="DocumentPreviewer" class="documentPreviewer">
	<telerik:RadToolBar ID="EmptyToolbar" runat="Server" Height="26px" Width="100%">
	</telerik:RadToolBar>
	<dc:LinkManagerDialog ID="linkManager" runat="server" StandAlone="false"></dc:LinkManagerDialog>
</div>