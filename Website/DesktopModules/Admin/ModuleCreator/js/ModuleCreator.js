function moduleCreatorPost(moduleid, method, data, node, callback) {
    sf = $.ServicesFramework(moduleid);
    $.ajax({
        type: "POST",
        beforeSend: sf.setModuleHeaders,
        data: data,
        url: sf.getServiceRoot('DotNetNuke.ModuleCreator') + "ModuleCreatorAPI/" + method
    }).done(function (data) {
        if (typeof (callback) != "undefined") {
            callback(node, data);
        }
    }).fail(function (xhr, result, status) {
        alert("Uh-oh, something broke: " + status);
    });
};