'use strict';

Object.defineProperty(exports, '__esModule', { value: true });

const index = require('./index-ca59ddc6.js');

/*
 Stencil Client Patch Esm v2.14.0 | MIT Licensed | https://stenciljs.com
 */
const patchEsm = () => {
    return index.promiseResolve();
};

const defineCustomElements = (win, options) => {
  if (typeof window === 'undefined') return Promise.resolve();
  return patchEsm().then(() => {
  return index.bootstrapLazy([["dnn-resource-manager.cjs",[[1,"dnn-resource-manager"]]]], options);
  });
};

exports.defineCustomElements = defineCustomElements;
