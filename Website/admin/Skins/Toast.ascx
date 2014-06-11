<%@ Control Language="C#" AutoEventWireup="false" Inherits="DotNetNuke.UI.Skins.Controls.Toast" CodeFile="Toast.ascx.cs" %>
<script type="text/javascript">
    $(document).ready(function () {

        if (typeof dnn == 'undefined') dnn = {};
        if (typeof dnn.toast == 'undefined') dnn.toast = {};
        var sf = $.ServicesFramework();

        dnn.toast.refreshUser = function () {
            $.ajax({
                type: "GET",
                url: sf.getServiceRoot('InternalServices') + 'NotificationsService/' + 'GetToasts',
                contentType: "application/json",
                dataType: "json",
                cache: false,
                success: function (data) {
                    if (!data || !data.Success) {
                        if (typeof dnn.toast.toastTimer !== 'undefined') {
                            // Cancel the periodic update.
                            clearTimeout(dnn.toast.toastTimer);
                            delete dnn.toast.toastTimer;
                        }
                        return;
                    }

                    var toastMessages = [];

                    for (var i = 0; i < data.Toasts.length; i++) {
                        var toast = {
                            subject: data.Toasts[i].Subject,
                            body: data.Toasts[i].Body
                        };

                        toastMessages.push(toast);
                    }

                    var message = {
                        messages: toastMessages,
                        seeMoreLink: '<%= GetNotificationLink() %>', seeMoreText: '<%= Localization.GetSafeJSString(GetNotificationLabel()) %>'
                    };

                    $().dnnToastMessage('showAllToasts', message);

                    dnn.toast.toastTimer = setTimeout(dnn.toast.refreshUser, 30000);
                },
                error: function (xhr, status, error) {
                    if (typeof dnn.toast.toastTimer !== 'undefined') {
                        // Cancel the periodic update.
                        clearTimeout(dnn.toast.toastTimer);
                        delete dnn.toast.toastTimer;
                    }
                }
            });
        };

        function checkLogin() {
            return '<%= IsOnline() %>' === 'True';
        };

        // initial setup for toast
        var pageUnloaded = window.dnnModal && window.dnnModal.pageUnloaded;
        if (checkLogin() && !pageUnloaded) {
            dnn.toast.toastTimer = setTimeout(dnn.toast.refreshUser, 30000);
            dnn.toast.refreshUser();
        }
    });

</script>