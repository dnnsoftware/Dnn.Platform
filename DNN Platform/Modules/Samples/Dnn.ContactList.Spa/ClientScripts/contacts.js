if (typeof contactList === 'undefined' || contactList === null) {
    contactList = {};
};

ko.extenders.required = function (target, options) {
    //add some sub-observables to our observable
    target.hasError = ko.observable();
    target.validationMessage = ko.observable();
    target.validationClass = ko.observable();

    var regEx = new RegExp(options.regEx);
    //define a function to do validation
    var errorMessage = options.overrideMessage || "This field is required";
    function validate(newValue) {
        var validated = regEx.test(newValue) && newValue !== "";
        target.hasError(!validated);
        target.validationClass(validated ? "form-control" : "form-control has-error");
        target.validationMessage(validated ? "" : errorMessage);
    }

    //validate whenever the value changes
    target.subscribe(validate);

    //return the original observable
    return target;
};

function clearErrors(obsArr) {
    for (var i = 0; i < obsArr.length; i++) {
        obsArr[i].hasError(false);
        obsArr[i].validationClass("form-control");
    }
}

contactList.contactsViewModel = function(config) {
    var self = this;
    var resx = config.resx;
    var util = config.util;
    var preloadedData = config.preloadedData;
    var $rootElement = config.$rootElement;

    var quickSettingsDispatcher = config.settings.quickSettingsDispatcher;
    var moduleId = config.settings.moduleId;

    util.contactService = function(){
        util.sf.serviceController = "Contact";
        return util.sf;
    };

    self.isFormEnabled = ko.observable(config.settings.isFormEnabled);
    self.isEditMode = ko.observable(false);
    self.contacts = ko.observableArray([]);
    self.totalResults = ko.observable(preloadedData.pageCount);
    self.pageIndex = ko.observable(0);

    self.selectedContact = new contactList.contactViewModel(self, config);

    var toggleView = function() {
        self.isEditMode(!self.isEditMode());
    };

    self.addContact = function(){
        toggleView();
        self.selectedContact.init();
        clearErrors([self.selectedContact.firstName, self.selectedContact.lastName, self.selectedContact.phone, self.selectedContact.email]);
    };

    self.closeEdit = function() {
        toggleView();
        self.refresh();
    }

    self.editContact = function(data, e) {
        self.getContact(data.contactId());
        toggleView();
    };

    self.getContact = function (contactId, cb) {
        var params = {
            contactId: contactId
        };

        util.contactService().get("GetContact", params,
            function(data) {
                if (typeof data !== "undefined" && data != null && data.success === true) {
                    //Success
                    self.selectedContact.load(data.data.contact);
                } else {
                    //Error
                }
            },

            function(){
                //Failure
            }
        );

        if(typeof cb === 'function') cb();
    };

    self.getContacts = function () {
        var params = {
            pageSize: self.pageSize,
            pageIndex: self.pageIndex(),
            searchTerm: ""
        };

        util.contactService().get("GetContacts", params,
            function(data) {
                if (typeof data !== "undefined" && data != null && data.success === true) {
                    //Success
                    self.totalResults(data.data.totalCount);
                    self.load(data.data);
                } else {
                    //Error
                }
            },
            function(){
                //Failure
            }
        );
    };

    var updateView = function updateViewHandler(settings) {
        self.isFormEnabled(settings.isFormEnabled);
    };

    self.init = function () {
        if (preloadedData) {
            self.load(preloadedData);
        } else {
            self.getContacts();
        }
        quickSettingsDispatcher.addSubcriber(moduleId, quickSettingsDispatcher.eventTypes.SAVE, updateView);
        pager.init(self, 5, self.refresh, resx);
    };

    self.load = function(data) {
        self.contacts.removeAll();
        for(var i=0; i < data.results.length; i++){
            var result = data.results[i];
            var contact = new contactList.contactViewModel(self, config);
            contact.load(result);
            self.contacts.push(contact);
        }
    };

    self.refresh = function() {
        self.getContacts();
    }
};

contactList.contactViewModel = function(parentViewModel, config) {
    var self = this;
    var resx = config.resx;
    var util = config.util;
    var $rootElement = config.$rootElement;

    self.parentViewModel = parentViewModel;
    self.contactId = ko.observable(-1);
    self.firstName = ko.observable('').extend({ required: { overrideMessage: "Please enter a first name" } });
    self.lastName = ko.observable('').extend({ required: { overrideMessage: "Please enter a last name" } });
    self.email = ko.observable('').extend({ required: { overrideMessage: "Please enter a valid email address", regEx: config.settings.emailRegex } });
    self.phone = ko.observable('').extend({ required: { overrideMessage: "Please enter a valid phone number in the format: 123-456-7890", regEx: /^(\+\d{1,2}\s)?\(?\d{3}\)?[\s.-]\d{3}[\s.-]\d{4}$/ } });
    self.twitter = ko.observable('');

    self.cancel = function () {
        clearErrors([self.firstName, self.lastName, self.email, self.phone]);
        parentViewModel.closeEdit();
    };

    self.deleteContact = function (data, e) {
        var opts = {
            callbackTrue: function () {
                var params = {
                    contactId: data.contactId(),
                    firstName: data.firstName(),
                    lastName: data.lastName(),
                    email: data.email(),
                    phone: data.phone(),
                    twitter: data.twitter()
                };

        util.contactService().post("DeleteContact", params,
            function(data){
                //Success
                parentViewModel.refresh();
            },

                    function (data) {
                        //Failure
                    }
                );
            },
            text: resx.DeleteConfirm,
            yesText: resx.Delete,
            noText: resx.Cancel,
            title: resx.ConfirmDeleteTitle.replace("{0}", data.firstName() + " " + data.lastName())
        };

        $.dnnConfirm(opts);

       
    };

    self.init = function(){
        self.contactId(-1);
        self.firstName("");
        self.lastName("");
        self.email("");
        self.phone("");
        self.twitter("");
    };

    self.load = function(data) {
        self.contactId(data.contactId);
        self.firstName(data.firstName);
        self.lastName(data.lastName);
        self.email(data.email);
        self.phone(data.phone);
        self.twitter(data.twitter);
    };

    self.saveContact = function (data, e) {
        self.firstName.valueHasMutated();
        self.lastName.valueHasMutated();
        self.phone.valueHasMutated();
        self.email.valueHasMutated();
        if ((self.firstName.hasError() || self.lastName.hasError() || self.email.hasError() || self.phone.hasError())) {
            return;
        }
        var params = {
            contactId: data.contactId(),
            firstName: data.firstName(),
            lastName: data.lastName(),
            email: data.email(),
            phone: data.phone(),
            twitter: data.twitter()
        };

        util.contactService().post("SaveContact", params,
            function(data){
                //Success
                self.cancel();
            },

            function(data){
                //Failure
            }
        )


    };
};

