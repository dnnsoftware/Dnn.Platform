import { r as registerInstance, h, H as Host } from './index-fc840d31.js';

const dnnResourceManagerCss = ":host{display:block}";

let DnnResourceManager = class {
  constructor(hostRef) {
    registerInstance(this, hostRef);
  }
  render() {
    return (h(Host, null, h("slot", null)));
  }
};
DnnResourceManager.style = dnnResourceManagerCss;

export { DnnResourceManager as dnn_resource_manager };
