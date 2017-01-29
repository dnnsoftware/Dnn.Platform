var Cookies = require('./js-cookie');

class DnnPrompt {
    constructor(vsn, util, params) {
        var self = this;
        self.version = vsn;
        self.util = util;
        self.params = params;
        self.tabId = null;
        self.history = []; // Command history
        // restore history if it exists
        if (sessionStorage) {
            if (sessionStorage.getItem('kb-prompt-console-history')) {
                self.history = JSON.parse(sessionStorage.getItem('kb-prompt-console-history'));
            }
        }
        self.cmdOffset = 0 // reverse offset into history
        self.commands = null;
        self.commandNames = null;
        self.initCommands();

        self.createElements();
        self.wireEvents();
        self.showGreeting();
        self.busy(false);
        self.focus();
    }

    wireEvents() {
        var self = this;

        // intermediary functions so that 'this' points to class and not event source
        self.keyDownHandler = function (e) { self.onKeyDown(e); };
        self.clickHandler = function (e) { self.onClickHandler(e); };

        document.addEventListener('keydown', self.keyDownHandler);
        self.ctrlEl.addEventListener('click', self.clickHandler);
    }

    onClickHandler(e) {
        if (e.target.classList.contains("kb-prompt-cmd-insert")) {
            // insert command and set focus
            this.inputEl.value = e.target.dataset.cmd.replace(/'/g, '"');
            this.inputEl.focus();
        } else {
            this.focus();
        }
    }

    onKeyDown(e) {
        var self = this, ctrlStyle = self.ctrlEl.style;
        if (self.isBusy) return;

        // Keys, only trap if focus is in console.
        if (self.inputEl === document.activeElement) {
            switch (e.keyCode) {
                case 13: // enter key
                    return self.runCmd();
                case 38: // Up arrow
                    if ((self.history.length + self.cmdOffset > 0)) {
                        self.cmdOffset--;
                        self.inputEl.value = self.history[self.history.length + self.cmdOffset];
                        e.preventDefault();
                    }
                    break;
                case 40: // Down arrow
                    if ((self.cmdOffset < -1)) {
                        self.cmdOffset++;
                        self.inputEl.value = self.history[self.history.length + self.cmdOffset];
                        e.preventDefault();
                    }
                    break;
            }
        }
    }

    runCmd() {
        var self = this;
        var txt = self.inputEl.value.trim();

        if (!self.tabId) { self.tabId = dnn.getVar("sf_tabId"); }

        self.cmdOffset = 0; // reset history index
        self.inputEl.value = ""; // clearn input for future commands.
        self.writeLine(txt, "cmd"); // Write cmd to output
        if (txt === "") { return; } // don't process if cmd is emtpy
        self.history.push(txt); // Add cmd to history
        if (sessionStorage) {
            sessionStorage.setItem('kb-prompt-console-history', JSON.stringify(self.history));
        }

        // Client Command
        var tokens = txt.split(" "),
            cmd = tokens[0].toUpperCase();

        if (cmd === "EXIT") { this.util.closePersonaBar() }
        if (cmd === "CLS" || cmd === "CLEAR-SCREEN") { self.outputEl.innerHTML = ""; return; }
        if (cmd === "CONFIG") { self.configConsole(tokens); return; }
        if (cmd === "CLH" || cmd === "CLEAR-HISTORY") {
            self.history = [];
            sessionStorage.removeItem('kb-prompt-console-history'); self.writeLine("Session command history cleared"); return;
        }
        if (cmd === "SET-MODE") { self.changeUserMode(tokens); return; }
        // using if/else to allow reload if hash in URL and also prevent 'syntax invalid' message;
        if (cmd === "RELOAD") {
            location.reload(true);
        } else {
            // Server Command 
            self.busy(true);
            // special handling for 'goto' command
            var bRedirect = false;
            if (cmd === "GOTO") { bRedirect = true; }

            //var afVal = document.getElementsByName('__RequestVerificationToken')[0].value;
            var afVal = this.util.sf.antiForgeryToken;

            var path = 'desktopmodules/kbprompt/api/prompt/cmd';
            if (this.util.sf) { path = this.util.sf.getSiteRoot() + path } else { path = '/' + path };

            fetch(path, {
                method: 'post',
                headers: new Headers({
                    'Content-Type': 'application/json',
                    'RequestVerificationToken': afVal
                }),
                credentials: 'include',
                body: JSON.stringify({ cmdLine: txt, currentPage: self.tabId })
            })
                .then(function (response) { return response.json(); })
                .then(function (result) {
                    if (result.Message) {
                        // dnn web api error
                        result.output = result.Message;
                        result.isError = true;
                    }
                    var output = result.output;
                    var style = result.isError ? "error" : "ok";
                    var data = result.data;

                    if (bRedirect) {
                        window.location.href = output;
                    } else {
                        if (data) {
                            var html = self.renderData(data);
                            self.writeHtml(html);
                            if (output) { self.writeLine(output); }
                        } else if (result.isHtml) {
                            self.writeHtml(output);
                        } else {
                            self.writeLine(output, style);
                            self.newLine(); // a little extra white space
                        }
                    }

                    if (result.mustReload) {
                        self.writeHtml('<div><strong>Relaoding in 3 seconds</strong></div>');
                        setTimeout(() => location.reload(true), 3000);
                    }
                })
                .catch(function () { self.writeLine("Error sending request to server", "error") })
                .then(function () {
                    // finally
                    self.busy(false);
                    self.focus();
                });

            self.inputEl.blur(); // remove focus from input elment
        }

    }

    focus() {
        this.inputEl.focus();
    }

    scrollToBottom() {
        this.ctrlEl.scrollTop = this.ctrlEl.scrollHeight;
    }

    newLine() {
        this.outputEl.appendChild(document.createElement('br'));
        this.scrollToBottom();
    }

    writeLine(txt, cssSuffix) {
        var span = document.createElement('span');
        cssSuffix = cssSuffix || 'ok';
        span.className = 'kb-prompt-' + cssSuffix;
        span.innerText = txt;
        this.outputEl.appendChild(span);
        this.newLine();
    }

    writeHtml(markup) {
        var div = document.createElement('div');
        div.innerHTML = markup;
        this.outputEl.appendChild(div);
        this.newLine();
    }

    renderData(data) {
        if (data.length > 1) {
            return this.renderTable(data);
        } else if (data.length == 1) {
            return this.renderObject(data[0]);
        }
        return "";
    }

    renderTable(rows) {
        var out = '<table class="kb-prompt-tbl"><thead><tr>';
        var linkFields = [];
        // find any command link fields
        for (var fld in rows[0]) {
            if (fld.startsWith("__")) {
                linkFields.push(fld.slice(2));
            }
        }

        for (var i = 0; i < rows.length; i++) {
            var row = rows[i];
            if (i == 0) {
                for (var key in row) {
                    if (key.startsWith("__")) {
                        out += `<th>${key.slice(2)}</th>`;
                    } else {
                        if (linkFields.indexOf(key) === -1) {
                            out += `<th>${key}</th>`;
                        }
                    }
                }
                out += '</tr></thead><tbody>';
            }
            out += '<tr>';
            for (var key in row) {
                if (key.startsWith("__")) {
                    out += `<td><a href="#" class="kb-prompt-cmd-insert" data-cmd="${row[key]}" title="${row[key].replace(/'/g, '&quot;')}">${row[key.slice(2)]}</a></td>`;
                } else if (linkFields.indexOf(key) === -1) {
                    out += `<td>${row[key]}</td>`;
                }
            }
            out += '</tr>'

        }
        out += '</tbody></table>'
        return out;
    }

    renderObject(data) {
        var linkFields = [];
        // find any link fields
        for (var fld in data) {
            if (fld.startsWith("__")) {
                linkFields.push(fld.slice(2));
            }
        }
        var out = '<table class="kb-prompt-tbl">'
        for (var key in data) {
            if (key.startsWith("__")) {
                out += `<tr><td class="kb-prompt-lbl">${key.slice(2)}</td><td>:</td><td><a href="#" class="kb-prompt-cmd-insert" data-cmd="${data[key]}" title="${data[key].replace(/'/g, '&quot;')}">${data[key.slice(2)]}</a></td></tr>`;
            } else {
                if (linkFields.indexOf(key) === -1) {
                    out += `<tr><td class="kb-prompt-lbl">${key}</td><td>:</td><td>${data[key]}</td></tr>`;
                }
            }

        }
        out += '</table>';
        return out;
    }

    showGreeting() {
        this.writeLine('Prompt [' + this.version + '] Type \'help\' to get a list of commands', 'cmd');
        this.newLine();
    }

    createElements() {
        var self = this;
        var doc = document;

        // Create and store CLI elements
        //self.ctrlEl = doc.createElement("div"); //CLI control outer frame
        self.ctrlEl = doc.getElementById("prompt"); //CLI control outer frame
        self.outputEl = doc.createElement("div"); //div holding cosole output
        self.inputEl = doc.createElement("input"); //Input control
        self.busyEl = doc.createElement("div"); // Indicate busy/loading


        // Add CSS
        self.ctrlEl.className = "kb-prompt";
        self.outputEl.className = "kb-prompt-output";
        self.inputEl.className = "kb-prompt-input";
        self.busyEl.className = "kb-prompt-busy"

        self.inputEl.setAttribute("spellcheck", "false");

        // Assemble HTML
        self.ctrlEl.appendChild(self.outputEl);
        self.ctrlEl.appendChild(self.inputEl);
        self.ctrlEl.appendChild(self.busyEl);

        // Hide control and add to DOM
        // var isVisible = Cookies.get("kb-prompt-console-visible") === "true";
        // if (isVisible) {
        self.ctrlEl.style.display = "block";
        // } else {
        //     self.ctrlEl.style.display = "none";
        // }
        //doc.body.appendChild(self.ctrlEl);
        var consoleHeight = Cookies.get("kb-prompt-console-height");
        if (consoleHeight) {
            self.configConsole(['config', consoleHeight]);
        }
        // if (isVisible) { self.inputEl.focus(); }
    }

    initCommands() {
        // eventually get this from the server
        this.commands = [
            { name: 'cls', flags: [] },
            { name: 'console', flags: [] },
            { name: 'reload', flags: [] },
            { name: 'get-module', flags: ['id'] },
            { name: 'list-modules', flags: ['name', 'title', 'all'] },
            { name: 'get-page', flags: ['id', 'name', 'parentid'] },
            { name: 'list-pages', flags: ['parentid'] },
            { name: 'set-page', flags: ['description', 'id', 'keywords', 'name', 'title', 'visible'] },
            { name: 'get-portal', flags: ['id'] },
            { name: 'list-roles', flags: [] },
            { name: 'new-role', flags: ['autoassign', 'description', 'name', 'public'] },
            { name: 'set-role', flags: ['description', 'id', 'name', 'public'] },
            { name: 'get-task', flags: ['id'] },
            { name: 'list-tasks', flags: ['enabled', 'name'] },
            { name: 'set-task', flags: ['enabled', 'id'] },
            { name: 'add-roles', flags: ['end', 'id', 'roles', 'start'] },
            { name: 'delete-user', flags: ['id', 'notify'] },
            { name: 'get-user', flags: ['email', 'id', 'username'] },
            { name: 'list-users', flags: ['email', 'role', 'email'] },
            { name: 'new-user', flags: ['approved', 'displayname', 'email', 'firstname', 'lastname', 'notify', 'password', 'username'] },
            { name: 'purge-user', flags: ['id'] },
            { name: 'reset-password', flags: ['id', 'notify'] },
            { name: 'restore-user', flags: ['id'] }
        ];
        var out = [];
        for (var i = 0; i < this.commands.length; i++) {
            out.push(this.commands[i].name);
        }
        this.commandNames = out;

    }

    busy(b) {
        this.isBusy = b;
        this.busyEl.style.display = b ? "block" : "none";
        this.inputEl.style.display = b ? "none" : "block";
    }

    isFlag(token) {
        return (token && token.startsWith('--'));
    }

    getFlag(flag, tokens) {
        var token = null;
        if (!tokens || tokens.length) { return null; }
        for (var i = 1; i < tokens.length; i++) {
            token = tokens[i];
            // did we find the flag name?
            if (this.isFlag(token) && (token.toUpperCase() === flag.toUpperCase())) {
                // is there a value to be had?
                if ((i + 1) < tokens.length) {
                    if (!this.isFlag(tokens[i + 1])) {
                        return tokens[i + 1];
                    } else {
                        // next token is a flag and not a value. return nothing.
                        return null;
                    }
                } else {
                    // found but no value
                    return null;
                }
            }
        }
        // not found
        return null;
    }

    hasFlag(flag, tokens) {
        if (!tokens || tokens.length) return false;
        for (var i = 0; i < tokens.length; i++) {
            if (tokens[i].toUpperCase === flag.toUpperCase()) { return true; }
        }
        return false;
    }

    // client commands
    configConsole(tokens) {
        var height = null;
        if (this.hasFlag("--height")) {
            height = this.getFlag("--height", tokens);
        } else if (!this.isFlag(tokens[1])) {
            height = tokens[1];
        }

        if (height) {
            this.ctrlEl.style.height = height;
            Cookies.set("kb-prompt-console-height", height)
        }
    }

    changeUserMode(tokens) {
        if (!tokens && tokens.length >= 2) { return; }
        var mode = null;
        if (this.hasFlag("--mode")) {
            mode = this.getFlag("--mode", tokens);
        } else if (!this.isFlag(tokens[1])) {
            mode = tokens[1];
        }
        if (mode) {
            var service = dnn.controlBar.getService();
            var serviceUrl = dnn.controlBar.getServiceUrl(service);
            $.ajax({
                url: serviceUrl + 'ToggleUserMode',
                type: 'POST',
                data: { UserMode: mode },
                beforeSend: service.setModuleHeaders,
                success: function () {
                    window.location.href = window.location.href;
                },
                error: function (xhr) {
                    dnn.controlBar.responseError(xhr);
                }
            });
        }
    }
}

window.DnnPrompt = DnnPrompt;
