'use strict';

Object.defineProperty(exports, '__esModule', { value: true });

const index = require('./index-ca59ddc6.js');

const dnnResourceManagerCss = ":host{display:block}";

let DnnResourceManager = class {
  constructor(hostRef) {
    index.registerInstance(this, hostRef);
  }
  render() {
    return (index.h(index.Host, null, index.h("slot", null)));
  }
};
DnnResourceManager.style = dnnResourceManagerCss;

exports.dnn_resource_manager = DnnResourceManager;
