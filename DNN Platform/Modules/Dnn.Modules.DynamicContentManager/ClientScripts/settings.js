if (typeof dcc === 'undefined' || dcc === null) {
    dcc = {};
};

dcc.settings = (function(ko, resx, settings){
    var heading = ko.observable(resx.settings);

    return {
        heading: heading
    }
})

