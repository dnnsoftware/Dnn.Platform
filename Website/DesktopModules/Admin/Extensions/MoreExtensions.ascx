<%@ Control Language="C#" AutoEventWireup="false" Inherits="DotNetNuke.Modules.Admin.Extensions.MoreExtensions"
    CodeFile="MoreExtensions.ascx.cs" %>
<%@ Import Namespace="DotNetNuke.Entities.Icons" %>
<%@ Register TagPrefix="dnn" TagName="Label" Src="~/controls/LabelControl.ascx" %>
<%@ Register TagPrefix="dnn" Namespace="DotNetNuke.Web.UI.WebControls" Assembly="DotNetNuke.Web" %>
<div id="dnnCatalog" class="dnnForm dnnCatalog dnnClear">
    <span id="loading" class="dnnCatalogLoading">Loading...</span>
    <div class="dnnCatalogSearch">
        <div class="dnnFormItem">            
            <dnn:Label ID="dnnlblType" runat="server" ResourceKey="CatalogSearchTitle" ControlName="typeDDL" />
            <input type="text" id="searchText" title="<%=LocalizeString("SearchLabel.Help")%>" />
            <dnn:DnnComboBox ID="typeDDL" runat="server" OnClientSelectedIndexChanged="typeDDLchanged">
                <Items>
                    <dnn:DnnComboBoxItem Value="all" Text="All" />
                    <dnn:DnnComboBoxItem Value="module" Text="Module" ResourceKey="CatalogModule" />
                    <dnn:DnnComboBoxItem Value="skin" Text="Skin" ResourceKey="CatalogSkin" />
                </Items>
            </dnn:DnnComboBox>

            <div class="dnnSearchActionBar">
            <a href="javascript:void(0);" id="search-go" class="dnnPrimaryAction">
                <%=LocalizeString("Search")%></a> <a href="javascript:void(0);" id="search-reset"
                    class="dnnSecondaryAction">
                    <%=LocalizeString("ClearSearch")%></a>
            </div>
        </div>
    </div>
    <div class="dnnClear"></div>
    <div class="dnnCatalogTags">
        <h2 class="dnnGallerySubHeading">
            <%=LocalizeString("TagCloud")%></h2>
        <div class="dnnCatalogTagList dnnClear" id="tag-list">
        </div>
        <div class="dnnFormMessage dnnFormInfo">
            <% =LocalizeString("ListExtensions") %>
        </div>
    </div>
    <div class="dnnCatalogListing">
        <div id="extensionDetail">
            <div id="extensionDetailInner">
            </div>
        </div>
      <%--  <h2 class="dnnGallerySubHeading">
            <%=LocalizeString("Extensions")%></h2>--%>
        <fieldset id="extensionsSummary">
            <div class="sort-options fullWidth-Center">
                <ul class="dnnButtonGroup">
                    <li class="dnnButtonGroup-first"></li>
                    <li>
                        <span class="sort-button-off"><a href="javascript:void(0);" id="NameSorter">
                    <%=LocalizeString("NameZA") %></a></span>
                    </li>
                    <li>
                        <span class="sort-button-off"><a href="javascript:void(0);"
                        id="PriceSorter">
                        <%=LocalizeString("PriceHighLow")%></a></span>
                    </li>
                </ul>
            </div>
            <div id="searchFilters" class="dnnSearchFilters dnnClear">
            </div>
            <div id="extensionList" class="extensionList">
            </div>
        </fieldset>
    </div>
    <script id="tag-tmpl" type="text/html">
{{if tagName}}
    <a href="javascript:void(0);" style="font-size:${fontSize}%" class="tag" alt="${tagName} (${TagCount})" title="${tagName} (${TagCount})" tagId="${tagID}">${tagName}</a>&nbsp;
{{/if}}
    </script>
    <script id="eTmpl" type="text/x-jquery-tmpl">
        {{if Catalog}}    
            {{if ExtensionName}}
    	        <div class="dnnProduct ${Catalog.CatalogCSS} dnnClear ext-${ExtensionID}">    
                    <a name="ext-${ExtensionID}"></a>
                    <div class="dnnProductImage">
                        {{if ImageURL}}
                            <img alt='${Title}' src="${ImageURL}" />
                        {{else}}
                            <img  alt='${Title}' src='<%=ResolveUrl("~/images/System-Box-Empty-icon.png")%>' />
                        {{/if}}
                        <div class='dnnProductPrice'>${_gallery.FormatCurrency(Price)}</div>
                        <div class="${Catalog.CatalogCSS}">
                            <img class='productTypeImage dnnIcon' alt='${ExtensionType}'  title='${ExtensionType}'  src='<%=IconController.IconURL("Catalog${ExtensionType}")%>' />                            
                            {{if License}}                                
                                <a class="galleryLink  inline" onclick="return _gallery.ShowDetails(${ExtensionID})">                    
                                    <img class='productTypeImage dnnIcon' alt='License for ${Title}' title='License Specified' src='<%=IconController.IconURL("CatalogLicense")%>' />
                                </a>
                            {{/if}}
                            {{if DetailURL}}
                                <a class="galleryLink inline" href="${DetailURL}" target="_new"><img class='deploy galleryLink' alt='Browse ${Catalog.CatalogName}' Title='Browse ${Catalog.CatalogName}' src='${_gallery.resolveImage(Catalog.CatalogIcon)}' /></a>
                            {{else Catalog.CatalogUrl}}                                            
                                <a class="galleryLink inline" href="${Catalog.CatalogUrl}" target="_new"><img class='deploy galleryLink' alt='Browse ${Catalog.CatalogName}' Title='Browse ${Catalog.CatalogName}' src='${_gallery.resolveImage(Catalog.CatalogIcon)}' /></a>
                            {{/if}}                
                        </div>
                    </div>
                    <div class="dnnProductDetails">
                        <h3 class="dnnProductTitle">${Title}</h3>
                        <p class="dnnProductOwner">
                            {{if OwnerName}}<%=LocalizeString("By") %> <a href="javascript:void(0)" onclick="_gallery.OwnerFilterGallery('${OwnerName}')">${OwnerName}</a>{{/if}}
                        </p>
		                <div class='dnnProductDescription dnnClear'>{{html Description}}&nbsp;{{if DetailURL}}<a href="${DetailURL}" target="_details"><%=LocalizeString("ExtensionDetail")%></a>{{/if}}</div>
                        <div class="dnnProductLicense">    
                            <span class="dnnProductLicenseLabel">
                                <%=LocalizeString("License") %>
                            </span>
                            {{if License}}
                                ${License}
                            {{else}}
                                <%=LocalizeString("NotSpecified") %>
                            {{/if}}
                        </div>
                        <div class="dnnProductVersion">    
                            <span class="dnnProductVersionLabel">
                                <%=LocalizeString("Version") %>
                            </span>
                            {{if MinDnnVersion}}
                                ${MinDnnVersion}
                            {{else}}
                                <%=LocalizeString("NotSpecified") %>
                            {{/if}}
                        </div>
            

                        {{if CatalogID !== 1 && DownloadURL }}<a class="dnnSecondaryAction" href="${_gallery.getDownloadUrl(ExtensionID)}">Deploy ${Title}</a>{{/if}}
                        {{if CatalogID === 1}}<a class="dnnSecondaryAction" href="${DetailURL}&PackageOptionID=0&action=Add" target="_cart">Buy ${Title}</a>{{/if}}
            
 
                        
                   </div>
                </div>
             {{/if}}
         {{/if}}
    </script>
    <script type="text/javascript">
		var _gallery; //global scope!
		(function($){
			$(document).ready(function () {
				_gallery = new Gallery(
					{
						host: '<%=DotNetNuke.UI.Utilities.ClientAPI.GetSafeJSString(LocalizeString("feedEndpoint")) %>'
						, NameTextASC: '<%=DotNetNuke.UI.Utilities.ClientAPI.GetSafeJSString(LocalizeString("NameAZ")) %>'
						, NameTextDESC: '<%=DotNetNuke.UI.Utilities.ClientAPI.GetSafeJSString(LocalizeString("NameZA")) %>'
						, PriceTextASC: '<%=DotNetNuke.UI.Utilities.ClientAPI.GetSafeJSString(LocalizeString("PriceLowHigh")) %>'
						, PriceTextDESC: '<%=DotNetNuke.UI.Utilities.ClientAPI.GetSafeJSString(LocalizeString("PriceHighLow")) %>'
						, tagLabel: '<%=DotNetNuke.UI.Utilities.ClientAPI.GetSafeJSString(LocalizeString("TagLabel")) %>'
						, searchLabel: '<%=DotNetNuke.UI.Utilities.ClientAPI.GetSafeJSString(LocalizeString("SearchLabel")) %>'
						, vendorLabel: '<%=DotNetNuke.UI.Utilities.ClientAPI.GetSafeJSString(LocalizeString("VendorLabel")) %>'
						, extensionLabel: '<%=DotNetNuke.UI.Utilities.ClientAPI.GetSafeJSString(LocalizeString("ExtensionsLabel")) %>'
						, noneLabel: '<%=DotNetNuke.UI.Utilities.ClientAPI.GetSafeJSString(LocalizeString("NoneLabel")) %>'
						, orderLabel: '<%=DotNetNuke.UI.Utilities.ClientAPI.GetSafeJSString(LocalizeString("OrderLabel")) %>'
						, typeLabel: '<%=DotNetNuke.UI.Utilities.ClientAPI.GetSafeJSString(LocalizeString("TypeLabel")) %>'
						, errorLabel: '<%=DotNetNuke.UI.Utilities.ClientAPI.GetSafeJSString(LocalizeString("ErrorLabel")) %>'
						, loadingLabel: '<%=DotNetNuke.UI.Utilities.ClientAPI.GetSafeJSString(LocalizeString("LoadingLabel")) %>'
						, siteRoot : '<%=ResolveUrl("~/")%>'
						, DataBaseVersion : "<%=DotNetNuke.Common.Globals.DataBaseVersion%>"
						, CacheTimeoutMinutes : <%=(IsDebugEnabled() ? 0: 1440) %>
						, BaseDownLoadUrl : "<%=ModuleContext.EditUrl("ExtensionID", "{{ExtensionID}}", "Download") %>"
					});

				setTimeout(function(){
					_gallery.getCatalogs(function(){
						_gallery.Search();
					});
					_gallery.getTags();
				}, 0);
			});
		}(jQuery));

        function typeDDLchanged(sender, e){
            var v = sender.get_value();
            if(v){
                _gallery.FilterGallery2(v);
            }
        }
    </script>
</div>
