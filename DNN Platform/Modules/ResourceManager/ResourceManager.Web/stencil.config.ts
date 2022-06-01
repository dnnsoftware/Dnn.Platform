import { Config } from '@stencil/core';
import { sass } from "@stencil/sass";
import dnnConfig from '../../../../settings.local.json';

export const config: Config = {
  namespace: 'dnn-resource-manager',
  outputTargets: [
    {
      type: 'dist',
      esmLoaderPath: '../loader',
    },
    {
      type: 'dist-custom-elements',
    },
    {
      type: 'docs-readme',
    },
    {
      type: 'www',
      serviceWorker: null, // disable service workers
    },
  ],
  plugins: [
    sass(),
  ],
  sourceMap: true,
  buildEs5: 'prod',
};
