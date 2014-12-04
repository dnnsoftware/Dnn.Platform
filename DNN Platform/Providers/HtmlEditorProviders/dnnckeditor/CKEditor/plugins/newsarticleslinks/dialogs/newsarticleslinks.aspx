<%@ Page Language="C#" AutoEventWireup="true" CodeFile="newsarticleslinks.aspx.cs" Inherits="WatchersNET.CKEditor.NewsArticlesLinks" %>

<!DOCTYPE html>
<html lang="en">
  <head>
   <title>News Articles Module Article Link Selector</title>
   <style type="text/css">
     body { font: normal 12px Arial,Helvetica,Tahoma,Verdana,Sans-Serif; }
   </style>
  </head>
 <body>
     <form id="DialogForm" runat="server">
      <div>
        <p><%= DotNetNuke.Services.Localization.Localization.GetString("NewsArcticlesModuleList.Text", this.ResXFile, this.LangCode) %></p>
	    <p style="margin-left:10px;"><asp:DropDownList runat="server" id="ModuleListDropDown" AutoPostBack="True"></asp:DropDownList></p>
      </div>
      <div>
        <p><%= DotNetNuke.Services.Localization.Localization.GetString("NewsArcticlesArticlesList.Text", this.ResXFile, this.LangCode) %></p>
        <p style="margin-left:10px;"><asp:DropDownList runat="server" id="ArticlesList"></asp:DropDownList></p>
      </div>
	</form>
  </body>
</html>
