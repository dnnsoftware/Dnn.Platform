function Manager($, ko, settings, resx){
    //var serviceFramework = settings.servicesFramework;
    //var baseServicepath = serviceFramework.getServiceRoot('Dnn/DynamicContentManager') + 'ContentManager/';
    var $rootElement;
    var activePanel;

    var init = function(element) {
        $rootElement = $(element);

        var menuButtons = $rootElement.find(".navbar .menu ul li");
        menuButtons.click(menuClick);
        activePanel = settings.initialPanel;;
    }

    var menuClick = function() {
        var $self = $(this);
        var panelId = $self.attr("data-panel-id")

        if(activePanel == panelId){
            return;
        }

        //slide panels in
        $(activePanel).animate({opacity: 0}, 1500, function(){
            $(this).offset({ left: -850 });
            $(this).css("opacity", 1);
        });
        $(panelId).animate({ left: 0}, 1500);

        activePanel = panelId;
    };

    return {
        init: init
    }
}