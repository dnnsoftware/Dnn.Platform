export default class DnnSf {
    moduleRoot = "";
    controller = "";
    siteRoot = "";
    tabId = -1;
    antiForgeryToken = "";
    
    constructor() {
        this.siteRoot = window.top.dnn.getVar("sf_siteRoot", "/");
        this.tabId = parseInt(window.top.dnn.getVar("sf_tabId", "-1"));
        this.antiForgeryToken = window.top.document.querySelector("input[name=__RequestVerificationToken]").value;
    }

    get = (method, params, success, failure, loading, beforeSend) => {
        return this.call("GET", method, params, success, failure, loading, beforeSend, false, false);
    }

    call = (httpMethod, method, params, success, failure, loading, beforeSend, sync, silence, postFile) => {
        let url = this.getServiceRoot() + this.controller + "/" + method;
        this.moduleRoot = "personaBar";
        this.controller = "";

        return this.rawCall(httpMethod, url, params, success, failure, loading, beforeSend, sync, silence, postFile);
    }

    getServiceRoot = () => {
        return this.siteRoot + "API/" + this.moduleRoot + "/";
    }

    rawCall = (httpMethod, url, params, success, failure, loading, beforeSend, sync, silence, postFile) => {
        let beforeCallback;
        if (typeof beforeSend === "function") {
            beforeCallback = (xhr) => {
                this.setHeaders(xhr);
                return beforeSend(xhr);
            };
        }
        else {
            beforeCallback = this.setHeaders;
        }

        let options = {
            url: url,
            beforeSend: beforeCallback,
            type: httpMethod,
            async: sync === false,
            success: function (d) {
                if (typeof loading === "function") {
                    loading(false);
                }

                if (typeof success === "function") {
                    success(d || {});
                }
            },
            error: function (xhr, status, err) {
                if (typeof loading === "function") {
                    loading(false);
                }

                if (typeof failure === "function") {
                    if (xhr) {
                        failure(xhr, err);
                    }
                    else {
                        failure(null, "Unknown error");
                    }
                }
            }
        };

        if (httpMethod === "GET") {
            options.data = params;
        } else if (postFile) {
            options.processData = false;
            options.contentType = false;
            options.data = params;
        }
        else {
            options.contentType = "application/json; charset=UTF-8";
            options.data = JSON.stringify(params);
            options.dataType = "json";
        }

        if (typeof loading === "function") {
            loading(true);
        }

        return window.$.ajax(options);
    }

    setHeaders = (xhr) => {
        if (this.tabId) {
            xhr.setRequestHeader("TabId", this.tabId);
        }

        if (this.antiForgeryToken.length > 0) {
            xhr.setRequestHeader("RequestVerificaitonToken", this.antiForgeryToken);
        }
    }
}