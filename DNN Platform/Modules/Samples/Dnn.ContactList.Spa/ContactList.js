function ContactList($, ko, settings, resx, preloadedData){
    var $rootElement;

    var viewModel = {};

    var init = function(element) {
        $rootElement = $(element);

        var config = {
            settings: settings,
            preloadedData: preloadedData,
            resx: resx,
            util: contactList.utility(settings, resx),
            $rootElement: $rootElement
        };

        viewModel = new contactList.contactsViewModel(config);
        viewModel.init();

        ko.applyBindings(viewModel, $rootElement[0]);
    }

    return {
        init: init
    }
}