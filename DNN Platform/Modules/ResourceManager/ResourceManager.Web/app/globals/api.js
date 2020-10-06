let tabId = -1;
let moduleId = -1;
let groupId = -1;
let moduleName = "";
let extensionWhitelist = "";
let validationCode = "";

const ANTIFORGERY_TOKEN_KEY = "__RequestVerificationToken";

function getAntiForgeryToken() {
    let inputNodes = document.getElementsByTagName("input");
    inputNodes = Array.prototype.slice.call(inputNodes);
    const antiforgeryTokenNodes = inputNodes.filter(function (inputNode) {
        return inputNode.getAttribute("name") === ANTIFORGERY_TOKEN_KEY;
    });

    if (antiforgeryTokenNodes.length > 0) {
        return antiforgeryTokenNodes[0].value;
    }

    return "";
}

/** function to perform raw http request */
function httpRequest(url, options) {
    const headersObject = {
        "ModuleId": moduleId,
        "TabId": tabId,
        "GroupId": groupId
    };

    if (options.headers) {
        Object.assign(headersObject, options.headers);
    }

    const antiforgeryToken = getAntiForgeryToken();
    if (antiforgeryToken) {
        headersObject.RequestVerificationToken = antiforgeryToken;
    }

    const headers = new Headers(headersObject);
    options = Object.assign(options, {
        credentials: "same-origin",
        headers: headers
    });

   
    return fetch(url, options).then(handleHttpResponse);
}

function SAHttpRequest(url, options) {
    // eslint-disable-next-line
    const request = require("superagent");
    const { method, body, progressTracker } = options;

    const headersObject = {
        "ModuleId": moduleId,
        "TabId": tabId,
        "GroupId": groupId
    };

    if (options.headers) {
        Object.assign(headersObject, options.headers);
    }

    const antiforgeryToken = getAntiForgeryToken();
    if (antiforgeryToken) {
        headersObject.RequestVerificationToken = antiforgeryToken;
    }

    return request(method, url).set(headersObject).send(body).on("progress", progressTracker)
        .then(response => JSON.parse(response.text), handleSAHttpRequestFailure);
}

function handleSAHttpRequestFailure(response) {
    const error = new Error(response.status + " - An expected error has been received from the server.");
    error.code = response.status;
    throw error;
}

/** function to handle http response */
function handleHttpResponse(response) {
    if (response.status >= 200 && response.status < 300) {
        return response.json();
    }

    return response.json()
        .then(function processErrorJson(json) {
            const error = new Error();
            error.code = response.status;
            error.data = json;
            throw error;
        }
        , () => {
            const error = new Error(response.status + " - An expected error has been received from the server.");
            error.code = response.status;
            throw error;
        });
}

/** function to serialize query string object */
function serializeQueryStringParameters(obj) {
    let s = [];
    for (let p in obj) {
        if (obj.hasOwnProperty(p)) {
            s.push(encodeURIComponent(p) + "=" + encodeURIComponent(obj[p]));
        }
    }
    return s.join("&");
}

/**
 * @module api
 */
const api = {
    /** initialize the api module. This call must be done before use any other method of the module 
     * @param {object} options - moduleId, tabId and moduleName are required 
     * */
    init(options) {
        if (!options) {
            throw new Error("The init method requires an options parameter with tabId, moduleId and moduleName");
        }

        if (typeof(options.moduleId) !== "number" || options.moduleId <= 0 ) {
            throw new Error("The moduleId must be a number greater than 0.");            
        }

        if (typeof(options.moduleName) !== "string" || !options.moduleName ) {
            throw new Error("The moduleName must be a string and it can't be empty. ");            
        }
        
        if (typeof(options.tabId) !== "number" || options.tabId <= 0 ) {
            throw new Error("The tabId must be a number greater than 0");            
        }

        moduleId = options.moduleId;
        moduleName = options.moduleName;
        tabId = options.tabId;
        groupId = options.groupId;
        extensionWhitelist = options.extensionWhitelist;
        validationCode = options.validationCode;
    },

    getAntiForgeryToken() {
        return getAntiForgeryToken();
    },

    getServiceRoot(ignoreCurrentModuleApi) {
        // eslint-disable-next-line no-undef    
        let serviceRoot = dnn.getVar("sf_siteRoot", "/");
        serviceRoot += "DesktopModules/";
        if (!ignoreCurrentModuleApi) {
            serviceRoot += moduleName + "/API/";
        }
        return serviceRoot;
    },

    getModuleRoot() {
        // eslint-disable-next-line no-undef
        let moduleRoot = dnn.getVar("sf_siteRoot", "/");
        moduleRoot += "DesktopModules/" + moduleName;

        return moduleRoot;
    },

    /** this function performs a get http request 
     * @param {string} url
     * @param {object} query string object
     * */
    get(url, queryString = null) {
    
        if (queryString) {
            url += (url.indexOf("?") > 0 ? "&" : "?") + serializeQueryStringParameters(queryString);
        }

        return httpRequest(url, {
            method: "GET"
        });
    },
    /** this function performs a post http request 
     * @param {string} url
     * @param {object} body
     * */
    post(url, body, headers) {
        let data = null;
        if (headers && headers["Content-Type"] === "multipart/form-data") {
            let formData = new FormData();
            for (const name in body) {
                formData.append(name, body[name]);
            }
            data = formData;

            delete headers["Content-Type"];
        }

        return httpRequest(url, {
            method: "POST",
            body: data || JSON.stringify(body),
            headers: headers
        });
    },
    /** Posta a primitive value (integer, string, etc.) 
     * @param {string} url
     * @param {any} value
    */
    postPrimitive(url, value) {
        var urlencoded = new URLSearchParams();
        urlencoded.append("", value.toString());
        return httpRequest(url, {
            method: "POST",
            body: urlencoded,
            headers: [{"Content-Type": "application/x-www-form-urlencoded"}],
        });
    },
    /** this function performs a post http request 
     * @param {string} url
     * @param {object} body
     * */
    postFile(url, formData, progressTracker) {
        return SAHttpRequest(url, {
            method: "POST",
            body: formData,
            progressTracker
        });

    },
    /** this function performs a put http request 
     * @param {string} url
     * @param {object} body
     * */
    put(url, body) {
        return httpRequest(url, {
            method: "PUT",
            body: JSON.stringify(body)
        });
    },
    /** this function performs a delete http request 
     * @param {string} url
     * @param {object} body
     * */
    delete(url, body) {
        return httpRequest(url, {
            method: "DELETE",
            body: JSON.stringify(body)
        });
    },
    getHeadersObject() {
        const headersObject = {
            moduleId,
            tabId,
            groupId
        };
        return headersObject;
    },
    getWhitelistObject() {
        const whitelistObject = {
            extensionWhitelist,
            validationCode
        };
        return whitelistObject;
    }
};
export default api;