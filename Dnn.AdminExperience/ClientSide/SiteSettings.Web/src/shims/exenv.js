"use strict";

const canUseDOM = typeof window !== "undefined" && !!(window.document && window.document.createElement);

module.exports = {
  canUseDOM,
  canUseWorkers: typeof Worker !== "undefined",
  canUseEventListeners: canUseDOM && !!(window.addEventListener || window.attachEvent),
  canUseViewport: canUseDOM && !!window.screen,
};
