import { config as originalConfig } from './stencil.config';
import dnnConfig from '../../../../settings.local.json';

export const config = {
  ...originalConfig,
  ouputTargets: [
    ...originalConfig.outputTargets,
    {
      // For DNN yarn watch --scope dnn-resource-manager
      type: 'dist',
      esmLoaderPath: '../loader',
      dir: dnnConfig?.WebsitePath ? `${dnnConfig.WebsitePath}\\DesktopModules\\ResourceManager\\Scripts` : '../Scripts',
    }
  ]
}