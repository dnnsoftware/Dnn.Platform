// The following are supported template features:
//
// Tokens:      A token is a namespace-prefixed RSS element name surrounded by double brackets.
//              Tokens are substituted with the actual value found for the corresponding element
//              in the RSS feed item.
//
//              For example:  [[title]]   will be replaced with the RSS feed item title.
//
// Functions:   If additional processing is required for each item, it is better to call a function
//              that returns the templatized tokens. This is done by making the template value be a 
//              call to a function with the tokens as parameters. When rendering an item, token values
//              will be replaced and then the function will be eval'd. A function is indicated by @@
//              in the first two character positions of the template.
//
//              For example: @@test("[[title]]","[[link]]")
//

var htmlTemplates = [];
var htmlHeaders = [];

// Default template
htmlTemplates['default'] = 

'<table width=\"100%\">' +
'<tr>' +
     '<td class=\"Head\" align=\"left\" valign=\"middle\" style=\"padding-bottom:10px\"><a href=\"[[link]]\" target=\"_new\" class=\"Head\">[[title]]</a></td>' +
'</tr>' + 
'<tr>' + 
      '<td align=\"left\" valign=\"top\" class=\"Normal\">' + 
            '[[description]]' + 
       '</td>' +
 '</tr>' + 
'</table><br /><br />';



// Marketplace template
htmlTemplates['marketplace'] = 
'@@marketplaceItemRenderer("[[title]]","[[link]]","[[description]]","[[enclosure.url]]",[[dnnmp:isReviewed]],"[[dnnmp:overview]]","[[dnnmp:vendor]]","[[dnnmp:vendorLink]]","[[dnnmp:price]]".split(".")[0])';


// DotNetNuke template
htmlTemplates['dotnetnuke'] = 

'<table width=\"100%\">' +
'<tr>' +
     '<td class=\"Head\" align=\"left\" valign=\"middle\" style=\"background-color:#cc0000;padding:5px\"><a href=\"[[link]]\" target=\"_new\" class=\"Head\" style=\"color:white\">[[title]]</a></td>' +
'</tr>' + 
'<tr>' + 
      '<td align=\"left\" valign=\"top\" class=\"Normal\" style=\"padding-top:5px\">' + 
            '[[description]]' + 
       '</td>' +
 '</tr>' + 
'</table><br /><br />';

// Hosting template
htmlTemplates['hosting'] = 

'<table width=\"100%\">' +
'<tr>' +
     '<td class=\"Head\" align=\"left\" valign=\"middle\" style=\"padding-bottom:10px\"><a href=\"[[link]]\" target=\"_new\" class=\"Head\">[[title]]</a></td>' +
'</tr>' + 
'<tr>' + 
      '<td align=\"left\" valign=\"top\" class=\"Normal\">' + 
            '[[description]]' + 
       '</td>' +
 '</tr>' + 
'</table><br /><br />';


htmlHeaders['dotnetnuke'] = '<div style=\"padding-bottom:5px;position:relative;top:-15px\"><a href=\"http://www.dotnetnuke.com\" target=\"_new\"><img src=\"http://www.dotnetnuke.com/portals/25/SolutionsExplorer/images/DNN-small.gif\" border=\"0\"></a></div>';
htmlHeaders['marketplace'] = '<div style=\"padding-bottom:5px;position:relative;top:-25px\"><a href=\"http://marketplace.dotnetnuke.com\" target=\"_new\"><img src=\"http://www.dotnetnuke.com/portals/25/SolutionsExplorer/images/DNNMarketplace-small.gif\" border=\"0\"></a></div>';

// Used by Marketplace template
function marketplacePreviewHandler(url)
{
    var previewBrowser = $get("PreviewBrowser");
    previewBrowser.contentWindow.location.replace(url);
    previewBrowser.style.visibility = "hidden";
    var preview = $get("PreviewContainer");
    preview.style.display = "block";
    previewBrowser.style.visibility = "visible";
}

function marketplaceItemRenderer(title, url, description, imageUrl, isReviewed, overviewUrl, vendor, vendorUrl, price)
{
    var itemTemplate = '';
    
    if (isReviewed)
    {
        itemTemplate +=  
        '<!-- 0: Reviewed Product -->' + // Add a comment to force reviewed products to be grouped first
        '<table width=\"100%\" style=\"margin-bottom:15px\">' +
        '<tr>' +
             '<td colspan=\"2\" class=\"Head\" align=\"left\" valign=\"middle\" style=\"padding:5px;background-color:#ededed\"><img src=\"http://marketplace.dotnetnuke.com/reviewprogram/logos/reviewed-tiny.gif\" align="right"><a href=\"' + url + '\" target=\"_new\" class=\"Head\">' + title + '</a> <span class=\"Normal\"><br /><small> by <a href=\"' + vendorUrl + '\" target=\"_new\" class=\"Normal\">' + vendor + '</a></small></span></td>' +
        '</tr>' +
        '<tr>' + 
              '<td align=\"center\" valign=\"top\" class=\"Normal\" style=\"width:100px;padding-top:10px\">' +
                 '<img src=\"' + imageUrl + '\" width=\"100\">' + 
                 '<br /><p>from $' + price + '</p>' +
                 '<p class=\"Head\">' + 
                 '<a href=\"javascript:void(0)\" onClick=\"marketplacePreviewHandler(\'' + overviewUrl + '\')\">Info</a> | ' +
                 '<a href=\"' + url + '\" target=\"_new\">BUY</a> <br />' +
                 '</p>'+ 
              '</td>' +
              '<td align=\"left\" valign=\"top\" class=\"Normal\" style=\"padding-left:10px;padding-top:10px\">' + 
                    description +  
               '</td>' +
         '</tr>' +
         '</table>';                
    } 
    else
    {
        itemTemplate +=  
        '<!-- 1: Non-Reviewed Product -->' + // Add a comment to force non-reviewed products to be grouped first
        '<table width=\"100%\" cellspacing=\"0\" style=\"margin-bottom:15px\">' +
        '<tr>' +
             '<td class=\"Head\" width=\"80%\"align=\"left\" valign=\"middle\" style=\"padding:5px;background-color:#f3f3f3\"><a href=\"' + url + '\" target=\"_new\" class=\"SubHead\">' + title + '</a> <span class=\"Normal\"><br/><small> by <a href=\"' + vendorUrl + '\" target=\"_new\" class=\"Normal\">' + vendor + '</a></small></td>' + 
             '<td class=\"Normal\" width=\"20%\" valign=\"middle\" style=\"padding:5px;background-color:#f3f3f3\">from $' + price + ' <a class=\"SubHead\" href=\"' + url + '\" target=\"_new\">BUY</a></span></td>' +
         '</tr>';
         
         var firstSentence = description.indexOf(". ");
         if (firstSentence > -1)
            itemTemplate += 
            '<tr>' +
              '<td colspan=\"2\" align=\"left\" valign=\"top\" class=\"Normal\" style=\"padding-top:10px\">' + 
                    description.substr(0, firstSentence+1) + 
               '</td>' +
         '</tr>' +
         '</table>';                
    }

    return(itemTemplate);
}
