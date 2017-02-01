/******/ (function(modules) { // webpackBootstrap
/******/ 	// The module cache
/******/ 	var installedModules = {};
/******/
/******/ 	// The require function
/******/ 	function __webpack_require__(moduleId) {
/******/
/******/ 		// Check if module is in cache
/******/ 		if(installedModules[moduleId])
/******/ 			return installedModules[moduleId].exports;
/******/
/******/ 		// Create a new module (and put it into the cache)
/******/ 		var module = installedModules[moduleId] = {
/******/ 			i: moduleId,
/******/ 			l: false,
/******/ 			exports: {}
/******/ 		};
/******/
/******/ 		// Execute the module function
/******/ 		modules[moduleId].call(module.exports, module, module.exports, __webpack_require__);
/******/
/******/ 		// Flag the module as loaded
/******/ 		module.l = true;
/******/
/******/ 		// Return the exports of the module
/******/ 		return module.exports;
/******/ 	}
/******/
/******/
/******/ 	// expose the modules object (__webpack_modules__)
/******/ 	__webpack_require__.m = modules;
/******/
/******/ 	// expose the module cache
/******/ 	__webpack_require__.c = installedModules;
/******/
/******/ 	// identity function for calling harmony imports with the correct context
/******/ 	__webpack_require__.i = function(value) { return value; };
/******/
/******/ 	// define getter function for harmony exports
/******/ 	__webpack_require__.d = function(exports, name, getter) {
/******/ 		if(!__webpack_require__.o(exports, name)) {
/******/ 			Object.defineProperty(exports, name, {
/******/ 				configurable: false,
/******/ 				enumerable: true,
/******/ 				get: getter
/******/ 			});
/******/ 		}
/******/ 	};
/******/
/******/ 	// getDefaultExport function for compatibility with non-harmony modules
/******/ 	__webpack_require__.n = function(module) {
/******/ 		var getter = module && module.__esModule ?
/******/ 			function getDefault() { return module['default']; } :
/******/ 			function getModuleExports() { return module; };
/******/ 		__webpack_require__.d(getter, 'a', getter);
/******/ 		return getter;
/******/ 	};
/******/
/******/ 	// Object.prototype.hasOwnProperty.call
/******/ 	__webpack_require__.o = function(object, property) { return Object.prototype.hasOwnProperty.call(object, property); };
/******/
/******/ 	// __webpack_public_path__
/******/ 	__webpack_require__.p = "/scripts/";
/******/
/******/ 	// Load entry module and return exports
/******/ 	return __webpack_require__(__webpack_require__.s = 3);
/******/ })
/************************************************************************/
/******/ ([
/* 0 */
/***/ (function(module, exports, __webpack_require__) {

"use strict";


exports.commands = function () {
  return [{ name: 'cls', flags: [] }, { name: 'console', flags: [] }, { name: 'reload', flags: [] }, { name: 'get-module', flags: ['id'] }, { name: 'list-modules', flags: ['name', 'title', 'all'] }, { name: 'get-page', flags: ['id', 'name', 'parentid'] }, { name: 'list-pages', flags: ['parentid'] }, { name: 'set-page', flags: ['description', 'id', 'keywords', 'name', 'title', 'visible'] }, { name: 'get-portal', flags: ['id'] }, { name: 'list-roles', flags: [] }, { name: 'new-role', flags: ['autoassign', 'description', 'name', 'public'] }, { name: 'set-role', flags: ['description', 'id', 'name', 'public'] }, { name: 'get-task', flags: ['id'] }, { name: 'list-tasks', flags: ['enabled', 'name'] }, { name: 'set-task', flags: ['enabled', 'id'] }, { name: 'add-roles', flags: ['end', 'id', 'roles', 'start'] }, { name: 'delete-user', flags: ['id', 'notify'] }, { name: 'get-user', flags: ['email', 'id', 'username'] }, { name: 'list-users', flags: ['email', 'role', 'email'] }, {
    name: 'new-user',
    flags: ['approved', 'displayname', 'email', 'firstname', 'lastname', 'notify', 'password', 'username']
  }, { name: 'purge-user', flags: ['id'] }, { name: 'reset-password', flags: ['id', 'notify'] }, { name: 'restore-user', flags: ['id'] }];
};

/***/ }),
/* 1 */
/***/ (function(module, exports, __webpack_require__) {

"use strict";
var __WEBPACK_AMD_DEFINE_FACTORY__, __WEBPACK_AMD_DEFINE_RESULT__;

var _typeof = typeof Symbol === "function" && typeof Symbol.iterator === "symbol" ? function (obj) { return typeof obj; } : function (obj) { return obj && typeof Symbol === "function" && obj.constructor === Symbol && obj !== Symbol.prototype ? "symbol" : typeof obj; };

/*!
 * JavaScript Cookie v2.1.3
 * https://github.com/js-cookie/js-cookie
 *
 * Copyright 2006, 2015 Klaus Hartl & Fagner Brack
 * Released under the MIT license
 */
;(function (factory) {
	var registeredInModuleLoader = false;
	if (true) {
		!(__WEBPACK_AMD_DEFINE_FACTORY__ = (factory),
				__WEBPACK_AMD_DEFINE_RESULT__ = (typeof __WEBPACK_AMD_DEFINE_FACTORY__ === 'function' ?
				(__WEBPACK_AMD_DEFINE_FACTORY__.call(exports, __webpack_require__, exports, module)) :
				__WEBPACK_AMD_DEFINE_FACTORY__),
				__WEBPACK_AMD_DEFINE_RESULT__ !== undefined && (module.exports = __WEBPACK_AMD_DEFINE_RESULT__));
		registeredInModuleLoader = true;
	}
	if (( false ? 'undefined' : _typeof(exports)) === 'object') {
		module.exports = factory();
		registeredInModuleLoader = true;
	}
	if (!registeredInModuleLoader) {
		var OldCookies = window.Cookies;
		var api = window.Cookies = factory();
		api.noConflict = function () {
			window.Cookies = OldCookies;
			return api;
		};
	}
})(function () {
	function extend() {
		var i = 0;
		var result = {};
		for (; i < arguments.length; i++) {
			var attributes = arguments[i];
			for (var key in attributes) {
				result[key] = attributes[key];
			}
		}
		return result;
	}

	function init(converter) {
		function api(key, value, attributes) {
			var result;
			if (typeof document === 'undefined') {
				return;
			}

			// Write

			if (arguments.length > 1) {
				attributes = extend({
					path: '/'
				}, api.defaults, attributes);

				if (typeof attributes.expires === 'number') {
					var expires = new Date();
					expires.setMilliseconds(expires.getMilliseconds() + attributes.expires * 864e+5);
					attributes.expires = expires;
				}

				// We're using "expires" because "max-age" is not supported by IE
				attributes.expires = attributes.expires ? attributes.expires.toUTCString() : '';

				try {
					result = JSON.stringify(value);
					if (/^[\{\[]/.test(result)) {
						value = result;
					}
				} catch (e) {}

				if (!converter.write) {
					value = encodeURIComponent(String(value)).replace(/%(23|24|26|2B|3A|3C|3E|3D|2F|3F|40|5B|5D|5E|60|7B|7D|7C)/g, decodeURIComponent);
				} else {
					value = converter.write(value, key);
				}

				key = encodeURIComponent(String(key));
				key = key.replace(/%(23|24|26|2B|5E|60|7C)/g, decodeURIComponent);
				key = key.replace(/[\(\)]/g, escape);

				var stringifiedAttributes = '';

				for (var attributeName in attributes) {
					if (!attributes[attributeName]) {
						continue;
					}
					stringifiedAttributes += '; ' + attributeName;
					if (attributes[attributeName] === true) {
						continue;
					}
					stringifiedAttributes += '=' + attributes[attributeName];
				}
				return document.cookie = key + '=' + value + stringifiedAttributes;
			}

			// Read

			if (!key) {
				result = {};
			}

			// To prevent the for loop in the first place assign an empty array
			// in case there are no cookies at all. Also prevents odd result when
			// calling "get()"
			var cookies = document.cookie ? document.cookie.split('; ') : [];
			var rdecode = /(%[0-9A-Z]{2})+/g;
			var i = 0;

			for (; i < cookies.length; i++) {
				var parts = cookies[i].split('=');
				var cookie = parts.slice(1).join('=');

				if (cookie.charAt(0) === '"') {
					cookie = cookie.slice(1, -1);
				}

				try {
					var name = parts[0].replace(rdecode, decodeURIComponent);
					cookie = converter.read ? converter.read(cookie, name) : converter(cookie, name) || cookie.replace(rdecode, decodeURIComponent);

					if (this.json) {
						try {
							cookie = JSON.parse(cookie);
						} catch (e) {}
					}

					if (key === name) {
						result = cookie;
						break;
					}

					if (!key) {
						result[name] = cookie;
					}
				} catch (e) {}
			}

			return result;
		}

		api.set = api;
		api.get = function (key) {
			return api.call(api, key);
		};
		api.getJSON = function () {
			return api.apply({
				json: true
			}, [].slice.call(arguments));
		};
		api.defaults = {};

		api.remove = function (key, attributes) {
			api(key, '', extend(attributes, {
				expires: -1
			}));
		};

		api.withConverter = init;

		return api;
	}

	return init(function () {});
});

/***/ }),
/* 2 */
/***/ (function(module, exports) {

module.exports = jQuery;

/***/ }),
/* 3 */
/***/ (function(module, exports, __webpack_require__) {

"use strict";
/* WEBPACK VAR INJECTION */(function($) {

var _createClass = function () { function defineProperties(target, props) { for (var i = 0; i < props.length; i++) { var descriptor = props[i]; descriptor.enumerable = descriptor.enumerable || false; descriptor.configurable = true; if ("value" in descriptor) descriptor.writable = true; Object.defineProperty(target, descriptor.key, descriptor); } } return function (Constructor, protoProps, staticProps) { if (protoProps) defineProperties(Constructor.prototype, protoProps); if (staticProps) defineProperties(Constructor, staticProps); return Constructor; }; }();

function _classCallCheck(instance, Constructor) { if (!(instance instanceof Constructor)) { throw new TypeError("Cannot call a class as a function"); } }

var Cookies = __webpack_require__(1);
var Commands = __webpack_require__(0);

var DnnPrompt = function () {
  function DnnPrompt(vsn, wrapper, util, params) {
    _classCallCheck(this, DnnPrompt);

    var self = this;

    self.version = vsn;
    self.util = util;
    self.wrapper = wrapper;
    console.log(self.util);
    self.params = params;
    self.tabId = null;
    self.history = []; // Command history
    // restore history if it exists
    if (sessionStorage) {
      if (sessionStorage.getItem('kb-prompt-console-history')) {
        self.history = JSON.parse(sessionStorage.getItem('kb-prompt-console-history'));
      }
    }
    self.cmdOffset = 0; // reverse offset into history
    self.commands = Commands;
    self.commandNames = null;
    self.initCommands();

    self.createElements();
    self.wireEvents();
    self.showGreeting();
    self.busy(false);
    self.focus();
  }

  _createClass(DnnPrompt, [{
    key: 'wireEvents',
    value: function wireEvents() {
      var self = this;

      // intermediary functions so that 'this' points to class and not event source
      self.keyDownHandler = function (e) {
        self.onKeyDown(e);
      };
      self.clickHandler = function (e) {
        self.onClickHandler(e);
      };

      // register on parent doc so panel can be loaded with keypress combo
      window.parent.document.addEventListener('keydown', self.keyDownHandler);
      document.addEventListener('keydown', self.keyDownHandler);
      self.ctrlEl.addEventListener('click', self.clickHandler);
    }
  }, {
    key: 'onClickHandler',
    value: function onClickHandler(e) {
      if (e.target.classList.contains("kb-prompt-cmd-insert")) {
        // insert command and set focus
        this.inputEl.value = e.target.dataset.cmd.replace(/'/g, '"');
        this.inputEl.focus();
      } else {
        this.focus();
      }
    }
  }, {
    key: 'onKeyDown',
    value: function onKeyDown(e) {
      var self = this;
      // CTRL + `
      if (e.ctrlKey && e.keyCode === 192) {
        if (self.wrapper[0].offsetLeft <= 0) {
          self.util.loadPanel("Dnn.Prompt", {
            moduleName: "Dnn.Prompt",
            folderName: "",
            identifier: "Dnn.Prompt",
            path: "Prompt"
          });
        } else {
          self.util.closePersonaBar();
        }
        return;
      }

      if (self.isBusy) return;

      // All other keys, only trap if focus is in console.
      if (self.inputEl === document.activeElement) {
        switch (e.keyCode) {
          case 13:
            // enter key
            return self.runCmd();
          case 38:
            // Up arrow
            if (self.history.length + self.cmdOffset > 0) {
              self.cmdOffset--;
              self.inputEl.value = self.history[self.history.length + self.cmdOffset];
              e.preventDefault();
            }
            break;
          case 40:
            // Down arrow
            if (self.cmdOffset < -1) {
              self.cmdOffset++;
              self.inputEl.value = self.history[self.history.length + self.cmdOffset];
              e.preventDefault();
            }
            break;
        }
      }
    }
  }, {
    key: 'runCmd',
    value: function runCmd() {
      var _this = this;

      var self = this;
      var txt = self.inputEl.value.trim();

      if (!self.tabId) {
        self.tabId = dnn.getVar("sf_tabId");
      }

      self.cmdOffset = 0; // reset history index
      self.inputEl.value = ""; // clearn input for future commands.
      self.writeLine(txt, "cmd"); // Write cmd to output
      if (txt === "") {
        return;
      } // don't process if cmd is emtpy
      self.history.push(txt); // Add cmd to history
      if (sessionStorage) {
        sessionStorage.setItem('kb-prompt-console-history', JSON.stringify(self.history));
      }

      // Client Command
      var tokens = txt.split(" "),
          cmd = tokens[0].toUpperCase();

      if (cmd === "CLS" || cmd === "CLEAR-SCREEN") {
        self.outputEl.innerHTML = "";
        return;
      }
      if (cmd === "EXIT") {
        this.util.closePersonaBar();
        return;
      }
      if (cmd === "HELP") {
        self.renderHelp(tokens);
        return;
      }
      if (cmd === "CONFIG") {
        self.configConsole(tokens);
        return;
      }
      if (cmd === "CLH" || cmd === "CLEAR-HISTORY") {
        self.history = [];
        sessionStorage.removeItem('kb-prompt-console-history');
        self.writeLine("Session command history cleared");
        return;
      }
      if (cmd === "SET-MODE") {
        self.changeUserMode(tokens);
        return;
      }
      // using if/else to allow reload if hash in URL and also prevent 'syntax invalid' message;
      if (cmd === "RELOAD") {
        location.reload(true);
      } else {
        (function () {
          // Server Command
          self.busy(true);
          // special handling for 'goto' command
          var bRedirect = false;
          if (cmd === "GOTO") {
            bRedirect = true;
          }

          var afVal = _this.util.sf.antiForgeryToken;

          var path = 'API/PersonaBar/Command/Cmd';
          if (_this.util.sf) {
            path = _this.util.sf.getSiteRoot() + path;
          } else {
            path = '/' + path;
          }

          fetch(path, {
            method: 'post',
            headers: new Headers({
              'Content-Type': 'application/json',
              'RequestVerificationToken': _this.util.sf.antiForgeryToken
            }),
            credentials: 'include',
            body: JSON.stringify({ cmdLine: txt, currentPage: self.tabId })
          }).then(function (response) {
            return response.json();
          }).then(function (result) {
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
                if (output) {
                  self.writeLine(output);
                }
              } else if (result.isHtml) {
                self.writeHtml(output);
              } else {
                self.writeLine(output, style);
              }
            }

            if (result.mustReload) {
              self.writeHtml('<div class="kb-prompt-ok"><strong>Reloading in 3 seconds</strong></div>');
              setTimeout(function () {
                return location.reload(true);
              }, 3000);
            }
          }).catch(function () {
            self.writeLine("Error sending request to server", "error");
          }).then(function () {
            // finally
            self.busy(false);
            self.focus();
          });

          self.inputEl.blur(); // remove focus from input elment
        })();
      }
    }
  }, {
    key: 'focus',
    value: function focus() {
      this.inputEl.focus();
    }
  }, {
    key: 'scrollToBottom',
    value: function scrollToBottom() {
      this.ctrlEl.scrollTop = this.ctrlEl.scrollHeight;
    }
  }, {
    key: 'newLine',
    value: function newLine() {
      this.outputEl.appendChild(document.createElement('br'));
      this.scrollToBottom();
    }
  }, {
    key: 'writeLine',
    value: function writeLine(txt, cssSuffix) {
      var span = document.createElement('span');
      cssSuffix = cssSuffix || 'ok';
      span.className = 'kb-prompt-' + cssSuffix;
      span.innerText = txt;
      this.outputEl.appendChild(span);
      this.newLine();
    }
  }, {
    key: 'writeHtml',
    value: function writeHtml(markup) {
      var div = document.createElement('div');
      div.innerHTML = markup;
      this.outputEl.appendChild(div);
      this.newLine();
    }
  }, {
    key: 'renderData',
    value: function renderData(data) {
      if (data.length > 1) {
        return this.renderTable(data);
      } else if (data.length == 1) {
        return this.renderObject(data[0]);
      }
      return "";
    }
  }, {
    key: 'renderTable',
    value: function renderTable(rows) {
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
              out += '<th>' + key.slice(2) + '</th>';
            } else {
              if (linkFields.indexOf(key) === -1) {
                out += '<th>' + key + '</th>';
              }
            }
          }
          out += '</tr></thead><tbody>';
        }
        out += '<tr>';
        for (var _key in row) {
          if (_key.startsWith("__")) {
            out += '<td><a href="#" class="kb-prompt-cmd-insert" data-cmd="' + row[_key] + '" title="' + row[_key].replace(/'/g, '&quot;') + '">' + row[_key.slice(2)] + '</a></td>';
          } else if (linkFields.indexOf(_key) === -1) {
            out += '<td>' + row[_key] + '</td>';
          }
        }
        out += '</tr>';
      }
      out += '</tbody></table>';
      return out;
    }
  }, {
    key: 'renderObject',
    value: function renderObject(data) {
      var linkFields = [];
      // find any link fields
      for (var fld in data) {
        if (fld.startsWith("__")) {
          linkFields.push(fld.slice(2));
        }
      }
      var out = '<table class="kb-prompt-tbl">';
      for (var key in data) {
        if (key.startsWith("__")) {
          out += '<tr><td class="kb-prompt-lbl">' + key.slice(2) + '</td><td>:</td><td><a href="#" class="kb-prompt-cmd-insert" data-cmd="' + data[key] + '" title="' + data[key].replace(/'/g, '&quot;') + '">' + data[key.slice(2)] + '</a></td></tr>';
        } else {
          if (linkFields.indexOf(key) === -1) {
            out += '<tr><td class="kb-prompt-lbl">' + key + '</td><td>:</td><td>' + data[key] + '</td></tr>';
          }
        }
      }
      out += '</table>';
      return out;
    }
  }, {
    key: 'renderHelp',
    value: function renderHelp(tokens) {
      var self = this;
      var path = 'DesktopModules/Admin/Dnn.PersonaBar/Modules/Dnn.Prompt/help/';
      if (!tokens || tokens.length == 1) {
        // render list of help commands
        path += 'index.html';
      } else {
        path += tokens[1] + '.html';
      }

      if (this.util.sf) {
        path = this.util.sf.getSiteRoot() + path;
      } else {
        path = '/' + path;
      }
      self.busy(true);
      fetch(path, {
        method: 'get',
        headers: new Headers({
          'Content-Type': 'text/html'
        }),
        credentials: 'include'
      }).then(function (response) {
        if (response.status == 200) {
          return response.text();
        }
        return '<div class="kb-prompt-error">Unable to find help for that command</div>';
      }).then(function (html) {
        self.writeHtml(html);
      }).catch(function () {
        self.writeLine("Error sending request to server", "error");
      }).then(function () {
        // finally
        self.busy(false);
        self.focus();
      });
    }
  }, {
    key: 'showGreeting',
    value: function showGreeting() {
      this.writeLine('Prompt [' + this.version + '] Type \'help\' to get a list of commands', 'cmd');
      this.newLine();
    }
  }, {
    key: 'createElements',
    value: function createElements() {
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
      self.busyEl.className = "kb-prompt-busy";

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
  }, {
    key: 'initCommands',
    value: function initCommands() {
      // eventually get this from the server
      this.commands = [{ name: 'cls', flags: [] }, { name: 'console', flags: [] }, { name: 'reload', flags: [] }, { name: 'get-module', flags: ['id'] }, { name: 'list-modules', flags: ['name', 'title', 'all'] }, { name: 'get-page', flags: ['id', 'name', 'parentid'] }, { name: 'list-pages', flags: ['parentid'] }, { name: 'set-page', flags: ['description', 'id', 'keywords', 'name', 'title', 'visible'] }, { name: 'get-portal', flags: ['id'] }, { name: 'list-roles', flags: [] }, { name: 'new-role', flags: ['autoassign', 'description', 'name', 'public'] }, { name: 'set-role', flags: ['description', 'id', 'name', 'public'] }, { name: 'get-task', flags: ['id'] }, { name: 'list-tasks', flags: ['enabled', 'name'] }, { name: 'set-task', flags: ['enabled', 'id'] }, { name: 'add-roles', flags: ['end', 'id', 'roles', 'start'] }, { name: 'delete-user', flags: ['id', 'notify'] }, { name: 'get-user', flags: ['email', 'id', 'username'] }, { name: 'list-users', flags: ['email', 'role', 'email'] }, {
        name: 'new-user',
        flags: ['approved', 'displayname', 'email', 'firstname', 'lastname', 'notify', 'password', 'username']
      }, { name: 'purge-user', flags: ['id'] }, { name: 'reset-password', flags: ['id', 'notify'] }, { name: 'restore-user', flags: ['id'] }];
      var out = [];
      for (var i = 0; i < this.commands.length; i++) {
        out.push(this.commands[i].name);
      }
      this.commandNames = out;
    }
  }, {
    key: 'busy',
    value: function busy(b) {
      this.isBusy = b;
      this.busyEl.style.display = b ? "block" : "none";
      this.inputEl.style.display = b ? "none" : "block";
    }
  }, {
    key: 'isFlag',
    value: function isFlag(token) {
      return token && token.startsWith('--');
    }
  }, {
    key: 'getFlag',
    value: function getFlag(flag, tokens) {
      var token = null;
      if (!tokens || tokens.length) {
        return null;
      }
      for (var i = 1; i < tokens.length; i++) {
        token = tokens[i];
        // did we find the flag name?
        if (this.isFlag(token) && token.toUpperCase() === flag.toUpperCase()) {
          // is there a value to be had?
          if (i + 1 < tokens.length) {
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
  }, {
    key: 'hasFlag',
    value: function hasFlag(flag, tokens) {
      if (!tokens || tokens.length) return false;
      for (var i = 0; i < tokens.length; i++) {
        if (tokens[i].toUpperCase === flag.toUpperCase()) {
          return true;
        }
      }
      return false;
    }

    // client commands

  }, {
    key: 'configConsole',
    value: function configConsole(tokens) {
      var height = null;
      if (this.hasFlag("--height")) {
        height = this.getFlag("--height", tokens);
      } else if (!this.isFlag(tokens[1])) {
        height = tokens[1];
      }

      if (height) {
        this.ctrlEl.style.height = height;
        Cookies.set("kb-prompt-console-height", height);
      }
    }
  }, {
    key: 'changeUserMode',
    value: function changeUserMode(tokens) {
      if (!tokens && tokens.length >= 2) {
        return;
      }
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
          success: function success() {
            window.location.href = window.location.href;
          },
          error: function error(xhr) {
            dnn.controlBar.responseError(xhr);
          }
        });
      }
    }
  }]);

  return DnnPrompt;
}();

window.DnnPrompt = DnnPrompt;
/* WEBPACK VAR INJECTION */}.call(exports, __webpack_require__(2)))

/***/ })
/******/ ]);
//# sourceMappingURL=prompt-bundle.js.map