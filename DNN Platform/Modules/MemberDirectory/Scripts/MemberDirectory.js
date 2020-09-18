function MemberDirectory($, ko, settings, composeMessageSettings) {
    var opts = $.extend({}, MemberDirectory.defaultSettings, settings);
    var serviceFramework = settings.servicesFramework;
    var baseServicepath = serviceFramework.getServiceRoot('MemberDirectory') + 'MemberDirectory/';
    var pageIndex = 0;
    var pageSize = opts.pageSize;
    var userId = opts.userId;
    var groupId = opts.groupId;
    var viewerId = opts.viewerId;
    var profileUrl = opts.profileUrl;
    var profileUrlUserToken = opts.profileUrlUserToken;
    var profilePicHandler = opts.profilePicHandler;
    var containerElement = null;

    function displayMessage(message, cssclass) {
        var messageNode = $("<div/>").addClass('dnnFormMessage ' + cssclass).text(message);
        $(containerElement).prepend(messageNode);
        messageNode.fadeOut(3000, 'easeInExpo', function () { messageNode.remove(); });
    };

    function Member(item) {
        var self = this;
        self.AddFriendText = opts.addFriendText;
        self.AcceptFriendText = opts.acceptFriendText;
        self.FriendPendingText = opts.friendPendingText;
        self.RemoveFriendText = opts.removeFriendText;
        self.FollowText = opts.followText;
        self.UnFollowText = opts.unFollowText;
        self.SendMessageText = opts.sendMessageText;
        self.UserNameText = opts.userNameText;
        self.EmailText = opts.emailText;
        self.CityText = opts.cityText;

        self.UserId = ko.observable(item.MemberId);
        self.LastName = ko.observable(item.LastName);
        self.FirstName = ko.observable(item.FirstName);
        self.UserName = ko.observable(item.UserName);
        self.DisplayName = ko.observable(item.DisplayName);
        self.Email = ko.observable(item.Email);

        self.IsUser = ko.observable(item.MemberId == viewerId);

        self.IsAuthenticated = (viewerId > 0);

        //Friend Observables
        self.FriendStatus = ko.observable(item.FriendStatus);
        self.FriendId = ko.observable(item.FriendId);

        self.IsFriend = ko.computed(function () {
            return self.FriendStatus() == 2;
        }, this);

        self.IsPending = ko.computed(function () {
            return self.FriendStatus() == 1 && self.FriendId() != viewerId;
        }, this);

        self.HasPendingRequest = ko.computed(function () {
            return self.FriendStatus() == 1 && self.FriendId() == viewerId;
        }, this);

        //Following Observables
        self.FollowingStatus = ko.observable(item.FollowingStatus);

        self.IsFollowing = ko.computed(function () {
            return self.FollowingStatus() == 2;
        }, this);

        //Follower Observables
        self.FollowerStatus = ko.observable(item.FollowerStatus);

        self.IsFollower = ko.computed(function () {
            return self.FollowerStatus() == 2;
        }, this);

        //Computed Profile Observables
        self.City = ko.observable(item.City);
        self.Title = ko.observable(item.Title);
        self.Country = ko.observable(item.Country);
        self.Phone = ko.observable(item.Phone);
        self.Website = ko.observable(item.Website);
        self.PhotoURL = ko.observable(item.PhotoURL);
        self.ProfileProperties = ko.observable(item.ProfileProperties);
        self.ProfileUrl  = ko.observable(item.ProfileUrl);
        self.Location = ko.computed(function () {
            var city = self.City();
            var country = self.Country();
            var location = (city != null) ? city : '';
            if (location != '' && country != null && country != '') {
                location += ', ';
            }
            if (country != null) {
                location += country;
            }

            return location;
        });
        
        self.getProfilePicture = function (w, h) {
            return profilePicHandler.replace("{0}", self.UserId()).replace("{1}", h).replace("{2}", w);
        };

        //Actions
        self.acceptFriend = function () {
            $.ajax({
                type: "POST",
                cache: false,
                url: baseServicepath + 'AcceptFriend',
                beforeSend: serviceFramework.setModuleHeaders,
                data: { friendId: self.UserId }
            }).done(function (data) {
                if (data.Result === "success") {
                    self.FriendStatus(2);
                } else {
                    displayMessage(settings.serverErrorText, "dnnFormWarning");
                }
            }).fail(function (xhr, status) {
                displayMessage(settings.serverErrorWithDescriptionText + status, "dnnFormWarning");
            });
        };

        self.addFriend = function () {
            $.ajax({
                type: "POST",
                cache: false,
                url: baseServicepath + 'AddFriend',
                beforeSend: serviceFramework.setModuleHeaders,
                data: { friendId: self.UserId }
            }).done(function (data) {
                if (data.Result === "success") {
                    self.FriendStatus(1);
                    self.FriendId(self.UserId);
                } else {
                    displayMessage(settings.serverErrorText, "dnnFormWarning");
                }
            }).fail(function (xhr, status) {
                displayMessage(settings.serverErrorWithDescriptionText + status, "dnnFormWarning");
            });
        };

        self.follow = function () {
            $.ajax({
                type: "POST",
                cache: false,
                url: baseServicepath + 'Follow',
                beforeSend: serviceFramework.setModuleHeaders,
                data: { followId: self.UserId }
            }).done(function (data) {
                if (data.Result === "success") {
                    self.FollowingStatus(2);
                } else {
                    displayMessage(settings.serverErrorText, "dnnFormWarning");
                }
            }).fail(function (xhr, status) {
                displayMessage(settings.serverErrorWithDescriptionText + status, "dnnFormWarning");
            });
        };

        self.removeFriend = function () {
            $.ajax({
                type: "POST",
                cache: false,
                url: baseServicepath + 'RemoveFriend',
                beforeSend: serviceFramework.setModuleHeaders,
                data: { friendId: self.UserId }
            }).done(function (data) {
                if (data.Result === "success") {
                    self.FriendStatus(0);
                } else {
                    displayMessage(settings.serverErrorText, "dnnFormWarning");
                }
            }).fail(function (xhr, status) {
                displayMessage(settings.serverErrorWithDescriptionText + status, "dnnFormWarning");
            });
        };

        self.unFollow = function () {
            $.ajax({
                type: "POST",
                cache: false,
                url: baseServicepath + 'UnFollow',
                beforeSend: serviceFramework.setModuleHeaders,
                data: { followId: self.UserId }
            }).done(function (data) {
                if (data.Result === "success") {
                    self.FollowingStatus(0);
                } else {
                    displayMessage(settings.serverErrorText, "dnnFormWarning");
                }
            }).fail(function (xhr, status) {
                displayMessage(settings.serverErrorWithDescriptionText + status, "dnnFormWarning");
            });
        };
    };

    function MemberDirectoryViewModel(initialData) {
        // Data
        var self = this;

        var initialMembers = $.map(initialData, function (item) { return new Member(item); });

        self.Visible = ko.observable(true);
        self.Members = ko.observableArray(initialMembers);
        self.CanLoadMore = ko.observable(initialMembers.length == pageSize);
        self.SearchTerm = ko.observable('');
		self.disablePrivateMessage = ko.observable(settings.disablePrivateMessage);

        self.ResetEnabled = ko.observable(false);

        self.HasMembers = ko.computed(function () {
            return self.Members().length > 0;
        }, this);

        self.AdvancedSearchTerm1 = ko.observable('');
        self.AdvancedSearchTerm2 = ko.observable('');
        self.AdvancedSearchTerm3 = ko.observable('');
        self.AdvancedSearchTerm4 = ko.observable('');

        self.LastSearch = ko.observable('Advanced');

        self.loadingData = ko.observable(false);

        //Action Methods
        self.advancedSearch = function () {
            pageIndex = 0;
            self.SearchTerm('');

            self.xhrAdvancedSearch();
            self.LastSearch('Advanced');
            self.ResetEnabled(true);
        };

        self.basicSearch = function () {
            pageIndex = 0;
            self.AdvancedSearchTerm1('');
            self.AdvancedSearchTerm2('');
            self.AdvancedSearchTerm3('');
            self.AdvancedSearchTerm4('');

            self.xhrBasicSearch();

            self.LastSearch('Basic');
            self.ResetEnabled(true);
        };

        self.getMember = function (item) {
            self.SearchTerm(item.value);
            $.ajax({
                type: "GET",
                cache: false,
                url: baseServicepath + "GetMember",
                beforeSend: serviceFramework.setModuleHeaders,
                data: {
                    userId: item.userId
                }
            }).done(function (members) {
                if (typeof members !== "undefined" && members != null) {
                    var mappedMembers = $.map(members, function (member) { return new Member(member); });
                    self.Members(mappedMembers);
                    self.CanLoadMore(false);
                } else {
                    displayMessage(settings.serverErrorText, "dnnFormWarning");
                }
            }).fail(function (xhr, status) {
                displayMessage(settings.serverErrorWithDescriptionText + status, "dnnFormWarning");
            });
        };

        self.getSuggestions = function (term, response) {
            $.ajax({
                type: "GET",
                cache: false,
                url: baseServicepath + "GetSuggestions",
                beforeSend: serviceFramework.setModuleHeaders,
                data: {
                    groupId: groupId,
                    displayName: term
                }
            }).done(function (data) {
                response(data);
            }).fail(function () {
                displayMessage(settings.searchErrorText, "dnnFormWarning");
                response({}); // From jQuery UI docs: You must always call the response callback even if you encounter an error
            });
        };

        self.handleAfterRender = function (elements, data) {
            for (var i in elements) {
                var element = elements[i];
                if (element.nodeType == 1) {
                    var config = {
                        over: function () {
                            $(this).find("div[id$='popUpPanel']").fadeIn("slow");
                        },
                        timeout: 500,
                        out: function () {
                            $(this).find("div[id$='popUpPanel']").fadeOut("fast");
                        }
                    };
                    $(element).hoverIntent(config);
                }
            }
        };

        self.isEven = function (item) {
            return (self.Members.indexOf(item) % 2 == 0);
        };

        self.loadMore = function () {
            pageIndex++;
            if (self.LastSearch() === 'Advanced') {
                self.xhrAdvancedSearch();
            }
            else {
                self.xhrBasicSearch();
            }
        };

        self.resetSearch = function () {
            self.SearchTerm('');
            self.AdvancedSearchTerm1('');
            self.AdvancedSearchTerm2('');
            self.AdvancedSearchTerm3('');
            self.AdvancedSearchTerm4('');

            self.xhrAdvancedSearch();
            self.LastSearch('Advanced');
            self.ResetEnabled(false);
        };

        self.xhrAdvancedSearch = function () {
            $.ajax({
                type: "GET",
                cache: false,
                url: baseServicepath + "AdvancedSearch",
                beforeSend: serviceFramework.setModuleHeaders,
                data: {
                    userId: userId,
                    groupId: groupId,
                    pageIndex: pageIndex,
                    pageSize: pageSize,
                    searchTerm1: self.AdvancedSearchTerm1(),
                    searchTerm2: self.AdvancedSearchTerm2(),
                    searchTerm3: self.AdvancedSearchTerm3(),
                    searchTerm4: self.AdvancedSearchTerm4()
                }
            }).done(function (members) {
                if (typeof members !== "undefined" && members != null) {
                    var mappedMembers = $.map(members, function (item) { return new Member(item); });
                    if (pageIndex === 0) {
                        self.Members(mappedMembers);
                    } else {
                        for (var i = 0; i < mappedMembers.length; i++) {
                            self.Members.push(mappedMembers[i]);
                        }
                    }
                    self.CanLoadMore(mappedMembers.length == pageSize);
                } else {
                    displayMessage(settings.searchErrorText, "dnnFormWarning");
                }
            }).fail(function (xhr, status) {
                displayMessage(settings.searchErrorText, "dnnFormWarning");
            });
        };

        self.xhrBasicSearch = function () {
            $.ajax({
                type: "GET",
                cache: false,
                url: baseServicepath + "BasicSearch",
                beforeSend: serviceFramework.setModuleHeaders,
                data: {
                    groupId: groupId,
                    searchTerm: self.SearchTerm(),
                    pageIndex: pageIndex,
                    pageSize: pageSize
                }
            }).done(function (members) {
                if (typeof members !== "undefined" && members != null) {
                    var mappedMembers = $.map(members, function (item) { return new Member(item); });
                    if (pageIndex === 0) {
                        self.Members(mappedMembers);
                    } else {
                        for (var i = 0; i < mappedMembers.length; i++) {
                            self.Members.push(mappedMembers[i]);
                        }
                    }
                    self.CanLoadMore(mappedMembers.length == pageSize);
                } else {
                    displayMessage(settings.searchErrorText, "dnnFormWarning");
                }
            }).fail(function (xhr, status) {
                displayMessage(settings.searchErrorText, "dnnFormWarning");
            });
        };
    };

    this.init = function (element) {
        containerElement = element;

        //load initial state of inbox
        $.ajax({
            type: "GET",
            cache: false,
            url: baseServicepath + "AdvancedSearch",
            beforeSend: serviceFramework.setModuleHeaders,
            data: {
                userId: userId,
                groupId: groupId,
                pageIndex: pageIndex,
                pageSize: pageSize,
                searchTerm1: '',
                searchTerm2: '',
                searchTerm3: '',
                searchTerm4: ''
            }
        }).done(function (members) {
            if (typeof members !== "undefined" && members != null) {
                var viewModel = new MemberDirectoryViewModel(members);
                ko.applyBindings(viewModel, document.getElementById($(element).attr("id")));

                //Basic Search
                $('input#mdBasicSearch').autocomplete({
                    source: function (request, response) {
                        viewModel.getSuggestions(request.term, response);
                        return;
                    },
                    minLength: 1,
                    select: function (event, ui) {
                        viewModel.getMember(ui.item);
                    }
                });

                //Advanced Search
                $('a#mdAdvancedSearch').click(function (event) {
                    event.preventDefault();
                    $('div#mdAdvancedSearchForm').slideDown();
                    $(this).addClass("active");
                    $(".mdSearch").addClass("active");
                });
                
                var timer;
                var cursorIsOnAdvancedSearchForm;
                $('a#mdAdvancedSearch').mouseleave(function () {
                    timer = setTimeout(function () {
                        if ($('div#mdAdvancedSearchForm').is(':visible') && !cursorIsOnAdvancedSearchForm) {
                            $('div#mdAdvancedSearchForm').hide();
                            $(this).removeClass("active");
                            $(".mdSearch").removeClass("active");
                        }
                    }, 150);

                });
                $('div#mdAdvancedSearchForm').mouseenter(function () {
                    cursorIsOnAdvancedSearchForm = true;
                });
                $('div#mdAdvancedSearchForm').mouseleave(function () {
                    clearTimeout(timer);
                    cursorIsOnAdvancedSearchForm = false;
                    $(this).hide();
                    $('a#mdAdvancedSearch').removeClass("active");
                    $(".mdSearch").removeClass("active");
                });

            	//Compose Message
	            if (!settings.disablePrivateMessage) {
		            var options = $.extend({}, {
			            openTriggerSelector: containerElement + " .ComposeMessage",
			            onPrePopulate: function(target) {
				            var context = ko.contextFor(target);
				            var prePopulatedRecipients = [{ id: "user-" + context.$data.UserId(), name: context.$data.DisplayName() }];
				            return prePopulatedRecipients;
			            },
			            servicesFramework: serviceFramework
		            }, composeMessageSettings);
		            $.fn.dnnComposeMessage(options);
	            }
            } else {
                displayMessage(settings.serverErrorText, "dnnFormWarning");
            }
        }).fail(function (xhr, status) {
            displayMessage(settings.serverErrorWithDescriptionText + status, "dnnFormWarning");
        });
    };
}

MemberDirectory.defaultSettings = {
    userId: -1,
    groupId: -1,
    viewerId: -1,
    profileUrl: "",
    profileUrlUserToken: "PROFILEUSER",
    addFriendText: "AddFriend",
    acceptFriendText: "AcceptFriend",
    friendPendingText: "FriendPendingText",
    removeFriendText: "RemoveFriendText",
    followText: "FollowText",
    unFollowText: "UnFollowText",
    sendMessageText: "SendMessageText",
    userNameText: "UserNameText",
    emailText: "EmailText",
    cityText: "CityText",
    serverErrorText: "ServerErrorText",
    serverErrorWithDescriptionText: "ServerErrorWithDescriptionText"
};
