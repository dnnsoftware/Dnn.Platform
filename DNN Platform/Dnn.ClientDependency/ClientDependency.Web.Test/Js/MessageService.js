var Messaging = {
    GetMessage: function(callback) {
        $.ajax({
            type: "POST",
            url: "/Services/MessageService.asmx/HelloWorld",
            data: "{}",
            contentType: "application/json; charset=utf-8",
            dataType: "json",
            success: function(msg) {
                callback.apply(this, [msg.d]);
            }
        });
    }
};    