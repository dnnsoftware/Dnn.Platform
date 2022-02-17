import { p as promiseResolve, b as bootstrapLazy } from './index-fc840d31.js';

/*
 Stencil Client Patch Esm v2.14.0 | MIT Licensed | https://stenciljs.com
 */
const patchEsm = () => {
    return promiseResolve();
};

const defineCustomElements = (win, options) => {
  if (typeof window === 'undefined') return Promise.resolve();
  return patchEsm().then(() => {
  return bootstrapLazy([["dnn-resource-manager",[[1,"dnn-resource-manager"]]]], options);
  });
};

export { defineCustomElements };
