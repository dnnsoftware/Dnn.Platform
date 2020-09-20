jQuery.PageMethod = function (n, t, i, r) {
    var u, e, f;
    if (n == null && (n = window.location.pathname, n.lastIndexOf("/") == n.length - 1 && (n = n + "default.aspx")), u = "", e = arguments.length, e > 4) for (f = 4; f < e - 1; f += 2) u.length != 0 && (u += ","), u += '"' + arguments[f] + '":"' + arguments[f + 1] + '"';
    return u = "{" + u + "}", jQuery.PageMethodToPage(n, t, i, r, u)
}, jQuery.PageMethodToPage = function (n, t, i, r, u) {
    jQuery.ajax({
        type: "POST",
        url: n + "/" + t,
        contentType: "application/json; charset=utf-8",
        data: u,
        dataType: "json",
        success: i,
        error: r
    })
}