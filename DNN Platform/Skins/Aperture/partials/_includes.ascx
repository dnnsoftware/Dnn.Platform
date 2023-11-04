<dnn:META ID="mobileScale" runat="server" Name="viewport" Content="width=device-width, initial-scale=1.0" />
<dnn:DnnCssExclude runat="server" Name="dnndefault" /> 

<dnn:DnnCssInclude runat="server" FilePath="css/style.min.css" Priority="110" PathNameAlias="SkinPath" />

<dnn:DnnJsInclude runat="server" FilePath="js/skin.min.js" ForceProvider="DnnFormBottomProvider" Priority="110" PathNameAlias="SkinPath" />

<script runat="server">
    protected void Page_Init()
    {
        var fonts = new string[]
        {
            "fonts/Ubuntu-Bold",
            "fonts/Ubuntu-BoldItalic",
            "fonts/Ubuntu-Italic",
            "fonts/Ubuntu-Light",
            "fonts/Ubuntu-LightItalic",
            "fonts/Ubuntu-Medium",
            "fonts/Ubuntu-MediumItalic",
            "fonts/Ubuntu-Regular"
        };

        var types = new Dictionary<string, string>();
        types.Add("woff2", "font/woff2");
        types.Add("woff", "font/woff");

        var defaultPage = (CDefault)this.Page;

        foreach (var type in types)
        {
            foreach (var font in fonts)
            {
                var fontLink = new HtmlLink();
                fontLink.Attributes.Add("rel", "preload");
                fontLink.Attributes.Add("as", "font");
                fontLink.Href = this.SkinPath + font + "." + type.Key;
                fontLink.Attributes.Add("type", type.Value);
                fontLink.Attributes.Add("crossorigin", "anonymous");

                defaultPage.Header.Controls.Add(fontLink);
            }
        }
    }
</script>